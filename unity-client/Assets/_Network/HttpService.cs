using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BattleStar.Network
{
    public class HttpService : MonoBehaviour
    {
        private static HttpService _instance;
        public static HttpService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<HttpService>();
                    if (_instance == null)
                    {
                        _instance = new GameObject("HttpService").AddComponent<HttpService>();
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }
                return _instance;
            }
        }

        private const string CONTENT_TYPE = "application/json";

        [Header("Configuration")]
        [SerializeField] private string baseUrl = "http://localhost:3000";
        [SerializeField] private int timeoutSeconds = 10;
        [SerializeField] private int maxRetries = 2;

        [Header("Logging")]
        [SerializeField] private bool enableDebugLogging = true;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task<HttpResult> SendRequestAsync<T>(string endpoint, string method, string jsonData = null, string authToken = null) where T : class
        {
            int retryCount = 0;
            
            LogDebug($"Sending {method} request to {endpoint}");

            while (retryCount < maxRetries)
            {
                try
                {
                    using var request = new UnityWebRequest($"{baseUrl}/{endpoint}", method)
                    {
                        downloadHandler = new DownloadHandlerBuffer(),
                        timeout = timeoutSeconds * 1000
                    };

                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        var bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                        LogDebug($"Request body: {jsonData}");
                    }

                    request.SetRequestHeader("Content-Type", CONTENT_TYPE);

                    if (!string.IsNullOrEmpty(authToken))
                    {
                        request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                    }

                    var operation = request.SendWebRequest();
                    
                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        LogDebug($"Request successful: {request.responseCode}");
                        LogDebug($"Response: {request.downloadHandler.text}");

                        var response = JsonUtility.FromJson<T>(request.downloadHandler.text);
                        return HttpResult.Success(response, request.responseCode);
                    }
                    else
                    {
                        string errorMessage = FormatErrorMessage(request);
                        LogDebug($"Request failed: {errorMessage} (Code: {request.responseCode})");

                        if (ShouldRetry(request.responseCode) && retryCount < maxRetries - 1)
                        {
                            retryCount++;
                            int delayMs = 1000 * retryCount;
                            LogDebug($"Retrying in {delayMs}ms... (attempt {retryCount + 1}/{maxRetries})");
                            await Task.Delay(delayMs);
                            continue;
                        }

                        return HttpResult.Error(errorMessage, request.responseCode);
                    }
                }
                catch (Exception ex)
                {
                    LogDebug($"Exception during request: {ex.Message}");

                    if (retryCount < maxRetries - 1)
                    {
                        retryCount++;
                        int delayMs = 1000 * retryCount;
                        await Task.Delay(delayMs);
                        continue;
                    }

                    return HttpResult.Error($"Request failed: {ex.Message}", 500);
                }
            }

            string finalError = "Max retry attempts exceeded.";
            LogDebug(finalError);
            return HttpResult.Error(finalError, 500);
        }

        public async Task<HttpResult> PostAsync<T>(string endpoint, object payload, string authToken = null) where T : class
        {
            string json = payload != null ? JsonUtility.ToJson(payload) : null;
            return await SendRequestAsync<T>(endpoint, UnityWebRequest.kHttpVerbPOST, json, authToken);
        }

        public async Task<HttpResult> GetAsync<T>(string endpoint, string authToken = null) where T : class
        {
            return await SendRequestAsync<T>(endpoint, UnityWebRequest.kHttpVerbGET, null, authToken);
        }

        private bool ShouldRetry(long statusCode)
        {
            return statusCode == 408 || 
                   statusCode == 429 || 
                   statusCode == 502 || 
                   statusCode == 503 || 
                   statusCode == 504;
        }

        private string FormatErrorMessage(UnityWebRequest request)
        {
            return request.result switch
            {
                UnityWebRequest.Result.ConnectionError => "Network connection error. Please check your internet connection.",
                UnityWebRequest.Result.ProtocolError => ParseServerError(request.downloadHandler?.text),
                UnityWebRequest.Result.DataProcessingError => "Data processing error occurred while handling the response.",
                _ => "An unexpected error occurred during the request."
            };
        }

        private string ParseServerError(string responseText)
        {
            if (string.IsNullOrEmpty(responseText)) return "Server error occurred.";

            try
            {
                var errorResponse = JsonUtility.FromJson<SimpleErrorResponse>(responseText);
                return errorResponse.message ?? errorResponse.error ?? "Server error occurred.";
            }
            catch
            {
                return $"Server error ({responseText})";
            }
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[HttpService] {message}");
            }
        }

        [Serializable]
        private class SimpleErrorResponse
        {
            public string message;
            public string error;
        }

        public string GetBaseUrl()
        {
            return baseUrl;
        }
    }

    public class HttpResult
    {
        public bool success;
        public object data;
        public string error;
        public long statusCode;

        private HttpResult() { }

        public static HttpResult Success(object data = null, long statusCode = 200)
        {
            return new HttpResult
            {
                success = true,
                data = data,
                statusCode = statusCode
            };
        }

        public static HttpResult Error(string error, long statusCode = 400)
        {
            return new HttpResult
            {
                success = false,
                error = error,
                statusCode = statusCode
            };
        }
    }
}