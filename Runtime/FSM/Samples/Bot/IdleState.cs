using System;
using UnityEngine;
using Unity.Core.FSM;
using Unity.Core.FSM.Visual;

/// <summary>
/// Custom FSM State (Pure C# - Not a MonoBehaviour component).
/// Managed by AIBehaviour via [SerializeReference].
/// </summary>
[Serializable]
public class IdleState : VisualStateNode
{
    public override void OnEnter(IState previousState)
    {
        base.OnEnter(previousState);
        // Code executed when entering this state
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        // Code executed every frame while active
    }

    public override void OnExit(IState nextState)
    {
        base.OnExit(nextState);
        // Code executed when exiting this state
    }
}
