export interface ELORating {
  K_FACTOR: number;
  MIN_RATING: number;
  MAX_RATING: number;
  calculateELOChange(winner: any, loser: any): { winnerChange: number; loserChange: number };
  clampRating(rating: number): number;
  calculateExpectedScore(playerRating: number, opponentRating: number): number;
}

export interface ELOChangeResult {
  winnerChange: number;
  loserChange: number;
}

export interface PlayerRating {
  id: number;
  rating: number;
  wins: number;
  losses: number;
}

export interface MatchResult {
  matchId: number;
  player1: PlayerRating;
  player2: PlayerRating;
  winnerId: number | null;
  isTie: boolean;
  isDisconnection: boolean;
}