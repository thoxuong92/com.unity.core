using System;
using System.Collections.Generic;
using Unity.Core.Logging;

namespace Unity.Core.FSM
{
    /// <summary>
    /// Enterprise-grade Finite State Machine (FSM) and Hierarchical State Machine (HFSM).
    /// Supports explicit transitions, conditional auto-transitions, global transitions (Any Transitions),
    /// strongly-typed payload passing, and sub-state machine nesting.
    /// </summary>
    public class StateMachine : IStateMachine, IState
    {
        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();
        private readonly List<IState> _stateList = new List<IState>();
        private readonly List<ITransition> _transitions = new List<ITransition>();
        private readonly List<ITransition> _anyTransitions = new List<ITransition>();

        public IStateMachine Parent { get; set; }
        public float StateTime { get; private set; }

        public IState CurrentState { get; private set; }
        public IState PreviousState { get; private set; }
        public IState DefaultState { get; set; }
        public IReadOnlyList<IState> RegisteredStates => _stateList;

        public event Action<IState, IState> OnStateChanged;

        public StateMachine(IState defaultState = null)
        {
            if (defaultState != null)
            {
                RegisterState(defaultState);
                DefaultState = defaultState;
            }
        }

        #region State Registration

        public void RegisterState<T>(T state) where T : class, IState
        {
            RegisterStateInternal(typeof(T), state);
        }

        public void RegisterState(IState state)
        {
            if (state == null) return;
            RegisterStateInternal(state.GetType(), state);
        }

        private void RegisterStateInternal(Type type, IState state)
        {
            if (state == null) return;

            state.Parent = this;

            if (_states.ContainsKey(type))
            {
                _states[type] = state;
            }
            else
            {
                _states.Add(type, state);
                _stateList.Add(state);
            }

            if (DefaultState == null)
            {
                DefaultState = state;
            }
        }

        #endregion

        #region Transitions Management

        public void AddTransition(IState from, IState to, Func<bool> condition)
        {
            if (from == null || to == null) return;
            RegisterState(from);
            RegisterState(to);
            _transitions.Add(new Transition(from, to, condition));
        }

        public void AddAnyTransition(IState to, Func<bool> condition)
        {
            if (to == null) return;
            RegisterState(to);
            _anyTransitions.Add(new Transition(null, to, condition));
        }

        #endregion

        #region State Changing

        public void ChangeState<T>() where T : class, IState
        {
            ChangeState(typeof(T));
        }

        public void ChangeState(Type stateType)
        {
            if (_states.TryGetValue(stateType, out var state))
            {
                TransitionTo(state);
            }
            else
            {
                AppLogger.LogError($"[StateMachine] State '{stateType.Name}' is not registered!");
            }
        }

        public void ChangeState(IState nextState)
        {
            if (nextState == null) return;

            if (!_stateList.Contains(nextState))
            {
                RegisterState(nextState);
            }

            TransitionTo(nextState);
        }

        public void ChangeState<T, TData>(TData data) where T : class, IStateWithData<TData>
        {
            var type = typeof(T);
            if (_states.TryGetValue(type, out var state) && state is IStateWithData<TData> dataState)
            {
                TransitionToWithData(dataState, data);
            }
            else
            {
                AppLogger.LogError($"[StateMachine] StateWithData '{type.Name}' is not registered!");
            }
        }

        public void RevertToPreviousState()
        {
            if (PreviousState != null)
            {
                TransitionTo(PreviousState);
            }
            else if (DefaultState != null)
            {
                TransitionTo(DefaultState);
            }
        }

        private void TransitionTo(IState nextState)
        {
            if (nextState == null || nextState == CurrentState) return;

            var oldState = CurrentState;
            oldState?.OnExit(nextState);

            PreviousState = oldState;
            CurrentState = nextState;
            CurrentState.OnEnter(oldState);

            OnStateChanged?.Invoke(oldState, CurrentState);
        }

        private void TransitionToWithData<TData>(IStateWithData<TData> nextState, TData data)
        {
            if (nextState == null || nextState == CurrentState) return;

            var oldState = CurrentState;
            oldState?.OnExit(nextState);

            PreviousState = oldState;
            CurrentState = nextState;
            nextState.OnEnter(oldState, data);

            OnStateChanged?.Invoke(oldState, CurrentState);
        }

        #endregion

        #region Execution Lifecycle

        public void OnEnter(IState previousState)
        {
            StateTime = 0f;
            if (CurrentState == null && DefaultState != null)
            {
                TransitionTo(DefaultState);
            }
            else
            {
                CurrentState?.OnEnter(previousState);
            }
        }

        public void OnUpdate(float deltaTime)
        {
            StateTime += deltaTime;

            if (CurrentState == null)
            {
                if (DefaultState != null) TransitionTo(DefaultState);
                return;
            }

            // Check Any Transitions first
            for (int i = 0; i < _anyTransitions.Count; i++)
            {
                var trans = _anyTransitions[i];
                if (trans.To != CurrentState && trans.Evaluate())
                {
                    TransitionTo(trans.To);
                    return;
                }
            }

            // Check From Transitions
            for (int i = 0; i < _transitions.Count; i++)
            {
                var trans = _transitions[i];
                if (trans.From == CurrentState && trans.Evaluate())
                {
                    TransitionTo(trans.To);
                    return;
                }
            }

            CurrentState?.OnUpdate(deltaTime);
        }

        public void OnLateUpdate(float deltaTime)
        {
            CurrentState?.OnLateUpdate(deltaTime);
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            CurrentState?.OnFixedUpdate(fixedDeltaTime);
        }

        public void OnExit(IState nextState)
        {
            CurrentState?.OnExit(nextState);
        }

        #endregion
    }
}
