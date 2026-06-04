using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    private Transform target;
    public float speed = 15f;

    public void Seek(Transform _target)
    {
        target = _target;
    }
    void Update()
    {
     if (target == null)
        {
            Destroy(gameObject); return;
        }   
     Vector3 dir = target.position - transform.position;    
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);

        transform.LookAt(target);
    }
    void HitTarget()
    {
        Destroy(target.gameObject);
        Destroy(gameObject);
    }
}
