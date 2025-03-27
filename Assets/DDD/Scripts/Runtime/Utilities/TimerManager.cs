using System;
using Cysharp.Threading.Tasks;

namespace LivingPixels.Runtime.Utilities
{
    public class TimerManager
    {
        // Event triggered every second
        public static event Action OnOneSecondElapsed;

        private bool isRunning;

        public void StartTimer()
        {
            if (isRunning) return;
            isRunning = true;
            RunTimer().Forget();
        }

        public void StopTimer()
        {
            isRunning = false;
        }

        private async UniTaskVoid RunTimer()
        {
            while (isRunning)
            {
                // Raise the event to notify subscribers
                OnOneSecondElapsed?.Invoke();

                // Wait for one second
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public void ClearSubscribers()
        {
            StopTimer();
            OnOneSecondElapsed = null;
        }
    }
}