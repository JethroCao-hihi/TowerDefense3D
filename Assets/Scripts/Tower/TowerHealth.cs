using UnityEngine;
using UnityEngine.UI;

public class TowerHealth : MonoBehaviour
{
    [Header("--- Chỉ số Máu Tháp ---")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("--- Giao diện Thanh Máu (UI) ---")]
    public Image healthBarFill; 

    [HideInInspector] public bool isTakingDamage = false; 

    void Start()
    {
        currentHealth = maxHealth;
        UpdateTowerHealthBar();
    }

    public void FuseFrom(TowerHealth otherTower)
    {
        if (otherTower == null) return;

        float currentHealthPercent = (this.currentHealth / this.maxHealth) * 100f;

        if (otherTower.currentHealth > this.currentHealth)
        {
            if (otherTower.currentHealth >= otherTower.maxHealth)
            {
                if (currentHealthPercent >= 100f) this.currentHealth = this.maxHealth;
                else if (currentHealthPercent >= 75f) this.currentHealth = this.maxHealth;
                else if (currentHealthPercent >= 50f) this.currentHealth += (this.maxHealth * 0.25f);
                else if (currentHealthPercent >= 20f) this.currentHealth += (this.maxHealth * 0.75f);
                else this.currentHealth += (this.maxHealth * 0.75f);
            }
            else
            {
                this.currentHealth += (otherTower.currentHealth * 0.75f);
            }
        }
        
        this.currentHealth = Mathf.Clamp(this.currentHealth, 0f, this.maxHealth);
        UpdateTowerHealthBar();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0f) currentHealth = 0f;

        UpdateTowerHealthBar();

        if (currentHealth <= 0f)
        {
            DestroyTower();
        }
    }

    private void UpdateTowerHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    private void DestroyTower()
    {
        if (TowerPlacementManager.Instance != null)
        {
            TowerPlacementManager.Instance.RemoveTowerFromGrid(gameObject);
        }
        Destroy(gameObject);
    }
}