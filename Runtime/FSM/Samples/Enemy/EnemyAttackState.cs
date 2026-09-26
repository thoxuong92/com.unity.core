using System;
using UnityEngine;
using Unity.Core.FSM.Visual;
using Unity.Core.Logging;

namespace Unity.Core.FSM.Samples
{
    /// <summary>
    /// Attack State for Enemy AI.
    /// Pure C# State that performs attacks against the target.
    /// </summary>
    [Serializable]
    public class EnemyAttackState : VisualStateNode<EnemyAIBehaviour>
    {
        private float _lastAttackTimestamp = -10f;

        public override void OnEnter(IState previousState)
        {
            base.OnEnter(previousState);
            AppLogger.Log("[EnemyState] Entered Attack State");
            PerformAttack();
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (Actor == null || Actor.Target == null) return;

            // Keep facing target while attacking
            Vector3 direction = (Actor.Target.position - Actor.transform.position);
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                Actor.transform.rotation = Quaternion.LookRotation(direction.normalized);
            }

            // Repeat attack after cooldown
            if (Time.time - _lastAttackTimestamp >= Actor.AttackCooldown)
            {
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            _lastAttackTimestamp = Time.time;
            if (Actor != null)
            {
                AppLogger.Log($"[EnemyState] ATTACK! Dealing {Actor.AttackDamage} damage to Target.");
            }
        }
    }
}
