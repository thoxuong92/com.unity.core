using UnityEngine;

public class DemoPlayer : TickBehaviour
{
    HealthComponent HealthComponent { get; set; }

    protected override void Awake()
    {
        base.Awake();
        HealthComponent = GetComponent<HealthComponent>();
        HealthComponent.OnHealthEmpty += OnHealthEmpty;
    }

    private void OnHealthEmpty()
    {
       // Destroy(gameObject, 1.0f);
    }
    [Button]
    void TakeDamage()
    {
        HealthComponent.TakeDamage(10);
    }

    [Button]
    void AddHealth()
    {
        HealthComponent.AddHealth(10);
    }
}
