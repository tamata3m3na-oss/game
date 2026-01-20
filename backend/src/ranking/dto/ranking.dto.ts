import { IsNumber, IsOptional, IsString, Min, Max, IsInt } from 'class-validator';
import { Transform } from 'class-transformer';

export class GetLeaderboardQueryDto {
  @Transform(({ value }) => parseInt(value))
  @IsNumber()
  @IsInt()
  @Min(1)
  @IsOptional()
  page?: number = 1;

  @Transform(({ value }) => parseInt(value))
  @IsNumber()
  @IsInt()
  @Min(1)
  @Max(100)
  @IsOptional()
  limit?: number = 100;
}

export class PlayerRankDto {
  @IsNumber()
  @IsInt()
  @Min(1)
  userId: number;

  @IsString()
  username: string;

  @IsNumber()
  @IsInt()
  @Min(0)
  @Max(3000)
  rating: number;

  @IsNumber()
  @IsInt()
  @Min(0)
  wins: number;

  @IsNumber()
  @IsInt()
  @Min(0)
  losses: number;
}

export class LeaderboardEntryDto {
  @IsNumber()
  @IsInt()
  @Min(1)
  rank: number;

  @IsNumber()
  @IsInt()
  @Min(1)
  userId: number;

  @IsString()
  username: string;

  @IsNumber()
  @IsInt()
  @Min(0)
  @Max(3000)
  rating: number;

  @IsNumber()
  @IsInt()
  @Min(0)
  wins: number;

  @IsNumber()
  @IsInt()
  @Min(0)
  losses: number;

  @IsString()
  winRate: string;
}
