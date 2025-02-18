using System;
using System.Collections;
using System.Collections.Generic;
using DDD.Game;
using DDD.Scripts.Core;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace DDD.Scripts.Game.rock_paper_scissors.Network
{
    public class DDDNetworkManagerRockPeper : DDDNetworkManager
    {
        [SerializeField] private string betID = "game/coin_flip";
        [SerializeField] private string gameEndpoint = "game/rock-paper-scissors-minus-one";
        [SerializeField] private string initEndpoint = "init";
        [SerializeField] private string removeHandEndpoint = "remove-hand";
        [SerializeField] private string mode => BetManager.instance.currencyType.ToString().ToLower();
        
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

    
        public void CallInitRequest(string mode, Action<RPSMinusOneInitResponse> callback)
        {
            StartCoroutine(InitRequestCoroutine(mode, callback));
        }
        
        private IEnumerator InitRequestCoroutine(string mode, Action<RPSMinusOneInitResponse> callback)
        {
            // Construct URL: e.g., {serverUrl}/game/rock-paper-scissors-minus-one/init
            string endpoint = $"{gameEndpoint}/{initEndpoint}";
            string url = BuildUrl(endpoint);

            // Create request body
            RPSMinusOneInitRequest requestData = new RPSMinusOneInitRequest
            {
                mode = mode
            };

            LogNetworkRequest("HTTP", url, requestData);

            using (UnityWebRequest request = CreatePostRequest(url, JsonUtility.ToJson(requestData)))
            {
                yield return request.SendWebRequest();
                LogNetworkResponse("HTTP", request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
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
            }
        }
        public void BetRequest(int speed, Action<string> callback)
        {
            Debug.Log($"[Network] Bet Request: {speed -1}");
            StartCoroutine(BetRequestEnumerator(speed-1, callback));
        }

        public IEnumerator BetRequestEnumerator(int speed, Action<string> callback)
        {
            string url = BuildUrl(betID);
            Debug.Log(url);
            var t = CreateBetRequestData(speed);

            using (UnityWebRequest request = CreatePostRequest(url, JsonConvert.SerializeObject(t)))
            {
                yield return request.SendWebRequest();
                LogNetworkResponse("Bet", request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(request.downloadHandler.text);
                }
                else if (request.responseCode == 401)
                {
                    Debug.LogError($"[Network] Request failed: {request.error}");
                }
                else
                {
                    Debug.LogWarning($"[Network] Request failed: {request.error}");
                }
            }
        }

        private RockPeperBetRequest CreateBetRequestData(int speed)
        {
            return new RockPeperBetRequest
            {
                betAmount = BetManager.instance.betAmount,
                playerInitialHands = new List<string>(),
                sessionId = "f3933089-74e3-40ca-bfb4-59b6f47c0f94"
            };
        }
        
        public void CallRemoveHandRequest(string playerHandToRemove, string sessionId, Action<RPSMinusOneRemoveHandResponse> callback)
        {
            StartCoroutine(RemoveHandCoroutine(playerHandToRemove, sessionId, callback));
        }
        
        private IEnumerator RemoveHandCoroutine(string playerHandToRemove, string sessionId, Action<RPSMinusOneRemoveHandResponse> callback)
        {
            string endpoint = $"{gameEndpoint}/{removeHandEndpoint}";
            string url = BuildUrl(endpoint);

            RPSMinusOneRemoveHandRequest requestData = new RPSMinusOneRemoveHandRequest
            {
                playerHandToRemove = playerHandToRemove,
                sessionId = sessionId
            };

            LogNetworkRequest("HTTP", url, requestData);

            using (UnityWebRequest request = CreatePostRequest(url, JsonUtility.ToJson(requestData)))
            {
                yield return request.SendWebRequest();
                LogNetworkResponse("HTTP", request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        RPSMinusOneRemoveHandResponse response = JsonUtility.FromJson<RPSMinusOneRemoveHandResponse>(request.downloadHandler.text);
                        callback?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        DDDDebug.LogException($"[Network] RemoveHand parse error: {e.Message}");
                        callback?.Invoke(null);
                    }
                }
                else if (request.responseCode == 401)
                {
                    DDDDebug.LogError($"[Network] RemoveHand unauthorized: {request.error}");
                }
                else
                {
                    DDDDebug.LogError($"[Network] RemoveHand error: {request.error}");
                    callback?.Invoke(null);
                }
            }
        }
    }
    
    
    [Serializable]
    public class RPSMinusOneInitRequest
    {
       
        public string mode;
    }

    [Serializable]
    public class RPSMinusOneInitResponse
    {
       
        public int[] betsValues;
        public string sessionId;
    }

    [Serializable]
    public class RPSMinusOneRemoveHandRequest
    {
        public string playerHandToRemove;
        public string sessionId;
    }

    [Serializable]
    public class RPSMinusOneRemoveHandResponse
    {
       
        public string pcHandToRemove;
        public float winAmount;
    }
}
    


