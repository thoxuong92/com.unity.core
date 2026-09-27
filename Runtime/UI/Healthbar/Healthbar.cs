using UnityEngine;
using Unity.Core.InGame;

namespace Unity.Core.UI.Healthbar
{
    /// <summary>
    /// Component quản lý hiển thị thanh máu trên đầu nhân vật/quái vật.
    /// Tự động liên kết với HealthComponent, cập nhật thanh tiến trình HealthProgress,
    /// và xoay theo Camera khi cần thiết.
    /// </summary>
    [AddComponentMenu("Unity Core/UI/Healthbar")]
    public class Healthbar : AlignCamera
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private Canvas canvas;
        [SerializeField] private HealthProgress progress;
        [SerializeField] private bool hideWhenEmpty = false;
        [SerializeField] private bool hideWhenFull = false;

        private void OnValidate()
        {
            if (health == null) health = GetComponentInParent<HealthComponent>();
            if (canvas == null) canvas = GetComponentInChildren<Canvas>();
            if (progress == null) progress = GetComponentInChildren<HealthProgress>();
        }

        private void Start()
        {
            if (health == null) health = GetComponentInParent<HealthComponent>();
            if (health != null)
            {
                health.OnHealthChanged += UpdateHealth;
                health.OnHealthEmpty += OnDead;
                UpdateHealth(health.CurrentHealth);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (health != null)
            {
                health.OnHealthChanged -= UpdateHealth;
                health.OnHealthEmpty -= OnDead;
            }
        }

        public void UpdateHealth(float currentHealth)
        {
            if (health == null) return;

            float max = health.MaxHealth;
            if (progress != null)
            {
                progress.SetValue(currentHealth, max);
                progress.SetFillDamage(currentHealth / max);
            }

            if (canvas != null)
            {
                if (hideWhenEmpty && currentHealth <= 0)
                {
                    canvas.enabled = false;
                }
                else if (hideWhenFull && Mathf.Approximately(currentHealth, max))
                {
                    canvas.enabled = false;
                }
                else
                {
                    canvas.enabled = true;
                }
            }
        }

        private void OnDead()
        {
            if (hideWhenEmpty && canvas != null)
            {
                canvas.enabled = false;
            }
        }
    }
}
