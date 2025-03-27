
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

using System.Threading.Tasks;
using Best.WebSockets;
using DDD.Game;
using DDD.Scripts.Core;
using Newtonsoft.Json;

public class DDDNetworkManager : DDDMonoBehaviour
{
    #region Singleton

    public static DDDNetworkManager Instance { get; protected set; }

    #endregion

    #region Serialized Fields

    [Header("Server Configuration")] [SerializeField]
    public string serverUrl = "https://backend-staging.dingdingding.world";

    [SerializeField] private string wsGameUri = "piggytap";
    [SerializeField] private string wsLiveUri = "wss://ws.dingdingding.world";
    [SerializeField] protected string wsLocalUri = "ws://localhost:8888";
    [SerializeField] private string refreshTokenLocalUri = "auth/refresh_token";

    [Header("Authentication")] [SerializeField]
    private string accessToken;

    [Header("Debug Settings")] [SerializeField]
    private bool useMockData = false;

    [SerializeField] private bool autoConnectWebSocket = false;
    [SerializeField] protected bool isLocalServer = false;

    #endregion

    #region Private Fields

    private WebSocket websocket;
    private bool isWebSocketConnected = false;
    [SerializeField] private string gameName = "game/coin_flip/play";

    #endregion

    #region Properties

    public bool IsLocalServer => isLocalServer;
    public bool IsWebSocketConnected => isWebSocketConnected;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeSingleton();

        CookieReader cookieReader = new CookieReader(() =>
        {
            var token = CookieReader.FetchCookieValue("AccessToken");
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError($"Failed to get cookie value for {gameName}");
            }
            else
            {
                Debug.Log($"Got cookie value for {token}");
                accessToken = token;
            }
        });
       


    }

    private async void Start()
    {
        if (autoConnectWebSocket)
        {
            await ConnectWebSocket();
        }
    }

    private void Update()
    {
      //  DispatchWebSocketMessages();
    }

    private async void OnDestroy()
    {
        await DisconnectWebSocket();
    }

    private async void OnApplicationQuit()
    {
        await DisconnectWebSocket();
    }

    #endregion

    #region Initialization

    public virtual void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DDDDebug.Log($"{Instance.gameObject.name} is already in use.", Instance.gameObject);
            Destroy(Instance.gameObject);
            accessToken =Instance.accessToken ;
            Instance = this;
            Manager.NetworkManager = this;
        }
    }

    #endregion

    #region HTTP Request Helpers

    protected string BuildUrl(string endpoint)
    {
      
        return isLocalServer ? $"{wsLocalUri}/{endpoint}" : $"{serverUrl}/{endpoint}";
    }


    protected UnityWebRequest CreateGetRequest(string url)
    {
        var request = UnityWebRequest.Get(url);
        SetCommonHeaders(request);
        return request;
    }

    protected UnityWebRequest CreatePostRequest(string url, string body)
    {
        var request = new UnityWebRequest(url, "POST");
        var bodyRaw = string.IsNullOrEmpty(body) ? new byte[0] : Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        SetCommonHeaders(request);
        return request;
    }

    private void SetCommonHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("accept", "application/json");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", accessToken);
      
    }

    #endregion

    #region User Authentication

    public void GetUserInfo(Action<ResponseWrapper> callback)
    {
        if (useMockData)
        {
            return;
        }

        StartCoroutine(GetUserInfoCoroutine(callback));
    }

    private IEnumerator GetUserInfoCoroutine(Action<ResponseWrapper> callback)
    {
        string url = BuildUrl("auth/current");
        LogNetworkRequest("HTTP", url);

        using (UnityWebRequest request = CreateGetRequest(url))
        {
            yield return request.SendWebRequest();
            LogNetworkResponse("HTTP", request);

            if (request.result == UnityWebRequest.Result.Success)
            {
                HandleSuccessfulUserInfoResponse(request, callback);
            }
            else if (request.responseCode == 401)
            {
                HandleUnauthorizedResponse(() => GetUserInfo(callback));
            }
            else
            {
                HandleFailedResponse(request, callback);
            }
        }
    }

    private IEnumerator RefreshTokenCoroutine(Action onComplete)
    {
        string url = BuildUrl(refreshTokenLocalUri);
        using (var request = UnityWebRequest.PostWwwForm(url, ""))
        {
            request.SetRequestHeader("authorization", accessToken);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                HandleSuccessfulTokenRefresh(request, onComplete);
            }
        }
    }

    #endregion

    #region Betting System

    public void SetBet(int betAmount, Action<BetResponse> callback = null)
    {
        StartCoroutine(SendBetRequest(betAmount, callback));
    }

    private IEnumerator SendBetRequest(int betAmount, Action<BetResponse> callback)
    {
        //todo replace string with const  
        if (useMockData)
        {
            HandleMockBetResponse(callback);
            yield break;
        }

        var requestData = CreateBetRequestData(betAmount);
        string url = BuildUrl(gameName);
        LogNetworkRequest("Bet", url, requestData);

        using (UnityWebRequest request = CreatePostRequest(url, JsonUtility.ToJson(requestData)))
        {
            yield return request.SendWebRequest();
            LogNetworkResponse("Bet", request);

            if (request.result == UnityWebRequest.Result.Success)
            {
                HandleSuccessfulBetResponse(request, callback);
            }
            else if (request.responseCode == 401)
            {
                HandleUnauthorizedResponse(() => StartCoroutine(SendBetRequest(betAmount, callback)));
            }
            else
            {
                HandleFailedBetResponse(request, callback);
            }
        }
    }

    #endregion

    #region WebSocket Management

    public async Task ConnectWebSocket()
    {
        if (websocket != null)
        {
            await DisconnectWebSocket();
        }

        //websocket = new WebSocket(wsLiveUri);
        //SetupWebSocketCallbacks();

        DDDDebug.Log($"[WebSocket] Connecting to {wsLiveUri}...");
        //await websocket.Connect();
    }

    public async Task DisconnectWebSocket()
    {
        if (websocket != null)
        {
           // await websocket.Close();
            websocket = null;
            isWebSocketConnected = false;
        }
    }
