import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { DatabaseModule } from './database/database.module';
import { AuthModule } from './auth/auth.module';
import { PlayerModule } from './player/player.module';
import { RedisModule } from './redis/redis.module';
import { MatchmakingModule } from './matchmaking/matchmaking.module';
import { RankingModule } from './ranking/ranking.module';

@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: '.env',
    }),
    DatabaseModule,
    RedisModule,
    AuthModule,
    PlayerModule,
    MatchmakingModule,
    RankingModule,
  ],
})
export class AppModule {}
