# 🔫 SILVER BULLET — Tài liệu Dự án Toàn tập

> **Thể loại:** 2D Top-Down Roguelite Shooter
> **Engine:** Unity 6 (URP 2D) | **Nền tảng:** PC Windows
> **Nhóm:** Group 7 — Đại học Duy Tân (CMU-CS 447)
> **GitHub:** https://github.com/StyxHavenVN/Game-2D-Top-down-Shooter
> **Phiên bản:** v0.1a (Endless Echoes) | **Cập nhật:** Tháng 1/2025

---

## ⚡ TÓM TẮT NHANH (30 giây)

Silver Bullet là game bắn súng góc nhìn từ trên xuống theo phong cách roguelite, lấy cảm hứng từ Vampire Survivors, Soul Knight và Brotato. Người chơi điều khiển nhân vật bằng WASD, ngắm bắn bằng chuột, tiêu diệt kẻ địch, lên cấp và đối mặt với Boss.

**Vòng lặp cốt lõi:**

```
Tiêu diệt Kẻ địch → Nhận EXP → Lên cấp → Chọn Nâng cấp → Boss xuất hiện (100–200 kills) → Chết → Chơi lại
```

**Ba vũ khí:**

| Vũ khí            | Đặc điểm                                        | Phong cách               |
| ------------------- | --------------------------------------------------- | ------------------------- |
| 🔫**Pistol**  | Nhanh, sát thương thấp, đạn xuyên kẻ địch | Spray & pray, kiting      |
| 🔫**Shotgun** | Chậm, sát thương cao, gây Burn                 | Burst damage, cận chiến |
| ⚔️**Sword** | Cận chiến, sát thương rất cao, hồi máu      | High risk, high reward    |

---

## 🎯 TRIẾT LÝ THIẾT KẾ (Core Design Pillars)

Trước khi đọc tài liệu kỹ thuật, cần nắm vững 3 nguyên tắc cốt lõi của game:

### 1. Tight Controls — Điều khiển siêu nhạy

Phản hồi phím bấm/chuột phải ngay lập tức. Người chơi phải cảm thấy họ có toàn quyền kiểm soát sinh tử của nhân vật. **Nếu chết là do kỹ năng, không phải do game lag.**

### 2. Never Stand Still — Không bao giờ đứng yên

Kẻ địch được thiết kế để liên tục dồn ép (Kamikaze lao vào, Ranged bắn cấu rỉa). **Di chuyển và Dash là cách duy nhất để sinh tồn.**

### 3. Fast Restart — Chơi lại cực nhanh

Giống Hotline Miami, vòng lặp game phải ngắn (15–20 phút). Khi người chơi chết, nút "Restart" phải đưa họ vào game ngay lập tức trong 1 giây, loại bỏ màn hình chờ rườm rà.

---

## 🎮 VÒNG LẶP GAMEPLAY

```
CHƠI GAME
   ↓
Tiêu diệt Kẻ địch → Nhận EXP
   ↓
Lên cấp → Chọn 1 trong 3 Thẻ Nâng cấp (Mutation)
   ↓0
Đạt Kill Threshold (15–20 kills) → Boss xuất hiện
   ↓
Đánh bại Boss hoặc Chết → Màn hình Game Over
   ↓
Quay về màn hình Chọn Vũ khí
```

**Điểm khác biệt so với game cùng thể loại:**

| Tính năng  | Silver Bullet       | Vampire Survivors | Soul Knight  |
| ------------ | ------------------- | ----------------- | ------------ |
| Nhắm bắn   | Thủ công (chuột) | Tự động        | Tự động   |
| Dodge Roll   | ✅ i-frames 0.3s    | ❌                | ✅           |
| Vũ khí     | Khóa từ đầu run | Mid-run           | Mid-run      |
| Tiến trình | Kill-based          | Timer-based       | Kill-based   |
| Upgrade      | Thẻ đột biến    | Cây kỹ năng    | Ngẫu nhiên |

---

## 🔫 HỆ THỐNG VŨ KHÍ

**File cơ sở:** `WeaponBase.cs` (abstract class)
**Cài đặt:** `PistolWeapon.cs`, `ShotgunWeapon.cs`, `SwordWeapon.cs`

### Thông số chi tiết

| Thông số             | Pistol              | Shotgun               | Sword                 |
| ---------------------- | ------------------- | --------------------- | --------------------- |
| Sát thương          | 5                   | 20                    | 30                    |
| Tốc độ bắn         | 0.15s               | 0.8s                  | 0.5s                  |
| Tầm                   | Xa                  | Gần-Trung            | Cận chiến           |
| Hiệu ứng đặc biệt | Pierce (xuyên qua) | Burn (2% HP/s × 10s) | Lifesteal (hồi máu) |
| Số đạn              | 1                   | 5 pellets             | 1 slash               |
| Rung màn hình        | Thấp               | **Cao**         | Trung bình           |