/*
    private void SetupWebSocketCallbacks()
    {
        websocket.OnOpen += HandleWebSocketOpen;
        websocket.OnError += HandleWebSocketError;
        websocket.OnClose += HandleWebSocketClose;
        websocket.OnMessage += HandleWebSocketMessage;
    }


    private void DispatchWebSocketMessages()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    public async Task SendWebSocketMessage(string type, object data)
    {
        if (!isWebSocketConnected)
        {
            Debug.LogWarning("[WebSocket] Not connected!");
            return;
        }

        var message = new { type, data };
        string jsonMessage = JsonUtility.ToJson(message);
        LogWebSocketMessage("Sending", jsonMessage);
        await websocket.SendText(jsonMessage);
    }

    #endregion

    #region Response Handlers
*/
    private void HandleSuccessfulUserInfoResponse(UnityWebRequest request, Action<ResponseWrapper> callback)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<ResponseWrapper>(request.downloadHandler.text);
            Manager.playerData.userDTO = response;
            callback?.Invoke(response);
        }
        catch (Exception e){
            
        Debug.LogError($"[Network] Failed to parse response: {e.Message}");
        }
    }

    private void HandleSuccessfulTokenRefresh(UnityWebRequest request, Action onComplete)
    {
        try
        {
            var response = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);
            accessToken = response.data.ACCESS_TOKEN;
            onComplete?.Invoke();
        }
        catch (Exception e)
        {
            DDDDebug.LogException($"[Network] Token refresh failed: {e.Message}");
        }
    }

    private void HandleSuccessfulBetResponse(UnityWebRequest request, Action<BetResponse> callback)
    {
        try
        {
            var responseData = JsonUtility.FromJson<BetResponseDTO>(request.downloadHandler.text);
            var response = new BetResponse
            {
                success = responseData.success,
                winAmount = responseData.data.amount,
            };
            callback?.Invoke(response);
        }
        catch (Exception e)
        {
            DDDDebug.LogException($"[Network] Failed to parse bet response: {e.Message}");
            callback?.Invoke(CreateErrorBetResponse("Failed to parse server response"));
        }
    }

    private void HandleFailedBetResponse(UnityWebRequest request, Action<BetResponse> callback)
    {
        DDDDebug.LogException($"[Network] Request failed: {request.error}");
        callback?.Invoke(CreateErrorBetResponse(request.error));
    }

    private void HandleUnauthorizedResponse(Action retryAction)
    {
        DDDDebug.LogWarning("[Network] Authentication failed - attempting token refresh");
        StartCoroutine(RefreshTokenCoroutine(retryAction));
    }

    #endregion

    #region WebSocket Event Handlers

    private void HandleWebSocketOpen()
    {
        DDDDebug.Log("[WebSocket] Connected!");
        isWebSocketConnected = true;
        SendWebSocketAuth();
    }

    private void HandleWebSocketError(string error)
    {
        DDDDebug.LogException($"[WebSocket] Error: {error}");
    }

    /*
    private void HandleWebSocketClose(WebSocketCloseCode closecode)
    {
        Debug.Log("[WebSocket] Connection closed!");
        isWebSocketConnected = false;
    }*/

    private void HandleWebSocketMessage(byte[] bytes)
    {
        var message = Encoding.UTF8.GetString(bytes);
        LogWebSocketMessage("Received", message);
        ProcessWebSocketMessage(message);
    }

    #endregion

    #region Utility Methods

    protected void LogNetworkRequest(string type, string url, object data = null)
    {
        DDDDebug.Log($"[Network] Making {type} request to: {url}");
        if (data != null)
        {
            DDDDebug.Log($"[Network] Request data: {JsonUtility.ToJson(data)}");
        }
    }

    protected void LogNetworkResponse(string type, UnityWebRequest request)
    {
        DDDDebug.Log($"[Network] {type} Response Code: {request.responseCode}");
        DDDDebug.Log($"[Network] {type} Response: {request.downloadHandler.text}");
    }

    private void LogWebSocketMessage(string direction, string message)
    {
        DDDDebug.Log($"[WebSocket] {direction}: {message}");
    }

    private BetResponse CreateErrorBetResponse(string error)
    {
        return new BetResponse
        {
            success = false,
            winAmount = 0
        };
    }

    private void HandleMockBetResponse(Action<BetResponse> callback)
    {
        var mockResponse = new BetResponse();
        callback?.Invoke(mockResponse);
    }

    #endregion

    #region Mock Data

    private void HandleMockData<T>(Action<T> callback, T mockResponse) where T : class
    {
        Debug.Log("[Network] Using mock data");
        callback?.Invoke(mockResponse);
    }

    #endregion

    #region Response Handlers

    private void HandleFailedResponse(UnityWebRequest request, Action<ResponseWrapper> callback)
    {
        DDDDebug.LogWarning($"[Network] Request failed: {request.error}");
    }

    private BetRequest CreateBetRequestData(int betAmount)
    {
        return new BetRequest
        {
            mode = BetManager.instance.currencyType.ToString(),
            amount = betAmount
        };
    }

    #endregion


    private async void SendWebSocketAuth()
    {
        if (!isWebSocketConnected) return;

        var auth = new WebSocketAuthMessage
        {
            type = "auth",
            token = accessToken
        };

        string jsonMessage = JsonUtility.ToJson(auth);
        LogWebSocketMessage("Sending auth", jsonMessage);
        //await websocket.SendText(jsonMessage);
    }

    private void ProcessWebSocketMessage(string message)
    {
        try
        {
            // Parse the message
            var wsMessage = JsonUtility.FromJson<WebSocketMessage>(message);

            switch (wsMessage.type)
            {
                case "auth_success":
                    DDDDebug.Log("[WebSocket] Authentication successful");
                    break;
                case "auth_error":
                    DDDDebug.LogException("[WebSocket] Authentication failed");
                    break;
                case "game_update":
                    HandleGameUpdate(wsMessage.data);
                    break;
                default:
                    DDDDebug.Log($"[WebSocket] Unhandled message type: {wsMessage.type}");
                    break;
            }
        }
        catch (Exception e)
        {
            DDDDebug.LogException($"[WebSocket] Failed to process message: {e.Message}");
        }
    }

    private void HandleGameUpdate(object data)
    {
        try
        {
            DDDDebug.Log($"[WebSocket] Game update received: {JsonUtility.ToJson(data)}");
        }
        catch (Exception e)
        {
            DDDDebug.LogException($"[WebSocket] Failed to process game update: {e.Message}");
        }
    }
}


[Serializable]
public class WebSocketMessage
{
    public string type;
    public object data;
}