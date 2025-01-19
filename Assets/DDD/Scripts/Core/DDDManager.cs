using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace DDD.Scripts.Core
{
    public class DDDManager
    {
        public Action onInitAction;
        public static DDDManager Instance { get; set; }
        public DDDEventsManager EventsManager { get; set; }

        public DDDFactoryManager FactoryManager { get; set; }

        public DDDTimeManager TimeManager { get; set; }

        public DDDManager(Action<bool> onInitAction)
        {
            if (Instance != null)
            {
                return;
            }

            Instance = this;
            try
            {
                EventsManager = new DDDEventsManager();
                FactoryManager = new DDDFactoryManager();
                TimeManager = new DDDTimeManager();
                onInitAction?.Invoke(true);
            }
            catch (Exception e)
            {
                onInitAction?.Invoke(false);
            }
        }
    }
}