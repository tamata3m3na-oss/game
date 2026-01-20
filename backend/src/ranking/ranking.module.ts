import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { User } from '../database/entities/user.entity';
import { Match } from '../database/entities/match.entity';
import { RankingService } from './ranking.service';
import { RankingController } from './ranking.controller';
import { ELOHelper } from './elo-helper.service';
import { ELOCalculatorService } from './elo-calculator.service';

@Module({
  imports: [TypeOrmModule.forFeature([User, Match])],
  controllers: [RankingController],
  providers: [RankingService, ELOHelper, ELOCalculatorService],
  exports: [RankingService],
})
export class RankingModule {}
