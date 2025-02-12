using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DDD.Scripts.Core
{
    public class DDDDebug
    {
        [Conditional("LOGS_ENABLE")]
        public static void Log(object message)
        {
            Debug.Log(message);
        } 
        [Conditional("LOGS_ENABLE")]
        public static void Log(object message,GameObject context)
        {
            Debug.Log(message, context);
        }
        
        [Conditional("LOGS_ENABLE")]
        public static void LogException(object message)
        {
            Debug.LogException(new Exception(message.ToString()));
        }

        [Conditional("LOGS_ENABLE")]
        public static void LogWarning(object message)
        {
            Debug.LogWarning(message.ToString());
        }
    }
}