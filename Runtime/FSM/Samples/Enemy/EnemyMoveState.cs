using System;
using UnityEngine;
using Unity.Core.FSM.Visual;
using Unity.Core.Logging;

namespace Unity.Core.FSM.Samples
{
    /// <summary>
    /// Move / Chase State for Enemy AI.
    /// Pure C# State that rotates and moves the host transform towards target.
    /// </summary>
    [Serializable]
    public class EnemyMoveState : VisualStateNode<EnemyAIBehaviour>
    {
        public override void OnEnter(IState previousState)
        {
            base.OnEnter(previousState);
            AppLogger.Log("[EnemyState] Entered Move / Chase State");
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (Actor == null || Actor.Target == null) return;

            // Look towards target
            Vector3 direction = (Actor.Target.position - Actor.transform.position);
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction.normalized);
                Actor.transform.rotation = Quaternion.Slerp(Actor.transform.rotation, targetRot, deltaTime * 10f);

                // Move forward
                Actor.transform.position += direction.normalized * (Actor.MoveSpeed * deltaTime);
            }
        }
    }
}
