import { Inject, Injectable, Logger } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { RedisClientType } from 'redis';
import { REDIS_CLIENT } from '../redis/redis.module';
import { User } from '../database/entities/user.entity';
import { Match } from '../database/entities/match.entity';
import { ELOHelper } from './elo-helper.service';
import { MatchResult, PlayerRating } from './interfaces/elo-rating.interface';

@Injectable()
export class RankingService {
  private readonly logger = new Logger(RankingService.name);

  // Cache keys
  private readonly LEADERBOARD_GLOBAL_KEY = 'leaderboard:global';
  private readonly LEADERBOARD_TOP100_KEY = 'leaderboard:top100';
  private readonly LEADERBOARD_CACHE_TTL = 300; // 5 minutes

  constructor(
    @InjectRepository(User) private readonly userRepository: Repository<User>,
    @InjectRepository(Match) private readonly matchRepository: Repository<Match>,
    @Inject(REDIS_CLIENT) private readonly redis: RedisClientType,
    private readonly eloHelper: ELOHelper,
  ) {}

  async processMatchResult(matchId: number): Promise<void> {
    this.logger.log(`Processing match result for match ${matchId}`);

    try {
      const match = await this.matchRepository.findOne({
        where: { id: matchId },
        relations: ['player1', 'player2', 'winner'],
      });

      if (!match) {
        this.logger.error(`Match ${matchId} not found`);
        return;
      }

      if (match.status !== 'completed') {
        this.logger.warn(`Match ${matchId} is not completed yet`);
        return;
      }

      const player1 = match.player1;
      const player2 = match.player2;
      const winner = match.winner;

      if (!player1 || !player2) {
        this.logger.error(`Invalid match data for match ${matchId}`);
        return;
      }

      // Prepare player rating objects
      const player1Rating: PlayerRating = {
        id: player1.id,
        rating: player1.rating,
        wins: player1.wins,
        losses: player1.losses,
      };

      const player2Rating: PlayerRating = {
        id: player2.id,
        rating: player2.rating,
        wins: player2.wins,
        losses: player2.losses,
      };

      // Determine match result type
      const matchResult: MatchResult = {
        matchId,
        player1: player1Rating,
        player2: player2Rating,
        winnerId: winner ? winner.id : null,
        isTie: !winner && match.endReason === 'tie',
        isDisconnection: match.endReason === 'disconnection',
      };

      await this.processMatchResultInternal(matchResult, match);

    } catch (error) {
      this.logger.error(`Error processing match result for match ${matchId}: ${error}`);
      throw error;
    }
  }

