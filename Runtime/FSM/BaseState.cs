namespace Unity.Core.FSM
{
    /// <summary>
    /// Abstract base class for all FSM states with default empty implementations
    /// and built-in duration tracking.
    /// </summary>
    public abstract class BaseState : IState
    {
        public IStateMachine Parent { get; set; }
        public float StateTime { get; private set; }

        public virtual void OnEnter(IState previousState)
        {
            StateTime = 0f;
        }

        public virtual void OnUpdate(float deltaTime)
        {
            StateTime += deltaTime;
        }

        public virtual void OnLateUpdate(float deltaTime)
        {
        }

        public virtual void OnFixedUpdate(float fixedDeltaTime)
        {
        }

        public virtual void OnExit(IState nextState)
        {
        }
    }

    /// <summary>
    /// Abstract base class for states with custom data payload on entry.
    /// </summary>
    public abstract class BaseStateWithData<TData> : BaseState, IStateWithData<TData>
    {
        public abstract void OnEnter(IState previousState, TData data);

        public override void OnEnter(IState previousState)
        {
            base.OnEnter(previousState);
            OnEnter(previousState, default);
        }
    }
}
