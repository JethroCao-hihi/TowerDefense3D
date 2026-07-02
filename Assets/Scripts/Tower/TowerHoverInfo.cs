using UnityEngine;

public class TowerHoverInfo : MonoBehaviour
{
    [Header("Kéo cái Cylinder (RangeVisual) vào đây")]
    public GameObject rangeVisual;

    private TowerShooting shooting;
    private TowerStats stats;

    void Start()
    {
        shooting = GetComponent<TowerShooting>();
        stats = GetComponent<TowerStats>();
        if (rangeVisual != null) rangeVisual.SetActive(false);
    }

    void OnMouseEnter()
    {
        // Khi di chuột vào -> Tính toán tầm xa hiện tại và hiện vòng ngắm
        if (Time.timeScale > 0f && rangeVisual != null && shooting != null)
        {
            float currentRange = shooting.range;
            if (stats != null) currentRange *= stats.rangeMultiplier;

            // Kích thước của Cylinder mặc định là 1. Để bao phủ bán kính, đường kính phải nhân 2
            float diameter = currentRange * 2f;
            rangeVisual.transform.localScale = new Vector3(diameter, 0.01f, diameter);
            
            rangeVisual.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        // Khi chuột rời đi -> Ẩn vòng ngắm
        if (rangeVisual != null) rangeVisual.SetActive(false);
    }
}