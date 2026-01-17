import { Inject, Injectable, Logger, OnModuleDestroy, OnModuleInit } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { RedisClientType } from 'redis';
import { REDIS_CLIENT } from '../redis/redis.module';
import { User } from '../database/entities/user.entity';
import { Match } from '../database/entities/match.entity';
import { WaitingPlayer } from './interfaces/waiting-player.interface';
import { PvpSessionService } from './pvp-session.service';
import { GameEngineService } from './game-engine.service';

interface QueueStatusPayload {
  position: number;
  estimatedWait: number;
}

@Injectable()
export class MatchmakingService implements OnModuleInit, OnModuleDestroy {
  private readonly logger = new Logger(MatchmakingService.name);

  private readonly bracketSize = 200;
  private readonly ratingRange = 200;

  private readonly queueCheckIntervalMs = 500;
  private readonly minWaitMs = 3000;
  private readonly quickMatchMinWaitMs = 500;

  private readonly bracketsKey = 'matchmaking:brackets';
  private readonly waitingPlayerKeyPrefix = 'matchmaking:waiting_player:';
  private readonly playerMatchKeyPrefix = 'matchmaking:player_match:';
  private readonly matchKeyPrefix = 'matchmaking:match:';
  private readonly recentOpponentKeyPrefix = 'matchmaking:recent_opponent:';
  private readonly recentOpponentTtlSeconds = 60;

  private interval: NodeJS.Timeout | null = null;
  private processing = false;

  private gameEngine: GameEngineService;

  constructor(
    @Inject(REDIS_CLIENT) private readonly redis: RedisClientType,
    @InjectRepository(User) private readonly userRepository: Repository<User>,
    @InjectRepository(Match) private readonly matchRepository: Repository<Match>,
    private readonly sessions: PvpSessionService,
  ) {}

  setGameEngine(gameEngine: GameEngineService) {
    this.gameEngine = gameEngine;
  }

  onModuleInit() {
    this.interval = setInterval(() => {
      void this.tick();
    }, this.queueCheckIntervalMs);
  }

  onModuleDestroy() {
    if (this.interval) {
      clearInterval(this.interval);
      this.interval = null;
    }
  }

  async joinQueue(playerId: number): Promise<QueueStatusPayload> {
    const existing = await this.redis.get(this.waitingPlayerKey(playerId));
    if (existing) {
      const status = await this.getQueueStatus(playerId);
      return status || { position: 0, estimatedWait: 0 };
    }

    const user = await this.userRepository.findOne({ where: { id: playerId } });
    if (!user) {
      throw new Error('User not found');
    }

    const joinedAt = Date.now();
    const bracket = this.getBracket(user.rating);

    const waitingPlayer: WaitingPlayer & { bracket: number } = {
      playerId,
      rating: user.rating,
      joinedAt,
      bracket,
    };

    const queueKey = this.queueKey(bracket);

    await this.redis.multi()
      .set(this.waitingPlayerKey(playerId), JSON.stringify(waitingPlayer), { EX: 60 * 30 })
      .zAdd(queueKey, { score: joinedAt, value: playerId.toString() })
      .sAdd(this.bracketsKey, bracket.toString())
      .exec();

    await this.broadcastQueueStatusForBracket(bracket);

    const status = await this.getQueueStatus(playerId);
    return status || { position: 0, estimatedWait: 0 };
  }

  async leaveQueue(playerId: number): Promise<void> {
    const waiting = await this.getWaitingPlayer(playerId);
    if (!waiting) {
      return;
    }

    await this.removeFromQueue(playerId, waiting.bracket, true);
  }

