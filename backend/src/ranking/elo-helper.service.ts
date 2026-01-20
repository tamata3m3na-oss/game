import { Injectable, Logger } from '@nestjs/common';
import { ELORating, ELOChangeResult, PlayerRating } from './interfaces/elo-rating.interface';

@Injectable()
export class ELOHelper implements ELORating {
  private readonly logger = new Logger(ELOHelper.name);

  readonly K_FACTOR = 32;
  readonly MIN_RATING = 0;
  readonly MAX_RATING = 3000;

  calculateExpectedScore(playerRating: number, opponentRating: number): number {
    // Expected score formula: E = 1 / (1 + 10^((opponentRating - playerRating) / 400))
    return 1 / (1 + Math.pow(10, (opponentRating - playerRating) / 400));
  }

  calculateELOChange(winner: PlayerRating, loser: PlayerRating): ELOChangeResult {
    // Calculate expected scores
    const winnerExpected = this.calculateExpectedScore(winner.rating, loser.rating);
    const loserExpected = this.calculateExpectedScore(loser.rating, winner.rating);

    // Calculate rating changes
    // Winner gets: K_FACTOR * (1 - winnerExpected)
    // Loser gets: -K_FACTOR * (1 - loserExpected)
    const winnerChange = Math.round(this.K_FACTOR * (1 - winnerExpected));
    const loserChange = -Math.round(this.K_FACTOR * (1 - loserExpected));

    this.logger.debug(
      `ELO calculation: Winner ${winner.id} (${winner.rating}) vs Loser ${loser.id} (${loser.rating})`,
    );
    this.logger.debug(
      `Expected: Winner=${winnerExpected.toFixed(3)}, Loser=${loserExpected.toFixed(3)}`,
    );
    this.logger.debug(`Changes: Winner=+${winnerChange}, Loser=${loserChange}`);

    return { winnerChange, loserChange };
  }

  calculateTieELOChange(player1: PlayerRating, player2: PlayerRating): ELOChangeResult {
    // For ties, both players get +5 rating points (draw bonus)
    // This is a simplified approach for tie handling
    const drawBonus = 5;

    this.logger.debug(`Tie handling: Both players get +${drawBonus} rating points`);

    return {
      winnerChange: drawBonus,
      loserChange: drawBonus,
    };
  }

  calculateDisconnectionPenalty(
    disconnectedPlayer: PlayerRating,
    opponent: PlayerRating,
  ): ELOChangeResult {
    // Disconnected player gets normal loss penalty
    // Opponent gets win + small bonus for opponent disconnecting
    const winner = opponent;
    const loser = disconnectedPlayer;

    const { winnerChange, loserChange } = this.calculateELOChange(winner, loser);

    // Give extra 5 points to opponent for dealing with disconnect
    const adjustedWinnerChange = winnerChange + 5;

    this.logger.debug(
      `Disconnection penalty: Winner=${winner.id} gets +${adjustedWinnerChange}, Loser=${loser.id} gets ${loserChange}`,
    );

    return {
      winnerChange: adjustedWinnerChange,
      loserChange,
    };
  }

  clampRating(rating: number): number {
    return Math.max(this.MIN_RATING, Math.min(this.MAX_RATING, rating));
  }

  applyRatingChange(currentRating: number, change: number): number {
    const newRating = currentRating + change;
    return this.clampRating(newRating);
  }

  calculateWinRate(wins: number, losses: number): number {
    const totalGames = wins + losses;
    if (totalGames === 0) return 0;
    return (wins / totalGames) * 100;
  }

  formatWinRate(wins: number, losses: number): string {
    const winRate = this.calculateWinRate(wins, losses);
    return winRate.toFixed(1) + '%';
  }
}
