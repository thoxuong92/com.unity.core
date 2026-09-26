namespace Unity.Core.FSM
{
    /// <summary>
    /// Core interface for states in the WASD Finite State Machine.
    /// Supports full lifecycle hooks: Enter, Update, LateUpdate, FixedUpdate, Exit, and Duration tracking.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Reference to the parent StateMachine owning this state.
        /// </summary>
        IStateMachine Parent { get; set; }

        /// <summary>
        /// Total time (in seconds) this state has been active since last entry.
        /// </summary>
        float StateTime { get; }

        /// <summary>
        /// Called when the state becomes active.
        /// </summary>
        /// <param name="previousState">The state being transitioned from (null if initial state).</param>
        void OnEnter(IState previousState);

        /// <summary>
        /// Called every frame during MonoBehaviour.Update.
        /// </summary>
        /// <param name="deltaTime">Time.deltaTime</param>
        void OnUpdate(float deltaTime);

        /// <summary>
        /// Called every frame during MonoBehaviour.LateUpdate.
        /// </summary>
        /// <param name="deltaTime">Time.deltaTime</param>
        void OnLateUpdate(float deltaTime);

        /// <summary>
        /// Called during physics ticks in MonoBehaviour.FixedUpdate.
        /// </summary>
        /// <param name="fixedDeltaTime">Time.fixedDeltaTime</param>
        void OnFixedUpdate(float fixedDeltaTime);

        /// <summary>
        /// Called when transitioning out of this state.
        /// </summary>
        /// <param name="nextState">The upcoming state.</param>
        void OnExit(IState nextState);
    }

    /// <summary>
    /// Generic state interface that accepts strongly-typed payload data on entry.
    /// </summary>
    public interface IStateWithData<in TData> : IState
    {
        /// <summary>
        /// Called when entering this state with custom parameter data.
        /// </summary>
        void OnEnter(IState previousState, TData data);
    }
}
