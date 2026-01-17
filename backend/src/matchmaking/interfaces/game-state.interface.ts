export interface PlayerState {
  id: number;
  x: number;
  y: number;
  rotation: number;
  health: number;
  shieldHealth: number;
  shieldActive: boolean;
  shieldEndTick: number;
  fireReady: boolean;
  fireReadyTick: number;
  abilityReady: boolean;
  lastAbilityTime: number;
  damageDealt: number;
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
