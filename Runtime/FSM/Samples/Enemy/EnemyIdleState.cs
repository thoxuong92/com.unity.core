using System;
using Unity.Core.FSM.Visual;
using Unity.Core.Logging;

namespace Unity.Core.FSM.Samples
{
    /// <summary>
    /// Idle State for Enemy AI.
    /// Pure C# State managed by EnemyAIBehaviour without MonoBehaviour overhead.
    /// </summary>
    [Serializable]
    public class EnemyIdleState : VisualStateNode<EnemyAIBehaviour>
    {
        public override void OnEnter(IState previousState)
        {
            base.OnEnter(previousState);
            AppLogger.Log("[EnemyState] Entered Idle State");
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            // Idle breathing / minor patrol behavior can be added here
        }
    }
}
