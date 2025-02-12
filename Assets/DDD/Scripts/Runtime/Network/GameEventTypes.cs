using System;

namespace DDD.Network
{
    public static class GameEventTypes
    {
        public const string LEVEL_CHANGED = "LEVEL_CHANGED";
        public const string BALANCE_CHANGED = "BALANCE_CHANGED";
        public const string COINS_CHANGED = "COINS_CHANGED";
        public const string CRYSTALS_CHANGED = "CRYSTALS_CHANGED";
        public const string SWEEPS_CHANGED = "SWEEPS_CHANGED";
        public const string BARRELS_CHANGED = "BARRELS_CHANGED";
        public const string NAVIGATE_TO = "NAVIGATE_TO";
        public const string GAME_CHANGED = "GAME_CHANGED";
    }
    
    [Serializable]
    public class GameEvent
    {
        public string eventType;
        public object value;
    }


}