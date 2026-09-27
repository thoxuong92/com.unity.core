using System;
using UnityEngine;
using Unity.Core.Tick;

namespace Unity.Core.UI.Healthbar
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho hệ thống máu/sức khỏe của nhân vật, quái vật, công trình.
    /// </summary>
    public abstract class HealthComponent : TickBehaviour
    {
        public Action<float> OnHealthChanged;
        public Action OnHealthEmpty;

        public abstract float MaxHealth { get; set; }
        public abstract float CurrentHealth { get; set; }
        public abstract bool IsAlive { get; }

        public abstract void AddHealth(float amount);
        public abstract void TakeDamage(float damage);
        public abstract void SetHealth(float health);
    }
}
