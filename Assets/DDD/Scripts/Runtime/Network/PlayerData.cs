using System;
using DDD.Scripts.Core;
using UnityEngine;

[Serializable]
public class PlayerData 
{
    private DDDNetworkManager _dddNetworkManager;
    public ResponseWrapper userDTO;
    public PlayerData (DDDNetworkManager dddNetworkManager,Action onConnected)
    {
        _dddNetworkManager = dddNetworkManager;
        FetchUserData(() =>
        {
            onConnected?.Invoke();
        });
    }

    public void FetchUserData(Action onConnected)
    {
       DDDDebug.Log("[Example] Fetching user data...");
        
        _dddNetworkManager.GetUserInfo((userData) => {
            if (userData.Success)
            {
                userDTO = userData;
                onConnected?.Invoke();
            }
            else
            {
                DDDDebug.LogException($"[Example] Error: {userData}");
            }
            
        });
    }
}