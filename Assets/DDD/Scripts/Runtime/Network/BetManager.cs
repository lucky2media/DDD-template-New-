using System;
using DDD.Scripts.Core;

namespace DDD.Game
{
    class BetManager : DDDMonoBehaviour
    {
        public Mode currencyType = Mode.coins;
        public int betAmount =1;
        public static BetManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                return;
            }
            Destroy(this);
        }

        private void Start()
        {
            Manager.EventsManager.AddListener(DDDEventNames.OnCurrencyChanged,SetCurrencyType);
            Manager.EventsManager.AddListener(DDDEventNames.OnValueChanged,SetAmount);
        }

        public void SetAmount(object obj)
        {
            betAmount = (int)obj;
        }

        private void SetCurrencyType(object obj)
        {
            currencyType = (Mode)obj;
        }
    }
}