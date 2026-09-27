using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Core;
using Unity.Core.Logging;
using Unity.Core.Services;
using Unity.Core.Services.Ads;
using Unity.Core.Services.Analytics;
using Unity.Core.Services.RemoteConfig;

namespace Unity.Core.Services
{
    /// <summary>
    /// API Service Locator Facade tương thích ngược với API.Get<T>().
    /// Cho phép truy xuất nhanh các service đã đăng ký trong ServiceRegistry.
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
    /// Service Adapter tương thích cho ServiceAds (WASD pattern).
    /// Chuyển tiếp toàn bộ cuộc gọi sang AdsService trung tâm của Unity Core.
    /// </summary>
    public class ServiceAds : IService
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
            if (!ServiceRegistry.IsRegistered<ServiceAds>())
            {
                ServiceRegistry.Register(new ServiceAds());
            }
        }
    }

    /// <summary>
    /// Service Adapter tương thích cho ServiceRemoteConfig (WASD pattern).
    /// </summary>
    public static class ServiceRemoteConfig
    {
        public static bool IsRemoteConfigInitialized => RemoteConfigService.IsFetched;
        public static T GetValue<T>(string key, T defaultValue = default) => RemoteConfigService.GetValue(key, defaultValue);
        public static void Fetch(Action<bool> onComplete = null) => RemoteConfigService.Fetch(onComplete);
    }

    /// <summary>
    /// Service Adapter tương thích cho ServiceLogEvent (WASD pattern).
    /// Chuyển tiếp sự kiện phân tích sang AnalyticsService.
    /// </summary>
    public class ServiceLogEvent : IService
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
            if (!ServiceRegistry.IsRegistered<ServiceLogEvent>())
            {
                ServiceRegistry.Register(new ServiceLogEvent());
            }
        }
    }
}

namespace WASD
{
    /// <summary>
    /// Base class giả lập cho TickBehaviour từ hệ thống WASD cũ, kế thừa MonoBehaviour chuẩn của Unity.
    /// </summary>
    public abstract class TickBehaviour : MonoBehaviour
    {
        protected virtual void Awake() { }
    }

    /// <summary>
    /// Lớp chuyển tiếp WASD.Splash tương thích hoàn toàn 100% với code mẫu của dự án WASD.
    /// Kế thừa toàn bộ logic Splash Loading tiên tiến từ Unity.Core.Boot.Splash.
    /// </summary>
    [HelpURL("https://github.com/thoxuong92/com.unity.core")]
    public class Splash : Unity.Core.Boot.Splash
    {
    }

    /// <summary>
    /// Logging bridge tương thích với cú pháp Wasd.Log.
    /// </summary>
    public static class Wasd
    {
        public static void Log(string message) => AppLogger.Log(message);
        public static void LogWarning(string message) => AppLogger.LogWarning(message);
        public static void LogError(string message) => AppLogger.LogError(message);
    }

    /// <summary>
    /// Thuộc tính ReadOnly trong namespace WASD.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : Unity.Core.ReadOnlyAttribute
    {
    }
}