### Luồng bắn súng

```
PlayerController.Update()
  → if (MouseButton) currentWeapon.TryShoot()
    → Kiểm tra cooldown
    → Lấy đạn từ ObjectPool.GetBullet()
    → Bullet.Initialize(vị trí, hướng, sát thương)
    → Bullet di chuyển → OnTriggerEnter(Enemy)
    → Enemy.TakeDamage(damage)
    → Trả đạn về pool nếu không có pierce
```

### Mở rộng WeaponData (ScriptableObject)

Các trường mới được thêm vào `WeaponData.cs` để hỗ trợ Synergy và Game Feel:

```csharp
class WeaponData : ScriptableObject
{
    string weaponName;
    WeaponType type;

    float baseDamage;
    float fireRate;
    float bulletSpeed;
    float range;

    int pelletCount;              // Cho shotgun
    float spreadAngle;            // Cho shotgun
    ItemEffectType specialEffect; // Pierce / Burn / Lifesteal

    // === Trường mở rộng (Game Feel & Synergy) ===
    public float knockbackForce;         // Lực đẩy lùi kẻ địch khi trúng đạn
    public int projectileCount;          // Hỗ trợ bắn nhiều viên cùng lúc (Extra Projectile)
    public float screenShakeIntensity;   // Mỗi vũ khí có độ rung màn hình riêng
}
```

---

## 👾 HỆ THỐNG KẺ ĐỊCH

**File spawner:** `EnemySpawner.cs`
**AI cơ sở:** `EnemyFollow.cs` (hành vi đuổi theo)

### Các loại kẻ địch

| Loại          | HP  | Tốc độ | Sát thương | AI                         | Drop                    |
| -------------- | --- | --------- | ------------- | -------------------------- | ----------------------- |
| Basic Enemy    | 10  | 2.0       | 5             | Chase + đánh cận chiến | EXP + item ngẫu nhiên |
| Ranged Enemy   | 15  | 1.5       | 3             | Giữ khoảng cách + bắn  | EXP + item ngẫu nhiên |
| Kamikaze       | 5   | 3.0       | Tự nổ       | Lao thẳng + nổ khi chạm | EXP                     |
| **Boss** | 200 | 1.0       | 15            | 3 phase tấn công         | EXP lớn                |

### Telegraphing — Báo trước đòn đánh (Hard but Fair)

Mọi đòn tấn công nguy hiểm **phải có dấu hiệu cảnh báo trước** để người chơi có cơ hội phản ứng:

- **Boss — Aim Shot:** Hiển thị tia laser nhắm mục tiêu, chuyển màu vàng → đỏ trong 2–3 giây trước khi bắn. Người chơi có thể dùng Dash (i-frames) để né.
- **Kamikaze — Nổ:** Vòng tròn đỏ dưới chân nhấp nháy nhanh dần + âm thanh tiếng bíp (beep) trước khi phát nổ.
- **Knockback:** Đạn của người chơi (đặc biệt Sword và Shotgun) gây lực đẩy lùi (Knockback), làm gián đoạn đòn tấn công của quái nhỏ, giúp vũ khí có cảm giác uy lực hơn.

### Công thức Scale độ khó

```
Hệ số HP kẻ địch  = 1 + (killCount / 500)
Hệ số sát thương  = 1 + (killCount / 300)

Ví dụ: Tại 100 kills → kẻ địch có HP × 1.2 và sát thương × 1.33
```

---

## 🧑 HỆ THỐNG NGƯỜI CHƠI

### Di chuyển & Ngắm bắn

- **Di chuyển:** WASD, input normalized
- **Ngắm:** Chuột, nhân vật xoay về phía con trỏ
- **Tốc độ:** 5 units/s (có thể chỉnh trong inspector)

### Dash (Lướt)

| Thông số        | Giá trị                                |
| ----------------- | ---------------------------------------- |
| Tốc độ dash    | 15 units/s                               |
| Thời gian dash   | 0.3s                                     |
| Cooldown          | 3s                                       |
| I-frames          | ✅ Miễn nhiễm sát thương trong dash |
| Hiệu ứng visual | DashGhost trail                          |

### Máu & Sát thương

- Máu được theo dõi bởi `HealthSystem.cs`
- Popup số sát thương qua `DamagePopup.cs` (dùng Object Pool)
- Chết → kích hoạt Game Over state

### Điều khiển

| Phím                 | Hành động                  |
| --------------------- | ----------------------------- |
| **WASD**        | Di chuyển                    |
| **Chuột**      | Ngắm bắn                    |
| **Click trái** | Bắn                          |
| **Space**       | Dash (cooldown 3s, i-frames)  |
| **ESC**         | Pause*(đang phát triển)* |

