export interface PlayerState {
  id: number;
  x: number;
  y: number;
  rotation: number;
  health: number;
  abilityReady: boolean;
  lastAbilityTime: number;
}

export interface GameState {
  matchId: number;
  player1: PlayerState;
  player2: PlayerState;
  tick: number;
  timestamp: number;
  winner?: number;
  status: 'active' | 'completed';
}
