using System;
using System.Collections.Generic;
using DDD.Scripts.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DDDGameLoader : MonoBehaviour
{
    public GameObject errorScreen;
    [SerializeField]private LobbyScreen lobbyScreen;
    [SerializeField] DDDNetworkManager networkManager;
    DDDManager manager;
    private void Awake()
    {
      manager =  new DDDManager((b =>
        {
            if (b)
            {

                
                if (lobbyScreen == null)
                {
                    return;
                   
                }

               
                ShowGames();
               
            }
          
        } ),networkManager);
    }

    private void ShowGames()
    {
        lobbyScreen.Init();
    }

    [ContextMenu("ShowWelcomeMessage")]
    public void ShowWelcomeMessage()
    {
        var t = DDDPopupData.WelcomeMessage;
        Debug.Log($"[DDDGameLoader] OnPopupOpen: {t.PopupType}");
        manager.PopupManager.AddPopupToQueue(t);
    }  
    
    [ContextMenu("ShowRulesPopup")]
    public void ShowRulesPopup()
    {
        var t = new DDDPopupData
        {
            Priority = 0,
            PopupType = PopupTypes.RulesPopup,
            OnPopupOpen = null,
            OnPopupClose = null,
            GenericData = null
        };
        Debug.Log($"[DDDGameLoader] OnPopupOpen: {t.PopupType}");
        manager.PopupManager.AddPopupToQueue(t);
    }
}