---

## 💥 GAME FEEL & COMBAT JUICE

Để đảm bảo trải nghiệm bắn súng thỏa mãn ("đã tay"), hệ thống combat tích hợp phản hồi thị giác và âm thanh mạnh mẽ:

### Camera Shake — Rung màn hình

Kích hoạt khi:

- Người chơi bắn **Shotgun** (cường độ cao)
- Người chơi **nhận sát thương**
- **Boss bị tiêu diệt** (rung mạnh)

**Triển khai:** Dùng **Cinemachine Impulse** thay vì code thủ công, tạo rung chấn tự nhiên hơn.

### Hit-Stop — Khựng khung hình

Khi Sword chém trúng kẻ địch hoặc hạ gục Boss, game tạm dừng **0.05 giây** để tạo cảm giác lực chém uy lực.

### Clarity in Chaos — Hỗn loạn có kiểm soát

Dù hiệu ứng hạt (vỏ đạn, tia lửa, máu) xuất hiện nhiều, màu sắc Enemy Bullet phải **tương phản cao** với nền và với đạn của người chơi, để người chơi dễ nhận diện và né tránh.

### Upbeat Soundtrack — Nhạc nền nhịp độ cao

Sử dụng nhạc nền dồn dập (phong cách Hotline Miami) để thúc đẩy nhịp độ, cuốn người chơi vào vòng lặp chiến đấu.

---

## 📈 HỆ THỐNG LÊN CẤP & NÂNG CẤP

### Luồng EXP → Lên cấp

```
Kẻ địch chết
  → Rơi EXP Orb (giá trị = exp reward của kẻ địch)

Người chơi chạm vào EXP Orb
  → LevelSystem.AddEXP(amount)
  → Thanh EXP đầy?
      → currentLevel++
      → GameManager.SetState(LevelUp)
      → Time.timeScale = 0 (DỪNG game)
      → Hiện 3 thẻ nâng cấp ngẫu nhiên

Người chơi chọn thẻ
  → LevelUpManager.ApplyUpgrade()
  → Cập nhật PlayerStats
  → GameManager.SetState(Playing)
  → Time.timeScale = 1 (TIẾP TỤC)
```

### Bảng Thẻ Nâng cấp (Mutations/Perks)

Thay vì chỉ tăng ATK/DEF/HP, hệ thống cung cấp đa dạng thẻ đột biến tạo Synergy chiến thuật:

| Nhóm             | Tên thẻ           | Hiệu ứng                          |
| ----------------- | ------------------- | ----------------------------------- |
| **Stat**    | Sức mạnh          | +20% ATK                            |
| **Stat**    | Máu dày           | +20% Max HP                         |
| **Stat**    | Tốc độ           | +15% tốc độ di chuyển           |
| **Utility** | Cooldown giảm      | Giảm 30% cooldown Dash             |
| **Utility** | Nam châm EXP       | Tăng phạm vi hút EXP orb         |
| **Combat**  | Đạn nảy          | Đạn nảy tường (Bouncy Bullets) |
| **Combat**  | Extra Projectile    | Thêm 1 tia đạn cho súng         |
| **Combat**  | Sóng năng lượng | Kiếm chém ra sóng năng lượng  |

### Dopamine Drop — Phần thưởng tức thời

- Rương đồ ngẫu nhiên rơi ra từ Boss
- Vật phẩm giá trị cao (Shield, Heal) rớt với hiệu ứng pop-up bắt mắt
- Kích thích não bộ, tăng cảm giác reward sau mỗi lần tiêu diệt Boss

---

## 🎵 HỆ THỐNG ÂM THANH

**File:** `AudioManager.cs` (Singleton)

Tất cả âm thanh đều được điều phối qua `AudioManager`. Các âm thanh cốt lõi:

- Bắn súng (mỗi vũ khí có âm thanh riêng)
- Trúng đòn / nhận sát thương
- Dash
- Lên cấp
- Game over
- Nhạc nền (upbeat soundtrack)

```csharp
// Phát âm thanh từ bất kỳ script nào
AudioManager.Instance.PlaySound("shoot");
```

---

## 🎬 CẤU TRÚC SCENE

### Scene 1: MainMenu

```
Canvas
└── WeaponSelectPanel
    ├── PistolButton
    ├── ShotgunButton
    ├── SwordButton
    └── WeaponInfoDisplay
```

### Scene 2: GameScene

