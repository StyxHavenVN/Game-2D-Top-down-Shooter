using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Kho Vũ Khí")]
    public GameObject pistolBulletPrefab;
    public GameObject shotgunBulletPrefab;
    public GameObject swordSlashPrefab;

    [Header("Chỉ số Súng Lục")]
    public float pistolForce = 20f;
    public float pistolCooldown = 0.2f;

    [Header("Đạn Súng Lục")]
    public int pistolMaxAmmo = 17;
    private int pistolCurrentAmmo;

    [Header("Chỉ số Shotgun")]
    public float shotgunForce = 15f;
    public float shotgunCooldown = 0.8f;
    public int pelletsCount = 5;
    public float spreadAngle = 45f;

    [Header("Đạn Shotgun")]
    public int shotgunMaxAmmo = 5;
    private int shotgunCurrentAmmo;

    [Header("Chỉ số Kiếm")]
    public float swordCooldown = 0.5f;
    public float swordRange = 1.2f;

    [Header("Nạp đạn")]
    public float pistolReloadTime = 1.2f;
    public float shotgunReloadTime = 1.8f;

    private bool isReloading = false;
    private float reloadEndTime = 0f;

    private float nextAttackTime = 0f;

    void Start()
    {
        pistolCurrentAmmo = pistolMaxAmmo;
        shotgunCurrentAmmo = shotgunMaxAmmo;
    }

    void Update()
    {
        HandleReload();

        if (isReloading) return;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (Time.time >= nextAttackTime)
            {
                Attack();
            }
        }
    }

    void HandleReload()
    {
        if (isReloading)
        {
            if (Time.time >= reloadEndTime)
            {
                FinishReload();
            }

            return;
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            StartReload();
        }
    }

    void StartReload()
    {
        MainMenuManager.WeaponType currentWeapon = MainMenuManager.selectedWeapon;

        if (currentWeapon == MainMenuManager.WeaponType.Pistol)
        {
            if (pistolCurrentAmmo >= pistolMaxAmmo)
            {
                Debug.Log("Pistol đã đầy đạn.");
                return;
            }

            isReloading = true;
            reloadEndTime = Time.time + pistolReloadTime;

            Debug.Log("Đang nạp đạn Pistol...");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPistolReloadSound();
            }
        }
        else if (currentWeapon == MainMenuManager.WeaponType.Shotgun)
        {
            if (shotgunCurrentAmmo >= shotgunMaxAmmo)
            {
                Debug.Log("Shotgun đã đầy đạn.");
                return;
            }

            isReloading = true;
            reloadEndTime = Time.time + shotgunReloadTime;

            Debug.Log("Đang nạp đạn Shotgun...");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShotgunReloadSound();
            }
        }
    }

    void FinishReload()
    {
        MainMenuManager.WeaponType currentWeapon = MainMenuManager.selectedWeapon;

        if (currentWeapon == MainMenuManager.WeaponType.Pistol)
        {
            pistolCurrentAmmo = pistolMaxAmmo;
            Debug.Log("Pistol đã nạp đầy: " + pistolCurrentAmmo + " / " + pistolMaxAmmo);
        }
        else if (currentWeapon == MainMenuManager.WeaponType.Shotgun)
        {
            shotgunCurrentAmmo = shotgunMaxAmmo;
            Debug.Log("Shotgun đã nạp đầy: " + shotgunCurrentAmmo + " / " + shotgunMaxAmmo);
        }

        isReloading = false;
    }

    void Attack()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector2 lookDir = mouseWorldPos - transform.position;

        if (lookDir == Vector2.zero)
        {
            lookDir = Vector2.right;
        }

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        MainMenuManager.WeaponType currentWeapon = MainMenuManager.selectedWeapon;

        if (currentWeapon == MainMenuManager.WeaponType.Pistol)
        {
            if (pistolCurrentAmmo <= 0)
            {
                Debug.Log("Pistol hết đạn! Bấm R để nạp.");
                return;
            }

            ShootPistol(lookDir, angle);

            pistolCurrentAmmo--;
            nextAttackTime = Time.time + pistolCooldown;

            Debug.Log("Pistol Ammo: " + pistolCurrentAmmo + " / " + pistolMaxAmmo);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPistolShootSound();
            }
        }
        else if (currentWeapon == MainMenuManager.WeaponType.Shotgun)
        {
            if (shotgunCurrentAmmo <= 0)
            {
                Debug.Log("Shotgun hết đạn! Bấm R để nạp.");
                return;
            }

            ShootShotgun(lookDir, angle);

            shotgunCurrentAmmo--;
            nextAttackTime = Time.time + shotgunCooldown;

            Debug.Log("Shotgun Ammo: " + shotgunCurrentAmmo + " / " + shotgunMaxAmmo);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShotgunShootSound();
            }
        }
        else if (currentWeapon == MainMenuManager.WeaponType.Sword)
        {
            SwingSword(lookDir, angle);

            nextAttackTime = Time.time + swordCooldown;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySwordSound();
            }
        }
    }

    void ShootPistol(Vector2 dir, float angle)
    {
        if (pistolBulletPrefab == null)
        {
            Debug.LogError("[PlayerAttack] Chưa kéo Pistol Bullet Prefab.");
            return;
        }

        GameObject bullet = Instantiate(
            pistolBulletPrefab,
            transform.position,
            Quaternion.Euler(0, 0, angle)
        );

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.AddForce(dir.normalized * pistolForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogError("[PlayerAttack] Pistol bullet thiếu Rigidbody2D.");
        }
    }

    void ShootShotgun(Vector2 dir, float baseAngle)
    {
        if (shotgunBulletPrefab == null)
        {
            Debug.LogError("[PlayerAttack] Chưa kéo Shotgun Bullet Prefab.");
            return;
        }

        if (pelletsCount <= 0)
        {
            pelletsCount = 1;
        }

        if (pelletsCount == 1)
        {
            SpawnShotgunPellet(baseAngle);
            return;
        }

        float angleStep = spreadAngle / (pelletsCount - 1);
        float startAngle = baseAngle - spreadAngle / 2f;

        for (int i = 0; i < pelletsCount; i++)
        {
            float pelletAngle = startAngle + angleStep * i;
            SpawnShotgunPellet(pelletAngle);
        }
    }

    void SpawnShotgunPellet(float angle)
    {
        GameObject pellet = Instantiate(
            shotgunBulletPrefab,
            transform.position,
            Quaternion.Euler(0, 0, angle)
        );

        Vector2 pelletDir = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        Rigidbody2D rb = pellet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.AddForce(pelletDir.normalized * shotgunForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogError("[PlayerAttack] Shotgun bullet thiếu Rigidbody2D.");
        }
    }

    void SwingSword(Vector2 dir, float angle)
    {
        if (swordSlashPrefab == null)
        {
            Debug.LogError("[PlayerAttack] Chưa kéo Sword Slash Prefab.");
            return;
        }

        Vector3 spawnPos = transform.position + (Vector3)(dir.normalized * swordRange);

        Instantiate(
            swordSlashPrefab,
            spawnPos,
            Quaternion.Euler(0, 0, angle)
        );
    }

    public int GetPistolCurrentAmmo()
    {
        return pistolCurrentAmmo;
    }

    public int GetPistolMaxAmmo()
    {
        return pistolMaxAmmo;
    }

    public int GetShotgunCurrentAmmo()
    {
        return shotgunCurrentAmmo;
    }

    public int GetShotgunMaxAmmo()
    {
        return shotgunMaxAmmo;
    }

    public bool IsReloading()
    {
        return isReloading;
    }
}