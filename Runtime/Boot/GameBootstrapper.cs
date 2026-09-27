using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Unity.Core.FSM;
using Unity.Core.Logging;
using Unity.Core.Services;

namespace Unity.Core.Boot
{
    /// <summary>
    /// Bootstrapper thuần C# tự động khởi chạy tại BeforeSceneLoad.
    /// Hoàn toàn không kế thừa MonoBehaviour, không dùng Singleton, không scan Assembly, không Find Object.
    /// </summary>
    public static class GameBootstrapper
    {
        public static GameStateMachine StateMachine { get; private set; }
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

            AppLogger.Log("[GameBootstrapper] Khởi chạy hệ thống Core...");
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = true;

            StateMachine = new GameStateMachine();

            // Đăng ký Update loop cho StateMachine thông qua PlayerLoop chuẩn của Unity
            RegisterPlayerLoop();

            // Đăng ký giải phóng tài nguyên tự động khi ứng dụng tắt
            Application.quitting -= OnApplicationQuitting;
            Application.quitting += OnApplicationQuitting;

            OnBoot?.Invoke();
        }

        private static void RegisterPlayerLoop()
        {
            try
            {
                var currentLoop = PlayerLoop.GetCurrentPlayerLoop();
                for (int i = 0; i < currentLoop.subSystemList.Length; i++)
                {
                    if (currentLoop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.Update))
                    {
                        var updateSystem = currentLoop.subSystemList[i];
                        var subSystems = new System.Collections.Generic.List<PlayerLoopSystem>(
                            updateSystem.subSystemList ?? Array.Empty<PlayerLoopSystem>()
                        );

                        subSystems.Add(new PlayerLoopSystem
                        {
                            type = typeof(GameBootstrapper),
                            updateDelegate = OnUpdate
                        });

                        updateSystem.subSystemList = subSystems.ToArray();
                        currentLoop.subSystemList[i] = updateSystem;
                        PlayerLoop.SetPlayerLoop(currentLoop);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"[GameBootstrapper] Không thể gắn PlayerLoop: {ex.Message}");
            }
        }

        private static void OnUpdate()
        {
            StateMachine?.Update();
        }

        private static void OnApplicationQuitting()
        {
            AppLogger.Log("[GameBootstrapper] Ứng dụng thoát. Giải phóng ServiceRegistry...");
            ServiceRegistry.ShutdownAll();
        }
    }
}
