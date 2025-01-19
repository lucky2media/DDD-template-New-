using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class LoginManager : MonoBehaviour
{
    private const string LOGIN_URL = "https://backend-staging.dingdingding.com/auth/login";

    [SerializeField] 
    private string email;
    [SerializeField] 
    private string password;

    [ContextMenu("Login")]
    public void LoginR()
    {
        StartCoroutine(Login());
    }
    public IEnumerator Login()
    {
        var loginData = new LoginRequest
        {
            email = email,
            password = password
        };
        
        string jsonData = JsonUtility.ToJson(loginData);

        using (UnityWebRequest request = new UnityWebRequest(LOGIN_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(request.downloadHandler.text);
                string token = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text).accessToken;
                Debug.Log("=" + token);
               
            }
            else
            {
                Debug.LogError(" " + request.error);
            }
        }
    }
}