```
GameManager (Singleton) ← Máy trạng thái game
AudioManager (Singleton) ← Quản lý âm thanh
ObjectPool ← Tái sử dụng đạn & kẻ địch
EnemySpawner ← Tạo wave kẻ địch
LevelSystem ← Theo dõi EXP

Player (Layer: Player)
├── PlayerMovement.cs
├── PlayerAttack.cs
├── PlayerStats.cs
└── Playerdash.cs

Enemies (Container)
├── BasicEnemy prefabs
├── RangedEnemy prefabs
├── KamikazeEnemy prefabs
└── Boss prefab

Items (Container)
└── Item pickups, EXP orbs

Tilemap
├── Ground layer
└── Wall layer

Canvas (UI)
├── HUD (luôn hiển thị)
│   ├── Thanh HP
│   ├── Thanh EXP
│   ├── Level text
│   ├── Kill count
│   └── Dash cooldown
├── UpgradePanel (hiện khi lên cấp)
└── GameOverPanel (hiện khi chết)
```

---

## 📁 CẤU TRÚC THƯ MỤC

```
Assets/
├── Scripts/
│   ├── Manager/
│   │   ├── GameManager.cs        ← Máy trạng thái, kill counter, boss trigger
│   │   ├── UIManager.cs          ← Cập nhật HUD
│   │   ├── AudioManager.cs       ← Âm thanh
│   │   ├── FeedbackManager.cs    ← Screen Shake, Hit-Stop (NEW)
│   │   ├── LevelUpManager.cs     ← Chọn và áp dụng nâng cấp
│   │   ├── ItemDropManager.cs    ← Spawning vật phẩm
│   │   ├── InventoryManager.cs   ← Theo dõi vật phẩm
│   │   ├── GameOverManager.cs    ← Màn hình Game Over
│   │   ├── MainMenuManager.cs    ← Màn hình chính
│   │   └── ObjectPool.cs         ← Tái sử dụng đạn, kẻ địch, DamagePopup
│   │
│   ├── Player/
│   │   ├── PlayerAttack.cs       ← Logic bắn súng
│   │   ├── PlayerMovement.cs     ← Di chuyển WASD
│   │   ├── PlayerStats.cs        ← ATK / DEF / HP
│   │   ├── Playerdash.cs         ← Cơ chế Dash + i-frames
│   │   └── DashGhost.cs          ← Hiệu ứng trail khi dash
│   │
│   ├── Weapons/
│   │   ├── WeaponBase.cs         ← Abstract base class
│   │   ├── PistolWeapon.cs
│   │   ├── ShotgunWeapon.cs
│   │   ├── SwordWeapon.cs
│   │   └── Bullet.cs             ← Logic projectile + pierce
│   │
│   ├── Enemies/
│   │   ├── EnemyFollow.cs        ← AI đuổi theo cơ bản
│   │   ├── RangedEnemy.cs        ← AI kẻ địch bắn xa
│   │   ├── KamikazeEnemy.cs      ← AI kẻ địch tự nổ
│   │   ├── BossController.cs     ← AI Boss + attack patterns
│   │   ├── EnemyHealth.cs        ← Hệ thống máu kẻ địch
│   │   ├── EnemyFlash.cs         ← Visual khi bị đánh
│   │   ├── EnemySpawner.cs       ← Tạo wave kẻ địch
│   │   └── EnemyData.cs          ← ScriptableObject dữ liệu kẻ địch
│   │
│   ├── Items/
│   │   ├── ItemData.cs
│   │   ├── ItemPickup.cs
│   │   └── LootItem.cs
│   │
│   ├── UI/
│   │   ├── UIManager.cs
│   │   ├── Dashcooldownui.cs
│   │   └── DamagePopup.cs        ← Số sát thương nổi (dùng Object Pool)
│   │
│   └── Other/
│       ├── CameraFollow.cs       ← Cinemachine camera
│       ├── CameraShake.cs        ← Screen shake (tích hợp Cinemachine Impulse)
│       ├── MapGenerator.cs       ← Sinh map ngẫu nhiên (đang phát triển)
│       ├── MeleeEffect.cs        ← Hiệu ứng chém kiếm
│       ├── DropShadow.cs
│       └── ShadowSync.cs
│
├── Data/ (ScriptableObjects)
│   ├── Pistol Data.asset
│   ├── Shotgun Data.asset
│   ├── Sword Data.asset
│   ├── Boss_Data.asset
│   └── (enemy & item data assets)
│
├── Prefabs/
│   ├── Player.prefab
│   ├── BasicEnemy.prefab
│   ├── RangedEnemy.prefab
│   ├── Quái Tự Nổ.prefab
│   ├── Boss.prefab
│   ├── Bullet.prefab
│   ├── ExpOrb.prefab
│   ├── LootItem.prefab
│   └── SwordSlash.prefab
│
├── Scenes/
│   ├── MainMenu.unity
│   └── Gun-v1.unity
│
└── Audio/
    └── (Sound effects & background music)
```

---

## ⚙️ CÁC HỆ THỐNG CỐT LÕI

### GameManager (Singleton)

**File:** `Manager/GameManager.cs`

