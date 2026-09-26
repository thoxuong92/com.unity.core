using System;
using UnityEngine;

namespace Unity.Core.FSM.Visual
{
    /// <summary>
    /// Base class for Pure C# Visual States (Non-MonoBehaviour).
    /// Managed directly inside AIBehaviour via [SerializeReference].
    /// Identified by StateName for human-readable inspector configuration.
    /// </summary>
    [Serializable]
    public abstract class VisualStateNode : IState
    {
        [SerializeField] private string _stateName;
        [SerializeField, HideInInspector] public Vector2 NodePosition = new Vector2(100, 100);

        public string StateName
        {
            get => string.IsNullOrEmpty(_stateName) ? GetType().Name : _stateName;
            set => _stateName = value;
        }

        public IStateMachine Parent { get; set; }
        public float StateTime { get; protected set; }

        /// <summary>
        /// Reference to the host AIBehaviour executing this state.
        /// </summary>
        public AIBehaviour Runner { get; internal set; }

        /// <summary>
        /// Shortcut to the host GameObject.
        /// </summary>
        public GameObject GameObject => Runner != null ? Runner.gameObject : null;

        /// <summary>
        /// Shortcut to the host Transform.
        /// </summary>
        public Transform Transform => Runner != null ? Runner.transform : null;

        /// <summary>
        /// Called when the AIBehaviour initializes the FSM on Awake.
        /// </summary>
        public virtual void OnInit(AIBehaviour runner)
        {
            Runner = runner;
        }

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
    /// Strongly-typed VisualStateNode providing direct type-safe access to the custom AIBehaviour actor.
    /// </summary>
    [Serializable]
    public abstract class VisualStateNode<T> : VisualStateNode where T : AIBehaviour
    {
        /// <summary>
        /// Strongly-typed reference to the host AIBehaviour.
        /// </summary>
        public T Actor => Runner as T;
    }
}
