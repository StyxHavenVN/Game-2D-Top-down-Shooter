using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Kho Vũ Khí (Kéo Prefabs vào đây)")]
    public GameObject pistolBulletPrefab;  
    public GameObject shotgunBulletPrefab;  
    public GameObject swordSlashPrefab;   

    [Header("Chỉ số Súng Lục (Spam)")]
    public float pistolForce = 20f;
    public float pistolCooldown = 0.2f; 

    [Header("Chỉ số Shotgun (Bắn chùm)")]
    public float shotgunForce = 15f;
    public float shotgunCooldown = 0.8f; 
    public int pelletsCount = 5; 
    public float spreadAngle = 45f; 

    [Header("Chỉ số Kiếm (Cận chiến)")]
    public float swordCooldown = 0.5f;
    public float swordRange = 1.2f; 

    private float nextAttackTime = 0f;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (Time.time >= nextAttackTime)
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector2 lookDir = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        MainMenuManager.WeaponType currentWeapon = MainMenuManager.selectedWeapon;

        if (currentWeapon == MainMenuManager.WeaponType.Pistol)
        {
            ShootPistol(lookDir, angle);
            nextAttackTime = Time.time + pistolCooldown;
        }
        else if (currentWeapon == MainMenuManager.WeaponType.Shotgun)
        {
            ShootShotgun(lookDir, angle);
            nextAttackTime = Time.time + shotgunCooldown;
        }
        else if (currentWeapon == MainMenuManager.WeaponType.Sword)
        {
            SwingSword(lookDir, angle);
            nextAttackTime = Time.time + swordCooldown;
        }
    }

    void ShootPistol(Vector2 dir, float angle)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShootSound();
        }
        GameObject bullet = Instantiate(pistolBulletPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(dir.normalized * pistolForce, ForceMode2D.Impulse);
    }

    void ShootShotgun(Vector2 dir, float baseAngle)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShootSound();
        }
        float angleStep = spreadAngle / (pelletsCount - 1);
        float startAngle = baseAngle - (spreadAngle / 2f);

        for (int i = 0; i < pelletsCount; i++)
        {
            float pelletAngle = startAngle + (angleStep * i);
            GameObject pellet = Instantiate(shotgunBulletPrefab, transform.position, Quaternion.Euler(0, 0, pelletAngle));

            Vector2 pelletDir = new Vector2(Mathf.Cos(pelletAngle * Mathf.Deg2Rad), Mathf.Sin(pelletAngle * Mathf.Deg2Rad));

            Rigidbody2D rb = pellet.GetComponent<Rigidbody2D>();
            rb.AddForce(pelletDir.normalized * shotgunForce, ForceMode2D.Impulse);
        }
    }

    void SwingSword(Vector2 dir, float angle)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnergySound();
        }
        Vector3 spawnPos = transform.position + (Vector3)(dir.normalized * swordRange);
        GameObject slash = Instantiate(swordSlashPrefab, spawnPos, Quaternion.Euler(0, 0, angle));
    }
}