import { Injectable, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { User } from '../database/entities/user.entity';

@Injectable()
export class PlayerService {
  constructor(
    @InjectRepository(User)
    private userRepository: Repository<User>,
  ) {}

  async getProfile(userId: number) {
    const user = await this.userRepository.findOne({
      where: { id: userId },
    });

    if (!user) {
      throw new NotFoundException('User not found');
    }

    return {
      id: user.id,
      username: user.username,
      rating: user.rating,
      wins: user.wins,
      losses: user.losses,
    };
  }

  async getMyProfile(user: User) {
    return {
      id: user.id,
      email: user.email,
      username: user.username,
      rating: user.rating,
      wins: user.wins,
      losses: user.losses,
    };
  }
}
