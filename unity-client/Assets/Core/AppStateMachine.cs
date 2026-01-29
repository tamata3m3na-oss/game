using System;
using UnityEngine;

namespace ShipBattle.Core
{
    /// <summary>
    /// Application state machine for managing game flow.
    /// Do not modify - this is Phase 1 completed code.
    /// </summary>
    public enum AppState
    {
        Splash,
        Login,
        Lobby,
        Matchmaking,
        InGame,
        Result
    }

    public class AppStateMachine
    {
        private AppState currentState;
        public AppState CurrentState => currentState;

        public event Action<AppState, AppState> OnStateChanged;

        public AppStateMachine()
        {
            currentState = AppState.Splash;
        }

        public void ChangeState(AppState newState)
        {
            if (currentState == newState)
            {
                Debug.LogWarning($"[AppStateMachine] Already in state: {newState}");
                return;
            }

            AppState oldState = currentState;
            currentState = newState;

            Debug.Log($"[AppStateMachine] State changed: {oldState} -> {newState}");
            OnStateChanged?.Invoke(oldState, newState);
        }

        public bool IsInState(AppState state)
        {
            return currentState == state;
        }
    }
}
