namespace Unity.Core.Services
{
    /// <summary>
    /// Danh mục các loại bên thứ 3 (3rd-Party SDK Providers) được hỗ trợ.
    /// </summary>
    public enum ServiceType : byte
    {
        None = 0,
        Mock = 1,
        Firebase = 2,
        AppLovinMax = 3,
        GoogleAdMob = 4,
        UnityAds = 5,
        Adjust = 6,
        AppsFlyer = 7,
        GameAnalytics = 8,
        UnityIAP = 9
    }
}
