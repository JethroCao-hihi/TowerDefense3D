using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MinimapPathDrawer : MonoBehaviour
{
    [Header("Danh sách các điểm quái rẽ (Waypoints)")]
    public Transform[] waypoints; 

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
        DrawPath();
    }

    void SetupLineRenderer()
    {
        // Chỉnh sửa độ rộng của vạch kẻ đường
        lineRenderer.startWidth = 1.5f; 
        lineRenderer.endWidth = 1.5f;

        // Đảm bảo đường vẽ này chỉ hiển thị trên Minimap
        gameObject.layer = LayerMask.NameToLayer("MinimapUI");
        
        // Căn chỉnh để vạch không đổ bóng
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    void DrawPath()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        lineRenderer.positionCount = waypoints.Length;

        for (int i = 0; i < waypoints.Length; i++)
        {
            // Lấy tọa độ của điểm Waypoint, nhưng nâng trục Y lên cao một chút 
            // để đường kẻ không bị chìm xuống dưới mặt đất (tùy chỉnh số 2f theo game của bạn)
            Vector3 pointPos = new Vector3(waypoints[i].position.x, waypoints[i].position.y + 2f, waypoints[i].position.z);
            
            lineRenderer.SetPosition(i, pointPos);
        }
    }
}