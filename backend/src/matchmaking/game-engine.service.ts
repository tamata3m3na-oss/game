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
  private readonly maxShield = 50;
  private readonly weaponDamage = 25;
  private readonly weaponCooldownTicks = 20; // 0.5s at 20Hz
  private readonly shieldDurationTicks = 40; // 2s at 20Hz
  private readonly abilityCooldownTicks = 100; // 5s at 20Hz
  private readonly bulletRange = 50; // units
  private readonly bulletSpeed = 8; // units per second
  private readonly bulletSpeedPerTick = this.bulletSpeed / this.tickRate; // 0.4 units per tick
  private readonly hitboxRadius = 2; // units
  private readonly maxMatchDurationMs = 5 * 60 * 1000; // 5 minutes

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
        shieldHealth: 0,
        shieldActive: false,
        shieldEndTick: 0,
        fireReady: true,
        fireReadyTick: 0,
        abilityReady: true,
        lastAbilityTime: 0,
        damageDealt: 0,
      },
      player2: {
        id: player2Id,
        x: 80,
        y: 50,
        rotation: 180,
        health: this.maxHealth,
        shieldHealth: 0,
        shieldActive: false,
        shieldEndTick: 0,
        fireReady: true,
        fireReadyTick: 0,
        abilityReady: true,
        lastAbilityTime: 0,
        damageDealt: 0,
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

    const match = await this.matchRepository.findOne({ where: { id: matchId } });
    if (!match) {
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
    this.updateFireCooldowns(state.player1, state.tick);
    this.updateFireCooldowns(state.player2, state.tick);
    this.updateShieldStatus(state.player1, state.tick);
    this.updateShieldStatus(state.player2, state.tick);

    if (player1Input?.fire) {
      this.processFire(state, state.player1, state.player2);
    }

    if (player2Input?.fire) {
      this.processFire(state, state.player2, state.player1);
    }

    if (player1Input?.ability) {
      this.processAbility(state, state.player1);
    }

    if (player2Input?.ability) {
      this.processAbility(state, state.player2);
    }

    state.tick++;
    state.timestamp = Date.now();

    // Check for timeout (5 minutes)
    const matchDuration = state.timestamp - (match.matchStartedAt?.getTime() || Date.now());
    if (matchDuration >= this.maxMatchDurationMs && !state.winner) {
      state.status = 'completed';
      await this.endMatch(matchId, state, 'timeout');
      return;
    }

    if (state.player1.health <= 0) {
      state.winner = state.player2.id;
      state.status = 'completed';
      await this.endMatch(matchId, state, 'defeat');
    } else if (state.player2.health <= 0) {
      state.winner = state.player1.id;
      state.status = 'completed';
      await this.endMatch(matchId, state, 'defeat');
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

  private updateFireCooldowns(player: PlayerState, currentTick: number): void {
    if (!player.fireReady && currentTick >= player.fireReadyTick) {
      player.fireReady = true;
    }
  }

  private updateShieldStatus(player: PlayerState, currentTick: number): void {
    if (player.shieldActive && currentTick >= player.shieldEndTick) {
      player.shieldActive = false;
      player.shieldHealth = 0;
    }
  }

  private processFire(state: GameState, shooter: PlayerState, target: PlayerState): void {
    if (!shooter.fireReady) {
      return;
    }

    const distance = Math.sqrt(
      Math.pow(target.x - shooter.x, 2) + Math.pow(target.y - shooter.y, 2)
    );

    if (distance <= this.bulletRange) {
      // Simple hit detection - check if target is within hitbox radius
      const hitDetected = distance <= this.hitboxRadius;

      if (hitDetected) {
        // Apply damage to shield first, then health
        if (target.shieldActive && target.shieldHealth > 0) {
         target.shieldHealth -= this.weaponDamage;
         if (target.shieldHealth < 0) {
           const overflowDamage = Math.abs(target.shieldHealth);
           target.health -= overflowDamage; // Apply overflow damage to health
           target.shieldActive = false;
           target.shieldHealth = 0;
         }
        } else {
         target.health -= this.weaponDamage;
        }

        // Update damage dealt
        shooter.damageDealt += this.weaponDamage;

        // Reset fire cooldown
        shooter.fireReady = false;
        shooter.fireReadyTick = state.tick + this.weaponCooldownTicks;
      }
    }
  }

  private processAbility(state: GameState, caster: PlayerState): void {
    if (!caster.abilityReady) {
      return;
    }

    // Activate shield
    caster.shieldActive = true;
    caster.shieldHealth = this.maxShield;
    caster.shieldEndTick = state.tick + this.shieldDurationTicks;

    // Reset ability cooldown
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

  private async endMatch(matchId: number, state: GameState, endReason: string): Promise<void> {
    const match = await this.matchRepository.findOne({ where: { id: matchId } });
    if (!match) {
      return;
    }

    const duration = state.timestamp - (match.matchStartedAt?.getTime() || Date.now());

    // Determine final health values
    const player1FinalHealth = Math.max(0, state.player1.health);
    const player2FinalHealth = Math.max(0, state.player2.health);

    // Save detailed match results
    await this.matchRepository.update(matchId, {
      status: 'completed',
      winnerId: state.winner,
      duration: Math.floor(duration / 1000),
      matchEndedAt: new Date(),
      player1FinalHealth,
      player2FinalHealth,
      player1DamageDealt: state.player1.damageDealt,
      player2DamageDealt: state.player2.damageDealt,
      endReason,
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
      endReason,
    });

    this.sessions.emitToPlayer(state.player2.id, 'game:end', {
      matchId,
      winner: state.winner,
      finalState: state,
      endReason,
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