  private async processMatchResultInternal(matchResult: MatchResult, match: Match): Promise<void> {
    const { player1, player2, winnerId, isTie, isDisconnection } = matchResult;

    let winnerChange = 0;
    let loserChange = 0;
    let winnerPlayer: PlayerRating;
    let loserPlayer: PlayerRating;

    // Handle different match outcomes
    if (isTie) {
      // Tie: both players get +5 rating points
      const changes = this.eloHelper.calculateTieELOChange(player1, player2);
      winnerChange = changes.winnerChange;
      loserChange = changes.loserChange;
      winnerPlayer = player1;
      loserPlayer = player2;
      
      this.logger.log(`Tie match: both players get +${winnerChange} rating points`);
    } else if (isDisconnection) {
      // Handle disconnection
      const disconnectedPlayerId = winnerId === player1.id ? player2.id : player1.id;
      const disconnectedPlayer = disconnectedPlayerId === player1.id ? player1 : player2;
      const opponent = disconnectedPlayerId === player1.id ? player2 : player1;

      const changes = this.eloHelper.calculateDisconnectionPenalty(disconnectedPlayer, opponent);
      winnerChange = changes.winnerChange;
      loserChange = changes.loserChange;
      winnerPlayer = opponent;
      loserPlayer = disconnectedPlayer;
      
      this.logger.log(`Disconnection: winner ${winnerPlayer.id} gets +${winnerChange}, loser ${loserPlayer.id} gets ${loserChange}`);
    } else if (winnerId) {
      // Normal win/loss
      winnerPlayer = winnerId === player1.id ? player1 : player2;
      loserPlayer = winnerId === player1.id ? player2 : player1;

      const changes = this.eloHelper.calculateELOChange(winnerPlayer, loserPlayer);
      winnerChange = changes.winnerChange;
      loserChange = changes.loserChange;
      
      this.logger.log(`Normal match: winner ${winnerPlayer.id} gets +${winnerChange}, loser ${loserPlayer.id} gets ${loserChange}`);
    } else {
      this.logger.warn(`Unknown match result type for match ${matchResult.matchId}`);
      return;
    }

    // Store rating before changes
    const player1RatingBefore = player1.rating;
    const player2RatingBefore = player2.rating;

    // Update player ratings and stats
    const player1RatingAfter = player1.id === winnerPlayer.id 
      ? this.eloHelper.applyRatingChange(player1.rating, winnerChange)
      : this.eloHelper.applyRatingChange(player1.rating, loserChange);

    const player2RatingAfter = player2.id === winnerPlayer.id 
      ? this.eloHelper.applyRatingChange(player2.rating, winnerChange)
      : this.eloHelper.applyRatingChange(player2.rating, loserChange);

    // Update wins/losses counts
    if (winnerId && !isTie) {
      if (winnerPlayer.id === player1.id) {
        player1.wins += 1;
        player2.losses += 1;
      } else {
        player1.losses += 1;
        player2.wins += 1;
      }
    }

    // Update player records
    await this.userRepository.update(player1.id, {
      rating: player1RatingAfter,
      wins: player1.wins,
      losses: player1.losses,
      ratingChange: winnerId === player1.id ? winnerChange : loserChange,
      lastMatchId: match.id,
      lastMatchAt: new Date(),
    });

    await this.userRepository.update(player2.id, {
      rating: player2RatingAfter,
      wins: player2.wins,
      losses: player2.losses,
      ratingChange: winnerId === player2.id ? winnerChange : loserChange,
      lastMatchId: match.id,
      lastMatchAt: new Date(),
    });

    // Update match record with rating changes
    await this.matchRepository.update(match.id, {
      player1RatingBefore,
      player1RatingAfter,
      player2RatingBefore,
      player2RatingAfter,
    });

    // Invalidate leaderboard cache
    await this.invalidateLeaderboardCache();

    // Update Redis leaderboard cache
    await this.updateLeaderboardCache();

    this.logger.log(`Successfully processed match ${match.id} result`);
  }

  async getLeaderboard(page: number = 1, limit: number = 100): Promise<any[]> {
    const offset = (page - 1) * limit;
    
    try {
      // Try to get from cache first
      const cached = await this.getCachedLeaderboard(page, limit);
      if (cached) {
        return cached;
      }

      // Query database
      const users = await this.userRepository
        .createQueryBuilder('user')
        .select([
          'user.id',
          'user.username',
          'user.rating',
          'user.wins',
          'user.losses',
        ])
        .orderBy('user.rating', 'DESC')
        .addOrderBy('user.wins', 'DESC')
        .offset(offset)
        .limit(limit)
        .getMany();

      // Add rank and win rate to each user
      const leaderboard = users.map((user, index) => ({
        rank: offset + index + 1,
        userId: user.id,
        username: user.username,
        rating: user.rating,
        wins: user.wins,
        losses: user.losses,
        winRate: this.eloHelper.formatWinRate(user.wins, user.losses),
      }));

      // Cache the result
      await this.cacheLeaderboard(page, limit, leaderboard);

      return leaderboard;
    } catch (error) {
      this.logger.error(`Error getting leaderboard: ${error}`);
      throw error;
    }
  }

  async getPlayerRank(userId: number): Promise<any> {
    try {
      // Get player's current data
      const user = await this.userRepository.findOne({
        where: { id: userId },
      });

      if (!user) {
        throw new Error('User not found');
      }

      // Get player's rank
      const playerRank = await this.userRepository
        .createQueryBuilder('user')
        .where('user.rating > :rating', { rating: user.rating })
        .getCount() + 1;

      // Get next rank player (if exists)
      const nextRankUser = await this.userRepository
        .createQueryBuilder('user')
        .where('user.rating < :rating', { rating: user.rating })
        .orderBy('user.rating', 'DESC')
        .getOne();

      const nextRank = nextRankUser ? playerRank + 1 : null;
      const ratingGap = nextRankUser ? user.rating - nextRankUser.rating : 0;

      return {
        userId: user.id,
        username: user.username,
        rating: user.rating,
        wins: user.wins,
        losses: user.losses,
        winRate: this.eloHelper.formatWinRate(user.wins, user.losses),
        rank: playerRank,
        nextRank,
        ratingGap,
      };
    } catch (error) {
      this.logger.error(`Error getting player rank for user ${userId}: ${error}`);
      throw error;
    }
  }

