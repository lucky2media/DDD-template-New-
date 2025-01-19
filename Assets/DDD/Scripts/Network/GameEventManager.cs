using DDD.Scripts.Core;
using UnityEngine;

namespace DDD.Network
{
    public class GameEventManager : DDDMonoBehaviour
    {
        private static GameEventManager instance;

        public static GameEventManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("GameEventManager");
                    instance = go.AddComponent<GameEventManager>();
                    DontDestroyOnLoad(go);
                }

                return instance;
            }
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SendEvent(string eventType, object value)
        {
            var eventData = new GameEvent
            {
                eventType = eventType,
                value = value
            };

            string json = JsonUtility.ToJson(eventData);
            SendMessageToWebContainer(json);
        }

        private void SendMessageToWebContainer(string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        SendWebMessage(message);
#else
            Debug.Log($"[GameEvent] Would send to web: {message}");
#endif
        }

        public void NotifyBalanceChanged()
        {
            SendEvent(GameEventTypes.BALANCE_CHANGED, null);
        }

        public void NotifyCoinsChanged(long coins)
        {
            SendEvent(GameEventTypes.COINS_CHANGED, coins);
        }

        public void NotifyCrystalsChanged(int crystals)
        {
            SendEvent(GameEventTypes.CRYSTALS_CHANGED, crystals);
        }

        public void NotifySweepsChanged(int sweeps)
        {
            SendEvent(GameEventTypes.SWEEPS_CHANGED, sweeps);
        }

        public void NotifyBarrelsChanged(int barrels)
        {
            SendEvent(GameEventTypes.BARRELS_CHANGED, barrels);
        }

        public void NavigateTo(string route)
        {
            SendEvent(GameEventTypes.NAVIGATE_TO, route);
        }

        public void NotifyGameChanged(string gameId)
        {
            SendEvent(GameEventTypes.GAME_CHANGED, gameId);
        }

        public void NotifyLevelChanged()
        {
            SendEvent(GameEventTypes.LEVEL_CHANGED, null);
        }
    }

    public static class WebGLPlugins
    {
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void SendMessageToParent(string message);

        public static void SendWebMessage(string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SendMessageToParent(message);
#endif
        }
    }
}