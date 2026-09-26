using System;
using System.Collections.Generic;

namespace Unity.Core.FSM
{
    public interface IStateMachine
    {
        IState CurrentState { get; }
        IState PreviousState { get; }
        IState DefaultState { get; set; }
        IReadOnlyList<IState> RegisteredStates { get; }

        event Action<IState, IState> OnStateChanged; // (previousState, newState)

        void RegisterState<T>(T state) where T : class, IState;
        void RegisterState(IState state);

        void ChangeState<T>() where T : class, IState;
        void ChangeState(Type stateType);
        void ChangeState(IState nextState);
        void ChangeState<T, TData>(TData data) where T : class, IStateWithData<TData>;

        void RevertToPreviousState();

        void AddTransition(IState from, IState to, Func<bool> condition);
        void AddAnyTransition(IState to, Func<bool> condition);

        void OnUpdate(float deltaTime);
        void OnLateUpdate(float deltaTime);
        void OnFixedUpdate(float fixedDeltaTime);
    }
}
