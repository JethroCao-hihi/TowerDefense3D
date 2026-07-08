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

    void Update()
    {
        if (agent == null || !agent.isActiveAndEnabled) return;

        if (Time.timeScale == 0f)
        {
            agent.isStopped = true; // Game đang tạm dừng -> Đứng im
        }
        else
        {
            // Khi game đang chạy bình thường (Time.timeScale == 1f)
            // và AI đang bị bắt dừng trước đó -> Cho phép chạy tiếp
            if (agent.isStopped) 
            {
                agent.isStopped = false; 
            }
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (Time.timeScale == 0f) return;

        if (other.CompareTag("Finish"))
        {
            Destroy(gameObject);
        }
    }
}