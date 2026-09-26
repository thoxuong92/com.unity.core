using System;
using UnityEngine;
using Unity.Core.FSM.Visual;
using Unity.Core.Logging;
using Unity.Core.Pool;

namespace Unity.Core.FSM.Samples
{
    /// <summary>
    /// Dead State for Enemy AI.
    /// Pure C# State that disables colliders and returns the enemy GameObject to the ObjectPool.
    /// </summary>
    [Serializable]
    public class EnemyDeadState : VisualStateNode<EnemyAIBehaviour>
    {
        [SerializeField] private float _despawnDelay = 2.0f;

        public override void OnEnter(IState previousState)
        {
            base.OnEnter(previousState);
            AppLogger.Log("[EnemyState] Enemy DIED.");

            if (Actor == null) return;

            // Disable 3D colliders
            var colliders = Actor.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // Disable 2D colliders
            var colliders2D = Actor.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders2D)
            {
                col.enabled = false;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (Actor != null && StateTime >= _despawnDelay)
            {
                AppLogger.Log("[EnemyState] Returning enemy GameObject to ObjectPool.");
                ObjectPooler.Despawn(Actor.gameObject);
            }
        }
    }
}
