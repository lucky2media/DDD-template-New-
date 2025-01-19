public static class StorageKeys
{
    public static readonly string APP_SETTINGS = "APP_SETTINGS";
    public static readonly string CURRENCY_MODE = "CURRENCY_MODE";
    public static readonly string USER_DTO = "USER_DTO";
    public static readonly string TOKEN = "TOKEN";
    public static readonly string BALANCE = "BALANCE";
    public static readonly string BET = "BET";
    public static readonly string BETS = "BETS";
    public static readonly string MULTIPLIER = "MULTIPLIER";
    public static readonly string AUTOSPINS = "AUTOSPINS";
    public static readonly string BET_SHOULD_BE_LOCKED = "BET_SHOULD_BE_LOCKED";
    public static readonly string JP_IS_PLAYING = "JP_IS_PLAYING";
    public static readonly string SLOT_PURCHASED = "SLOT_PURCHASED";
    public static readonly string FS_COUNT = "FS_COUNT";
    public static readonly string GAME_NAME ;
}


[System.Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[System.Serializable]
public class LoginResponse
{
    public string accessToken;
}