  async handleDisconnect(playerId: number): Promise<void> {
    await this.leaveQueue(playerId);

    const matchIdStr = await this.redis.get(this.playerMatchKey(playerId));
    if (!matchIdStr) {
      return;
    }

    const matchId = parseInt(matchIdStr, 10);
    const matchKey = this.matchKey(matchId);
    const matchState = await this.redis.hGetAll(matchKey);

    if (!matchState.player1Id || !matchState.player2Id) {
      await this.cleanupMatchState(matchId);
      return;
    }

    const started = matchState.started === '1';
    const player1Id = parseInt(matchState.player1Id, 10);
    const player2Id = parseInt(matchState.player2Id, 10);
    const opponentId = playerId === player1Id ? player2Id : player1Id;

    if (started) {
      if (this.gameEngine && this.gameEngine.isMatchActive(matchId)) {
        await this.gameEngine.stopMatch(matchId);
      }

      await this.matchRepository.update(matchId, { 
        status: 'completed',
        winnerId: opponentId,
      });

      const winner = await this.userRepository.findOne({ where: { id: opponentId } });
      if (winner) {
        await this.userRepository.update(opponentId, {
          wins: winner.wins + 1,
          rating: winner.rating + 25,
        });

        const loser = await this.userRepository.findOne({ where: { id: playerId } });
        if (loser) {
          await this.userRepository.update(playerId, {
            losses: loser.losses + 1,
            rating: Math.max(0, loser.rating - 25),
          });
        }
      }

      this.sessions.emitToPlayer(opponentId, 'game:end', {
        matchId,
        winner: opponentId,
        reason: 'opponent_disconnected',
      });

      await this.cleanupMatchState(matchId);
      return;
    }

    await this.matchRepository.update(matchId, { status: 'completed' });

    await this.cleanupMatchState(matchId);

    const opponentSocketId = this.sessions.getSocketId(opponentId);
    if (!opponentSocketId) {
      return;
    }

    try {
      const status = await this.joinQueue(opponentId);
      this.sessions.emitToPlayer(opponentId, 'queue:status', status);
    } catch {
      // ignore
    }
  }

  async getQueueStatus(playerId: number): Promise<QueueStatusPayload | null> {
    const waiting = await this.getWaitingPlayer(playerId);
    if (!waiting) {
      return null;
    }

    const rank = await this.redis.zRank(this.queueKey(waiting.bracket), playerId.toString());
    if (rank === null) {
      return null;
    }

    return {
      position: rank + 1,
      estimatedWait: this.estimateWaitSeconds(rank + 1),
    };
  }

  async isPlayerInQueue(playerId: number): Promise<boolean> {
    const exists = await this.redis.exists(this.waitingPlayerKey(playerId));
    return exists === 1;
  }

  async markPlayerReady(playerId: number, matchId: number): Promise<void> {
    const matchKey = this.matchKey(matchId);
    const state = await this.redis.hGetAll(matchKey);

    if (!state.player1Id || !state.player2Id) {
      return;
    }

    const player1Id = parseInt(state.player1Id, 10);
    const player2Id = parseInt(state.player2Id, 10);

    if (playerId !== player1Id && playerId !== player2Id) {
      return;
    }

    if (state.started === '1') {
      return;
    }

    if (playerId === player1Id) {
      await this.redis.hSet(matchKey, { player1Ready: '1' });
    } else {
      await this.redis.hSet(matchKey, { player2Ready: '1' });
    }

    const after = await this.redis.hGetAll(matchKey);
    const bothReady = after.player1Ready === '1' && after.player2Ready === '1';

    if (!bothReady) {
      return;
    }

    const startedLockKey = `${matchKey}:started_lock`;
    const acquired = await this.redis.set(startedLockKey, '1', { NX: true, EX: 30 });
    if (!acquired) {
      return;
    }

    await this.redis.hSet(matchKey, { started: '1' });

    await this.matchRepository.update(matchId, {
      status: 'active',
      matchStartedAt: new Date(),
    });

    const [player1, player2] = await Promise.all([
      this.userRepository.findOne({ where: { id: player1Id } }),
      this.userRepository.findOne({ where: { id: player2Id } }),
    ]);

    if (!player1 || !player2) {
      return;
    }

    this.sessions.emitToPlayer(player1Id, 'match:start', {
      matchId,
      opponent: { id: player2.id, username: player2.username, rating: player2.rating },
      color: 'white',
    });

    this.sessions.emitToPlayer(player2Id, 'match:start', {
      matchId,
      opponent: { id: player1.id, username: player1.username, rating: player1.rating },
      color: 'black',
    });

    await this.redis.expire(matchKey, 60 * 60);
    await this.redis.expire(this.playerMatchKey(player1Id), 60 * 60);
    await this.redis.expire(this.playerMatchKey(player2Id), 60 * 60);

    if (this.gameEngine) {
      await this.gameEngine.startMatch(matchId, player1Id, player2Id);
    }
  }

  private async tick(): Promise<void> {
    if (this.processing) {
      return;
    }

    this.processing = true;

    try {
      await this.processMatches();
    } catch (error) {
      this.logger.error(`Matchmaking tick error: ${(error as Error).message}`);
    } finally {
      this.processing = false;
    }
  }

