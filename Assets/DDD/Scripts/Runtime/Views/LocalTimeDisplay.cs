using System;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LivingPixels.Runtime.Views
{
public class LocalTimeDisplay : MonoBehaviour
{
    public bool showAmPm = true;
    [SerializeField] private TextMeshProUGUI _timeText;
    private bool _isRunning = true;

    private void Start()
    {
        if (_timeText == null)
        {
            Debug.LogError("TextMeshProUGUI reference is missing!");
            return;
        }

        StartCountdown().Forget();
    }

    private async UniTaskVoid StartCountdown()
    {
        _isRunning = true;

        while (_isRunning)
        {
            UpdateTime();

            // Calculate time until the next full minute (sync to real time)
            var now = DateTime.Now;
            var nextMinute = now.AddMinutes(1).AddSeconds(-now.Second);
            var delay = nextMinute - now;

            await UniTask.Delay(delay);
        }
    }

    private void UpdateTime()
    {
        _timeText.text = showAmPm? DateTime.Now.ToString("hh:mm tt") : DateTime.Now.ToString("HH:mm"); // Displays 12-hour format with AM/PM
    }

    private void OnDestroy()
    {
        _isRunning = false;
    }
}
}