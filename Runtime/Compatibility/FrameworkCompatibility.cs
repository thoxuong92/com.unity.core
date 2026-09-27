using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Core.Logging;
using Unity.Core.Services;
using Unity.Core.Services.Ads;
using Unity.Core.Services.Analytics;
using Unity.Core.Services.RemoteConfig;
using Unity.Core.Services.Tracking;

namespace Unity.Core.Services
{
    /// <summary>
    /// API Service Locator Facade tương thích ngược với API.Get<T>().
    /// Cho phép truy xuất nhanh các service 3rd-party đã đăng ký trong ServiceRegistry.
    /// </summary>
    public static class API
    {
        public static T Get<T>() where T : class, IService
        {
            return ServiceRegistry.Get<T>();
        }

        public static bool TryGet<T>(out T service) where T : class, IService
        {
            return ServiceRegistry.TryGet(out service);
        }
    }

    /// <summary>
    /// Service Adapter trung gian cho hệ thống Ads (AppLovin, AdMob, UnityAds, Mock).
    /// Chuyển tiếp toàn bộ cuộc gọi sang AdsService trung tâm của Unity Core.
    /// </summary>
    public class GameAds : IService
    {
        public AdsSettings Setting => AdsService.Settings;

        public bool ShowedFirstOpen
        {
            get => AdsService.ShowedFirstOpen;
            set => AdsService.ShowedFirstOpen = value;
        }

        public bool ShowFirstOpenDone
        {
            get => AdsService.ShowFirstOpenDone;
            set => AdsService.ShowFirstOpenDone = value;
        }

        public bool CanShowAppOpen() => AdsService.CanShowAppOpen;
        public bool CanShowBanner() => AdsService.CanShowBanner;
        public bool CanShowInterstitial() => AdsService.CanShowInterstitial;
        public bool CanShowRewarded() => AdsService.CanShowRewarded;

        public void OnAppOpenLoaded(Action onClosed = null) => AdsService.OnAppOpenLoaded(onClosed);
        public void ShowBanner(string placement = null) => AdsService.ShowBanner(placement);
        public void HideBanner() => AdsService.HideBanner();
        public void ShowAppOpen(string placement, Action onClosed = null) => AdsService.ShowAppOpen(placement, onClosed);
        public void ShowInterstitial(string placement, Action onClosed = null) => AdsService.ShowInterstitial(placement, onClosed);
        public void ShowRewarded(string placement, Action<bool> onReward = null) => AdsService.ShowRewarded(placement, onReward);

        public void Initialize() { }
        public void Shutdown() { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoRegister()
        {
            if (!ServiceRegistry.IsRegistered<GameAds>())
            {
                ServiceRegistry.Register(new GameAds());
            }
        }
    }

    /// <summary>
    /// Lớp chuyển tiếp tương thích ngược cho ServiceAds.
    /// </summary>
    public class ServiceAds : GameAds
    {
    }

    /// <summary>
    /// Service Adapter trung gian cho hệ thống Remote Config (Firebase Remote Config).
    /// </summary>
    public class GameRemoteConfig : IService
    {
        public T GetValue<T>(string key, T defaultValue = default)
        {
            return RemoteConfigService.GetValue(key, defaultValue);
        }

        public void Initialize() { }
        public void Shutdown() { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoRegister()
        {
            if (!ServiceRegistry.IsRegistered<GameRemoteConfig>())
            {
                ServiceRegistry.Register(new GameRemoteConfig());
            }
        }
    }

    /// <summary>
    /// Lớp chuyển tiếp tương thích ngược cho ServiceRemoteConfig.
    /// </summary>
    public class ServiceRemoteConfig : GameRemoteConfig
    {
    }

    /// <summary>
    /// Service Adapter trung gian cho hệ thống Analytics & Event Logging (Firebase Analytics).
    /// </summary>
    public class GameAnalytics : IService
    {
        public void Log(string eventName, Dictionary<string, object> parameters = null)
        {
            AnalyticsService.LogEvent(eventName, parameters);
        }

