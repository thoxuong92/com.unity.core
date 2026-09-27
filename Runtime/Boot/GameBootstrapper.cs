using System;
using UnityEngine;
using Unity.Core.Logging;
using Unity.Core.Services;

namespace Unity.Core.Boot
{
    /// <summary>
    /// Bootstrapper thuần C# tự động khởi chạy tại BeforeSceneLoad để khởi tạo nền tảng Service Registry.
    /// Hoàn toàn không kế thừa MonoBehaviour, không dùng Singleton, không scan Assembly, không Find Object.
    /// </summary>
    public static class GameBootstrapper
    {
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Sự kiện bắn ra khi GameBootstrapper hoàn tất khởi tạo ban đầu.
        /// </summary>
        public static event Action OnBoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnInit()
        {
            if (IsInitialized) return;
            IsInitialized = true;

            AppLogger.Log("[GameBootstrapper] Khởi chạy hệ thống Core Services...");
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = true;

            // Đăng ký giải phóng tài nguyên tự động khi ứng dụng tắt
            Application.quitting -= OnApplicationQuitting;
            Application.quitting += OnApplicationQuitting;

            OnBoot?.Invoke();
        }

        private static void OnApplicationQuitting()
        {
            AppLogger.Log("[GameBootstrapper] Ứng dụng thoát. Giải phóng ServiceRegistry...");
            ServiceRegistry.ShutdownAll();
        }
    }
}
