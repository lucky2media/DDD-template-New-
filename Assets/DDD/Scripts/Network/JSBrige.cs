using System.Runtime.InteropServices;

public static class JSBrige
{
   

    [DllImport("__Internal")]
    public static extern string GetCookie(string key);

}