Quản lý trạng thái tổng thể của game, theo dõi kill count và điều phối các event.

```csharp
// Các method chính
GameManager.Instance.RegisterKill();           // Gọi khi kẻ địch chết
GameManager.Instance.SetState(GameState.xxx);  // Thay đổi trạng thái

// Events phát đi
OnKillCountChanged     // → UIManager cập nhật HUD
OnBossThresholdReached // → EnemySpawner spawns Boss
OnGameOver             // → GameOverUI hiển thị
```

### FeedbackManager (Singleton) — MỚI

**File:** `Manager/FeedbackManager.cs`

Lắng nghe event từ `GameManager` và `HealthSystem`, xử lý toàn bộ phản hồi cảm giác (Game Juice) một cách độc lập, không làm phình `GameManager`:

```csharp
FeedbackManager.Instance.TriggerHitStop(0.05f);          // Khựng 0.05 giây
FeedbackManager.Instance.TriggerScreenShake(intensity);   // Rung màn hình
// Tích hợp Cinemachine Impulse để rung tự nhiên hơn
```

### ObjectPool

**File:** `Manager/ObjectPool.cs`

Tái sử dụng các đối tượng thay vì Instantiate/Destroy:

```csharp
// Lấy đạn từ pool
Bullet bullet = ObjectPool.Instance.GetBullet();

// Trả đạn về pool
ObjectPool.Instance.ReturnBullet(bullet);

// Pool bao gồm: Bullet, Enemy, EXP Orb, DamagePopup
```

---

## 🏗️ KIẾN TRÚC CODE

### Phân cấp class vũ khí

```
WeaponBase (abstract)
├── PistolWeapon    → Pierce (đạn xuyên)
├── ShotgunWeapon   → Burn (sát thương theo thời gian)
└── SwordWeapon     → Lifesteal (hồi máu)

Sử dụng bởi:
PlayerController → giữ reference đến weapon hiện tại
                → gọi weapon.TryShoot() khi có input
```

### Phân cấp class kẻ địch

```
EnemyFollow (AI đuổi theo cơ bản)
├── MeleeEnemy    → Chase & đánh cận chiến
├── RangedEnemy   → Giữ khoảng cách & bắn
└── KamikazeEnemy → Lao thẳng & nổ

Quản lý bởi: EnemySpawner → ObjectPool
```

### Kiến trúc Manager

```
GameManager (Singleton)
├── Phát event đến: UIManager, EnemySpawner, LevelUpManager, AudioManager
└── Nhận từ: Enemy.Die(), LevelSystem, PlayerHealth

FeedbackManager (Singleton) — tách biệt khỏi GameManager
├── Lắng nghe: OnPlayerHit, OnEnemyKill, OnBossKill
└── Xử lý: Screen Shake, Hit-Stop

AudioManager (Singleton)
└── Nhận yêu cầu phát âm từ mọi system

UIManager
└── Lắng nghe GameManager events → cập nhật HUD
```

---

## 🔄 LUỒNG SỰ KIỆN (Event Flow)

### Khi người chơi bắn trúng kẻ địch và hạ gục

```
1.  Bullet.OnTriggerEnter(Enemy)
2.  Enemy.TakeDamage(damage)
3.  EnemyFlash → visual feedback
4.  DamagePopup.Show(damage) → từ ObjectPool
5.  HP ≤ 0? → Enemy.Die()
6.  GameManager.RegisterKill()       ← killCount++
7.  FeedbackManager.TriggerShake()   ← Game Juice
8.  ItemDropManager.SpawnDrop()      ← drop EXP/item
9.  ObjectPool.ReturnEnemy(this)
10. UIManager cập nhật kill count
```

### Khi người chơi lên cấp

```
EXP đầy → LevelSystem.OnLevelUp()
  → GameManager.SetState(LevelUp)
  → Time.timeScale = 0 (DỪNG)
  → AudioManager.Play("levelup")
  → LevelUpManager.ShowUpgrades() → 3 thẻ ngẫu nhiên

Chọn thẻ → LevelUpManager.ApplyUpgrade(choice)
  → Cập nhật PlayerStats
  → GameManager.SetState(Playing)
  → Time.timeScale = 1 (TIẾP TỤC)
```

### Khi Boss xuất hiện

```
killCount >= bossKillThreshold
  → GameManager.SetState(BossSpawned)
  → EnemySpawner dừng spawn wave thường
  → Instantiate(Boss prefab)
  → BossController.Start()
     → Hiển thị thanh HP Boss
     → Bắt đầu Phase 1
```

### Khi người chơi chết

```
PlayerHealth.currentHP ≤ 0
  → PlayerHealth.Die()
  → GameManager.SetState(GameOver)
  → Time.timeScale = 0
  → FeedbackManager: screen shake mạnh
  → AudioManager.Play("gameover")
  → GameOverPanel.Show(kills, level)
```

