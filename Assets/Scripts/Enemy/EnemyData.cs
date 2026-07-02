using UnityEngine;

[CreateAssetMenu(fileName = "Du Lieu UFO", menuName = "Tower Defense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Thong tin UFO")]
    public string enemyName = "UFO Normal";
    
    [Header("Chi so Di chuyen & Sinh ton")]
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
    
    [Header("Chi so Tan cong (Ban Thap)")]
    public float attackDamage = 10f; // Sát thương UFO gây ra cho tháp
    public float fireRate = 1.5f;    // Tốc độ nhả đạn của UFO
    public float attackRange = 3f;   // Tầm phát hiện tháp để dừng lại bắn
    
    [Header("Phan thuong & Hinh phat")]
    public int goldReward = 15;      
    public int damageToBase = 1;     
}