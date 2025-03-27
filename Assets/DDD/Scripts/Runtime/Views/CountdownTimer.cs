using System;
using TMPro;
using Cysharp.Threading.Tasks;
using LivingPixels.Runtime.Utilities;
using UnityEngine;

namespace LivingPixels.Runtime.Views
{
    public class CountdownTimer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private bool _usePrettyFormat;
        [SerializeField] private string _timeEndMessage = "Time's up!";
        private DateTime _targetDateTime;
        private TimeSpan _timeSpan;

        private bool _isRunning;

        private void Awake()
        {
            if (_countdownText == null)
            {
                Debug.LogError("TextMeshProUGUI reference is missing!");
                return;
            }
        }

        /// <summary>
        /// If set with this method it will use its own UniTask for countdown instead of using TimeManager's event
        /// </summary>
        /// <param name="target"></param>
        public void SetIndependentTargetDateTime(DateTime target)
        {
            _targetDateTime = target;
            if (!_isRunning)
            {
                StartCountdown().Forget();
            }
        }

        private async UniTaskVoid StartCountdown()
        {
            _isRunning = true;

            while (_isRunning)
            {
                TimeSpan countdown = _targetDateTime - DateTime.Now;

                if (countdown.TotalSeconds <= 0)
                {
                    _countdownText.text = _timeEndMessage;
                    _isRunning = false;
                    return;
                }

                if (_usePrettyFormat && countdown.TotalHours < 1)
                    _countdownText.text = FormatShortCountDown(countdown);
                else
                    _countdownText.text = FormatTimerCountdown(countdown);
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public void SetTimeSpan(DateTime target)
        {
            _targetDateTime = target;
            SetTimeSpan(target - DateTime.Now);
        }

        public void SetTimeSpan(TimeSpan span)
        {
            _timeSpan = span;
            UnregisterEvents();
            if (_timeSpan < TimeSpan.Zero)
            {
                _isRunning = false;
                _countdownText.text = _timeEndMessage;
                return;
            }

            _isRunning = true;
            if (_usePrettyFormat && span.TotalHours < 1)
            {
                TimerManager.OnOneSecondElapsed += HandleShortOneSecondElapsed;
                HandleShortOneSecondElapsed();
            }
            else
            {
                TimerManager.OnOneSecondElapsed += HandleOneSecondElapsedTimer;
                HandleOneSecondElapsedTimer();
            }
        }

        private void UnregisterEvents()
        {
            TimerManager.OnOneSecondElapsed -= HandleShortOneSecondElapsed;
            TimerManager.OnOneSecondElapsed -= HandleOneSecondElapsedTimer;
        }

        private void OnEnable()
        {
            if (_targetDateTime != DateTime.MinValue && _targetDateTime > DateTime.Now)
            {
                SetTimeSpan(_targetDateTime);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && _isRunning)
            {
                SetTimeSpan(_targetDateTime);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && _isRunning)
            {
                SetTimeSpan(_targetDateTime);
            }
        }

        public void StopCountdown()
        {
            _isRunning = false;
            UnregisterEvents();
        }

        private void OnDisable()
        {
            StopCountdown();
        }

        private void HandleOneSecondElapsedTimer()
        {
            _timeSpan = _timeSpan.Subtract(TimeSpan.FromSeconds(1));
            if (_timeSpan.TotalSeconds >= 0)
            {
                _countdownText.text = FormatTimerCountdown(_timeSpan);
            }
            else
            {
                _countdownText.text = _timeEndMessage;
                _isRunning = false;
            }
        }

        private void HandleShortOneSecondElapsed()
        {
            _timeSpan = _timeSpan.Subtract(TimeSpan.FromSeconds(1));
            if (_timeSpan.TotalSeconds >= 0)
            {
                _countdownText.text = FormatShortCountDown(_timeSpan);
            }
            else
            {
                _countdownText.text = _timeEndMessage;
                _isRunning = false;
            }
        }

        private string FormatTimerCountdown(TimeSpan countdown)
        {
            return countdown.TotalHours < 1
                ? $"{countdown.Minutes:D2}:{countdown.Seconds:D2}"
                : $"{countdown.Hours:D2}:{countdown.Minutes:D2}:{countdown.Seconds:D2}";
        }

        private string FormatShortCountDown(TimeSpan timeSpan)
        {
            return timeSpan.TotalMinutes > 1
                ? $"{timeSpan.Minutes:D1}m {timeSpan.Seconds:D1}s"
                : $"{timeSpan.Seconds:D1}s";
        }

        public void SetText(string text)
        {
            _isRunning = false;
            UnregisterEvents();
            _countdownText.text = text;
        }
    }
}