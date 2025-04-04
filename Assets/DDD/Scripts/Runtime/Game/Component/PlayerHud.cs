using System.Collections;
using DDD.Game;
using DDD.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DDD.Scripts.Game.Component
{
    public class PlayerHud : DDDMonoBehaviour
    {
        public PlayerData playerData;
      private DDDNetworkManager dddNetworkManager => DDDNetworkManager.Instance;

        [SerializeField] private TextMeshProUGUI userBalanceText;
        private Balance balance => Manager.playerData.userDTO.Data.Balance;
        public Mode currencyType = Mode.coins;
        public Image currencyImage;
        public Sprite coinsSprite;
        public Sprite sweepsSprite;
        [Tooltip("switch the currency type")] public Button iconButton;
        public static PlayerHud instance;
    
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1);
            if (Manager.NetworkManager == null)
            {
                Debug.LogError($"{nameof(Manager)} is null");
            }
            Manager.playerData = new PlayerData(Manager.NetworkManager,()=>
            {
                UpdateUI(null);
            });
            iconButton.onClick.RemoveAllListeners();
            iconButton.onClick.AddListener((() => { ChangeCurrency(); }));
            Manager.EventsManager.AddListener(DDDEventNames.OnUserBalanceChanged,UpdateUI);
            Manager.EventsManager.AddListener(DDDEventNames.OnValueChanged,UpdateUI);
        }



        private void UpdateUI(object obj)
        {
            Manager. playerData.FetchUserData(() =>
            {
                var t = "";
                switch (currencyType)
                {
                    case Mode.coins:
                        t = balance.Coins.ToString();
                        currencyImage.sprite = coinsSprite;
                        break;
                    case Mode.sweeps:
                        t = balance.Sweeps.SweepsFormat().ToString();
                        currencyImage.sprite = sweepsSprite;
                        break;
                }
            
                userBalanceText.text = t;
                
            });
            
        }

        private void ChangeCurrency()
        {
            if (currencyType == Mode.coins)
            {
                currencyType = Mode.sweeps;
            }
            else
            {
                currencyType = Mode.coins;
            }
            Manager.EventsManager.InvokeEvent(DDDEventNames.OnCurrencyChanged,currencyType);
            UpdateUI(null);
        }
    }
}