---

## 📊 MÁY TRẠNG THÁI (State Machine)

```csharp
enum GameState
{
    Playing,      // Chơi bình thường - timeScale = 1
    LevelUp,      // Chọn nâng cấp  - timeScale = 0
    BossSpawned,  // Boss đang hoạt động
    GameOver      // Người chơi chết - timeScale = 0
}

// Chuyển tiếp trạng thái:
Playing     ──→ LevelUp     ──→ Playing  (lặp lại)
Playing     ──→ BossSpawned
BossSpawned ──→ Playing     (nếu Boss chết)
BossSpawned ──→ GameOver    (nếu người chơi chết)
Playing     ──→ GameOver    (nếu người chơi chết)
```

---

## 🎯 LAYER & TAG

### Physics2D Layers

| Layer        | Index | Mục đích                   |
| ------------ | ----- | ----------------------------- |
| Default      | 0     | Đối tượng chung           |
| Player       | 6     | Nhân vật người chơi      |
| Enemy        | 7     | Tất cả kẻ địch           |
| PlayerBullet | 8     | Đạn do người chơi bắn   |
| EnemyBullet  | 9     | Đạn do kẻ địch bắn      |
| Item         | 10    | Vật phẩm pickup             |
| Wall         | 11    | Tường / chướng ngại vật |

### Quy tắc va chạm (Collision Matrix)

```
PlayerBullet va chạm với: Enemy ✅, Wall ✅
EnemyBullet  va chạm với: Player ✅, Wall ✅
Player       va chạm với: Enemy ✅, Wall ✅, Item ✅
Enemy        va chạm với: Wall ✅ (không va với nhau → tối ưu hiệu năng)
```

### Tags

```
Player | Enemy | Boss | Bullet | Item | Wall
```

---

## 📊 CÔNG THỨC CHIẾN ĐẤU

### Tính sát thương

```
Sát thương cuối = Sát thương cơ bản × (1 + Hệ số ATK từ nâng cấp)
                × (0.5 nếu kẻ địch có shield, hoặc 1.0)

Ví dụ: Pistol (5 dmg) + 2 lần nâng cấp ATK (+40%) = 7 dmg/viên
```

### Lifesteal (Kiếm)

```
Lượng hồi máu = Sát thương gây ra × Lifesteal%
Ví dụ: Kiếm 20 dmg, 20% lifesteal → hồi 4 HP
```

### Burn (Shotgun)

```
Sát thương Burn = 2% HP tối đa kẻ địch/giây
Thời gian: 10 giây
Tổng Burn = 20% HP tối đa kẻ địch
```

---

## 🔧 GIÁ TRỊ CẤU HÌNH

Tất cả có thể chỉnh trong Inspector:

| Hệ thống | Thông số     | Mặc định | File               |
| ---------- | -------------- | ----------- | ------------------ |
| Player     | Move Speed     | 5           | PlayerMovement.cs  |
| Player     | Dash Speed     | 15          | Playerdash.cs      |
| Player     | Dash Cooldown  | 3s          | Playerdash.cs      |
| Player     | Dash Duration  | 0.3s        | Playerdash.cs      |
| Pistol     | Fire Rate      | 0.15s       | Pistol Data.asset  |
| Pistol     | Damage         | 5           | Pistol Data.asset  |
| Shotgun    | Fire Rate      | 0.8s        | Shotgun Data.asset |
| Shotgun    | Damage         | 20          | Shotgun Data.asset |
| Shotgun    | Pellets        | 5           | Shotgun Data.asset |
| Sword      | Damage         | 30          | Sword Data.asset   |
| Sword      | Attack Speed   | 0.5s        | Sword Data.asset   |
| Enemy      | Base HP        | 10–15      | EnemyData.asset    |
| Enemy      | Base Speed     | 1.5–2      | EnemyData.asset    |
| Enemy      | EXP Reward     | 10          | EnemyData.asset    |
| Boss       | HP             | 200         | Boss_Data.asset    |
| Boss Spawn | Kill Threshold | 100–200    | GameManager.cs     |
| Upgrade    | Stat Boost %   | 20%         | LevelUpManager.cs  |

---

## 🔗 SƠ ĐỒ PHỤ THUỘC (Dependency Graph)