  async getMyRank(userId: number): Promise<any> {
    return this.getPlayerRank(userId);
  }

  async getRankingStats(): Promise<any> {
    try {
      const totalMatches = await this.matchRepository.count();
      const activeUsers = await this.userRepository
        .createQueryBuilder('user')
        .where('(user.wins + user.losses) > 0')
        .getCount();
      
      const topRating = await this.userRepository
        .createQueryBuilder('user')
        .select('MAX(user.rating)', 'maxRating')
        .getRawOne();

      const averageRating = await this.userRepository
        .createQueryBuilder('user')
        .select('AVG(user.rating)', 'avgRating')
        .getRawOne();

      return {
        totalMatches,
        activeUsers,
        topRating: parseInt(topRating?.maxRating || '0'),
        averageRating: Math.round(parseFloat(averageRating?.avgRating || '0')),
      };
    } catch (error) {
      this.logger.error(`Error getting ranking stats: ${error}`);
      throw error;
    }
  }

  // Cache management methods
  private async invalidateLeaderboardCache(): Promise<void> {
    try {
      const keys = await this.redis.keys('leaderboard:*');
      if (keys.length > 0) {
        await this.redis.del(keys);
        this.logger.debug(`Invalidated ${keys.length} leaderboard cache keys`);
      }
    } catch (error) {
      this.logger.warn(`Failed to invalidate leaderboard cache: ${error}`);
    }
  }

  private async updateLeaderboardCache(): Promise<void> {
    try {
      // Get top 100 players
      const topUsers = await this.userRepository
        .createQueryBuilder('user')
        .select([
          'user.id',
          'user.username',
          'user.rating',
          'user.wins',
          'user.losses',
        ])
        .orderBy('user.rating', 'DESC')
        .limit(100)
        .getMany();

      // Update sorted set (for ranking queries)
      const pipeline = this.redis.multi();
      await this.redis.del(this.LEADERBOARD_GLOBAL_KEY);

      for (const user of topUsers) {
        pipeline.zAdd(this.LEADERBOARD_GLOBAL_KEY, { score: user.rating, value: user.id.toString() });
      }
      pipeline.expire(this.LEADERBOARD_GLOBAL_KEY, this.LEADERBOARD_CACHE_TTL);
      await pipeline.exec();

      // Update hash (for quick lookup)
      const hashData: Record<string, any> = {};
      topUsers.forEach((user, index) => {
        hashData[user.id] = JSON.stringify({
          rank: index + 1,
          username: user.username,
          rating: user.rating,
          wins: user.wins,
          losses: user.losses,
          winRate: this.eloHelper.formatWinRate(user.wins, user.losses),
        });
      });

      await this.redis.hSet(this.LEADERBOARD_TOP100_KEY, hashData);
      await this.redis.expire(this.LEADERBOARD_TOP100_KEY, this.LEADERBOARD_CACHE_TTL);

      this.logger.debug('Updated leaderboard cache');
    } catch (error) {
      this.logger.warn(`Failed to update leaderboard cache: ${error}`);
    }
  }

  private async getCachedLeaderboard(page: number, limit: number): Promise<any[] | null> {
    try {
      // For now, just cache the first page with default limit
      if (page === 1 && limit <= 100) {
        const cached = await this.redis.hGetAll(this.LEADERBOARD_TOP100_KEY);
        if (cached && Object.keys(cached).length > 0) {
          return Object.values(cached).map(item => JSON.parse(item));
        }
      }
      return null;
    } catch (error) {
      this.logger.warn(`Failed to get cached leaderboard: ${error}`);
      return null;
    }
  }

  private async cacheLeaderboard(page: number, limit: number, data: any[]): Promise<void> {
    try {
      if (page === 1 && limit <= 100) {
        const hashData: Record<string, any> = {};
        data.forEach((entry) => {
          hashData[entry.userId] = JSON.stringify(entry);
        });
        await this.redis.hSet(this.LEADERBOARD_TOP100_KEY, hashData);
        await this.redis.expire(this.LEADERBOARD_TOP100_KEY, this.LEADERBOARD_CACHE_TTL);
      }
    } catch (error) {
      this.logger.warn(`Failed to cache leaderboard: ${error}`);
    }
  }
}