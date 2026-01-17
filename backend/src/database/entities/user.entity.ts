import { Entity, PrimaryGeneratedColumn, Column, CreateDateColumn, OneToMany } from 'typeorm';
import { Match } from './match.entity';

@Entity('users')
export class User {
  @PrimaryGeneratedColumn()
  id: number;

  @Column({ unique: true })
  email: string;

  @Column({ unique: true })
  username: string;

  @Column({ name: 'password_hash' })
  passwordHash: string;

  @Column({ default: 1000 })
  rating: number;

  @Column({ default: 0 })
  wins: number;

  @Column({ default: 0 })
  losses: number;

  @Column({ name: 'rating_change', default: 0 })
  ratingChange: number;

  @Column({ name: 'last_match_id', nullable: true })
  lastMatchId: number;

  @Column({ name: 'last_match_at', type: 'timestamp', nullable: true })
  lastMatchAt: Date;

  @CreateDateColumn({ name: 'created_at' })
  createdAt: Date;

  @OneToMany(() => Match, match => match.player1)
  matchesAsPlayer1: Match[];

  @OneToMany(() => Match, match => match.player2)
  matchesAsPlayer2: Match[];

  @OneToMany(() => Match, match => match.winner)
  matchesAsWinner: Match[];
}
