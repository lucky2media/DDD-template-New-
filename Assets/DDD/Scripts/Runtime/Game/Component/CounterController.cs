using System;
using System.Collections;
using DDD.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        [SerializeField] private int startingValue = 0;
        [SerializeField] private int minValue = int.MinValue;
        [SerializeField] private int maxValue = int.MaxValue;
        [SerializeField] private int incrementAmount = 1;
    
        [Header("Events")]
        public Action<int> onValueChanged;
    
        private int currentValue;
    
        [Header(("onPush"))]
        private bool isHolding = false;
        private float holdDelay = 0.1f; 

        private void Awake()
        {
            currentValue = startingValue;
    
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

        public EventTrigger.Entry SetEntry(Action callback,EventTriggerType triggerType)
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
            if (currentValue + incrementAmount <= maxValue)
            {
                currentValue += incrementAmount;
                UpdateDisplay();
                Manager.EventsManager.InvokeEvent(DDDEventNames.OnValueChanged,GetCurrentValue());
            }
        }
    
        public void Decrement()
        {
            if (currentValue - incrementAmount >= minValue)
            {
                currentValue -= incrementAmount;
                UpdateDisplay();
                Manager.EventsManager.InvokeEvent(DDDEventNames.OnValueChanged,GetCurrentValue());
            }
        }
    
        private void UpdateDisplay()
        {
            if (countText != null)
            {
                countText.text = currentValue.ToString();
            }
        }
    
        public int GetCurrentValue()
        {
            return currentValue;
        }
    
        public void SetValue(int newValue)
        {
            currentValue = Mathf.Clamp(newValue, minValue, maxValue);
            UpdateDisplay();
            onValueChanged?.Invoke(currentValue);
        }
        
        private IEnumerator HoldAction(Action action)
        {
            while (isHolding)
            {
                action.Invoke();
                yield return new WaitForSeconds(holdDelay);
            }
        }
    }
}