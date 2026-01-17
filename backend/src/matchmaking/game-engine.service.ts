import { Inject, Injectable, Logger } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { RedisClientType } from 'redis';
import { REDIS_CLIENT } from '../redis/redis.module';
import { Match } from '../database/entities/match.entity';
import { User } from '../database/entities/user.entity';
import { GameState, PlayerState } from './interfaces/game-state.interface';
import { PlayerInput } from './interfaces/player-input.interface';
import { PvpSessionService } from './pvp-session.service';

@Injectable()
export class GameEngineService {
  private readonly logger = new Logger(GameEngineService.name);

  private readonly tickRate = 20;
  private readonly tickInterval = 1000 / this.tickRate; // 50ms
  private readonly playerSpeed = 5; // units per second
  private readonly movePerTick = this.playerSpeed / this.tickRate; // 0.25 units per tick
  private readonly mapWidth = 100;
  private readonly mapHeight = 100;
  private readonly abilityCooldown = 5000; // 5 seconds
  private readonly maxHealth = 100;

  private readonly activeMatches = new Map<number, NodeJS.Timeout>();
  private readonly inputQueues = new Map<number, Map<number, PlayerInput[]>>();

  constructor(
    @Inject(REDIS_CLIENT) private readonly redis: RedisClientType,
    @InjectRepository(Match) private readonly matchRepository: Repository<Match>,
    @InjectRepository(User) private readonly userRepository: Repository<User>,
    private readonly sessions: PvpSessionService,
  ) {}

  async startMatch(matchId: number, player1Id: number, player2Id: number): Promise<void> {
    if (this.activeMatches.has(matchId)) {
      this.logger.warn(`Match ${matchId} already running`);
      return;
    }

    const initialState: GameState = {
      matchId,
      player1: {
        id: player1Id,
        x: 20,
        y: 50,
        rotation: 0,
        health: this.maxHealth,
        abilityReady: true,
        lastAbilityTime: 0,
      },
      player2: {
        id: player2Id,
        x: 80,
        y: 50,
        rotation: 180,
        health: this.maxHealth,
        abilityReady: true,
        lastAbilityTime: 0,
      },
      tick: 0,
      timestamp: Date.now(),
      status: 'active',
    };

    await this.saveGameState(matchId, initialState);

    this.inputQueues.set(matchId, new Map([
      [player1Id, []],
      [player2Id, []],
    ]));

    const interval = setInterval(async () => {
      try {
        await this.gameTick(matchId);
      } catch (error) {
        this.logger.error(`Error in game tick for match ${matchId}: ${(error as Error).message}`);
      }
    }, this.tickInterval);

    this.activeMatches.set(matchId, interval);
    this.logger.log(`Started game loop for match ${matchId} at ${this.tickRate}Hz`);
  }

  async stopMatch(matchId: number): Promise<void> {
    const interval = this.activeMatches.get(matchId);
    if (interval) {
      clearInterval(interval);
      this.activeMatches.delete(matchId);
      this.inputQueues.delete(matchId);
      await this.redis.del(this.gameStateKey(matchId));
      this.logger.log(`Stopped game loop for match ${matchId}`);
    }
  }

  async handlePlayerInput(matchId: number, playerId: number, input: PlayerInput): Promise<void> {
    if (!this.activeMatches.has(matchId)) {
      return;
    }

    if (!this.validateInput(input)) {
      this.logger.warn(`Invalid input from player ${playerId} in match ${matchId}`);
      return;
    }

    const matchQueues = this.inputQueues.get(matchId);
    if (!matchQueues) {
      return;
    }

    const playerQueue = matchQueues.get(playerId);
    if (!playerQueue) {
      return;
    }

    playerQueue.push(input);

    if (playerQueue.length > 10) {
      playerQueue.shift();
    }
  }

  private async gameTick(matchId: number): Promise<void> {
    const state = await this.getGameState(matchId);
    if (!state || state.status !== 'active') {
      await this.stopMatch(matchId);
      return;
    }

    const matchQueues = this.inputQueues.get(matchId);
    if (!matchQueues) {
      return;
    }

    const player1Input = this.getLatestInput(matchQueues, state.player1.id);
    const player2Input = this.getLatestInput(matchQueues, state.player2.id);

    this.processPlayerMovement(state.player1, player1Input);
    this.processPlayerMovement(state.player2, player2Input);

    this.updateAbilityCooldowns(state.player1);
    this.updateAbilityCooldowns(state.player2);

    if (player1Input?.fire) {
      this.processFire(state, state.player1, state.player2);
    }

    if (player2Input?.fire) {
      this.processFire(state, state.player2, state.player1);
    }

    if (player1Input?.ability && state.player1.abilityReady) {
      this.processAbility(state, state.player1, state.player2);
    }

    if (player2Input?.ability && state.player2.abilityReady) {
      this.processAbility(state, state.player2, state.player1);
    }

    state.tick++;
    state.timestamp = Date.now();

    if (state.player1.health <= 0) {
      state.winner = state.player2.id;
      state.status = 'completed';
      await this.endMatch(matchId, state);
    } else if (state.player2.health <= 0) {
      state.winner = state.player1.id;
      state.status = 'completed';
      await this.endMatch(matchId, state);
    }

    await this.saveGameState(matchId, state);

    this.sessions.emitToPlayer(state.player1.id, 'game:snapshot', state);
    this.sessions.emitToPlayer(state.player2.id, 'game:snapshot', state);
  }

