import { Controller, Get, Param, UseGuards, Request } from '@nestjs/common';
import { PlayerService } from './player.service';
import { JwtAuthGuard } from '../auth/guards/jwt-auth.guard';

@Controller('player')
export class PlayerController {
  constructor(private readonly playerService: PlayerService) {}

  @Get('me')
  @UseGuards(JwtAuthGuard)
  async getMyProfile(@Request() req) {
    return this.playerService.getMyProfile(req.user);
  }

  @Get(':id')
  async getProfile(@Param('id') id: string) {
    return this.playerService.getProfile(parseInt(id));
  }
}
