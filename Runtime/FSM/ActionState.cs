using System;

namespace Unity.Core.FSM
{
    /// <summary>
    /// Lightweight state configured via actions/lambdas.
    /// Eliminates the need to create dedicated C# classes for simple or ad-hoc states.
    /// </summary>
    public class ActionState : BaseState
    {
        private Action<IState> _onEnter;
        private Action<float> _onUpdate;
        private Action<float> _onLateUpdate;
        private Action<float> _onFixedUpdate;
        private Action<IState> _onExit;

        public string Name { get; set; }

        public ActionState(string name = "ActionState")
        {
            Name = name;
        }

        public ActionState SetOnEnter(Action<IState> onEnter)
        {
            _onEnter = onEnter;
            return this;
        }

        public ActionState SetOnUpdate(Action<float> onUpdate)
        {
            _onUpdate = onUpdate;
            return this;
        }

        public ActionState SetOnLateUpdate(Action<float> onLateUpdate)
        {
            _onLateUpdate = onLateUpdate;
            return this;
        }

        public ActionState SetOnFixedUpdate(Action<float> onFixedUpdate)
        {
            _onFixedUpdate = onFixedUpdate;
            return this;
        }

        public ActionState SetOnExit(Action<IState> onExit)
        {
            _onExit = onExit;
            return this;
        }

        public override void OnEnter(IState previousState)
        {
            base.OnEnter(previousState);
            _onEnter?.Invoke(previousState);
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            _onUpdate?.Invoke(deltaTime);
        }

        public override void OnLateUpdate(float deltaTime)
        {
            base.OnLateUpdate(deltaTime);
            _onLateUpdate?.Invoke(deltaTime);
        }

        public override void OnFixedUpdate(float fixedDeltaTime)
        {
            base.OnFixedUpdate(fixedDeltaTime);
            _onFixedUpdate?.Invoke(fixedDeltaTime);
        }

        public override void OnExit(IState nextState)
        {
            base.OnExit(nextState);
            _onExit?.Invoke(nextState);
        }
    }
}
