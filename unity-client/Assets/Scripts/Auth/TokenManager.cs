using UnityEngine;
using PvpGame.Utils;

namespace PvpGame.Auth
{
    public class TokenManager
    {
        private const string ACCESS_TOKEN_KEY = "pvp_access_token";
        private const string REFRESH_TOKEN_KEY = "pvp_refresh_token";

        public string AccessToken
        {
            get => PlayerPrefs.GetString(ACCESS_TOKEN_KEY, null);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    PlayerPrefs.DeleteKey(ACCESS_TOKEN_KEY);
                }
                else
                {
                    PlayerPrefs.SetString(ACCESS_TOKEN_KEY, value);
                }
                PlayerPrefs.Save();
            }
        }

        public string RefreshToken
        {
            get => PlayerPrefs.GetString(REFRESH_TOKEN_KEY, null);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    PlayerPrefs.DeleteKey(REFRESH_TOKEN_KEY);
                }
                else
                {
                    PlayerPrefs.SetString(REFRESH_TOKEN_KEY, value);
                }
                PlayerPrefs.Save();
            }
        }

        public bool HasTokens()
        {
            return !string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(RefreshToken);
        }

        public void ClearTokens()
        {
            AccessToken = null;
            RefreshToken = null;
            AppLogger.LogAuth("Tokens cleared");
        }
    }
}
