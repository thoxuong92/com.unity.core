using UnityEngine;

namespace Unity.Core.FSM
{
    /// <summary>
    /// MonoBehaviour runner that owns and automatically ticks an FSM instance.
    /// Useful for Character AI, Enemy AI, Boss Controllers, and Level Controllers.
    /// </summary>
    public class FSMRunner : MonoBehaviour
    {
        public StateMachine FSM { get; private set; }

        [SerializeField] private bool _autoInitialize = true;
        [SerializeField] private bool _tickLateUpdate = true;
        [SerializeField] private bool _tickFixedUpdate = true;

        protected virtual void Awake()
        {
            FSM = new StateMachine();
            if (_autoInitialize)
            {
                SetupStateMachine(FSM);
            }
        }

        protected virtual void Start()
        {
            if (FSM.CurrentState == null && FSM.DefaultState != null)
            {
                FSM.ChangeState(FSM.DefaultState);
            }
        }

        protected virtual void Update()
        {
            FSM?.OnUpdate(Time.deltaTime);
        }

        protected virtual void LateUpdate()
        {
            if (_tickLateUpdate)
            {
                FSM?.OnLateUpdate(Time.deltaTime);
            }
        }

        protected virtual void FixedUpdate()
        {
            if (_tickFixedUpdate)
            {
                FSM?.OnFixedUpdate(Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Override this to register states and configure transitions.
        /// </summary>
        protected virtual void SetupStateMachine(StateMachine fsm)
        {
        }
    }
}