```
PlayerController
├─ phụ thuộc: Rigidbody2D, Camera, WeaponBase
├─ gọi: WeaponBase.TryShoot(), DashController.TryDash()
└─ cập nhật HUD qua: UIManager

WeaponBase (Pistol / Shotgun / Sword)
├─ phụ thuộc: Bullet (hoặc SwordSlash effect)
├─ gọi: ObjectPool.GetBullet()
└─ bắn đạn vào: Enemy layer

Bullet
├─ phụ thuộc: Rigidbody2D, CircleCollider2D
├─ gọi: Enemy.TakeDamage()
└─ trả về: ObjectPool

Enemy
├─ phụ thuộc: Rigidbody2D, EnemyData
├─ gọi: PlayerHealth.TakeDamage()
├─ phát: GameManager.RegisterKill()
└─ thả: ItemDropManager.SpawnDrop()

GameManager
├─ phát event đến: UIManager, EnemySpawner, LevelUpManager, AudioManager
├─ nhận từ: Enemy.Die(), LevelSystem, PlayerHealth
└─ Singleton (DontDestroyOnLoad)

FeedbackManager ← TÁCH BIỆT với GameManager
├─ lắng nghe: OnPlayerHit, OnEnemyKill, OnBossKill
└─ xử lý: ScreenShake (Cinemachine Impulse), HitStop
```

---

## 🏃 LUỒNG THỰC THI (Một frame)

```
Frame bắt đầu
├─ Input Phase
│  ├─ PlayerController.Update()   ← đọc WASD/Chuột/Space
│  └─ DashController.Update()     ← cập nhật cooldown
│
├─ Physics Phase (FixedUpdate)
│  ├─ PlayerMovement → velocity
│  ├─ Enemy → velocity
│  ├─ Bullet → vị trí
│  └─ Physics2D → kiểm tra va chạm
│
├─ Gameplay Phase
│  ├─ WeaponBase.Update()         ← fire rate timer
│  ├─ Bullet.OnTriggerEnter()     ← xử lý trúng đích
│  ├─ Enemy.TakeDamage()          ← trừ HP
│  └─ EnemySpawner.Update()       ← spawn wave
│
├─ Systems Phase
│  ├─ GameManager.Update()        ← kiểm tra trạng thái
│  ├─ LevelSystem.Update()        ← theo dõi EXP
│  └─ ItemDropManager.Update()    ← loot roll
│
├─ Feedback Phase
│  ├─ FeedbackManager.Update()    ← Screen Shake, Hit-Stop
│  └─ DamagePopup.Update()        ← số nổi (từ pool)
│
└─ UI Phase
   ├─ UIManager.Update()          ← cập nhật HUD
   └─ DashCooldownUI.Update()
```

---

## 🚀 HƯỚNG DẪN THÊM NỘI DUNG MỚI

### Thêm Vũ khí mới

1. Tạo `NewWeapon.cs` kế thừa `WeaponBase`
2. Implement `TryShoot()` method
3. Tạo `NewWeaponData.asset` (ScriptableObject) với stats
4. Thêm button vào `WeaponSelectPanel` trong MainMenu
5. Thêm logic chọn vũ khí

```csharp
// Ví dụ: Fast Gun bắn 2 đạn cùng lúc
public class FastGunWeapon : WeaponBase
{
    private float fireRateCounter = 0f;

    public override void TryShoot()
    {
        if (fireRateCounter > 0) return;
        SpawnBullet(Vector2.right);
        SpawnBullet(Vector2.left);
        fireRateCounter = 0.1f;
    }
}
```

### Thêm Kẻ địch mới

1. Tạo `NewEnemy.cs` kế thừa `EnemyFollow`
2. Override `Attack()` hoặc tạo AI custom
3. Thêm Telegraphing (cảnh báo đòn đánh trước) vào animation
4. Tạo `NewEnemyData.asset`
5. Tạo `NewEnemy.prefab` với sprite + scripts
6. Thêm prefab vào `EnemySpawner.spawnablePrefabs[]`

### Thêm Thẻ Nâng cấp mới

1. Thêm logic nâng cấp vào `LevelUpManager.ApplyUpgrade()`
2. Thêm UI button vào UpgradePanel
3. Cập nhật `PlayerStats` hoặc `WeaponData` nếu cần
4. Cân nhắc Synergy với các thẻ hiện có

### Thêm Vật phẩm mới

1. Tạo script item hoặc mở rộng `ItemPickup.cs`
2. Thêm effect type vào enum `ItemEffectType`
3. Tạo `NewItemData.asset` với drop chance
4. Tạo prefab với sprite + ItemPickup script

---

## 🐛 DEBUG THƯỜNG GẶP

