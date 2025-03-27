using System;
using System.Collections;
using DDD.Scripts.Core;
using DDD.Scripts.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DDD.Game; // Add reference to BetManager namespace

namespace DDD.Scripts.Game.Component
{
    public class CounterController : DDDMonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private EventTrigger incrementButton;
        [SerializeField] private EventTrigger decrementButton;
    
        [Header("Display")]
        [SerializeField] private TextMeshProUGUI countText;
    
        [Header("Settings")]
        [SerializeField] private int startingIndex = 0;
    
        [Header("Events")]
        public Action<int> onValueChanged;
    
        private int currentIndex;
    
        [Header(("onPush"))]
        private bool isHolding = false;
        private float holdDelay = 0.1f; 

        private void Awake()
        {
            currentIndex = startingIndex;
    
            incrementButton.triggers.Clear();
            decrementButton.triggers.Clear();
    
            incrementButton.triggers.Add(SetEntry(() => 
            {
                isHolding = true;
                StartCoroutine(HoldAction(Increment));
            }, EventTriggerType.PointerDown));
    
            incrementButton.triggers.Add(SetEntry(() => 
            {
                isHolding = false;
            }, EventTriggerType.PointerUp));
    
            decrementButton.triggers.Add(SetEntry(() => 
            {
                isHolding = true;
                StartCoroutine(HoldAction(Decrement));
            }, EventTriggerType.PointerDown));
    
            decrementButton.triggers.Add(SetEntry(() => 
            {
                isHolding = false;
            }, EventTriggerType.PointerUp));
    
            UpdateDisplay();
        }

        public EventTrigger.Entry SetEntry(Action callback, EventTriggerType triggerType)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = triggerType;
            entry.callback.AddListener((data) =>
            {
                callback?.Invoke();
            });
            return entry;
        }
        
        public void Increment()
        {
            if (BetManager.instance != null && currentIndex + 1 < BetManager.instance.betAmounts.Length)
            {
                currentIndex++;
                UpdateDisplay();
                Manager.EventsManager.InvokeEvent(DDDEventNames.OnValueChanged, currentIndex);
            }
        }
    
        public void Decrement()
        {
            if (BetManager.instance != null && currentIndex - 1 >= 0)
            {
                currentIndex--;
                UpdateDisplay();
                Manager.EventsManager.InvokeEvent(DDDEventNames.OnValueChanged, currentIndex);
            }
        }
    
        private void UpdateDisplay()
        {
            
            if (countText != null && BetManager.instance != null)
            {
                var c = BetManager.instance.betAmounts[currentIndex];
                if (BetManager.instance.currencyType == Mode.coins)
                {
                    
                }
                else
                {
                   c = c.SweepsFormat();
                }
                countText.text = c.ToString();
            }
        }
    
        public int GetCurrentIndex()
        {
            return currentIndex;
        }
        
        public int GetCurrentBetAmount()
        {
            if (BetManager.instance != null)
            {
                return BetManager.instance.betAmounts[currentIndex];
            }
            return 0;
        }
    
        public void SetIndex(int newIndex)
        {
            if (BetManager.instance != null)
            {
                currentIndex = Mathf.Clamp(newIndex, 0, BetManager.instance.betAmounts.Length - 1);
                UpdateDisplay();
                onValueChanged?.Invoke(currentIndex);
            }
        }
        
        private IEnumerator HoldAction(Action action)
        {
            while (isHolding)
            {
                action.Invoke();
                DDDAudioManager.instance.PlayButtonPressSound();
                yield return new WaitForSeconds(holdDelay);
            }
        }
    }
}