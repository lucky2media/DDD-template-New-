using System;
using System.Collections.Generic;
using DDD.Scripts.Core;
using DDD.Scripts.Lobby;
using UnityEngine;

public class LobbyScreen : DDDMonoBehaviour
{
    [SerializeField] List<GameData> banners = new List<GameData>();
    [SerializeField] GameBanner bannerPrefab;
    [SerializeField] Transform bannerParent;

    public void Init()
    {
        foreach (var banner in banners)
        {
            var b = Instantiate(bannerPrefab, bannerParent);
            b.Init(banner);
        }
    }
}