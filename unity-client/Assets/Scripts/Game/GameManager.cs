using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PvpGame.Network;
using PvpGame.Auth;
using PvpGame.Input;
using PvpGame.Utils;
using PvpGame.Config;

namespace PvpGame.Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        public ShipController playerShip;
        public ShipController opponentShip;
        public InputController inputController;
        public WeaponController weaponController;
        public AbilityController abilityController;

        [Header("Settings")]
        public int targetInputFps = 60;

        private int localPlayerId;
        private int currentMatchId;
        private bool isGameActive;
        private float inputTimer;
        private float inputInterval;
        private GameConfig config;
        private NetworkManager networkManager;

        private void Awake()
        {
            config = GameConfig.Instance;
            networkManager = NetworkManager.Instance;
            inputInterval = 1f / targetInputFps;
        }

        private void OnEnable()
        {
            if (networkManager != null)
            {
                networkManager.OnGameSnapshot += HandleGameSnapshot;
                networkManager.OnGameEnd += HandleGameEnd;
            }
        }

        private void OnDisable()
        {
            if (networkManager != null)
            {
                networkManager.OnGameSnapshot -= HandleGameSnapshot;
                networkManager.OnGameEnd -= HandleGameEnd;
            }
        }

        public void StartMatch(int matchId, int playerId, int opponentId)
        {
            currentMatchId = matchId;
            localPlayerId = playerId;
            isGameActive = true;

            Logger.LogGame($"Match started: {matchId}, LocalPlayer: {playerId}");

            playerShip.Initialize(playerId, true);
            opponentShip.Initialize(opponentId, false);

            inputController.ResetInput();
        }

        private void Update()
        {
            if (!isGameActive) return;

            inputTimer += Time.deltaTime;

            if (inputTimer >= inputInterval)
            {
                inputTimer = 0f;
                SendInputToServer();
            }
        }

        private async void SendInputToServer()
        {
            var inputData = inputController.GetInputData();

            if (inputData.fire && weaponController != null)
            {
                weaponController.TryFire();
            }

            if (inputData.ability && abilityController != null)
            {
                abilityController.TryUseAbility();
            }

            await networkManager.SendEventAsync("game:input", inputData);
        }

        private void HandleGameSnapshot(GameStateData snapshot)
        {
            if (snapshot == null || snapshot.matchId != currentMatchId)
            {
                return;
            }

            if (snapshot.player1 != null)
            {
                if (snapshot.player1.id == localPlayerId)
                {
                    playerShip.UpdateFromState(snapshot.player1);
                    if (abilityController != null)
                    {
                        abilityController.UpdateAbilityState(snapshot.player1.abilityReady);
                    }
                }
                else
                {
                    opponentShip.UpdateFromState(snapshot.player1);
                }
            }

            if (snapshot.player2 != null)
            {
                if (snapshot.player2.id == localPlayerId)
                {
                    playerShip.UpdateFromState(snapshot.player2);
                    if (abilityController != null)
                    {
                        abilityController.UpdateAbilityState(snapshot.player2.abilityReady);
                    }
                }
                else
                {
                    opponentShip.UpdateFromState(snapshot.player2);
                }
            }

            if (snapshot.status == "completed" && snapshot.winner.HasValue)
            {
                EndMatch(snapshot.winner.Value);
            }
        }

        private void HandleGameEnd(GameEndData gameEndData)
        {
            if (gameEndData.matchId != currentMatchId)
            {
                return;
            }

            Logger.LogGame($"Game ended. Winner: {gameEndData.winner}, ELO Change: {gameEndData.eloChange}");
            EndMatch(gameEndData.winner);
        }

        private void EndMatch(int winnerId)
        {
            isGameActive = false;
            Logger.LogGame($"Match ended. Winner: {winnerId}");

            bool didWin = winnerId == localPlayerId;
            StartCoroutine(TransitionToResultScene(didWin));
        }

        private IEnumerator TransitionToResultScene(bool won)
        {
            yield return new WaitForSeconds(2f);

            PlayerPrefs.SetInt("LastMatchWon", won ? 1 : 0);
            PlayerPrefs.Save();

            UnityEngine.SceneManagement.SceneManager.LoadScene("Result");
        }

        public void StopMatch()
        {
            isGameActive = false;
        }
    }
}
