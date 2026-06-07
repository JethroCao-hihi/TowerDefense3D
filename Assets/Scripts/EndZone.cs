using UnityEngine;

public class EndZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.LoseLife(1);
            }
            Destroy(other.gameObject);
        }
    }
}
