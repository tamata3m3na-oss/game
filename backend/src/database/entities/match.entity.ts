import { Entity, PrimaryGeneratedColumn, Column, CreateDateColumn, ManyToOne, JoinColumn } from 'typeorm';
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

  @CreateDateColumn({ name: 'created_at' })
  createdAt: Date;

  @ManyToOne(() => User, user => user.matchesAsPlayer1)
  @JoinColumn({ name: 'player1_id' })
  player1: User;

  @ManyToOne(() => User, user => user.matchesAsPlayer2)
  @JoinColumn({ name: 'player2_id' })
  player2: User;

  @ManyToOne(() => User, user => user.matchesAsWinner)
  @JoinColumn({ name: 'winner_id' })
  winner: User;
}
