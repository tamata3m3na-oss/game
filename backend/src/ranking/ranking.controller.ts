import { Controller, Get, Param, Query, UseGuards, Req } from '@nestjs/common';
import { JwtAuthGuard } from '../auth/guards/jwt-auth.guard';
import { RankingService } from './ranking.service';
import { GetLeaderboardQueryDto } from './dto/ranking.dto';

@Controller('ranking')
export class RankingController {
  constructor(private readonly rankingService: RankingService) {}

  @Get('leaderboard')
  async getLeaderboard(@Query() query: GetLeaderboardQueryDto) {
    const { page = 1, limit = 100 } = query;
    return this.rankingService.getLeaderboard(page, limit);
  }

  @Get('player/:id')
  async getPlayerRank(@Param('id') id: string) {
    const userId = parseInt(id, 10);
    return this.rankingService.getPlayerRank(userId);
  }

  @UseGuards(JwtAuthGuard)
  @Get('me')
  async getMyRank(@Req() req: any) {
    return this.rankingService.getMyRank(req.user.id);
  }

  @Get('stats')
  async getRankingStats() {
    return this.rankingService.getRankingStats();
  }
}