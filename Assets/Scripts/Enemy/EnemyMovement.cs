using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Target Setup")]
    public Transform endPoint;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (endPoint != null) MoveToTarget();
    }

    public void SetTarget(Transform target)
    {
        endPoint = target;
        MoveToTarget();
    }

    private void MoveToTarget()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            agent.SetDestination(endPoint.position);
        }
    }

    // CÁCH SIÊU ĐƠN GIẢN: Tự động chạy khi thân quái chạm vào vùng End
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem có phải vừa chạm vào điểm có Tag "Finish" không
        if (other.CompareTag("Finish"))
        {
            Debug.Log($"🎯 {gameObject.name} đã va chạm điểm End và bị xóa!");
            Destroy(gameObject); // Xóa quái
        }
    }
}