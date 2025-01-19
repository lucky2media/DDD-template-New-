using System;
using System.Collections.Generic;

namespace DDD.Scripts.Core
{
    public class DDDEventsManager
    {
        private Dictionary<DDDEventNames, List<Action<object>>> activeListeners = new();

        public void AddListener(DDDEventNames eventName, Action<object> onGameStart)
        {
            if (activeListeners.TryGetValue(eventName, out var listOfEvents))
            {
                listOfEvents.Add(onGameStart);
                return;
            }
            
            activeListeners.Add(eventName, new List<Action<object>>{onGameStart});
        }
        
        public void RemoveListener(DDDEventNames eventName, Action<object> onGameStart)
        {
            if (activeListeners.TryGetValue(eventName, out var listOfEvents))
            {
                listOfEvents.Remove(onGameStart);

                if (listOfEvents.Count <= 0)
                {
                    activeListeners.Remove(eventName);
                }
            }
        }
        
        public void InvokeEvent(DDDEventNames eventName, object obj)
        {
            if (activeListeners.TryGetValue(eventName, out var listOfEvents))
            {
                for (int i = 0; i < listOfEvents.Count; i++)
                {
                    var action = listOfEvents[i];
                    action.Invoke(obj);
                }
            }
        }
    }
    public enum DDDEventNames
    {
        OnPause,
        OfflineTimeRefreshed,
        OnUserBalanceChanged,
        OnValueChanged,
        OnCurrencyChanged
    }
    
    
}