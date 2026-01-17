import { IsNumber, IsBoolean, Min, Max } from 'class-validator';

export class GameInputDto {
  @IsNumber()
  @Min(-1)
  @Max(1)
  moveX: number;

  @IsNumber()
  @Min(-1)
  @Max(1)
  moveY: number;

  @IsBoolean()
  fire: boolean;

  @IsBoolean()
  ability: boolean;

  @IsNumber()
  timestamp: number;
}
