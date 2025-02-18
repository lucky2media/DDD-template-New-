using System;
using System.Runtime.InteropServices;
using DDD.Scripts.Core;

namespace DDD.Game
{
   
    public class CookieReader 
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern IntPtr GetCookieValue(string cookieName);
#endif

        public CookieReader(Action onInit)
        {
            onInit?.Invoke();
        }
        public static string FetchCookieValue(string cookieName)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Call the JS plugin function, which returns a pointer to a UTF8 string
        IntPtr ptr = GetCookieValue(cookieName);

        // Convert that pointer into a C# string
        string value = Marshal.PtrToStringAnsi(ptr);

        return value;
#else
            
            return null;
#endif
        }

        
    }
}