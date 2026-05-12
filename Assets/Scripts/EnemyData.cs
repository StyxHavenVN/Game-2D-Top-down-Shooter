using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "SilverBullet/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float maxHP;
    public float moveSpeed;
    public float damage;
    public float attackRange;
    public float attackCooldown;
    public float expReward;
    public float detectionRange;

    [Header("Ranged Only")]
    public float preferredDistance;
    public float bulletSpeed;

    [Header("Boss")]
    public bool isBoss;
}
