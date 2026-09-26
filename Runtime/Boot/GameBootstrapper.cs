using UnityEngine;
using Unity.Core.FSM;
using Unity.Core.Logging;
using Unity.Core.Services;

namespace Unity.Core.Boot
{
    /// <summary>
    /// Base Bootstrapper MonoBehaviour for every Unity Core game.
    /// Manages service initialization and initial state startup.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public abstract class GameBootstrapper : MonoBehaviour
    {
        public static GameBootstrapper Instance { get; private set; }
        protected GameStateMachine StateMachine { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
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
        /// Register all required game services into ServiceRegistry.
        /// </summary>
        protected abstract void RegisterServices();

        /// <summary>
        /// Register FSM States into StateMachine.
        /// </summary>
        protected abstract void RegisterStates(GameStateMachine fsm);

        /// <summary>
        /// Trigger initial state transition (e.g. Enter Splash / MainMenu).
        /// </summary>
        protected abstract void OnBootCompleted();
    }
}
