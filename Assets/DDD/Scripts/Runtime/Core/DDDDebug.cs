using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DDD.Scripts.Core
{
    public class DDDDebug
    {
        public static void Log(object message)
        {
            Debug.Log(message);
        } 
        public static void Log(object message,GameObject context)
        {
            Debug.Log(message, context);
        }
        
        public static void LogException(object message)
        {
            Debug.LogException(new Exception(message.ToString()));
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(message.ToString());
        }

        public static void LogError(string p0)
        {
            Debug.LogError(p0);
        }
    }
}