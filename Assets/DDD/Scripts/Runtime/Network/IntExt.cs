namespace DDD.Game
{
    public static class IntExt 
    {
        public static int SweepsFormat(this int value)
        {
            return value / 100;
        }    
        public static float SweepsFormat(this float value)
        {
            return value / 100;
        }
    }
}