using System;
using System.Collections;
using System.Collections.Generic;
using DDD.Game;
using DDD.Scripts.Core;
using DDD.Scripts.Game.RockPaperScissors;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace DDD.Scripts.Game.rock_paper_scissors.Network
{
    public class DDDNetworkManagerRockPeper : DDDNetworkManager
    {
        // Game endpoints and URLs
        [SerializeField] private string gameEndpoint = "game/rock-paper-scissors-minus-one";
        [SerializeField] private string baseUrl = "http://localhost:3000/";
        [SerializeField] private int gameId = 1104;
        [SerializeField] private DDDRockPaperScissorsManager _rockPaperScissorsManager;

        public override void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.Log($"{Instance.gameObject.name} is already in use.", Instance.gameObject);
                Destroy(gameObject);
            }
        }

        // Override the BuildUrl method to use the correct base URL
        protected  string BuildUrl(string endpoint)
        {
            if (!isLocalServer)
            {
                baseUrl = serverUrl;
            }
            return $"{baseUrl}/{endpoint}";
        }

        // Initialize the game session
        public void CallInitRequest(string mode, Action<string> callback)
        {
            StartCoroutine(InitRequestCoroutine(mode, callback));
        }

        private IEnumerator InitRequestCoroutine(string mode, Action<string> callback)
        {
           // string endpoint = $"{gameEndpoint}/init?mode={mode}&gameId={gameId}";
            //string url = BuildUrl(endpoint);

           string endpoint = $"{gameEndpoint}/init?mode=&gameId=1104";
           
            string url = BuildUrl(endpoint);
            url = $"{serverUrl}/game/rock-paper-scissors-minus-one/init?mode={mode}";

            RPSMinusOneInitRequest requestData = new RPSMinusOneInitRequest
            {
                mode = mode
            };

            LogNetworkRequest("HTTP", url, requestData);
            using (UnityWebRequest request = CreateGetRequest(url))
            {
                yield return request.SendWebRequest();
                LogNetworkResponse("HTTP", request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[Network] Success");
                    Debug.Log($"[Network] Request data: {JsonUtility.ToJson(request.downloadHandler.text)}");
                    callback?.Invoke(request.downloadHandler.text);
                }
                else if (request.responseCode == 401)
                {
                    Debug.LogError($"[Network] Request failed: {request.error}");
                }
                else
                {
                    Debug.Log($"[Network] Request failed: {request.error}");
                    Debug.Log($"[Network] Request failed: {request.downloadHandler.text}");
                }
            }
            /*

            using (UnityWebRequest request = CreatePostRequest(url, JsonUtility.ToJson(requestData)))
            {
                Debug.Log(request.GetRequestHeader("Content-Type"));
                Debug.Log(request.GetRequestHeader("Authorization"));
                yield return request.SendWebRequest();
                LogNetworkResponse("HTTP", request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Debug.Log($"{request.downloadHandler.text}");
                        RPSMinusOneInitResponse response = JsonUtility.FromJson<RPSMinusOneInitResponse>(request.downloadHandler.text);
                        callback?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        DDDDebug.LogException($"[Network] InitRequest parse error: {e.Message}");
                        callback?.Invoke(null);
                    }
                }
                else if (request.responseCode == 401)
                {
                    DDDDebug.LogError($"[Network] InitRequest unauthorized: {request.error}");
                }
                else
                {
                    DDDDebug.LogError($"[Network] InitRequest error: {request.error}");
                }
            }*/
        }

        // Place a bet
        public void BetRequest(int amount, Action<string,Action> callback)
        {
            Debug.Log($"[Network] Bet Request: {amount}");
            StartCoroutine(BetRequestEnumerator(amount, callback));
        }

        public IEnumerator BetRequestEnumerator(int amount, Action<string,Action> callback)
        {
            string url = BuildUrl($"{gameEndpoint}/bet");
            
            var betRequestData = new RockPeperBetRequest
            {
                betAmount =  BetManager.instance.GetBetAmount(), 
                sessionId = DDDRockPaperScissorsManager._sessionId,
                gameId = gameId.ToString(),
            };
            
            Debug.Log($"[Network] Bet Request: {url}");
            Debug.Log($"[Network] Bet Request: {JsonConvert.SerializeObject(betRequestData)}");

            using (UnityWebRequest request = CreatePostRequest(url, JsonConvert.SerializeObject(betRequestData)))
            {
                yield return request.SendWebRequest();
                LogNetworkResponse("Bet", request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[Network] Bet Success: {request.downloadHandler.text}");
                    callback?.Invoke(request.downloadHandler.text,null);
                }
                else
                {
                    callback?.Invoke(request.downloadHandler.text,null);
                    Debug.LogError($"[Network] Bet Request failed: {request.error}");
                   
                }
            }
        }

        // Send first hand choice
        public void FirstPick(DDDRockPaperScissorsManager.Choice choice, string sessionId,
            Action<RPSFirestHanddResponse> callback)
        {
            StartCoroutine(FirstHandCoroutine(choice, sessionId, callback));
        }

        private IEnumerator FirstHandCoroutine(DDDRockPaperScissorsManager.Choice choice, string sessionId, 
            Action<RPSFirestHanddResponse> callback)
        {
            string url = BuildUrl($"{gameEndpoint}/firstHand");
            
            RPSFirestHandRequest requestData = new RPSFirestHandRequest
            {
                gameId = gameId,
                sessionId = sessionId,
                playerHand = choice.ToString().ToLower()
            };

            Debug.Log($"[Network] First Hand Request: {JsonConvert.SerializeObject(requestData)}");

            using (UnityWebRequest request = CreatePostRequest(url, JsonConvert.SerializeObject(requestData)))
            {
                yield return request.SendWebRequest();
                LogNetworkResponse("FirstHand", request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Debug.Log($"[Network] First Hand Response: {request.downloadHandler.text}");
                        RPSFirestHanddResponse response = 
                            JsonConvert.DeserializeObject<RPSFirestHanddResponse>(request.downloadHandler.text);
                        callback?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        callback?.Invoke(null);
                    }
                }
                else
                {
                    Debug.LogError($"[Network] First Hand Request failed: {request.error}");
                    callback?.Invoke(null);
                }
            }
        }

        // Send second hand choice
        public void SecondPick(DDDRockPaperScissorsManager.Choice choice, string sessionId,
            Action<RPSFirestHanddResponse> callback)
        {
            StartCoroutine(SecondHandCoroutine(choice, sessionId, callback));
        }

        private IEnumerator SecondHandCoroutine(DDDRockPaperScissorsManager.Choice choice, string sessionId, 
            Action<RPSFirestHanddResponse> callback)
        {
            string url = BuildUrl($"{gameEndpoint}/secondHand");
            
            RPSFirestHandRequest requestData = new RPSFirestHandRequest
            {
                gameId = gameId,
                sessionId = sessionId,
                playerHand = choice.ToString().ToLower()
            };

            Debug.Log($"[Network] Second Hand Request: {JsonConvert.SerializeObject(requestData)}");

            using (UnityWebRequest request = CreatePostRequest(url, JsonConvert.SerializeObject(requestData)))
            {
                yield return request.SendWebRequest();
                LogNetworkResponse("SecondHand", request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Debug.Log($"[Network] Second Hand Response: {request.downloadHandler.text}");
                        RPSFirestHanddResponse response = 
                            JsonConvert.DeserializeObject<RPSFirestHanddResponse>(request.downloadHandler.text);
                        callback?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        callback?.Invoke(null);
                    }
                }
                else
                {
                    Debug.LogError($"[Network] Second Hand Request failed: {request.error}");
                    callback?.Invoke(null);
                }
            }
        }

        // Remove a hand
        public void CallRemoveHandRequest(string[] playerHands, string playerHandToRemove, string sessionId,
            Action<RPSMinusOneRemoveHandResponse> callback)
        {
            StartCoroutine(RemoveHandCoroutine(playerHandToRemove, sessionId, callback));
        }

        private IEnumerator RemoveHandCoroutine(string playerHandToRemove, string sessionId,
            Action<RPSMinusOneRemoveHandResponse> callback)
        {
            string url = BuildUrl($"{gameEndpoint}/remove-hand");

            RPSMinusOneRemoveHandRequest requestData = new RPSMinusOneRemoveHandRequest
            {
                playerHandToRemove = playerHandToRemove.ToLower(),
                sessionId = sessionId,
                gameId = gameId
            };

            Debug.Log($"[Network] Remove Hand Request: {JsonConvert.SerializeObject(requestData)}");

            using (UnityWebRequest request = CreatePostRequest(url, JsonConvert.SerializeObject(requestData)))
            {
                yield return request.SendWebRequest();
                LogNetworkResponse("RemoveHand", request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Debug.Log($"[Network] Remove Hand Response: {request.downloadHandler.text}");
                        RPSMinusOneRemoveHandResponse response =
                            JsonConvert.DeserializeObject<RPSMinusOneRemoveHandResponse>(request.downloadHandler.text);
                        callback?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        callback?.Invoke(null);
                    }
                }
                else
                {
                    Debug.LogError($"[Network] Remove Hand Request failed: {request.error}");
                    callback?.Invoke(null);
                }
            }
        }
    }

    [Serializable]
    public class RPSMinusOneInitRequest
    {
        public string mode;
        public string gameId;
    }

    [Serializable]
    public class RockPeperBetRequest
    {
        public int betAmount;
        public string sessionId;
        public string gameId;
    }

    [Serializable]
    public class RPSBetRespone
    {
        public bool success;
        public object data;
    }

    [Serializable]
    public class RPSMinusOneInitResponse
    {
        public int[] betsValues;
        public string sessionId;
    }

    [Serializable]
    public class RPSMinusOneInitResponseInit
    {
        public RPSMinusOneInitResponse data;
        public bool success;
    }

    [Serializable]
    public class RPSMinusOneRemoveHandRequest
    {
        public string playerHandToRemove;
        public string sessionId;
        public int gameId;
    }

    [Serializable]
    public class RPSFirestHandRequest
    {
        public int gameId;
        public string sessionId;
        public string playerHand;
    }

    [Serializable]
    public class RPSFirestHanddResponse
    {
        public bool success;
        public RPSFirestHanddResponseData data;
    }

    [Serializable]
    public class RPSFirestHanddResponseData
    {
        public string pcHand;
    }

    [Serializable]
    public class RPSMinusOneRemoveHandResponseData
    {
        public string pcHandToRemove;
        public float winAmount;
    }

    [Serializable]
    public class RPSMinusOneRemoveHandResponse
    {
        public RPSMinusOneRemoveHandResponseData data;
        public bool success;
    }
}