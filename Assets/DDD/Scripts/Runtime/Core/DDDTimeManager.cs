using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDD.Scripts.Core
{
    public class DDDTimeManager
    {
        private bool isLooping = false;

        private Dictionary<int, List<DDDTimerData>> timerActions = new();
        private List<DDDAlarmData> activeAlarms = new();

        private DDDOfflineTime _dddOfflineTime;

        private int counter;
        private int alarmCounter;
        private int offlineSeconds;

        public DDDTimeManager()
        {
            TimerLoop();


            DDDManager.Instance.EventsManager.AddListener(DDDEventNames.OnPause, OnPause);
        }

        private void OnPause(object pauseStatus)
        {
            if (!(bool)pauseStatus)
            {
                CheckOfflineTime();
            }
        }

        ~DDDTimeManager()
        {
            isLooping = false;
            DDDManager.Instance.EventsManager.RemoveListener(DDDEventNames.OnPause, OnPause);
        }

        private void CheckOfflineTime()
        {
            var timePassed = DateTime.Now - _dddOfflineTime.LastCheck;
            offlineSeconds = (int)timePassed.TotalSeconds;
            _dddOfflineTime.LastCheck = DateTime.Now;
            //DDDManager.Instance.SaveManager.Save(_dddOfflineTime);


            DDDManager.Instance.EventsManager.InvokeEvent(DDDEventNames.OfflineTimeRefreshed, offlineSeconds);
        }

        public int GetLastOfflineTimeSeconds()
        {
            return offlineSeconds;
        }

        private async Task TimerLoop()
        {
            isLooping = true;

            while (isLooping)
            {
                await Task.Delay(1000);
                InvokeTime();
            }

            isLooping = false;
        }

        private void InvokeTime()
        {
            counter++;

            foreach (var timers in timerActions)
            {
                foreach (var timer in timers.Value)
                {
                    var offsetCounter = counter - timer.StartCounter;

                    if (offsetCounter % timers.Key == 0)
                    {
                        timer.TimerAction.Invoke();
                    }
                }
            }

            for (var index = 0; index < activeAlarms.Count; index++)
            {
                var alarmData = activeAlarms[index];

                if (DateTime.Compare(alarmData.AlarmTime, DateTime.Now) < 0)
                {
                    alarmData.AlarmAction.Invoke();
                    activeAlarms.Remove(alarmData);
                }
            }
        }

        public void SubscribeTimer(int intervalSeconds, Action onTickAction)
        {
            if (!timerActions.ContainsKey(intervalSeconds))
            {
                timerActions.Add(intervalSeconds, new List<DDDTimerData>());
            }

            timerActions[intervalSeconds].Add(new DDDTimerData(counter, onTickAction));
        }

        public void UnSubscribeTimer(int intervalSeconds, Action onTickAction)
        {
            timerActions[intervalSeconds].RemoveAll(x => x.TimerAction == onTickAction);
        }

        public int SetAlarm(int seconds, Action onAlarmAction)
        {
            alarmCounter++;

            var alarmData = new DDDAlarmData
            {
                ID = alarmCounter,
                AlarmTime = DateTime.Now.AddSeconds(seconds),
                AlarmAction = onAlarmAction
            };

            activeAlarms.Add(alarmData);
            return alarmCounter;
        }

        public void DisableAlarm(int alarmID)
        {
            activeAlarms.RemoveAll(x => x.ID == alarmID);
        }

        public int GetLeftOverTime(OfflineTimes timeType)
        {
            if (!_dddOfflineTime.LeftOverTimes.ContainsKey(timeType))
            {
                return 0;
            }

            return _dddOfflineTime.LeftOverTimes[timeType];
        }

        public void SetLeftOverTime(OfflineTimes timeType, int timeAmount)
        {
            _dddOfflineTime.LeftOverTimes[timeType] = timeAmount;
        }
    }


    public class DDDTimerData
    {
        public Action TimerAction;
        public int StartCounter;

        public DDDTimerData(int counter, Action onTickAction)
        {
            TimerAction = onTickAction;
            StartCounter = counter;
        }
    }

    public class DDDAlarmData
    {
        public int ID;
        public DateTime AlarmTime;
        public Action AlarmAction;
    }

    [Serializable]
    public class DDDOfflineTime : IDDDSaveData
    {
        public DateTime LastCheck;
        public Dictionary<OfflineTimes, int> LeftOverTimes = new();
    }

    public enum OfflineTimes
    {
        DailyBonus,
        ExtraBonus
    }


    public interface IDDDSaveData
    {
    }
    
    


namespace HOG.Core
{
    public class DDDPool
    {
        public Queue<DDDPoolable> AllPoolables = new();
        public Queue<DDDPoolable> UsedPoolables = new();
        public Queue<DDDPoolable> AvailablePoolables = new();

        public int MaxPoolables = 100;
    }

    public enum PoolNames
    {
        NA = -1,
        ScoreToast = 0,
        TrianglePool = 1
    }
}
}