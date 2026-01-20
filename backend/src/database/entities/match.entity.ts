import {
  Entity,
  PrimaryGeneratedColumn,
  Column,
  CreateDateColumn,
  ManyToOne,
  JoinColumn,
} from 'typeorm';
import { User } from './user.entity';

@Entity('matches')
export class Match {
  @PrimaryGeneratedColumn()
  id: number;

  @Column({ name: 'player1_id' })
  player1Id: number;

  @Column({ name: 'player2_id' })
  player2Id: number;

  @Column({ name: 'winner_id', nullable: true })
  winnerId: number;

  @Column({ nullable: true })
  duration: number;

  @Column({ type: 'varchar', length: 20, default: 'pending' })
  status: 'pending' | 'active' | 'completed';

  @Column({ name: 'match_started_at', type: 'timestamp', nullable: true })
  matchStartedAt: Date;

  @Column({ name: 'match_ended_at', type: 'timestamp', nullable: true })
  matchEndedAt: Date;

  @Column({ name: 'player1_final_health', nullable: true })
  player1FinalHealth: number;

  @Column({ name: 'player2_final_health', nullable: true })
  player2FinalHealth: number;

  @Column({ name: 'player1_damage_dealt', nullable: true })
  player1DamageDealt: number;

  @Column({ name: 'player2_damage_dealt', nullable: true })
  player2DamageDealt: number;

  @Column({ name: 'end_reason', nullable: true })
  endReason: string;

  @Column({ name: 'player1_rating_before', nullable: true })
  player1RatingBefore: number;

  @Column({ name: 'player1_rating_after', nullable: true })
  player1RatingAfter: number;

  @Column({ name: 'player2_rating_before', nullable: true })
  player2RatingBefore: number;

  @Column({ name: 'player2_rating_after', nullable: true })
  player2RatingAfter: number;

  @CreateDateColumn({ name: 'created_at' })
  createdAt: Date;

  @ManyToOne(() => User, (user) => user.matchesAsPlayer1)
  @JoinColumn({ name: 'player1_id' })
  player1: User;

  @ManyToOne(() => User, (user) => user.matchesAsPlayer2)
  @JoinColumn({ name: 'player2_id' })
  player2: User;

  @ManyToOne(() => User, (user) => user.matchesAsWinner)
  @JoinColumn({ name: 'winner_id' })
  winner: User;
}
