using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TowerRange : MonoBehaviour
{
    public int segments = 50; // Độ mịn của vòng tròn
    private LineRenderer line;
    private TowerShooting towerShooting;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        towerShooting = GetComponent<TowerShooting>();

        // Cài đặt nét vẽ
        line.positionCount = segments + 1;
        line.useWorldSpace = false;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;

        // Vẽ vòng tròn dựa trên biến 'range' của tháp
        float x, z;
        float angle = 0f;
        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * towerShooting.range;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * towerShooting.range;
            line.SetPosition(i, new Vector3(x, 0.2f, z)); // 0.2f để vòng tròn nổi lên trên mặt đất
            angle += (360f / segments);
        }

        line.enabled = false; // Mặc định ẩn đi
    }

    // Tự động bật vòng tròn khi lia chuột vào
    void OnMouseEnter()
    {
        if (Time.timeScale > 0f) line.enabled = true;
    }

    // Tự động tắt khi lia chuột ra chỗ khác
    void OnMouseExit()
    {
        line.enabled = false;
    }
}