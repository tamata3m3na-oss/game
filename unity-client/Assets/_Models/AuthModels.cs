using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleStar.Models
{
    [Serializable]
    public class RegisterRequest
    {
        public string email;
        public string username;
        public string password;

        public RegisterRequest(string email, string username, string password)
        {
            this.email = email;
            this.username = username;
            this.password = password;
        }
    }

    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;

        public LoginRequest(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }

    [Serializable]
    public class AuthResponse
    {
        public string accessToken;
        public string refreshToken;
        public PlayerData player;
    }

    [Serializable]
    public class RefreshRequest
    {
        public string refreshToken;

        public RefreshRequest(string refreshToken)
        {
            this.refreshToken = refreshToken;
        }
    }

    [Serializable]
    public class PlayerData
    {
        public string id;
        public string name;
        public string email;
        public int rating;
        public int wins;
        public int losses;
        public int draws;
        public int totalMatches;
    }

    [Serializable]
    public class ErrorResponse
    {
        public string message;
        public string error;
    }
}