| Vấn đề                                      | Nguyên nhân                  | Giải pháp                                                   |
| ---------------------------------------------- | ------------------------------ | ------------------------------------------------------------- |
| Đạn không gây sát thương cho kẻ địch | Layer sai                      | Kiểm tra Bullet layer = PlayerBullet, Enemy layer = Enemy    |
| Người chơi không di chuyển được        | Thiếu Rigidbody2D             | Thêm Rigidbody2D, đặt Gravity Scale = 0                    |
| Kẻ địch không spawn                        | EnemySpawner chưa chạy       | Kiểm tra GameScene loaded, EnemySpawner.Start() được gọi |
| Không lên cấp                               | EXP threshold sai              | Kiểm tra tính toán threshold trong LevelSystem             |
| Vũ khí không bắn                           | WeaponBase chưa init          | Kiểm tra currentWeapon đã gán trong Inspector             |
| Game không dừng khi level up                 | timeScale chưa set            | Kiểm tra GameManager.SetState() có đặt timeScale = 0      |
| Dash không có i-frames                       | DashController chưa kết nối | Kiểm tra flag immunity trong PlayerStats                     |
| Screen Shake không hoạt động               | FeedbackManager thiếu         | Kiểm tra Cinemachine Impulse Source đã gắn                |

---

## ✅ DANH SÁCH KIỂM TRA TRƯỚC KHI BUILD

- [ ] Người chơi di chuyển được 4 hướng
- [ ] Người chơi xoay về phía con trỏ chuột
- [ ] Vũ khí bắn khi click chuột
- [ ] Đạn trúng kẻ địch đúng layer
- [ ] DamagePopup hiển thị khi đánh trúng
- [ ] Kẻ địch chết → rơi EXP
- [ ] Người chơi nhặt EXP → lên cấp
- [ ] Màn hình chọn nâng cấp hiện 3 thẻ ngẫu nhiên
- [ ] Nâng cấp được áp dụng vào PlayerStats
- [ ] Dash hoạt động + có cooldown + có i-frames
- [ ] Boss spawn đúng kill threshold
- [ ] Telegraphing hoạt động (laser Boss, vòng đỏ Kamikaze)
- [ ] Screen shake khi bắn Shotgun / nhận sát thương
- [ ] Game Over hiển thị kill count + level
- [ ] Nút Restart load lại scene đúng
- [ ] Nút Main Menu về đúng scene
- [ ] Âm thanh phát đúng cho các sự kiện

---

## 📊 TRẠNG THÁI DỰ ÁN HIỆN TẠI

### Đã hoàn thành ✅

- Di chuyển và ngắm bắn cơ bản
- Hệ thống vũ khí (Pistol, Shotgun, Sword)
- AI kẻ địch (Melee, Ranged, Kamikaze)
- Hệ thống Boss với attack patterns
- Hệ thống lên cấp và nâng cấp
- Vật phẩm và pickup
- Object Pooling (đạn, kẻ địch)
- Hệ thống âm thanh
- UI / HUD đầy đủ
- Game Over và Restart

### Đang phát triển 🔨

- Procedural Map Generation (`MapGenerator.cs`)
- Advanced Boss phase 2/3
- Hoàn thiện bảng Mutation/Perk (10+ thẻ)
- FeedbackManager (Screen Shake, Hit-Stop)
- Pause Menu (ESC)
- Object Pool cho EXP Orb và DamagePopup

### Định hướng tương lai 🎯

- Meta-progression (unlock giữa các run)
- Thêm loại vũ khí (RPG, Laser…)
- Mini-boss và Boss phase
- Special events (meteor shower, elite enemies)
- Bảng xếp hạng (Leaderboard)
- Achievements / Trophies

---

## 📝 QUY ƯỚC CODE

| Loại           | Quy ước            | Ví dụ                              |
| --------------- | -------------------- | ------------------------------------ |
| Class           | PascalCase           | `PlayerController`, `BulletPool` |
| Method          | PascalCase           | `TryShoot()`, `OnTriggerEnter()` |
| Variable        | camelCase            | `currentEXP`, `moveSpeed`        |
| Constant        | UPPER_SNAKE_CASE     | `MAX_LEVEL`, `DEFAULT_DAMAGE`    |
| Inspector field | `[SerializeField]` | `[SerializeField] float damage;`   |

- Physics code → `FixedUpdate`
- Input + Logic → `Update`
- Khởi tạo → `Awake` (Singleton) hoặc `Start` (setup)
- Mỗi file một class (hoặc các class liên quan nhỏ)

---

## 🔖 SNIPPETS NHANH

```csharp
// Ghi nhận kill
GameManager.Instance.RegisterKill();

// Gây sát thương người chơi
PlayerStats.Instance.TakeDamage(damage);

// Spawn vật phẩm
ItemDropManager.Instance.SpawnDrop(position);

// Dừng / tiếp tục game
Time.timeScale = 0f; // dừng
Time.timeScale = 1f; // tiếp tục

// Phát âm thanh
AudioManager.Instance.PlaySound("shoot");

// Lấy / trả đạn từ pool
Bullet b = ObjectPool.Instance.GetBullet();
ObjectPool.Instance.ReturnBullet(b);

// Kích hoạt Screen Shake
FeedbackManager.Instance.TriggerScreenShake(0.5f);

// Kích hoạt Hit-Stop
FeedbackManager.Instance.TriggerHitStop(0.05f);
```
