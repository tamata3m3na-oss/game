#!/usr/bin/env node
/**
 * Game Loop Integration Test
 * 
 * This script tests the 20Hz game loop by simulating two players in a match.
 * 
 * Prerequisites:
 * 1. Backend server running (npm run start:dev)
 * 2. Two registered users in database
 * 3. npm install socket.io-client
 * 
 * Usage:
 *   node test-game-loop.js <player1_token> <player2_token>
 */

const io = require('socket.io-client');

if (process.argv.length < 4) {
  console.error('Usage: node test-game-loop.js <player1_token> <player2_token>');
  console.error('');
  console.error('Get tokens by calling POST /auth/login for each user');
  process.exit(1);
}

const token1 = process.argv[2];
const token2 = process.argv[3];

const serverUrl = process.env.SERVER_URL || 'http://localhost:3000';

console.log('=== Game Loop Integration Test ===\n');
console.log(`Server: ${serverUrl}/pvp`);
console.log('Connecting players...\n');

let matchId = null;
let tickCount = 0;
let startTime = null;
const tickIntervals = [];

const socket1 = io(`${serverUrl}/pvp`, {
  auth: { token: token1 }
});

const socket2 = io(`${serverUrl}/pvp`, {
  auth: { token: token2 }
});

socket1.on('connect', () => {
  console.log('✓ Player 1 connected');
  socket1.emit('queue:join');
});

socket2.on('connect', () => {
  console.log('✓ Player 2 connected');
  socket2.emit('queue:join');
});

socket1.on('queue:status', (data) => {
  console.log(`Player 1 queue status: Position ${data.position}, Wait ${data.estimatedWait}s`);
});

socket2.on('queue:status', (data) => {
  console.log(`Player 2 queue status: Position ${data.position}, Wait ${data.estimatedWait}s`);
});

socket1.on('match:found', (data) => {
  matchId = data.matchId;
  console.log(`\n✓ Match found: ${matchId}`);
  console.log(`Player 1 vs ${data.opponent.username} (Rating: ${data.opponent.rating})`);
  
  setTimeout(() => {
    console.log('Player 1 marking ready...');
    socket1.emit('match:ready', { matchId });
  }, 500);
});

socket2.on('match:found', (data) => {
  console.log(`Player 2 vs ${data.opponent.username} (Rating: ${data.opponent.rating})`);
  
  setTimeout(() => {
    console.log('Player 2 marking ready...');
    socket2.emit('match:ready', { matchId: data.matchId });
  }, 500);
});

socket1.on('match:start', (data) => {
  console.log(`\n✓ Match started! Player 1 color: ${data.color}`);
  console.log('Game loop should now be running at 20Hz...\n');
  startTime = Date.now();
  
  // Player 1: Move right and fire
  setInterval(() => {
    socket1.emit('game:input', {
      moveX: 1,
      moveY: 0,
      fire: true,
      ability: false,
      timestamp: Date.now()
    });
  }, 50);
});

socket2.on('match:start', (data) => {
  console.log(`Player 2 color: ${data.color}`);
  
  // Player 2: Move left
  setInterval(() => {
    socket2.emit('game:input', {
      moveX: -1,
      moveY: 0,
      fire: false,
      ability: false,
      timestamp: Date.now()
    });
  }, 50);
});

let lastSnapshotTime = null;

socket1.on('game:snapshot', (state) => {
  tickCount++;
  
  const now = Date.now();
  if (lastSnapshotTime) {
    const interval = now - lastSnapshotTime;
    tickIntervals.push(interval);
  }
  lastSnapshotTime = now;
  
  if (tickCount % 20 === 0) {
    const avgInterval = tickIntervals.length > 0 
      ? tickIntervals.reduce((a, b) => a + b, 0) / tickIntervals.length 
      : 0;
    const minInterval = tickIntervals.length > 0 ? Math.min(...tickIntervals) : 0;
    const maxInterval = tickIntervals.length > 0 ? Math.max(...tickIntervals) : 0;
    
    console.log(`\n[Tick ${state.tick}]`);
    console.log(`  Player 1: (${state.player1.x.toFixed(2)}, ${state.player1.y.toFixed(2)}) HP:${state.player1.health} Ability:${state.player1.abilityReady ? 'Ready' : 'Cooldown'}`);
    console.log(`  Player 2: (${state.player2.x.toFixed(2)}, ${state.player2.y.toFixed(2)}) HP:${state.player2.health} Ability:${state.player2.abilityReady ? 'Ready' : 'Cooldown'}`);
    console.log(`  Tick Rate: Avg=${avgInterval.toFixed(1)}ms Min=${minInterval}ms Max=${maxInterval}ms`);
    
    if (maxInterval > 55) {
      console.log(`  ⚠️  WARNING: Tick spike detected (${maxInterval}ms > 55ms)`);
    }
    
    tickIntervals.length = 0;
  }
});

socket1.on('game:end', (data) => {
  const duration = (Date.now() - startTime) / 1000;
  
  console.log('\n=== GAME ENDED ===');
  console.log(`Match ID: ${data.matchId}`);
  console.log(`Winner: Player ${data.winner}`);
  console.log(`Duration: ${duration.toFixed(1)}s`);
  console.log(`Total Ticks: ${tickCount}`);
  console.log(`Average Tick Rate: ${(tickCount / duration).toFixed(1)} Hz`);
  
  if (data.finalState) {
    console.log('\nFinal State:');
    console.log(`  Player 1 HP: ${data.finalState.player1.health}`);
    console.log(`  Player 2 HP: ${data.finalState.player2.health}`);
  }
  
  const expectedHz = 20;
  const actualHz = tickCount / duration;
  const deviation = Math.abs(actualHz - expectedHz);
  
  if (deviation < 1) {
    console.log(`\n✓ PASS: Tick rate within tolerance (${actualHz.toFixed(1)}Hz vs ${expectedHz}Hz)`);
  } else {
    console.log(`\n✗ FAIL: Tick rate out of tolerance (${actualHz.toFixed(1)}Hz vs ${expectedHz}Hz)`);
  }
  
  console.log('\nTest completed. Disconnecting...');
  
  setTimeout(() => {
    socket1.disconnect();
    socket2.disconnect();
    process.exit(0);
  }, 1000);
});

socket2.on('game:end', (data) => {
  console.log(`Player 2 received game end notification`);
});

socket1.on('disconnect', () => {
  console.log('Player 1 disconnected');
});

socket2.on('disconnect', () => {
  console.log('Player 2 disconnected');
});

socket1.on('connect_error', (error) => {
  console.error('Player 1 connection error:', error.message);
  process.exit(1);
});

socket2.on('connect_error', (error) => {
  console.error('Player 2 connection error:', error.message);
  process.exit(1);
});

// Timeout after 60 seconds
setTimeout(() => {
  console.error('\nTest timeout after 60 seconds');
  socket1.disconnect();
  socket2.disconnect();
  process.exit(1);
}, 60000);
