using System;
using System.Collections;
using UnityEngine;

namespace DDD.Scripts.Core
{
    public class DDDMonoBehaviour : MonoBehaviour
    {
        public DDDManager Manager => DDDManager.Instance;


        protected void AddListener(DDDEventNames eventName, Action<object> onGameStart) =>
            Manager.EventsManager.AddListener(eventName, onGameStart);

        protected void RemoveListener(DDDEventNames eventName, Action<object> onGameStart) =>
            Manager.EventsManager.RemoveListener(eventName, onGameStart);

        protected void InvokeEvent(DDDEventNames eventName, object obj) =>
            Manager.EventsManager.InvokeEvent(eventName, obj);

        public Coroutine WaitForSeconds(float time, Action onComplete)
        {
            return StartCoroutine(WaitForSecondsCoroutine(time, onComplete));
        }

        private IEnumerator WaitForSecondsCoroutine(float time, Action onComplete)
        {
            yield return new WaitForSeconds(time);
            onComplete?.Invoke();
        }

        public Coroutine WaitForFrame(Action onComplete)
        {
            return StartCoroutine(WaitForFrameCoroutine(onComplete));
        }

        private IEnumerator WaitForFrameCoroutine(Action onComplete)
        {
            yield return null;
            onComplete?.Invoke();
        }
    }
}