# 🔫 SILVER BULLET: The Survival Chronicles
### Vibe Coding Master Document — Unity 6 + Visual Studio
> **CMU-CS 447 | Group 7 | Duy Tan University**  
> Version 1.1 — Compiled from Proposal + Design Document + Code Monkey Top Down Shooter Reference  
> Platform: PC Windows | Engine: Unity 6 (URP 2D)

---

## 📋 TABLE OF CONTENTS
1. [Project Overview](#1-project-overview)
2. [Unity Project Setup](#2-unity-project-setup)
3. [Scene Structure](#3-scene-structure)
4. [ScriptableObject Data Design](#4-scriptableobject-data-design)
5. [Script Map — Complete List](#5-script-map--complete-list)
6. [System 1: GameManager](#6-system-1-gamemanager)
7. [System 2: PlayerController](#7-system-2-playercontroller)
8. [System 3: WeaponSystem](#8-system-3-weaponsystem)
9. [System 4: BulletPool (Object Pooling)](#9-system-4-bulletpool-object-pooling)
10. [System 5: EnemyAI](#10-system-5-enemyai)
11. [System 6: EnemySpawner](#11-system-6-enemyspawner)
12. [System 7: BossController](#12-system-7-bosscontroller)
13. [System 8: LevelSystem](#13-system-8-levelsystem)
14. [System 9: ItemSystem](#14-system-9-itemsystem)
15. [System 10: UIManager](#15-system-10-uimanager)
16. [System 11: AudioManager](#16-system-11-audiomanager)
17. [System 12: CameraFollow](#17-system-12-camerafollow)
18. [Combat Formulas](#18-combat-formulas)
19. [Gameplay Numbers Reference](#19-gameplay-numbers-reference)
20. [Physics Layers & Tags](#20-physics-layers--tags)
21. [Build Checklist](#21-build-checklist)

---

## 1. PROJECT OVERVIEW

**Genre:** 2D Top-Down Roguelite Shooter  
**Platform:** PC (Windows), Unity 6, Simply 2D mode  
**Core Loop:** Kill Enemy → Gain EXP → Level Up → Choose Upgrade → Boss → Repeat → Game Over  

### What Makes Silver Bullet Different
| Problem in other games | Silver Bullet's solution |
|---|---|
| No manual aiming (Vampire Survivors) | Mouse-aimed shooting |
| No dodge mechanic | Dash with i-frames (3s cooldown) |
| Timer-based pacing (Brotato) | Kill-count pacing — player controls speed |
| Weapon chosen mid-run (Soul Knight) | Weapon locked in at Start Screen — builds identity |
| Complex upgrade trees | Only 3 upgrade choices: ATK / DEF / HP |

### Weapon Identity Summary
| Weapon | Damage | Fire Rate | Range | Special Effect | Playstyle |
|---|---|---|---|---|---|
| Pistol | Low | Fast | Far | Pierce (bullets pass through enemies) | Spam, kite |
| Shotgun | High | Slow | Near-Mid | Burn (2% HP/s for 10s) | Burst, close range |
| Sword | Very High | Medium | Melee | Lifesteal (heal % of damage dealt) | High risk, high reward |

---

## 2. UNITY PROJECT SETUP

### 2.1 Create Project
```
Unity Hub → New Project → Universal 2D (URP) → Name: SilverBullet
```

### 2.2 Required Packages (Package Manager)
```
- Cinemachine           (camera follow)
- 2D Tilemap Extras     (map design)
- TextMeshPro           (UI text)
- Input System          (new input — optional, or use legacy Input)
```

### 2.3 Folder Structure
```
Assets/
├── _Scripts/
│   ├── Core/           (GameManager, AudioManager, ObjectPool)
│   ├── Player/         (PlayerController, HealthSystem, DashController)
│   ├── Weapons/        (WeaponBase, PistolWeapon, ShotgunWeapon, SwordWeapon, Bullet)
│   ├── Enemies/        (EnemyBase, MeleeEnemy, RangedEnemy, BossController, EnemySpawner)
│   ├── Systems/        (LevelSystem, ItemSystem, ItemPickup)
│   └── UI/             (UIManager, UpgradePanel, WeaponSelectUI, GameOverUI)
├── _Data/              (ScriptableObjects: WeaponData, EnemyData, ItemData)
├── _Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── Bullets/
│   ├── Items/
│   └── UI/
├── _Sprites/
├── _Animations/
├── _Audio/
├── _Tilemaps/
└── _Scenes/
    ├── MainMenu
    └── GameScene
```

### 2.4 Physics Layer Setup
Go to **Edit → Project Settings → Physics 2D → Layer Collision Matrix**

| Layer Name | Layer Index |
|---|---|
| Default | 0 |
| Player | 6 |
| Enemy | 7 |
| PlayerBullet | 8 |
| EnemyBullet | 9 |
| Item | 10 |
| Wall | 11 |

**Collision Matrix Rules:**
- PlayerBullet hits: Enemy, Wall (NOT Player, NOT PlayerBullet)
- EnemyBullet hits: Player, Wall (NOT Enemy, NOT EnemyBullet)
- Player hits: Enemy, Wall, Item
- Enemy hits: Wall (enemies collide with walls, not each other for performance)

### 2.5 Tags
Create these tags in **Edit → Project Settings → Tags and Layers:**
```
Player
Enemy
Boss
Bullet
Item
Wall
```

---

## 3. SCENE STRUCTURE

### Scene 1: MainMenu
```
Canvas (Screen Space - Overlay)
├── WeaponSelectPanel
│   ├── Title Text (TMP)
│   ├── PistolButton
│   ├── ShotgunButton
│   ├── SwordButton
│   ├── WeaponInfoPanel
│   │   ├── WeaponNameText
│   │   ├── DamageText
│   │   ├── SpeedText
│   │   ├── RangeText
│   │   └── EffectText
│   └── StartButton
```

### Scene 2: GameScene
```
── [GameManager]        (singleton, no destroy)
── [AudioManager]       (singleton, no destroy)
── [ObjectPool]         (bullet pool, enemy pool)
── [EnemySpawner]
── [LevelSystem]
── [ItemSystem]
│
── Player               (tag: Player, layer: Player)
│   ├── Sprite
│   ├── Rigidbody2D
│   ├── CircleCollider2D
│   ├── PlayerController.cs
│   ├── HealthSystem.cs
│   ├── DashController.cs
│   └── WeaponHolder
│       └── [Active Weapon Prefab]
│
── Enemies (parent empty object)
│
── Items (parent empty object)
│
── Tilemap (ground layer)
│
── WallTilemap (collision layer, layer: Wall)
│
── VirtualCamera (Cinemachine)
│   └── Follow: Player transform
│
── Canvas (Screen Space - Camera)
│   ├── HUD
│   │   ├── HPBar (Slider)
│   │   ├── EXPBar (Slider)
│   │   ├── LevelText (TMP)
│   │   ├── KillCountText (TMP)
│   │   └── DashCooldownIcon
│   ├── UpgradePanel (hidden by default)
│   │   ├── UpgradeOption1Button
│   │   ├── UpgradeOption2Button
│   │   └── UpgradeOption3Button
│   └── GameOverPanel (hidden by default)
│       ├── KillsResultText
│       ├── LevelResultText
│       ├── RestartButton
│       └── MainMenuButton
```

---

## 4. SCRIPTABLEOBJECT DATA DESIGN

All data lives in ScriptableObjects under `Assets/_Data/`. Adding a new weapon = create a new asset, zero code changes.

### 4.1 WeaponData.cs
```csharp
[CreateAssetMenu(fileName = "WeaponData", menuName = "SilverBullet/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public Sprite weaponIcon;
    public WeaponType weaponType; // enum: Pistol, Shotgun, Sword

    [Header("Stats")]
    public float baseDamage;
    public float fireRate;        // shots per second
    public float bulletSpeed;     // for ranged weapons
    public float range;           // for sword hitbox radius

    [Header("Shotgun Only")]
    public int pelletCount = 1;   // 5 for shotgun
    public float spreadAngle = 0f;

    [Header("Special Effect")]
    public ItemEffectType specialEffect; // Pierce / Burn / Lifesteal
    public string description;
}

public enum WeaponType { Pistol, Shotgun, Sword }
```

### 4.2 EnemyData.cs
```csharp
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
```

### 4.3 ItemData.cs
```csharp
[CreateAssetMenu(fileName = "ItemData", menuName = "SilverBullet/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemEffectType effectType;
    public float effectValue;     // 0.15 for 15% heal, 5f for 5s shield, etc.
    public float duration;        // 0 = instant
    public float dropChance;      // 0.0 to 1.0
}

public enum ItemEffectType
{
    Heal,
    Shield,
    Burn,
    Pierce,
    Lifesteal,
    None
}
```

---

## 5. SCRIPT MAP — COMPLETE LIST

| # | Script | Location | Responsibility |
|---|---|---|---|
| 1 | `GameManager.cs` | Core/ | Game state machine. Singleton. |
| 2 | `AudioManager.cs` | Core/ | Sound effects. Singleton. |
| 3 | `ObjectPool.cs` | Core/ | Generic pool for bullets & enemies. |
| 4 | `PlayerController.cs` | Player/ | WASD move, mouse aim, shoot input. |
| 5 | `HealthSystem.cs` | Player/ | HP tracking, damage, death event. |
| 6 | `DashController.cs` | Player/ | Dash movement + i-frames. |
| 7 | `WeaponBase.cs` | Weapons/ | Abstract base for all weapons. |
| 8 | `PistolWeapon.cs` | Weapons/ | Pistol logic: single bullet, pierce toggle. |
| 9 | `ShotgunWeapon.cs` | Weapons/ | Shotgun logic: pellet spread. |
| 10 | `SwordWeapon.cs` | Weapons/ | Melee hitbox swing, lifesteal. |
| 11 | `Bullet.cs` | Weapons/ | Bullet movement, collision, pierce logic. |
| 12 | `EnemyBase.cs` | Enemies/ | Abstract enemy: HP, take damage, die. |
| 13 | `MeleeEnemy.cs` | Enemies/ | Chase → attack state machine. |
| 14 | `RangedEnemy.cs` | Enemies/ | Keep distance → shoot state machine. |
| 15 | `EnemySpawner.cs` | Enemies/ | Spawn waves, scale difficulty. |
| 16 | `BossController.cs` | Enemies/ | Boss HP + 3 skill state machine. |
| 17 | `BossBullet.cs` | Enemies/ | Boss-specific bullet behaviors. |
| 18 | `LevelSystem.cs` | Systems/ | EXP tracking, level-up trigger. |
| 19 | `UpgradeManager.cs` | Systems/ | Random upgrade selection, apply stats. |
| 20 | `ItemPickup.cs` | Systems/ | Trigger pickup, apply item effect. |
| 21 | `ItemSpawner.cs` | Systems/ | Roll drop chance on enemy death. |
| 22 | `UIManager.cs` | UI/ | Update all HUD elements. |
| 23 | `WeaponSelectUI.cs` | UI/ | Start screen weapon selection. |
| 24 | `UpgradePanel.cs` | UI/ | Show/hide upgrade choices. |
| 25 | `GameOverUI.cs` | UI/ | Show stats on game over. |
| 26 | `DamagePopup.cs` | UI/ | Floating damage numbers (Code Monkey style). |
| 27 | `CameraFollow.cs` | Core/ | Cinemachine camera follow setup. |

---

## 6. SYSTEM 1: GameManager

**Pattern:** Singleton. Never destroyed. Owns game state.

```csharp
// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, LevelUp, BossSpawned, GameOver }
    public GameState CurrentState { get; private set; }

    [HideInInspector] public int killCount = 0;
    [HideInInspector] public int bossKillThreshold; // set on Start, random 100-200

    // Events other systems subscribe to
    public event System.Action<int> OnKillCountChanged;
    public event System.Action OnBossThresholdReached;
    public event System.Action OnGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        bossKillThreshold = Random.Range(100, 201); // 100 to 200 inclusive
        SetState(GameState.Playing);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        switch (newState)
        {
            case GameState.LevelUp:
                Time.timeScale = 0f; // pause game during level up
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                OnGameOver?.Invoke();
                break;
        }
    }

    public void RegisterKill()
    {
        if (CurrentState != GameState.Playing) return;
        killCount++;
        OnKillCountChanged?.Invoke(killCount);

        if (killCount >= bossKillThreshold && CurrentState != GameState.BossSpawned)
        {
            SetState(GameState.BossSpawned);
            OnBossThresholdReached?.Invoke();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
```

---

## 7. SYSTEM 2: PlayerController

**Inspiration:** Code Monkey — *Aim at Mouse in Unity 2D*, *How to Make Simple Character Dodge Roll*

```csharp
// PlayerController.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("References")]
    public WeaponBase currentWeapon;
    public DashController dashController;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera mainCam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
    }

    private void Update()
    {
        // Input
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Aim at mouse (Code Monkey technique)
        AimAtMouse();

        // Shoot
        if (Input.GetMouseButton(0) && currentWeapon != null)
            currentWeapon.TryShoot();

        // Dash
        if (Input.GetKeyDown(KeyCode.Space))
            dashController.TryDash(moveInput);
    }

    private void FixedUpdate()
    {
        // Don't move during dash — DashController handles movement
        if (!dashController.IsDashing)
            rb.linearVelocity = moveInput * moveSpeed;
    }

    private void AimAtMouse()
    {
        // Convert mouse screen position to world position
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        // Rotate player to face mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip sprite based on facing direction (optional)
        // GetComponent<SpriteRenderer>().flipY = direction.x < 0;
    }
}
```

### DashController.cs
**Inspiration:** Code Monkey — *How to Make Simple Character Dodge Roll*

```csharp
// DashController.cs
using System.Collections;
using UnityEngine;

public class DashController : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 3f;

    public bool IsDashing { get; private set; }
    public float CooldownRemaining { get; private set; }

    private Rigidbody2D rb;
    private HealthSystem healthSystem;
    private bool canDash = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void Update()
    {
        if (CooldownRemaining > 0)
            CooldownRemaining -= Time.deltaTime;
    }

    public void TryDash(Vector2 direction)
    {
        if (!canDash) return;
        if (direction == Vector2.zero) direction = transform.right; // dash forward if no input
        StartCoroutine(DashRoutine(direction));
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        canDash = false;
        IsDashing = true;
        CooldownRemaining = dashCooldown;

        // Grant i-frames
        healthSystem.SetInvincible(true);

        rb.linearVelocity = direction * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        IsDashing = false;
        healthSystem.SetInvincible(false);
        rb.linearVelocity = Vector2.zero;

        // Wait out rest of cooldown
        yield return new WaitForSeconds(dashCooldown - dashDuration);
        canDash = true;
    }
}
```

### HealthSystem.cs
**Inspiration:** Code Monkey — *How to make a Health System*

```csharp
// HealthSystem.cs
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 100f;
    public float currentHP { get; private set; }

    // Player stat modifiers (set by UpgradeManager)
    [HideInInspector] public float atkMultiplier = 1f;   // 1.0 = no bonus
    [HideInInspector] public float defMultiplier = 0f;   // 0 = 0% damage reduction

    private bool isInvincible = false;

    public event System.Action<float, float> OnHPChanged; // (current, max)
    public event System.Action OnDeath;

    private void Awake() => currentHP = maxHP;

    public void TakeDamage(float rawDamage)
    {
        if (isInvincible) return;

        // Formula: DamageTaken = Damage × (1 - DEF%)
        float actualDamage = rawDamage * (1f - defMultiplier);
        currentHP = Mathf.Max(0, currentHP - actualDamage);

        // Floating damage number (optional visual)
        DamagePopup.Create(transform.position, (int)actualDamage);

        OnHPChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0) Die();
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public void HealPercent(float percent) => Heal(maxHP * percent);

    public void SetInvincible(bool state) => isInvincible = state;

    public void IncreaseMaxHP(float amount)
    {
        maxHP += amount;
        currentHP += amount;
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        if (CompareTag("Player"))
            GameManager.Instance.SetState(GameManager.GameState.GameOver);
        else
            Destroy(gameObject);
    }
}
```

---

## 8. SYSTEM 3: WeaponSystem

### WeaponBase.cs (Abstract)
```csharp
// WeaponBase.cs
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData data;
    protected float nextFireTime = 0f;
    protected HealthSystem playerHealth; // needed for lifesteal

    protected virtual void Awake()
    {
        playerHealth = GetComponentInParent<HealthSystem>();
    }

    /// <summary>Called each frame when fire button is held.</summary>
    public void TryShoot()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + (1f / data.fireRate);
            Shoot();
            AudioManager.Instance.PlayShoot(data.weaponType);
        }
    }

    protected abstract void Shoot();

    protected Vector2 GetAimDirection()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return (mouseWorld - transform.parent.position).normalized;
    }
}
```

### PistolWeapon.cs
```csharp
// PistolWeapon.cs
using UnityEngine;

public class PistolWeapon : WeaponBase
{
    private bool pierceActive = false;
    private float pierceTimer = 0f;

    private void Update()
    {
        if (pierceActive)
        {
            pierceTimer -= Time.deltaTime;
            if (pierceTimer <= 0) pierceActive = false;
        }
    }

    protected override void Shoot()
    {
        Vector2 dir = GetAimDirection();
        Bullet bullet = ObjectPool.Instance.GetBullet();
        bullet.Init(
            transform.parent.position,
            dir,
            data.baseDamage * playerHealth.atkMultiplier,
            data.bulletSpeed,
            pierce: pierceActive,
            isPlayerBullet: true
        );
    }

    public void ActivatePierce(float duration)
    {
        pierceActive = true;
        pierceTimer = duration;
    }
}
```

### ShotgunWeapon.cs
```csharp
// ShotgunWeapon.cs
using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    private bool burnActive = false;
    private float burnTimer = 0f;

    private void Update()
    {
        if (burnActive) { burnTimer -= Time.deltaTime; if (burnTimer <= 0) burnActive = false; }
    }

    protected override void Shoot()
    {
        Vector2 baseDir = GetAimDirection();
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < data.pelletCount; i++)
        {
            float spread = Random.Range(-data.spreadAngle / 2f, data.spreadAngle / 2f);
            float pelletAngle = (baseAngle + spread) * Mathf.Deg2Rad;
            Vector2 pelletDir = new Vector2(Mathf.Cos(pelletAngle), Mathf.Sin(pelletAngle));

            Bullet bullet = ObjectPool.Instance.GetBullet();
            bullet.Init(
                transform.parent.position,
                pelletDir,
                data.baseDamage * playerHealth.atkMultiplier,
                data.bulletSpeed,
                pierce: false,
                isPlayerBullet: true,
                applyBurn: burnActive
            );
        }
    }

    public void ActivateBurn(float duration) { burnActive = true; burnTimer = duration; }
}
```

### SwordWeapon.cs
```csharp
// SwordWeapon.cs
using UnityEngine;

public class SwordWeapon : WeaponBase
{
    public CircleCollider2D swordHitbox;
    public float lifestealPercent = 0f; // 0 = no lifesteal, set when item active

    protected override void Shoot()
    {
        // "Shoot" means swing for sword
        StartCoroutine(SwingRoutine());
    }

    private System.Collections.IEnumerator SwingRoutine()
    {
        swordHitbox.enabled = true;
        yield return new WaitForSeconds(0.15f); // hitbox active window
        swordHitbox.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!swordHitbox.enabled) return;
        if (!other.CompareTag("Enemy")) return;

        float damage = data.baseDamage * playerHealth.atkMultiplier;
        other.GetComponent<HealthSystem>()?.TakeDamage(damage);

        // Lifesteal
        if (lifestealPercent > 0)
            playerHealth.Heal(damage * lifestealPercent);

        AudioManager.Instance.PlayHit();
    }
}
```

---

## 9. SYSTEM 4: BulletPool (Object Pooling)

**Why:** Instantiate/Destroy every bullet = GC spikes = frame drops. Pool recycles objects.  
**Inspiration:** Code Monkey — *How to make Bullet Tracer Rounds* + performance section of design doc.

```csharp
// ObjectPool.cs
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [Header("Bullet Pool")]
    public GameObject bulletPrefab;
    public int bulletPoolSize = 100;

    [Header("Enemy Pool")]
    public GameObject meleeEnemyPrefab;
    public GameObject rangedEnemyPrefab;
    public int enemyPoolSize = 40;

    private Queue<Bullet> bulletPool = new Queue<Bullet>();

    private void Awake()
    {
        Instance = this;
        // Pre-warm bullet pool
        for (int i = 0; i < bulletPoolSize; i++)
        {
            var obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            bulletPool.Enqueue(obj.GetComponent<Bullet>());
        }
    }

    public Bullet GetBullet()
    {
        if (bulletPool.Count > 0)
        {
            var bullet = bulletPool.Dequeue();
            bullet.gameObject.SetActive(true);
            return bullet;
        }
        // Expand pool if exhausted
        var newObj = Instantiate(bulletPrefab);
        return newObj.GetComponent<Bullet>();
    }

    public void ReturnBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}
```

### Bullet.cs
```csharp
// Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float damage;
    private float speed;
    private bool pierce;
    private bool isPlayerBullet;
    private bool applyBurn;
    private Rigidbody2D rb;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Init(Vector2 startPos, Vector2 direction, float dmg, float spd,
                     bool pierce = false, bool isPlayerBullet = true, bool applyBurn = false)
    {
        transform.position = startPos;
        this.damage = dmg;
        this.speed = spd;
        this.pierce = pierce;
        this.isPlayerBullet = isPlayerBullet;
        this.applyBurn = applyBurn;

        rb.linearVelocity = direction * speed;

        // Auto-return after 3 seconds if no collision
        Invoke(nameof(ReturnToPool), 3f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<HealthSystem>();
            if (health != null) health.TakeDamage(damage);

            if (applyBurn)
                other.GetComponent<EnemyBase>()?.ApplyBurn(damage * 0.02f, 10f);

            if (!pierce) ReturnToPool();
        }
        else if (!isPlayerBullet && other.CompareTag("Player"))
        {
            other.GetComponent<HealthSystem>()?.TakeDamage(damage);
            ReturnToPool();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        CancelInvoke();
        rb.linearVelocity = Vector2.zero;
        ObjectPool.Instance.ReturnBullet(this);
    }
}
```

---

## 10. SYSTEM 5: EnemyAI

**Inspiration:** Code Monkey — *A\* Pathfinding in Unity*, *Flying Body*

### EnemyBase.cs (Abstract)
```csharp
// EnemyBase.cs
using UnityEngine;

[RequireComponent(typeof(HealthSystem), typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour
{
    public EnemyData data;
    protected Transform player;
    protected HealthSystem health;
    protected Rigidbody2D rb;

    // Burn DOT state
    private bool burning = false;
    private float burnDmgPerSec;
    private float burnTimer;

    protected virtual void Awake()
    {
        health = GetComponent<HealthSystem>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Initialize health from data
        health.maxHP = data.maxHP;
    }

    protected virtual void Start()
    {
        health.OnDeath += OnDeath;
    }

    protected virtual void Update()
    {
        HandleBurn();
        if (player == null) return;
        AIUpdate();
    }

    protected abstract void AIUpdate();

    protected float DistanceToPlayer() => Vector2.Distance(transform.position, player.position);

    protected Vector2 DirectionToPlayer() => (player.position - transform.position).normalized;

    public void ApplyBurn(float dmgPerSec, float duration)
    {
        burning = true;
        burnDmgPerSec = dmgPerSec;
        burnTimer = duration;
    }

    private void HandleBurn()
    {
        if (!burning) return;
        burnTimer -= Time.deltaTime;
        health.TakeDamage(burnDmgPerSec * Time.deltaTime);
        if (burnTimer <= 0) burning = false;
    }

    protected virtual void OnDeath()
    {
        GameManager.Instance.RegisterKill();
        LevelSystem.Instance.AddEXP(data.expReward);
        ItemSpawner.Instance.TrySpawnItem(transform.position);
        // Destroy or return to pool
        Destroy(gameObject);
    }
}
```

### MeleeEnemy.cs
```csharp
// MeleeEnemy.cs
using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    private enum State { Idle, Chase, Attack }
    private State state = State.Idle;
    private float attackTimer = 0f;

    protected override void AIUpdate()
    {
        float dist = DistanceToPlayer();

        switch (state)
        {
            case State.Idle:
                if (dist <= data.detectionRange) state = State.Chase;
                break;

            case State.Chase:
                rb.linearVelocity = DirectionToPlayer() * data.moveSpeed;
                if (dist <= data.attackRange) state = State.Attack;
                break;

            case State.Attack:
                rb.linearVelocity = Vector2.zero;
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    attackTimer = data.attackCooldown;
                    player.GetComponent<HealthSystem>()?.TakeDamage(data.damage);
                    AudioManager.Instance.PlayHit();
                }
                if (dist > data.attackRange * 1.2f) state = State.Chase;
                break;
        }
    }
}
```

### RangedEnemy.cs
```csharp
// RangedEnemy.cs
using UnityEngine;

public class RangedEnemy : EnemyBase
{
    private enum State { Idle, MaintainDistance, Shoot }
    private State state = State.Idle;
    private float shootTimer = 0f;

    protected override void AIUpdate()
    {
        float dist = DistanceToPlayer();

        switch (state)
        {
            case State.Idle:
                if (dist <= data.detectionRange) state = State.MaintainDistance;
                break;

            case State.MaintainDistance:
                // Move away if too close, toward if too far
                if (dist < data.preferredDistance - 1f)
                    rb.linearVelocity = -DirectionToPlayer() * data.moveSpeed;
                else if (dist > data.preferredDistance + 1f)
                    rb.linearVelocity = DirectionToPlayer() * data.moveSpeed;
                else
                {
                    rb.linearVelocity = Vector2.zero;
                    state = State.Shoot;
                }
                break;

            case State.Shoot:
                shootTimer -= Time.deltaTime;
                if (shootTimer <= 0)
                {
                    shootTimer = data.attackCooldown;
                    FireAtPlayer();
                }
                // Reposition if player gets too close
                if (dist < data.preferredDistance - 2f) state = State.MaintainDistance;
                break;
        }
    }

    private void FireAtPlayer()
    {
        Bullet bullet = ObjectPool.Instance.GetBullet();
        bullet.Init(transform.position, DirectionToPlayer(), data.damage, data.bulletSpeed,
                    pierce: false, isPlayerBullet: false);
        AudioManager.Instance.PlayEnemyShoot();
    }
}
```

---

## 11. SYSTEM 6: EnemySpawner

```csharp
// EnemySpawner.cs
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject meleeEnemyPrefab;
    public GameObject rangedEnemyPrefab;

    [Header("Spawn Settings")]
    public float spawnRadius = 12f;     // spawn outside screen
    public float baseSpawnInterval = 2f;
    public int baseEnemiesPerWave = 3;

    private bool bossActive = false;
    private Coroutine spawnRoutine;

    private void Start()
    {
        GameManager.Instance.OnBossThresholdReached += StopSpawning;
        GameManager.Instance.OnGameOver += StopSpawning;
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (!bossActive)
        {
            int killCount = GameManager.Instance.killCount;

            // Scale difficulty: more enemies, shorter interval as kill count rises
            int enemiesToSpawn = baseEnemiesPerWave + killCount / 20;
            float interval = Mathf.Max(0.5f, baseSpawnInterval - killCount * 0.005f);

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnEnemy()
    {
        // Spawn at random position around player, outside camera view
        Vector2 spawnPos = GetSpawnPosition();

        // 70% melee, 30% ranged
        bool isMelee = Random.value < 0.7f;
        Instantiate(isMelee ? meleeEnemyPrefab : rangedEnemyPrefab, spawnPos, Quaternion.identity);
    }

    private Vector2 GetSpawnPosition()
    {
        Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return new Vector2(playerPos.x + Mathf.Cos(angle) * spawnRadius,
                           playerPos.y + Mathf.Sin(angle) * spawnRadius);
    }

    private void StopSpawning()
    {
        bossActive = true;
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
    }
}
```

---

## 12. SYSTEM 7: BossController

**Boss AI — 3 Skills:**
1. **Circular Shot** — 360° bullet spread
2. **Aim Shot** — 5s telegraph then precision shot
3. **Dash Attack** — charge at player (stretch goal)

```csharp
// BossController.cs
using System.Collections;
using UnityEngine;

public class BossController : EnemyBase
{
    [Header("Boss Skills")]
    public int circularShotCount = 16;    // bullets in 360°
    public float aimShotTelegraphTime = 5f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.5f;

    public GameObject bossBulletPrefab;
    public GameObject telegraphIndicatorPrefab;

    private enum BossPhase { EnterArena, Skill1_Circular, Skill2_Aim, Skill3_Dash, Cooldown }
    private BossPhase currentPhase;
    private bool isActing = false;

    protected override void Start()
    {
        base.Start();
        AudioManager.Instance.PlayBossAppear();
        StartCoroutine(BossAIRoutine());
    }

    protected override void AIUpdate()
    {
        // AI is driven by the coroutine, not Update loop
        // Just face the player
        Vector2 dir = DirectionToPlayer();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private IEnumerator BossAIRoutine()
    {
        yield return new WaitForSeconds(2f); // enter arena pause

        while (true)
        {
            // Alternate between skills
            yield return StartCoroutine(DoCircularShot());
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(DoAimShot());
            yield return new WaitForSeconds(2f);
            // Uncomment when implementing skill 3:
            // yield return StartCoroutine(DoDashAttack());
            // yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator DoCircularShot()
    {
        float angleStep = 360f / circularShotCount;
        for (int i = 0; i < circularShotCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnBossBullet(dir, data.damage);
        }
        AudioManager.Instance.PlayBossShoot();
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator DoAimShot()
    {
        // Show telegraph
        Vector3 playerPos = player.position;
        var telegraph = Instantiate(telegraphIndicatorPrefab, playerPos, Quaternion.identity);

        yield return new WaitForSeconds(aimShotTelegraphTime); // 5 second warning

        Destroy(telegraph);

        // Fire at ORIGINAL position (player can dodge)
        Vector2 dir = ((Vector3)playerPos - transform.position).normalized;
        SpawnBossBullet(dir, data.damage * 3f);
        AudioManager.Instance.PlayBossShoot();

        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator DoDashAttack()
    {
        Vector2 targetDir = DirectionToPlayer();
        float timer = dashDuration;
        health.SetInvincible(true); // boss immune during dash

        while (timer > 0)
        {
            rb.linearVelocity = targetDir * dashSpeed;
            timer -= Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        health.SetInvincible(false);
    }

    private void SpawnBossBullet(Vector2 direction, float dmg)
    {
        Bullet bullet = ObjectPool.Instance.GetBullet();
        bullet.Init(transform.position, direction, dmg, 6f, pierce: false, isPlayerBullet: false);
    }
}
```

---

## 13. SYSTEM 8: LevelSystem

```csharp
// LevelSystem.cs
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem Instance { get; private set; }

    public int currentLevel { get; private set; } = 1;
    public float currentEXP { get; private set; } = 0f;
    public float expToNextLevel => 10f * Mathf.Pow(currentLevel, 1.5f); // Formula: 10 × level^1.5

    public event System.Action<int> OnLevelUp;
    public event System.Action<float, float> OnEXPChanged; // (current, max)

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddEXP(float amount)
    {
        currentEXP += amount;
        OnEXPChanged?.Invoke(currentEXP, expToNextLevel);

        while (currentEXP >= expToNextLevel)
        {
            currentEXP -= expToNextLevel;
            currentLevel++;
            OnLevelUp?.Invoke(currentLevel);
            GameManager.Instance.SetState(GameManager.GameState.LevelUp);
            UIManager.Instance.ShowUpgradePanel();
            AudioManager.Instance.PlayLevelUp();
        }
    }
}
```

### UpgradeManager.cs
```csharp
// UpgradeManager.cs
using UnityEngine;

public enum UpgradeType { ATK, DEF, HP }

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Upgrade Values")]
    public float atkBonus = 0.10f;   // +10% damage
    public float defBonus = 0.10f;   // -10% damage taken
    public float hpBonus = 20f;      // +20 max HP

    private HealthSystem playerHealth;

    private void Awake()
    {
        Instance = this;
        playerHealth = FindObjectOfType<PlayerController>().GetComponent<HealthSystem>();
    }

    /// <summary>Apply the chosen upgrade and resume game.</summary>
    public void ApplyUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.ATK:
                playerHealth.atkMultiplier += atkBonus;
                break;
            case UpgradeType.DEF:
                playerHealth.defMultiplier = Mathf.Min(0.8f, playerHealth.defMultiplier + defBonus); // cap at 80%
                break;
            case UpgradeType.HP:
                playerHealth.IncreaseMaxHP(hpBonus);
                break;
        }
        GameManager.Instance.SetState(GameManager.GameState.Playing);
        UIManager.Instance.HideUpgradePanel();
    }

    /// <summary>Get 3 random non-duplicate upgrade choices.</summary>
    public UpgradeType[] GetRandomUpgrades()
    {
        UpgradeType[] all = { UpgradeType.ATK, UpgradeType.DEF, UpgradeType.HP };
        // Shuffle
        for (int i = all.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (all[i], all[j]) = (all[j], all[i]);
        }
        return all; // all 3 are always different
    }
}
```

---

## 14. SYSTEM 9: ItemSystem

### ItemSpawner.cs
```csharp
// ItemSpawner.cs
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner Instance { get; private set; }

    public ItemData[] allItems; // assign in Inspector: Heal, Shield, Burn, Pierce, Lifesteal
    public GameObject itemPickupPrefab;

    private void Awake() => Instance = this;

    public void TrySpawnItem(Vector2 position)
    {
        foreach (var item in allItems)
        {
            if (Random.value < item.dropChance)
            {
                var obj = Instantiate(itemPickupPrefab, position, Quaternion.identity);
                obj.GetComponent<ItemPickup>().itemData = item;
                break; // only drop one item per enemy
            }
        }
    }
}
```

### ItemPickup.cs
```csharp
// ItemPickup.cs
using System.Collections;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponent<HealthSystem>();
        var player = other.GetComponent<PlayerController>();

        switch (itemData.effectType)
        {
            case ItemEffectType.Heal:
                health.HealPercent(itemData.effectValue); // effectValue = 0.15
                break;

            case ItemEffectType.Shield:
                StartCoroutine(ApplyShield(health, itemData.duration));
                break;

            case ItemEffectType.Pierce:
                if (player.currentWeapon is PistolWeapon p)
                    p.ActivatePierce(itemData.duration); // only activates with Pistol
                break;

            case ItemEffectType.Burn:
                if (player.currentWeapon is ShotgunWeapon sg)
                    sg.ActivateBurn(itemData.duration); // only activates with Shotgun
                break;

            case ItemEffectType.Lifesteal:
                if (player.currentWeapon is SwordWeapon sw)
                    sw.lifestealPercent = itemData.effectValue; // only activates with Sword
                StartCoroutine(RemoveLifesteal(player, itemData.duration));
                break;
        }

        AudioManager.Instance.PlayPickup();
        Destroy(gameObject);
    }

    private IEnumerator ApplyShield(HealthSystem health, float duration)
    {
        health.SetInvincible(true);
        // TODO: show shield VFX
        yield return new WaitForSeconds(duration);
        health.SetInvincible(false);
    }

    private IEnumerator RemoveLifesteal(PlayerController player, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (player.currentWeapon is SwordWeapon sw)
            sw.lifestealPercent = 0f;
    }
}
```

---

## 15. SYSTEM 10: UIManager

**Inspiration:** Code Monkey — *How to make Damage Popup Text*, *Scene Manager in Unity*

```csharp
// UIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public Slider hpBar;
    public Slider expBar;
    public TMP_Text levelText;
    public TMP_Text killCountText;
    public Image dashCooldownFill;

    [Header("Panels")]
    public GameObject upgradePanel;
    public UpgradePanel upgradePanelScript;
    public GameObject gameOverPanel;
    public TMP_Text gameOverKillsText;
    public TMP_Text gameOverLevelText;

    private DashController playerDash;

    private void Awake()
    {
        Instance = this;
        upgradePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        // Subscribe to events
        var health = FindObjectOfType<HealthSystem>();
        health.OnHPChanged += UpdateHP;

        LevelSystem.Instance.OnEXPChanged += UpdateEXP;
        LevelSystem.Instance.OnLevelUp += UpdateLevel;

        GameManager.Instance.OnKillCountChanged += UpdateKillCount;
        GameManager.Instance.OnGameOver += ShowGameOver;

        playerDash = FindObjectOfType<DashController>();
    }

    private void Update()
    {
        // Update dash cooldown icon
        if (playerDash != null && dashCooldownFill != null)
            dashCooldownFill.fillAmount = 1f - (playerDash.CooldownRemaining / 3f);
    }

    private void UpdateHP(float current, float max) => hpBar.value = current / max;
    private void UpdateEXP(float current, float max) => expBar.value = current / max;
    private void UpdateLevel(int level) => levelText.text = $"Lv.{level}";
    private void UpdateKillCount(int count) => killCountText.text = $"Kills: {count}";

    public void ShowUpgradePanel()
    {
        upgradePanel.SetActive(true);
        upgradePanelScript.Refresh(UpgradeManager.Instance.GetRandomUpgrades());
    }

    public void HideUpgradePanel() => upgradePanel.SetActive(false);

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        gameOverKillsText.text = $"Kills: {GameManager.Instance.killCount}";
        gameOverLevelText.text = $"Level: {LevelSystem.Instance.currentLevel}";
    }
}
```

### DamagePopup.cs
**Inspiration:** Code Monkey — *How to make Damage Popup Text*

```csharp
// DamagePopup.cs
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TMP_Text text;
    private float lifetime = 1f;
    private Vector3 moveDir = new Vector3(0.3f, 1f);

    public static void Create(Vector3 worldPos, int damage)
    {
        // Requires a DamagePopup prefab in Resources folder
        var prefab = Resources.Load<GameObject>("DamagePopup");
        var obj = Instantiate(prefab, worldPos, Quaternion.identity);
        obj.GetComponent<DamagePopup>().Setup(damage);
    }

    private void Awake() => text = GetComponent<TMP_Text>();

    public void Setup(int damage)
    {
        text.text = damage.ToString();
        text.color = Color.red;
    }

    private void Update()
    {
        transform.position += moveDir * Time.deltaTime;
        lifetime -= Time.deltaTime;
        if (lifetime <= 0) Destroy(gameObject);
        // Fade out
        var c = text.color;
        c.a = lifetime;
        text.color = c;
    }
}
```

---

## 16. SYSTEM 11: AudioManager

**Inspiration:** Code Monkey — *Simple Sound Manager*

```csharp
// AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Weapon SFX")]
    public AudioClip pistolShoot;
    public AudioClip shotgunShoot;
    public AudioClip swordSwing;
    public AudioClip enemyShoot;

    [Header("Event SFX")]
    public AudioClip hitSound;
    public AudioClip levelUpSound;
    public AudioClip bossAppearSound;
    public AudioClip bossShootSound;
    public AudioClip pickupSound;
    public AudioClip deathSound;

    [Header("Music")]
    public AudioClip bgmGameplay;
    public AudioClip bgmBoss;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
    }

    private void Start() => PlayMusic(bgmGameplay);

    public void PlayShoot(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Pistol:   sfxSource.PlayOneShot(pistolShoot); break;
            case WeaponType.Shotgun:  sfxSource.PlayOneShot(shotgunShoot); break;
            case WeaponType.Sword:    sfxSource.PlayOneShot(swordSwing); break;
        }
    }

    public void PlayHit()           => sfxSource.PlayOneShot(hitSound);
    public void PlayLevelUp()       => sfxSource.PlayOneShot(levelUpSound);
    public void PlayBossAppear()    { sfxSource.PlayOneShot(bossAppearSound); PlayMusic(bgmBoss); }
    public void PlayBossShoot()     => sfxSource.PlayOneShot(bossShootSound);
    public void PlayPickup()        => sfxSource.PlayOneShot(pickupSound);
    public void PlayEnemyShoot()    => sfxSource.PlayOneShot(enemyShoot);

    private void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }
}
```

---

## 17. SYSTEM 12: CameraFollow

**Inspiration:** Code Monkey — *How to make a Camera Follow System*  
**Preferred method:** Use **Cinemachine** (no code needed for basic follow):

```
Hierarchy → Create → Cinemachine → 2D Camera
→ CinemachineVirtualCamera Inspector:
   Follow: [Player Transform]
   Body: Framing Transposer
   Dead Zone Width: 0.1 | Dead Zone Height: 0.1
   Damping: 0.5
```

**Alternative: Script-based follow**
```csharp
// CameraFollow.cs (use only if NOT using Cinemachine)
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
```

---

## 18. COMBAT FORMULAS

| Formula | Expression | Notes |
|---|---|---|
| Actual Damage | `rawDamage × (1 - DEF%)` | DEF% capped at 0.8 (80%) |
| Weapon Damage | `baseDamage × atkMultiplier` | atkMultiplier starts at 1.0, +0.1 per ATK upgrade |
| EXP to Level Up | `10 × level^1.5` | Level 1→2: 10 EXP, Level 5→6: 111 EXP |
| Burn DOT | `2% enemy HP / second × 10s` | Applied by Shotgun item |
| Heal | `15% of max HP (instant)` | From Heal item |
| Lifesteal | `damage dealt × lifestealPercent` | lifestealPercent set by item |
| Boss Spawn | `Random.Range(100, 201) kills` | Rolled at game start |

### Upgrade Stack Examples
| Upgrades Taken | atkMultiplier | Damage from Pistol (base 10) |
|---|---|---|
| 0 ATK upgrades | 1.0 | 10 |
| 3 ATK upgrades | 1.3 | 13 |
| 5 ATK upgrades | 1.5 | 15 |

---

## 19. GAMEPLAY NUMBERS REFERENCE

### Player Stats
| Stat | Value |
|---|---|
| Starting HP | 100 |
| Move Speed | 5 |
| Dash Speed | 15 |
| Dash Duration | 0.3s |
| Dash Cooldown | 3s |

### Weapon Stats
| Weapon | Base Damage | Fire Rate | Pellets | Effect Duration |
|---|---|---|---|---|
| Pistol | 10 | 5/s | 1 | Pierce: 12s |
| Shotgun | 25 (per pellet) | 1/s | 5 | Burn: 10s |
| Sword | 40 | 1.5/s | melee | Lifesteal: 8s |

### Enemy Stats
| Enemy | HP | Speed | Damage | EXP |
|---|---|---|---|---|
| Melee | 30 | 3 | 10 | 5 |
| Ranged | 20 | 2 | 8 (bullet) | 7 |
| Boss | 1000 | 2 | 20 (bullets) | 500 |

### Item Drop Rates
| Item | Drop Chance | Effect |
|---|---|---|
| Heal | 15% | +15% max HP instantly |
| Shield | 10% | Invincible 5s |
| Burn (Shotgun) | 5% | Enemy DOT 2%/s for 10s |
| Pierce (Pistol) | 5% | Bullets pierce for 12s |
| Lifesteal (Sword) | 5% | Heal 20% of damage dealt for 8s |

---

## 20. PHYSICS LAYERS & TAGS

### Collision Matrix (✅ = collides)
|  | Player | Enemy | PlayerBullet | EnemyBullet | Item | Wall |
|---|---|---|---|---|---|---|
| **Player** | — | ✅ | — | ✅ | ✅ | ✅ |
| **Enemy** | ✅ | — | ✅ | — | — | ✅ |
| **PlayerBullet** | — | ✅ | — | — | — | ✅ |
| **EnemyBullet** | ✅ | — | — | — | — | ✅ |
| **Item** | ✅ | — | — | — | — | — |
| **Wall** | ✅ | ✅ | ✅ | ✅ | — | — |

### Important Rigidbody2D Settings
```
Player:
  Body Type: Dynamic
  Constraints: Freeze Rotation Z ✅
  Collision Detection: Continuous

Enemy:
  Body Type: Dynamic
  Constraints: Freeze Rotation Z ✅

Bullet:
  Body Type: Kinematic (velocity set manually)
  Is Trigger: ✅ (use OnTriggerEnter2D)
```

---

## 21. BUILD CHECKLIST

### Sprint 1 — MVP (Week 1-2)
- [ ] Unity project created, folder structure ready, GitHub pushed
- [ ] Player moves WASD, aims at mouse
- [ ] Pistol fires bullets, bullets deal damage
- [ ] MeleeEnemy spawns, chases, attacks player
- [ ] HealthSystem: player takes damage, dies → GameOver
- [ ] EXP drops from enemy, LevelSystem accumulates
- [ ] UpgradePanel shows on level up (3 choices), applies stat
- [ ] Kill counter increments
- [ ] HP bar, EXP bar, Level text, Kill count UI visible
- [ ] Start screen (weapon select) → loads GameScene
- [ ] Game Over screen with kill count + restart

### Sprint 2 — Weapons + AI (Week 3)
- [ ] Shotgun fires pellet spread
- [ ] Sword melee hitbox works
- [ ] Weapon selected on start screen loads correct prefab
- [ ] Dash with 3s cooldown + i-frames
- [ ] RangedEnemy spawns, keeps distance, shoots
- [ ] Boss spawns at kill threshold (random 100-200)
- [ ] Boss Circular Shot skill works
- [ ] Boss Aim Shot + telegraph works
- [ ] Object Pooling for bullets (no Instantiate in fire loop)
- [ ] Heal and Shield item drops and pickups work

### Sprint 3 — Polish + Audio (Week 4)
- [ ] Pierce item activates only with Pistol
- [ ] Burn item activates only with Shotgun
- [ ] Lifesteal item activates only with Sword
- [ ] Shoot SFX per weapon type
- [ ] Hit SFX, level up SFX, boss appear SFX
- [ ] Damage popup floats off enemies
- [ ] Boss Dash skill (stretch goal)
- [ ] Cinemachine camera follow smooth
- [ ] 60 FPS confirmed on mid-spec hardware
- [ ] Build exports to Windows .exe < 200MB
- [ ] README with setup instructions

---

## REFERENCES

| Source | URL | Used for |
|---|---|---|
| Unity Code Monkey — Top Down Shooter | https://unitycodemonkey.com/game.php?g=topdownshooter | Reference architecture, techniques |
| Code Monkey — Aim at Mouse | https://unitycodemonkey.com/video.php?v=fuGQFdhSPg4 | PlayerController mouse aim |
| Code Monkey — Health System | https://unitycodemonkey.com/video.php?v=0T5ei9jN63M | HealthSystem.cs |
| Code Monkey — Dodge Roll | https://unitycodemonkey.com/video.php?v=ptPAg83fQ4M | DashController.cs |
| Code Monkey — Damage Popup | https://unitycodemonkey.com/video.php?v=iD1_JczQcFY | DamagePopup.cs |
| Code Monkey — Sound Manager | https://unitycodemonkey.com/video.php?v=QL29aTa7J5Q | AudioManager.cs |
| Code Monkey — Camera Follow | https://unitycodemonkey.com/video.php?v=U66VYM-ShVg | CameraFollow / Cinemachine |
| Code Monkey — A* Pathfinding | https://unitycodemonkey.com/video.php?v=alU04hvz6L4 | EnemyAI pathfinding |
| Code Monkey — Bullet Tracer | https://unitycodemonkey.com/video.php?v=jDiAzxkYzpI | Object pooling pattern |
| Kenney.nl | https://kenney.nl | Free sprites, audio |
| OpenGameArt.org | https://opengameart.org | Free assets |

---

*SilverBullet.md — Group 7, CMU-CS 447, Duy Tan University*  
*Compiled by: Nguyen, Nguyen Minh | Last updated: April 2026*
