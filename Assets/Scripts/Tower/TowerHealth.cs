using UnityEngine;

public class TowerHealth : MonoBehaviour
{
    [Header("Chi so mau thap")]
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (TowerPlacementManager.Instance != null)
        {
            TowerPlacementManager.Instance.RemoveTowerFromGrid(gameObject);

            Destroy(gameObject);
        }
    }
}
