import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { JwtModule } from '@nestjs/jwt';
import { MatchmakingGateway } from './matchmaking.gateway';
import { MatchmakingService } from './matchmaking.service';
import { Match } from '../database/entities/match.entity';
import { User } from '../database/entities/user.entity';
import { RedisModule } from '../redis/redis.module';
import { PvpSessionService } from './pvp-session.service';

@Module({
  imports: [TypeOrmModule.forFeature([Match, User]), RedisModule, JwtModule.register({})],
  providers: [MatchmakingGateway, MatchmakingService, PvpSessionService],
  exports: [MatchmakingService],
})
export class MatchmakingModule {}
