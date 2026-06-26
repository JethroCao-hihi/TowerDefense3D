using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float destroyTime = 1f;

    void Start()
    {
        // Tự động bốc hơi sau 1 giây
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // Bay từ từ lên trời
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Luôn xoay mặt chữ về phía Camera để dễ đọc
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    // Hàm để chèn nội dung chữ
    public void Setup(string textToDisplay, Color color)
    {
        TextMeshPro textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = textToDisplay;
            textMesh.color = color;
        }
    }
}