  private async processMatches(): Promise<void> {
    const brackets = await this.redis.sMembers(this.bracketsKey);
    if (brackets.length === 0) {
      return;
    }

    const bracketEntries: Array<{ bracket: number; entries: Array<{ playerId: number; joinedAt: number }> }> = [];

    for (const b of brackets) {
      const bracket = parseInt(b, 10);
      const queueKey = this.queueKey(bracket);

      const entries = await this.redis.zRangeWithScores(queueKey, 0, -1);
      if (entries.length === 0) {
        await this.redis.sRem(this.bracketsKey, b);
        continue;
      }

      bracketEntries.push({
        bracket,
        entries: entries.map(e => ({ playerId: parseInt(e.value, 10), joinedAt: e.score })),
      });
    }

    const allPlayerIds: number[] = [];
    const joinedAtByPlayer = new Map<number, number>();

    for (const be of bracketEntries) {
      for (const e of be.entries) {
        allPlayerIds.push(e.playerId);
        joinedAtByPlayer.set(e.playerId, e.joinedAt);
      }
    }

    if (allPlayerIds.length < 2) {
      return;
    }

    const waitingKeys = allPlayerIds.map(id => this.waitingPlayerKey(id));
    const rawPlayers = await this.redis.mGet(waitingKeys);

    const players: Array<WaitingPlayer & { bracket: number }> = [];
    const stalePlayerIds: number[] = [];

    for (let i = 0; i < allPlayerIds.length; i++) {
      const id = allPlayerIds[i];
      const raw = rawPlayers[i];

      if (!raw) {
        stalePlayerIds.push(id);
        continue;
      }

      try {
        const parsed = JSON.parse(raw) as WaitingPlayer & { bracket: number };
        const joinedAt = joinedAtByPlayer.get(id);
        players.push({ ...parsed, joinedAt: joinedAt ?? parsed.joinedAt });
      } catch {
        stalePlayerIds.push(id);
      }
    }

    if (stalePlayerIds.length > 0) {
      await this.cleanupStalePlayers(stalePlayerIds);
    }

    if (players.length < 2) {
      return;
    }

    players.sort((a, b) => a.joinedAt - b.joinedAt);

    const now = Date.now();
    const minWait = players.length <= 2 ? this.quickMatchMinWaitMs : this.minWaitMs;

    const matched = new Set<number>();

    for (let i = 0; i < players.length; i++) {
      const p1 = players[i];
      if (matched.has(p1.playerId)) {
        continue;
      }

      if (now - p1.joinedAt < minWait) {
        continue;
      }

      for (let j = i + 1; j < players.length; j++) {
        const p2 = players[j];

        if (matched.has(p2.playerId)) {
          continue;
        }

        if (now - p2.joinedAt < minWait) {
          continue;
        }

        if (Math.abs(p1.rating - p2.rating) > this.ratingRange) {
          continue;
        }

        const recent = await this.isRecentOpponent(p1.playerId, p2.playerId);
        if (recent) {
          continue;
        }

        await this.createAndEmitMatch(p1, p2);

        matched.add(p1.playerId);
        matched.add(p2.playerId);
        break;
      }
    }
  }

  private async createAndEmitMatch(
    p1: WaitingPlayer & { bracket: number },
    p2: WaitingPlayer & { bracket: number },
  ): Promise<void> {
    const match = this.matchRepository.create({
      player1Id: p1.playerId,
      player2Id: p2.playerId,
      status: 'pending',
    });

    await this.matchRepository.save(match);

    await Promise.all([
      this.removeFromQueue(p1.playerId, p1.bracket, false),
      this.removeFromQueue(p2.playerId, p2.bracket, false),
    ]);

    await Promise.all([
      this.broadcastQueueStatusForBracket(p1.bracket),
      this.broadcastQueueStatusForBracket(p2.bracket),
    ]);

    await this.redis.multi()
      .hSet(this.matchKey(match.id), {
        player1Id: p1.playerId.toString(),
        player2Id: p2.playerId.toString(),
        player1Ready: '0',
        player2Ready: '0',
        started: '0',
      })
      .expire(this.matchKey(match.id), 60 * 10)
      .set(this.playerMatchKey(p1.playerId), match.id.toString(), { EX: 60 * 10 })
      .set(this.playerMatchKey(p2.playerId), match.id.toString(), { EX: 60 * 10 })
      .set(this.recentOpponentKey(p1.playerId), p2.playerId.toString(), { EX: this.recentOpponentTtlSeconds })
      .set(this.recentOpponentKey(p2.playerId), p1.playerId.toString(), { EX: this.recentOpponentTtlSeconds })
      .exec();

    const [u1, u2] = await Promise.all([
      this.userRepository.findOne({ where: { id: p1.playerId } }),
      this.userRepository.findOne({ where: { id: p2.playerId } }),
    ]);

    if (!u1 || !u2) {
      return;
    }

    this.sessions.emitToPlayer(u1.id, 'match:found', {
      matchId: match.id,
      opponent: { id: u2.id, username: u2.username, rating: u2.rating },
    });

    this.sessions.emitToPlayer(u2.id, 'match:found', {
      matchId: match.id,
      opponent: { id: u1.id, username: u1.username, rating: u1.rating },
    });

    this.logger.log(`Match ${match.id} found: ${u1.id} vs ${u2.id}`);
  }

