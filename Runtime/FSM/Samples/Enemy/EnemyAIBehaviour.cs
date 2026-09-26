using UnityEngine;
using Unity.Core.FSM.Visual;
using Unity.Core.Logging;
using Unity.Core.Pool;

namespace Unity.Core.FSM.Samples
{
    /// <summary>
    /// Complete Enemy AI Behaviour managing pure C# states and perception conditions.
    /// Replaces the old Controller pattern for clean GameObject component management.
    /// Implements IPoolable for zero-allocation pooling via ObjectPooler.
    /// </summary>
    [SelectionBase]
    public class EnemyAIBehaviour : AIBehaviour, IPoolable
    {
        [Header("Enemy Attributes")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth = 100f;
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _detectRange = 8.0f;
        [SerializeField] private float _attackRange = 1.8f;
        [SerializeField] private float _attackDamage = 15f;
        [SerializeField] private float _attackCooldown = 1.2f;

        [Header("Status Effects")]
        [SerializeField] private bool _isStunned = false;

        [Header("Target Perception")]
        [SerializeField] private Transform _target;

        [Header("Debug / Gizmos")]
        [SerializeField] private bool _drawGizmos = true;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float DetectRange => _detectRange;
        public float AttackRange => _attackRange;
        public float AttackDamage => _attackDamage;
        public float AttackCooldown => _attackCooldown;
        public bool IsStunned { get => _isStunned; set => _isStunned = value; }
        public Transform Target { get => _target; set => _target = value; }

        public float DistanceToTarget
        {
            get
            {
                if (_target == null) return float.MaxValue;
                return Vector3.Distance(transform.position, _target.position);
            }
        }

        protected override void Awake()
        {
            _currentHealth = _maxHealth;

            // Auto-find player tag if target is null
            if (_target == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) _target = playerObj.transform;
            }

            base.Awake();
        }

        #region IPoolable Implementation

        public virtual void OnSpawn()
        {
            _currentHealth = _maxHealth;
            _isStunned = false;

            // Re-enable 3D colliders
            var colliders = GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                col.enabled = true;
            }

            // Re-enable 2D colliders
            var colliders2D = GetComponentsInChildren<Collider2D>(true);
            foreach (var col in colliders2D)
            {
                col.enabled = true;
            }

            // Reset FSM to initial state
            if (FSM != null && InitialState != null)
            {
                FSM.ChangeState(InitialState);
            }
        }

        public virtual void OnDespawn()
        {
        }

        #endregion

        #region Custom Condition Methods (Marked with [FSMCondition])

        [FSMCondition("Mục tiêu bị choáng (IsStunned)")]
        public bool CheckIsStunned()
        {
            return _isStunned;
        }

        [FSMCondition("Nhìn thấy người chơi rõ ràng (HasClearSight)")]
        public bool CheckHasClearSight()
        {
            return Target != null && DistanceToTarget <= _detectRange;
        }

        [FSMCondition("Đồng minh gần đó cầu cứu (AllyNeedsHelp)")]
        public bool CheckAllyNeedsHelp()
        {
            return false;
        }

        #endregion

        /// <summary>
        /// Evaluates individual condition items for multi-condition transitions.
        /// </summary>
        protected override bool EvaluateSingleCondition(TransitionConditionItem condition)
        {
            if (condition == null) return true;

            switch (condition.ConditionType)
            {
                case TransitionConditionType.HealthZero:
                    return _currentHealth <= 0f;

                case TransitionConditionType.HealthBelowThreshold:
                    return _currentHealth <= condition.NumericThreshold;

                case TransitionConditionType.TargetDetected:
                    return _currentHealth > 0f && DistanceToTarget <= _detectRange && DistanceToTarget > _attackRange;

                case TransitionConditionType.InAttackRange:
                    return _currentHealth > 0f && DistanceToTarget <= _attackRange;

                case TransitionConditionType.TargetLost:
                    return _currentHealth > 0f && DistanceToTarget > _detectRange;

                case TransitionConditionType.DistanceLessThan:
                    return _currentHealth > 0f && DistanceToTarget <= condition.NumericThreshold;

                case TransitionConditionType.DistanceGreaterThan:
                    return _currentHealth > 0f && DistanceToTarget > condition.NumericThreshold;

                case TransitionConditionType.TimerExpired:
                    return FSM?.CurrentState != null && FSM.CurrentState.StateTime >= condition.DurationThreshold;

                case TransitionConditionType.CustomCondition:
                    return EvaluateCustomCondition(condition.CustomConditionName);

                case TransitionConditionType.AlwaysTrue:
                    return true;

                default:
                    return base.EvaluateSingleCondition(condition);
            }
        }

        public void TakeDamage(float amount)
        {
            if (_currentHealth <= 0f) return;

            _currentHealth -= amount;
            AppLogger.Log($"[Enemy AI] Took {amount} damage. Health: {_currentHealth}/{_maxHealth}");

            if (_currentHealth <= 0f)
            {
                _currentHealth = 0f;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos) return;

            // Detect Range (Yellow)
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, _detectRange);

            // Attack Range (Red)
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}
