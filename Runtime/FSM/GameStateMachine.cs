using System;
using UnityEngine;

namespace Unity.Core.FSM
{
    /// <summary>
    /// Specialized High-Level State Machine for Global Game Flow (Boot, Splash, MainMenu, Gameplay, GameOver).
    /// Inherits from StateMachine to gain all transitions, payloads, and lifecycle features.
    /// </summary>
    public class GameStateMachine : StateMachine
    {
        public GameStateMachine(IState defaultState = null) : base(defaultState)
        {
        }

        public void Update()
        {
            OnUpdate(Time.deltaTime);
        }
    }
}
