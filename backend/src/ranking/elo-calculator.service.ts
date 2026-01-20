import { Injectable, Logger } from '@nestjs/common';
import { ELOHelper } from './elo-helper.service';

@Injectable()
export class ELOCalculatorService {
  private readonly logger = new Logger(ELOCalculatorService.name);

  constructor(private readonly eloHelper: ELOHelper) {}

  async testELOCalculations(): Promise<void> {
    this.logger.log('Testing ELO calculations...');

    // Test case 1: Equal players
    const player1 = { id: 1, rating: 1500, wins: 10, losses: 10 };
    const player2 = { id: 2, rating: 1500, wins: 10, losses: 10 };

    const result1 = this.eloHelper.calculateELOChange(player1, player2);
    this.logger.log(
      `Test 1 - Equal players (1500 vs 1500): Winner +${result1.winnerChange}, Loser ${result1.loserChange}`,
    );

    // Test case 2: Stronger vs weaker
    const strongPlayer = { id: 3, rating: 1800, wins: 50, losses: 20 };
    const weakPlayer = { id: 4, rating: 1200, wins: 5, losses: 30 };

    const result2 = this.eloHelper.calculateELOChange(strongPlayer, weakPlayer);
    this.logger.log(
      `Test 2 - Strong vs Weak (1800 vs 1200): Winner +${result2.winnerChange}, Loser ${result2.loserChange}`,
    );

    // Test case 3: Tie
    const tieResult = this.eloHelper.calculateTieELOChange(player1, player2);
    this.logger.log(`Test 3 - Tie: Both players +${tieResult.winnerChange}`);

    // Test case 4: Rating clamping
    const overMax = this.eloHelper.applyRatingChange(2990, 20);
    const underMin = this.eloHelper.applyRatingChange(10, -15);
    this.logger.log(`Test 4 - Clamping: 2990+20=${overMax}, 10-15=${underMin}`);

    // Test case 5: Win rate calculation
    const winRate = this.eloHelper.calculateWinRate(75, 25);
    const formattedWinRate = this.eloHelper.formatWinRate(75, 25);
    this.logger.log(`Test 5 - Win rate: ${winRate}% (${formattedWinRate})`);
  }
}
