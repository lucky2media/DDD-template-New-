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
    private void Awake()
    {
        new DDDManager((b =>
        {
            if (b)
            {
                if (lobbyScreen == null) return;
                ShowGames();
            }
            else
            {
               
            }
        } ));
    }

    private void ShowGames()
    {
        lobbyScreen.Init();
    }
}


