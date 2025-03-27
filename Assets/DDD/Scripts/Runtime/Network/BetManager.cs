using System;
using System.Collections.Generic;
using DDD.Scripts.Core;

namespace DDD.Game
{
    class BetManager : DDDMonoBehaviour
    {
        public Mode currencyType = Mode.coins;
        public int[] betAmounts = { 1, 5, 10, 50, 100 }; // Default values
        public int betAmountIndex = 0;
        public int betAmount => betAmounts[betAmountIndex];
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
            Manager.EventsManager.AddListener(DDDEventNames.OnCurrencyChanged, SetCurrencyType);
            Manager.EventsManager.AddListener(DDDEventNames.OnValueChanged, SetAmountIndex);
        }

        public void PopulateBetAmounts(List<int> newBetAmounts)
        {
            if (newBetAmounts != null && newBetAmounts.Count > 0)
            {
                betAmounts = newBetAmounts.ToArray();
                
                // Reset the index when we change the array
                betAmountIndex = 0;
            }
            Manager.EventsManager.AddListener(DDDEventNames.OnCurrencyChanged, SetCurrencyType);
            Manager.EventsManager.AddListener(DDDEventNames.OnValueChanged, SetAmountIndex);
            Manager.EventsManager.InvokeEvent(DDDEventNames.OnValueChanged,null);
        }

        public void PopulateBetAmounts()
        {
            List<int> defaultAmounts;
            
            if (currencyType == Mode.coins)
            {
                defaultAmounts = new List<int> { 1, 5, 10, 25, 50, 100 };
            }
            else
            {
                defaultAmounts = new List<int> { 1, 2, 5, 10, 20 };
            }
            
            PopulateBetAmounts(defaultAmounts);
        }

        public void SetAmountIndex(object obj)
        {
            betAmountIndex = (int)obj;
            
            if (betAmountIndex < 0 || betAmountIndex >= betAmounts.Length)
            {
                betAmountIndex = 0;
            }
        }

        private void SetCurrencyType(object obj)
        {
            currencyType = (Mode)obj;
          
        }

        public int GetBetAmount()
        {
            return betAmounts[betAmountIndex];
        }
    }
}