  private getLatestInput(matchQueues: Map<number, PlayerInput[]>, playerId: number): PlayerInput | null {
    const queue = matchQueues.get(playerId);
    if (!queue || queue.length === 0) {
      return null;
    }

    return queue.shift() || null;
  }

  private processPlayerMovement(player: PlayerState, input: PlayerInput | null): void {
    if (!input) {
      return;
    }

    const magnitude = Math.sqrt(input.moveX * input.moveX + input.moveY * input.moveY);
    if (magnitude === 0) {
      return;
    }

    const normalizedX = input.moveX / magnitude;
    const normalizedY = input.moveY / magnitude;

    const deltaX = normalizedX * this.movePerTick;
    const deltaY = normalizedY * this.movePerTick;

    const newX = player.x + deltaX;
    const newY = player.y + deltaY;

    player.x = Math.max(0, Math.min(this.mapWidth, newX));
    player.y = Math.max(0, Math.min(this.mapHeight, newY));

    if (input.moveX !== 0 || input.moveY !== 0) {
      player.rotation = Math.atan2(input.moveY, input.moveX) * (180 / Math.PI);
    }
  }

  private updateAbilityCooldowns(player: PlayerState): void {
    if (!player.abilityReady && player.lastAbilityTime > 0) {
      const timeSinceAbility = Date.now() - player.lastAbilityTime;
      if (timeSinceAbility >= this.abilityCooldown) {
        player.abilityReady = true;
      }
    }
  }

  private processFire(state: GameState, shooter: PlayerState, target: PlayerState): void {
    const distance = Math.sqrt(
      Math.pow(target.x - shooter.x, 2) + Math.pow(target.y - shooter.y, 2)
    );

    if (distance <= 10) {
      target.health = Math.max(0, target.health - 10);
    }
  }

  private processAbility(state: GameState, caster: PlayerState, target: PlayerState): void {
    if (!caster.abilityReady) {
      return;
    }

    const distance = Math.sqrt(
      Math.pow(target.x - caster.x, 2) + Math.pow(target.y - caster.y, 2)
    );

    if (distance <= 15) {
      target.health = Math.max(0, target.health - 25);
    }

    caster.abilityReady = false;
    caster.lastAbilityTime = Date.now();
  }

  private validateInput(input: PlayerInput): boolean {
    if (input.timestamp > Date.now() + 1000) {
      return false;
    }

    if (Math.abs(input.moveX) > 1 || Math.abs(input.moveY) > 1) {
      return false;
    }

    const magnitude = Math.sqrt(input.moveX * input.moveX + input.moveY * input.moveY);
    if (magnitude > 1.1) {
      return false;
    }

    return true;
  }

  private async endMatch(matchId: number, state: GameState): Promise<void> {
    const match = await this.matchRepository.findOne({ where: { id: matchId } });
    if (!match) {
      return;
    }

    const duration = state.timestamp - (match.matchStartedAt?.getTime() || Date.now());

    await this.matchRepository.update(matchId, {
      status: 'completed',
      winnerId: state.winner,
      duration: Math.floor(duration / 1000),
    });

    if (state.winner) {
      const winner = await this.userRepository.findOne({ where: { id: state.winner } });
      const loser = await this.userRepository.findOne({
        where: { id: state.winner === state.player1.id ? state.player2.id : state.player1.id }
      });

      if (winner && loser) {
        await this.userRepository.update(winner.id, {
          wins: winner.wins + 1,
          rating: winner.rating + 25,
        });

        await this.userRepository.update(loser.id, {
          losses: loser.losses + 1,
          rating: Math.max(0, loser.rating - 25),
        });
      }
    }

    this.sessions.emitToPlayer(state.player1.id, 'game:end', {
      matchId,
      winner: state.winner,
      finalState: state,
    });

    this.sessions.emitToPlayer(state.player2.id, 'game:end', {
      matchId,
      winner: state.winner,
      finalState: state,
    });

    await this.stopMatch(matchId);
  }

  private async saveGameState(matchId: number, state: GameState): Promise<void> {
    const key = this.gameStateKey(matchId);
    await this.redis.set(key, JSON.stringify(state), { EX: 3660 });
  }

  private async getGameState(matchId: number): Promise<GameState | null> {
    const key = this.gameStateKey(matchId);
    const data = await this.redis.get(key);
    if (!data) {
      return null;
    }

    try {
      return JSON.parse(data) as GameState;
    } catch {
      return null;
    }
  }

  private gameStateKey(matchId: number): string {
    return `matchmaking:match:${matchId}:game`;
  }

  isMatchActive(matchId: number): boolean {
    return this.activeMatches.has(matchId);
  }
}
