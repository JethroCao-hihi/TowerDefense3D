using UnityEngine;

[CreateAssetMenu(fileName = "Du Lieu Thap Moi", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Dinh danh")]
    public string towerType = "Cannon";
    public int baseCost = 100;

    [Header("Chi so Sinh ton (MỚI)")]
    public float maxHealth = 150f; // <<== THÊM BIẾN NÀY VÀO

    [Header("Chi so Tan cong")]
    public float baseDamage = 25f;  
    public float baseFireRate = 1f; 
    public float baseRange = 3f;
}