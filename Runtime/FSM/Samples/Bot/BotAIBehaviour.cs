using UnityEngine;
using Unity.Core.FSM.Visual;

/// <summary>
/// Custom AI Behaviour running an FSM without attaching state components to the GameObject.
/// </summary>
[SelectionBase]
public class BotAIBehaviour : AIBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        // Custom initialization
    }

    /// <summary>
    /// Custom condition evaluations for transitions configured in the Visual FSM Graph.
    /// </summary>
    protected override bool EvaluateCondition(VisualTransitionConfig transition)
    {
        // Example:
        // if (transition.ConditionType == TransitionConditionType.TargetDetected) return CheckTarget();

        return base.EvaluateCondition(transition);
    }
}