        public void Initialize() { }
        public void Shutdown() { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoRegister()
        {
            if (!ServiceRegistry.IsRegistered<GameAnalytics>())
            {
                ServiceRegistry.Register(new GameAnalytics());
            }
        }
    }

    /// <summary>
    /// Lớp chuyển tiếp tương thích ngược cho ServiceLogEvent.
    /// </summary>
    public class ServiceLogEvent : GameAnalytics
    {
    }

    /// <summary>
    /// Service Adapter trung gian cho hệ thống Tracking / Attribution (Adjust, AppsFlyer).
    /// </summary>
    public class GameTracking : IService
    {
        public void TrackEvent(string eventToken, double? revenue = null, string currency = null)
        {
            TrackingService.TrackEvent(eventToken, revenue, currency);
        }

        public void TrackRevenue(AdRevenueInfo revenueInfo)
        {
            TrackingService.TrackRevenue(revenueInfo);
        }

        public void TrackRevenue(string source, double revenue, string currency, string networkName = null)
        {
            TrackingService.TrackRevenue(new AdRevenueInfo
            {
                Source = source,
                Revenue = revenue,
                Currency = currency,
                NetworkName = networkName
            });
        }

        public void Initialize() { }
        public void Shutdown() { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoRegister()
        {
            if (!ServiceRegistry.IsRegistered<GameTracking>())
            {
                ServiceRegistry.Register(new GameTracking());
            }
        }
    }
}

namespace GameFramework
{
    /// <summary>
    /// Lớp Splash điều phối khởi động game dùng chung, kết nối Remote Config, AppOpen Ads và Scene transition.
    /// </summary>
    [HelpURL("https://github.com/thoxuong92/com.unity.core")]
    public class Splash : Unity.Core.Boot.Splash
    {
    }

    /// <summary>
    /// Logging bridge dùng chung (Framework Log).
    /// </summary>
    public static class GameLog
    {
        public static void Log(string message) => AppLogger.Log(message);
        public static void Log(string tag, string message) => AppLogger.Log($"[{tag}] {message}");
        public static void LogWarning(string message) => AppLogger.LogWarning(message);
        public static void LogError(string message) => AppLogger.LogError(message);
    }

    /// <summary>
    /// Alias cho GameLog.
    /// </summary>
    public static class FrameworkLog
    {
        public static void Log(string message) => GameLog.Log(message);
        public static void Log(string tag, string message) => GameLog.Log(tag, message);
        public static void LogWarning(string message) => GameLog.LogWarning(message);
        public static void LogError(string message) => GameLog.LogError(message);
    }

    /// <summary>
    /// Thuộc tính ReadOnly trong namespace GameFramework.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : Unity.Core.ReadOnlyAttribute
    {
    }
}

namespace WASD
{
    /// <summary>
    /// Lớp chuyển tiếp WASD.Splash tương thích ngược cho các dự án cũ.
    /// </summary>
    [Obsolete("Vui lòng sử dụng GameFramework.Splash hoặc Unity.Core.Boot.Splash.")]
    [HelpURL("https://github.com/thoxuong92/com.unity.core")]
    public class Splash : GameFramework.Splash
    {
    }

    /// <summary>
    /// Logging bridge tương thích ngược cho cú pháp Wasd.Log cũ.
    /// </summary>
    [Obsolete("Vui lòng sử dụng GameFramework.GameLog hoặc Unity.Core.Logging.AppLogger thay cho Wasd.")]
    public static class Wasd
    {
        public static void Log(string message) => GameFramework.GameLog.Log(message);
        public static void Log(string tag, string message) => GameFramework.GameLog.Log(tag, message);
        public static void LogWarning(string message) => GameFramework.GameLog.LogWarning(message);
        public static void LogError(string message) => GameFramework.GameLog.LogError(message);
    }

    /// <summary>
    /// Thuộc tính ReadOnly tương thích ngược trong namespace WASD.
    /// </summary>
    [Obsolete("Vui lòng sử dụng GameFramework.ReadOnlyAttribute hoặc Unity.Core.ReadOnlyAttribute.")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : GameFramework.ReadOnlyAttribute
    {
    }
}
