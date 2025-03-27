#region Data Classes

using System;
using Newtonsoft.Json;

[Serializable]
public class WinRequest
{
    public int amount;
}

[System.Serializable]
public class BetResponse
{
    public bool success;
    [JsonProperty("amount")]
    public int winAmount;
}

[System.Serializable]
public class BetRequestData
{
    public int bet;
}

[System.Serializable]
public class BetResponseDTO
{
    [JsonProperty("data")]
    public BetData data;
    
    [JsonProperty("success")]
    public bool success;
}

[System.Serializable]
public class BetData
{
    [JsonProperty("amount")]
    public int amount;
}
[Serializable]
public class CashoutResponseDTO
{
    public bool success;
    public string message;
    public CashoutDTO data;
}

[Serializable]
public class CashoutDTO
{
    public int win;
    public long balance;
}

[Serializable]
public class WebSocketAuthMessage
{
    public string type;
    public string token;
}

[Serializable]
public class TokenResponse
{
    public bool success;
    public TokenData data;
}

[Serializable]
public class TokenData
{
    public string ACCESS_TOKEN;
    public string REFRESH_TOKEN;
}


[Serializable]
public class ResponseWrapper
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("data")]
    public UserData Data { get; set; }
}
[Serializable]
public class UserData
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("balance")]
    public Balance Balance { get; set; }

    [JsonProperty("levelId")]
    public int LevelId { get; set; }

    [JsonProperty("experience")]
    public string Experience { get; set; }

    [JsonProperty("challengesRerolledToday")]
    public int ChallengesRerolledToday { get; set; }

    [JsonProperty("unlockedGames")]
    public int[] UnlockedGames { get; set; }

    [JsonProperty("receivedRewardsLevels")]
    public int[] ReceivedRewardsLevels { get; set; }

    [JsonProperty("avatarId")]
    public int? AvatarId { get; set; }

    [JsonProperty("paymentsSpendLimit")]
    public PaymentsSpendLimit PaymentsSpendLimit { get; set; }

    [JsonProperty("paymentsSpendLimitChanged")]
    public DateTime? PaymentsSpendLimitChanged { get; set; }

    [JsonProperty("stamps")]
    public string[] Stamps { get; set; }

    [JsonProperty("challengeRerollCostRefreshedAt")]
    public DateTime ChallengeRerollCostRefreshedAt { get; set; }

    [JsonProperty("purchased")]
    public int Purchased { get; set; }

    [JsonProperty("withdrawn")]
    public int Withdrawn { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("Level")]
    public Level Level { get; set; }

    [JsonProperty("UserProfile")]
    public UserProfile UserProfile { get; set; }

    [JsonProperty("AuthData")]
    public AuthData AuthData { get; set; }

    [JsonProperty("idHashed")]
    public string IdHashed { get; set; }
}
[Serializable]
public class Balance
{
    [JsonProperty("coins")]
    public long Coins { get; set; }

    [JsonProperty("crystals")]
    public int Crystals { get; set; }

    [JsonProperty("barrels")]
    public int Barrels { get; set; }

    [JsonProperty("globalFreeSpins")]
    public int GlobalFreeSpins { get; set; }

    [JsonProperty("rSweeps")]
    public int RSweeps { get; set; }

    [JsonProperty("bSweeps")]
    public int BSweeps { get; set; }

    [JsonProperty("sweeps")]
    public float Sweeps { get; set; }

    [JsonProperty("frozenRSweeps")]
    public int FrozenRSweeps { get; set; }

    [JsonProperty("piggyBankCoins")]
    public int PiggyBankCoins { get; set; }

    [JsonProperty("piggyBankSweeps")]
    public int PiggyBankSweeps { get; set; }

    [JsonProperty("vipPoints")]
    public int VipPoints { get; set; }

    [JsonProperty("frozenVipPoints")]
    public int FrozenVipPoints { get; set; }
}
[Serializable]
public class PaymentsSpendLimit
{
    [JsonProperty("day")]
    public int Day { get; set; }

    [JsonProperty("week")]
    public int Week { get; set; }

    [JsonProperty("month")]
    public int Month { get; set; }
}
[Serializable]
public class Level
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("level")]
    public int LevelNumber { get; set; }

    [JsonProperty("experience")]
    public string Experience { get; set; }

    [JsonProperty("maxBet")]
    public int MaxBet { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
[Serializable]
public class UserProfile
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("userId")]
    public int UserId { get; set; }

    [JsonProperty("isLocked")]
    public bool IsLocked { get; set; }

    [JsonProperty("verificationStatus")]
    public string VerificationStatus { get; set; }

    [JsonProperty("firstName")]
    public string FirstName { get; set; }

    [JsonProperty("lastName")]
    public string LastName { get; set; }

    [JsonProperty("gender")]
    public string Gender { get; set; }

    [JsonProperty("address")]
    public string? Address { get; set; }

    [JsonProperty("zipCode")]
    public string? ZipCode { get; set; }

    [JsonProperty("city")]
    public string? City { get; set; }

    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("dateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [JsonProperty("privacySettings")]
    public object PrivacySettings { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
[Serializable]
public class AuthData
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("userName")]
    public string? UserName { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("phone")]
    public string Phone { get; set; }

    [JsonProperty("facebookId")]
    public string? FacebookId { get; set; }

    [JsonProperty("guestToken")]
    public string? GuestToken { get; set; }

    [JsonProperty("isPhoneVerified")]
    public bool IsPhoneVerified { get; set; }

    [JsonProperty("isEmailVerified")]
    public bool IsEmailVerified { get; set; }

    [JsonProperty("loginCount")]
    public int LoginCount { get; set; }

    [JsonProperty("lastLoginDate")]
    public DateTime? LastLoginDate { get; set; }

    [JsonProperty("googleId")]
    public string? GoogleId { get; set; }

    [JsonProperty("banReason")]
    public string? BanReason { get; set; }

    [JsonProperty("ip")]
    public string Ip { get; set; }

    [JsonProperty("crmMetaData")]
    public CrmMetaData CrmMetaData { get; set; }

    [JsonProperty("isOptinEmail")]
    public bool IsOptinEmail { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
[Serializable]
public class CrmMetaData
{
    [JsonProperty("aid")]
    public int Aid { get; set; }

    [JsonProperty("trafid")]
    public int TrafId { get; set; }

    [JsonProperty("subid")]
    public int SubId { get; set; }

    [JsonProperty("originalAid")]
    public int? OriginalAid { get; set; }

    [JsonProperty("userAgent")]
    public string UserAgent { get; set; }
}
[Serializable]
public class BetRequest
{
    [JsonProperty("mode")] public string mode;
    [JsonProperty("amount")] public int amount;
    
}

public enum Mode
{
    coins = 0,
    sweeps
}
#endregion