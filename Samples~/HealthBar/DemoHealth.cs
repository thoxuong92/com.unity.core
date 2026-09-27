using UnityEngine;

public class DemoHealth : HealthComponent
{
    [Tooltip("The max amount of health that can be assigned")]
    [SerializeField]
    private float maxHealth = 100.0f;

    /// <summary>
    /// Gets or sets the max amount of health
    /// </summary>
    public override float MaxHealth { get => maxHealth; set => maxHealth = value; }

    /// <summary>
    /// Gets the current amount of health
    /// </summary>
    public override float CurrentHealth { get; set; } = 0.0f;

    /// <summary>
    /// Returns true if the current amount of health is greater than zero
    /// </summary>
    public override bool IsAlive => CurrentHealth > 0.0f;

    protected override void Awake()
    {
        base.Awake();
        SetHealth(maxHealth );
    }

    /// <summary>
    /// Sets the given amount of health
    /// </summary>
    /// <param name="health"></param>
    public override void SetHealth(float health)
    {
        float previousHealth = CurrentHealth;

        CurrentHealth = Mathf.Clamp(health, 0, MaxHealth);

        float difference = health - previousHealth;

        if (difference > 0.0f)
        {
            OnHealthChanged?.Invoke(difference);
        }
    }

    /// <summary>
    /// Adds the given amount of health
    /// </summary>
    /// <param name="amount"></param>
    public override void AddHealth(float amount)
    {
        if (IsAlive == false)
        {
            return;
        }

        float previousHealth = CurrentHealth;

        CurrentHealth += amount;

        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        float changeAmount = CurrentHealth - previousHealth;

        if (changeAmount > 0.0f)
        {
            OnHealthChanged?.Invoke(changeAmount);
        }
    }

    /// <summary>
    /// Applies the given amount of damage
    /// </summary>
    /// <param name="damage"></param>
    public override void TakeDamage(float damage)
    {
        if (IsAlive == false)
        {
            return;
        }

        float previousHealth = CurrentHealth;

        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, MaxHealth);

        float changeAmount = CurrentHealth - previousHealth;

        if (Mathf.Abs(changeAmount) > 0.0f)
        {
            OnHealthChanged?.Invoke(changeAmount);

            if (CurrentHealth <= 0.0f)
            {
                OnHealthEmpty?.Invoke();
            }
        }
    }
}