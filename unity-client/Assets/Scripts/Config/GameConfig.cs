using UnityEngine;

namespace PvpGame.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "PvP/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Server Configuration")]
        public string restApiUrl = "http://localhost:3000";
        public string websocketUrl = "ws://localhost:3000/pvp";

        [Header("REST API Endpoints")]
        public string authEndpoint = "/auth";
        public string playerEndpoint = "/player";
        public string rankingEndpoint = "/ranking";

        [Header("Game Settings")]
        public int targetInputFps = 60;
        public int serverTickRate = 20;
        public float connectionTimeout = 30f;

        [Header("Gameplay Constants")]
        public float mapWidth = 100f;
        public float mapHeight = 100f;
        public float playerSpeed = 5f;
        public int maxHealth = 100;
        public int fireRange = 10;
        public int fireDamage = 10;
        public int abilityRange = 15;
        public int abilityDamage = 25;
        public float abilityCooldown = 5f;

        [Header("Visual Settings")]
        public float shipScale = 1f;
        public float bulletScale = 0.5f;
        public float interpolationSpeed = 10f;

        public static GameConfig Instance { get; private set; }

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
