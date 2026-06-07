using UnityEngine;
using UnityEngine.UI;
public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Health Bar")]
    public Image healthBarFill;
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damegeAmout)
    {
        currentHealth -= damegeAmout;

        healthBarFill.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    public void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }
}
