using System;
using UnityEngine;

namespace Unity.Core.InGame
{
    /// <summary>
    /// Đối tượng chứa thông tin về sát thương trong game (lượng sát thương, loại sát thương, điểm va chạm, lực đẩy).
    /// </summary>
    [Serializable]
    public class Damage
    {
        public float Amount { get; private set; }
        public string DamageType { get; private set; }
        public Vector3 HitPoint { get; private set; }
        public Vector3 HitNormal { get; private set; }
        public float KnockbackForce { get; private set; }

        public Damage(float amount = 0, string damageType = "Normal", Vector3 hitPoint = default, Vector3 hitNormal = default, float knockback = 0)
        {
            Amount = amount;
            DamageType = damageType;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            KnockbackForce = knockback;
        }

        public override bool Equals(object obj)
        {
            if (obj is Damage other)
            {
                return Mathf.Approximately(Amount, other.Amount) && DamageType == other.DamageType;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, DamageType);
        }
    }

    /// <summary>
    /// Giao diện cho bất kỳ thực thể nào có thể nhận sát thương (nhân vật, quái vật, vật thể phá hủy).
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(IDamageDealer source, Damage damage);
    }

    /// <summary>
    /// Giao diện cho thực thể gây sát thương (vũ khí, đạn, bẫy, kẻ thù).
    /// </summary>
    public interface IDamageDealer
    {
        void DealDamage(IDamageable target, Damage damage);
    }
}
