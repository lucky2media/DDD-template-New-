using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DDD.Scripts.Core
{
    public class DDDPopupManager
    {
        public List<DDDPopupData> PopupsData = new();
        public Canvas popupsCanvas;

        private Dictionary<PopupTypes, DDDPopupComponentBase> cachedPopups = new();

        public DDDPopupManager()
        {
            CreateCanvas();
        }

        private void CreateCanvas()
        {
            DDDManager.Instance.FactoryManager.CreateAsync("PopupCanvas", Vector3.zero, (Canvas canvas) =>
            {
                popupsCanvas = canvas;
                Object.DontDestroyOnLoad(popupsCanvas);
            });
        }

        public void AddPopupToQueue(DDDPopupData popupData)
        {
            PopupsData.Add(popupData);
            TryShowNextPopup();
        }

        public void TryShowNextPopup()
        {
            if (PopupsData.Count <= 0)
            {
                return;
            }
            
            SortPopups();
            OpenPopup(PopupsData[0]);
        }

        public void SortPopups()
        {
            PopupsData = PopupsData.OrderBy(x => x.Priority).ToList();
        }
        
        public void OpenPopup(DDDPopupData dddPopupData)
        {
            Debug.Log("Open Popup");
            dddPopupData.OnPopupClose += OnClosePopup;
            PopupsData.Remove(dddPopupData);
            Debug.Log(dddPopupData.ToString());
            if (cachedPopups.ContainsKey(dddPopupData.PopupType))
            {
                var pop = cachedPopups[dddPopupData.PopupType];
                pop.gameObject.SetActive(true);
                pop.Init(dddPopupData);
            }
            else
            {
                Debug.Log(dddPopupData.PopupType.ToString());
                DDDManager.Instance.FactoryManager.CreateAsync(dddPopupData.PopupType.ToString(), 
                    Vector3.zero, (DDDPopupComponentBase popupComponent) =>
                    {
                        cachedPopups.Add(dddPopupData.PopupType, popupComponent);
                        popupComponent.transform.SetParent(popupsCanvas.transform, false);
                        popupComponent.Init(dddPopupData);
                    });
            }
        }

        private void OnClosePopup(DDDPopupComponentBase hogPopupComponentBase)
        {
            hogPopupComponentBase.gameObject.SetActive(false);
            TryShowNextPopup();
        }
    }
    
    public class DDDPopupData
    {
        public int Priority;
        public PopupTypes PopupType;

        public Action OnPopupOpen;
        public Action<DDDPopupComponentBase> OnPopupClose;

        public object GenericData;

        public static DDDPopupData WelcomeMessage = new()
        {
            Priority = 0,
            PopupType = PopupTypes.WelcomeMessage,
            GenericData = "Hello world :-)"
        };
        
        public static DDDPopupData UpgradePopupData = new()
        {
            Priority = 0,
            PopupType = PopupTypes.UpgradePopupMenu
        };
    }

    public enum PopupTypes
    {
        WelcomeMessage,
        Store,
        UpgradePopupMenu,
        WinLosePopup,
        RulesPopup
    }
}