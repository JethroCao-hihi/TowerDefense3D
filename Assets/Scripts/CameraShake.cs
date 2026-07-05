using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // Tạo Singleton để gọi từ bất kỳ script nào mà không cần GetComponent
    public static CameraShake Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Tạo ra độ rung ngẫu nhiên trong khoảng -1 đến 1, nhân với cường độ (magnitude)
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;

            yield return null; // Chờ frame tiếp theo
        }

        // Trả camera về vị trí cũ sau khi rung xong
        transform.localPosition = originalPos;
    }
}