using System;
using DDD.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DDD.Scripts.Game.Component
{
    public class ShowRulesButton : DDDMonoBehaviour
    {
     [SerializeField] Button rulesButton;

     private void Start()
     {
         rulesButton.onClick.RemoveAllListeners();
         rulesButton.onClick.AddListener(ShowRulesPopup);
     }

     private void ShowRulesPopup()
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
         Manager.PopupManager.AddPopupToQueue(t);
     }
    }
}