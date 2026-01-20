import { Module, OnModuleInit } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { JwtModule } from '@nestjs/jwt';
import { ModuleRef } from '@nestjs/core';
import { MatchmakingGateway } from './matchmaking.gateway';
import { MatchmakingService } from './matchmaking.service';
import { GameEngineService } from './game-engine.service';
import { Match } from '../database/entities/match.entity';
import { User } from '../database/entities/user.entity';
import { RedisModule } from '../redis/redis.module';
import { PvpSessionService } from './pvp-session.service';
import { RankingModule } from '../ranking/ranking.module';

@Module({
  imports: [
    TypeOrmModule.forFeature([Match, User]),
    RedisModule,
    JwtModule.register({}),
    RankingModule,
  ],
  providers: [MatchmakingGateway, MatchmakingService, GameEngineService, PvpSessionService],
  exports: [MatchmakingService],
})
export class MatchmakingModule implements OnModuleInit {
  constructor(private readonly moduleRef: ModuleRef) {}

  onModuleInit() {
    const matchmakingService = this.moduleRef.get(MatchmakingService, { strict: false });
    const gameEngineService = this.moduleRef.get(GameEngineService, { strict: false });
    matchmakingService.setGameEngine(gameEngineService);
  }
}
