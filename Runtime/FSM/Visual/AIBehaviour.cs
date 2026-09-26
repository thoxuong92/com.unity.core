using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unity.Core.FSM.Visual
{
    /// <summary>
    /// Core AI Behaviour component running a Finite State Machine (FSM).
    /// Manages Pure C# Visual States via [SerializeReference] without cluttering the GameObject with components.
    /// Supports multi-condition transitions, AND/OR logic, and automatic discovery of [FSMCondition] methods.
    /// </summary>
    [SelectionBase]
    public class AIBehaviour : MonoBehaviour
    {
        [Header("State Configuration")]
        [SerializeField] private string _initialStateName;
        [SerializeReference] private List<VisualStateNode> _states = new List<VisualStateNode>();

        [Header("Visual Transitions")]
        [SerializeField] private List<VisualTransitionConfig> _transitions = new List<VisualTransitionConfig>();

        public StateMachine FSM { get; private set; }
        public VisualStateNode CurrentVisualState => FSM?.CurrentState as VisualStateNode;
        public IReadOnlyList<VisualStateNode> States => _states;
        public IReadOnlyList<VisualTransitionConfig> Transitions => _transitions;

        // Cached delegates for [FSMCondition] methods for zero-allocation runtime performance
        private readonly Dictionary<string, Func<bool>> _cachedCustomConditions = new Dictionary<string, Func<bool>>(StringComparer.OrdinalIgnoreCase);

        public string InitialStateName
        {
            get => _initialStateName;
            set => _initialStateName = value;
        }

        public VisualStateNode InitialState
        {
            get => GetState(_initialStateName) ?? (_states.Count > 0 ? _states[0] : null);
            set => _initialStateName = value != null ? value.StateName : string.Empty;
        }

        public event Action<VisualStateNode, VisualStateNode> OnVisualStateChanged;

        protected virtual void Awake()
        {
            CacheCustomConditions();
            InitializeFSM();
        }

        private void CacheCustomConditions()
        {
            _cachedCustomConditions.Clear();
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                if (method.ReturnType != typeof(bool) || method.GetParameters().Length != 0) continue;

                var attr = method.GetCustomAttribute<FSMConditionAttribute>();
                if (attr != null)
                {
                    var func = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), this, method);
                    _cachedCustomConditions[method.Name] = func;

                    if (!string.IsNullOrEmpty(attr.DisplayName))
                    {
                        _cachedCustomConditions[attr.DisplayName] = func;
                    }
                }
            }
        }

        public void InitializeFSM()
        {
            if (FSM != null) return;

            FSM = new StateMachine();

            // Initialize all Pure C# State instances
            foreach (var state in _states)
            {
                if (state == null) continue;
                state.OnInit(this);
                FSM.RegisterState(state);
            }

            var defaultState = InitialState;
            FSM.DefaultState = defaultState;

            // Register visual transitions
            foreach (var t in _transitions)
            {
                if (t == null || string.IsNullOrEmpty(t.ToState)) continue;

                var toState = GetState(t.ToState);
                if (toState == null) continue;

                Func<bool> conditionEvaluator = () => EvaluateTransition(t);

                if (!string.IsNullOrEmpty(t.FromState))
                {
                    var fromState = GetState(t.FromState);
                    if (fromState != null)
                    {
                        FSM.AddTransition(fromState, toState, conditionEvaluator);
                    }
                }
                else
                {
                    // Any State Transition (Global)
                    FSM.AddAnyTransition(toState, conditionEvaluator);
                }
            }

            FSM.OnStateChanged += (prev, next) =>
            {
                OnVisualStateChanged?.Invoke(prev as VisualStateNode, next as VisualStateNode);
            };
        }

        protected virtual void Start()
        {
            if (FSM != null && FSM.CurrentState == null && InitialState != null)
            {
                FSM.ChangeState(InitialState);
            }
        }

        protected virtual void Update()
        {
            FSM?.OnUpdate(Time.deltaTime);
        }

        protected virtual void LateUpdate()
        {
            FSM?.OnLateUpdate(Time.deltaTime);
        }

        protected virtual void FixedUpdate()
        {
            FSM?.OnFixedUpdate(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Evaluates a transition by combining all its conditions using AND / OR LogicMode.
        /// </summary>
        public virtual bool EvaluateTransition(VisualTransitionConfig transition)
        {
            if (transition == null) return false;

            // Backward-compatibility: if Conditions list is empty, evaluate single condition
            if (transition.Conditions == null || transition.Conditions.Count == 0)
            {
                return EvaluateCondition(transition);
            }

            if (transition.LogicMode == ConditionLogicMode.All)
            {
                // AND: All conditions must be met
                for (int i = 0; i < transition.Conditions.Count; i++)
                {
                    var cond = transition.Conditions[i];
                    if (cond != null && !EvaluateSingleCondition(cond))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                // OR: Any condition met triggers transition
                for (int i = 0; i < transition.Conditions.Count; i++)
                {
                    var cond = transition.Conditions[i];
                    if (cond != null && EvaluateSingleCondition(cond))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Evaluates a single condition item. Override in specialized AI classes (e.g. EnemyAIBehaviour).
        /// </summary>
        protected virtual bool EvaluateSingleCondition(TransitionConditionItem condition)
        {
            if (condition == null) return true;

            switch (condition.ConditionType)
            {
                case TransitionConditionType.AlwaysTrue:
                    return true;

                case TransitionConditionType.TimerExpired:
                    return FSM?.CurrentState != null && FSM.CurrentState.StateTime >= condition.DurationThreshold;

                case TransitionConditionType.CustomCondition:
                    return EvaluateCustomCondition(condition.CustomConditionName);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Evaluates a custom condition by name. First checks cached [FSMCondition] methods, then user overrides.
        /// </summary>
        public virtual bool EvaluateCustomCondition(string conditionName)
        {
            if (string.IsNullOrEmpty(conditionName)) return false;

            if (_cachedCustomConditions.TryGetValue(conditionName, out var func))
            {
                return func.Invoke();
            }

            return false;
        }

        /// <summary>
        /// Fallback condition evaluation for single-condition transitions.
        /// </summary>
        protected virtual bool EvaluateCondition(VisualTransitionConfig transition)
        {
            switch (transition.ConditionType)
            {
                case TransitionConditionType.TimerExpired:
                    return FSM?.CurrentState != null && FSM.CurrentState.StateTime >= transition.DurationThreshold;

                case TransitionConditionType.CustomCondition:
                    return EvaluateCustomCondition(transition.CustomConditionName);

                case TransitionConditionType.AlwaysTrue:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Finds a state by its display name or class type name.
        /// </summary>
        public VisualStateNode GetState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName)) return null;

            for (int i = 0; i < _states.Count; i++)
            {
                var s = _states[i];
                if (s == null) continue;

                if (s.StateName.Equals(stateName, StringComparison.OrdinalIgnoreCase) ||
                    s.GetType().Name.Equals(stateName, StringComparison.OrdinalIgnoreCase))
                {
                    return s;
                }
            }

            return null;
        }

        /// <summary>
        /// Alias for GetState for backwards compatibility.
        /// </summary>
        public VisualStateNode GetStateByName(string stateName) => GetState(stateName);

        /// <summary>
        /// Manually force transition to a state (useful for inspector debug buttons).
        /// </summary>
        public void ForceTransition(VisualStateNode targetState)
        {
            if (targetState != null && FSM != null)
            {
                FSM.ChangeState(targetState);
            }
        }
    }
}