  private async broadcastQueueStatusForBracket(bracket: number): Promise<void> {
    const queueKey = this.queueKey(bracket);
    const playerIds = await this.redis.zRange(queueKey, 0, -1);

    for (let idx = 0; idx < playerIds.length; idx++) {
      const playerId = parseInt(playerIds[idx], 10);
      this.sessions.emitToPlayer(playerId, 'queue:status', {
        position: idx + 1,
        estimatedWait: this.estimateWaitSeconds(idx + 1),
      });
    }
  }

  private estimateWaitSeconds(position: number): number {
    if (position <= 1) {
      return 0;
    }

    return Math.max(1, (position - 1) * 3);
  }

  private async isRecentOpponent(playerId: number, opponentId: number): Promise<boolean> {
    const recent = await this.redis.get(this.recentOpponentKey(playerId));
    if (!recent) {
      return false;
    }

    return parseInt(recent, 10) === opponentId;
  }

  private async cleanupStalePlayers(playerIds: number[]): Promise<void> {
    const brackets = await this.redis.sMembers(this.bracketsKey);

    for (const b of brackets) {
      const bracket = parseInt(b, 10);
      const queueKey = this.queueKey(bracket);
      await this.redis.zRem(queueKey, playerIds.map(id => id.toString()));

      const len = await this.redis.zCard(queueKey);
      if (len === 0) {
        await this.redis.sRem(this.bracketsKey, b);
      }
    }
  }

  private async cleanupMatchState(matchId: number): Promise<void> {
    const matchKey = this.matchKey(matchId);
    const state = await this.redis.hGetAll(matchKey);

    const player1Id = state.player1Id ? parseInt(state.player1Id, 10) : null;
    const player2Id = state.player2Id ? parseInt(state.player2Id, 10) : null;

    const multi = this.redis.multi().del(matchKey);

    if (player1Id) {
      multi.del(this.playerMatchKey(player1Id));
    }

    if (player2Id) {
      multi.del(this.playerMatchKey(player2Id));
    }

    await multi.exec();
  }

  private async removeFromQueue(playerId: number, bracket: number, broadcast: boolean): Promise<void> {
    const queueKey = this.queueKey(bracket);

    await this.redis.multi()
      .del(this.waitingPlayerKey(playerId))
      .zRem(queueKey, playerId.toString())
      .exec();

    const remaining = await this.redis.zCard(queueKey);
    if (remaining === 0) {
      await this.redis.sRem(this.bracketsKey, bracket.toString());
    }

    if (broadcast) {
      await this.broadcastQueueStatusForBracket(bracket);
    }
  }

  private getBracket(rating: number): number {
    return Math.floor(rating / this.bracketSize) * this.bracketSize;
  }

  private queueKey(bracket: number): string {
    return `waiting_players:${bracket}`;
  }

  private waitingPlayerKey(playerId: number): string {
    return `${this.waitingPlayerKeyPrefix}${playerId}`;
  }

  private playerMatchKey(playerId: number): string {
    return `${this.playerMatchKeyPrefix}${playerId}`;
  }

  private matchKey(matchId: number): string {
    return `${this.matchKeyPrefix}${matchId}`;
  }

  private recentOpponentKey(playerId: number): string {
    return `${this.recentOpponentKeyPrefix}${playerId}`;
  }

  private async getWaitingPlayer(
    playerId: number,
  ): Promise<(WaitingPlayer & { bracket: number }) | null> {
    const raw = await this.redis.get(this.waitingPlayerKey(playerId));
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as WaitingPlayer & { bracket: number };
    } catch {
      return null;
    }
  }

  async getPlayerActiveMatch(playerId: number): Promise<number | null> {
    const matchIdStr = await this.redis.get(this.playerMatchKey(playerId));
    if (!matchIdStr) {
      return null;
    }

    const matchId = parseInt(matchIdStr, 10);
    if (this.gameEngine && this.gameEngine.isMatchActive(matchId)) {
      return matchId;
    }

    return null;
  }
}
