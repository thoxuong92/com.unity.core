using System;
using UnityEngine;
using Unity.Core.FSM;
using Unity.Core.Logging;
using Unity.Core.Services;

namespace Unity.Core.Boot
{
    /// <summary>
    /// Base Bootstrapper MonoBehaviour cho toàn bộ game nền Unity Core.
    /// Quản lý khởi tạo Service, cấu hình Frame Rate và khởi động Game State Machine.
    /// Tự động kích hoạt tại BeforeSceneLoad mà KHÔNG CẦN gán component MonoBehaviour thủ công trong scene.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameBootstrapper : MonoBehaviour
    {
        public static GameBootstrapper Instance { get; private set; }

        /// <summary>
        /// Cho phép bật/tắt cơ chế tự động khởi chạy GameBootstrapper lúc game bật.
        /// </summary>
        public static bool EnableAutoBootstrap { get; set; } = true;

        public GameStateMachine StateMachine { get; protected set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (!EnableAutoBootstrap || Instance != null) return;

#if UNITY_2023_1_OR_NEWER
            var existing = UnityEngine.Object.FindFirstObjectByType<GameBootstrapper>();
#else
            var existing = UnityEngine.Object.FindObjectOfType<GameBootstrapper>();
#endif
            if (existing != null)
            {
                Instance = existing;
                return;
            }

            // Tự động tìm subclass của GameBootstrapper do developer định nghĩa (nếu có)
            Type targetType = typeof(GameBootstrapper);
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    var asm = assemblies[i];
                    var name = asm.GetName().Name;
                    if (name.StartsWith("System") || name.StartsWith("mscorlib") || name.StartsWith("UnityEngine"))
                    {
                        continue;
                    }

                    var types = asm.GetTypes();
                    for (int j = 0; j < types.Length; j++)
                    {
                        var t = types[j];
                        if (t != typeof(GameBootstrapper) && typeof(GameBootstrapper).IsAssignableFrom(t) && !t.IsAbstract)
                        {
                            targetType = t;
                            break;
                        }
                    }

                    if (targetType != typeof(GameBootstrapper))
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"[GameBootstrapper] Lỗi quét custom bootstrapper: {ex.Message}. Sử dụng default GameBootstrapper.");
            }

            var rootGO = BootstrapperRoot.GetOrCreate();
            rootGO.AddComponent(targetType);
            AppLogger.Log($"[GameBootstrapper] Tự động khởi chạy GameBootstrapper ({targetType.Name}) không cần gán component thủ công.");
        }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            AppLogger.Log("Game Bootstrapper starting...");
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = true;

            StateMachine = new GameStateMachine();

            RegisterServices();
            RegisterStates(StateMachine);
        }

        protected virtual void Start()
        {
            OnBootCompleted();
        }

        protected virtual void Update()
        {
            StateMachine?.Update();
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceRegistry.ShutdownAll();
                Instance = null;
            }
        }

        /// <summary>
        /// Đăng ký các dịch vụ gameplay bổ sung vào ServiceRegistry.
        /// </summary>
        protected virtual void RegisterServices() { }

        /// <summary>
        /// Đăng ký các FSM State bổ sung vào GameStateMachine.
        /// </summary>
        protected virtual void RegisterStates(GameStateMachine fsm) { }

        /// <summary>
        /// Kích hoạt state chuyển cảnh ban đầu sau khi boot xong.
        /// </summary>
        protected virtual void OnBootCompleted()
        {
            AppLogger.Log("[GameBootstrapper] Boot completed.");
        }
    }
}
