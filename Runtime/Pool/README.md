# Unity Core Object Pooler

Hệ thống Object Pooler hiệu năng cao, tối ưu tuyệt đối bộ nhớ và triệt tiêu giật lag (GC Spikes) cho Unity (Mobile / PC / WebGL).

---

## 🚀 Các Tính Năng Nổi Bật

1. **Prewarm Nạp Sẵn**:
   - Khởi tạo trước số lượng object mong muốn lúc tải game hoặc vào Scene để tránh đóng băng khung hình (freeze) khi Spawn lần đầu.
2. **Giao Diện Ngắn Gọn Qua Extension Methods**:
   - `prefab.Spawn(position, rotation, parent)`
   - `instance.Despawn(delaySeconds)`
   - `prefab.Prewarm(count)`
3. **Despawn Trì Hoãn (Delayed Despawn)**:
   - Tự động trả về Pool sau một khoảng thời gian: `enemy.Despawn(2.0f)`.
4. **Phân Nhóm Gọn Gàng Trong Hierarchy**:
   - Tự động gom các object theo từng Prefab: `[UnityCore_PoolRoot] / Pool_Enemy (Pooled)`, không làm rối cửa sổ Hierarchy trong Play Mode.
5. **Giao Diện `IPoolable`**:
   - Tự động gọi `OnSpawn()` khi object được lấy ra từ Pool (reset máu, hồi sinh, bật lại collider).
   - Tự động gọi `OnDespawn()` khi object bị thu hồi về Pool.
6. **Component Tiện Ích `AutoDespawn`**:
   - Gắn lên Đạn (Bullets), Hiệu ứng kỹ năng (FX/VFX), Floating Damage Text, v.v. để tự động trả về Pool sau khi hết thời gian hoặc khi Particle / Audio phát xong.
7. **Component `PoolPrewarmManager`**:
   - Cho phép thiết lập danh sách Prefab và số lượng nạp sẵn trực tiếp trên Inspector của Scene.

---

## 🛠️ Hướng Dẫn Sử Dụng

### 1. Spawn & Despawn Cơ Bản

```csharp
using UnityEngine;
using Unity.Core.Pool;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;

    private void Shoot()
    {
        // 1. Spawn từ Pool
        GameObject bullet = _bulletPrefab.Spawn(transform.position, transform.rotation);

        // 2. Despawn sau 3 giây
        bullet.Despawn(3.0f);
    }
}
```

---

### 2. Spawn Trả Về Component Mạnh Kiểu (Generic)

```csharp
[SerializeField] private EnemyAIBehaviour _enemyPrefab;

private void SpawnEnemy(Vector3 spawnPos)
{
    // Trả về trực tiếp kiểu EnemyAIBehaviour mà không cần GetComponent
    EnemyAIBehaviour enemy = _enemyPrefab.Spawn(spawnPos, Quaternion.identity);
    enemy.MoveSpeed = 4.0f;
}
```

---

### 3. Tự Động Reset Khi Spawn Với `IPoolable`

```csharp
using UnityEngine;
using Unity.Core.Pool;

public class Monster : MonoBehaviour, IPoolable
{
    private float _hp;

    public void OnSpawn()
    {
        // Reset máu và trạng thái khi lấy ra từ Pool
        _hp = 100f;
        Debug.Log("Monster hồi sinh từ Pool!");
    }

    public void OnDespawn()
    {
        // Dọn dẹp trước khi trả về Pool
        Debug.Log("Monster quay về Pool.");
    }
}
```

---

### 4. Nạp Sẵn Trong Code (Prewarm)

```csharp
private void Awake()
{
    // Nạp sẵn 100 viên đạn và 20 quái vào Pool
    _bulletPrefab.Prewarm(100);
    _enemyPrefab.Prewarm(20);
}
```

---

### 5. Sử Dụng `AutoDespawn` Cho Hiệu Ứng / Đạn / Âm Thanh

1. Gắn component `AutoDespawn` vào Prefab hiệu ứng / đạn.
2. Thiết lập `_lifetime = 2.5s` hoặc tích chọn `_despawnOnParticleStopped = true`.
3. Khi Spawn, component sẽ tự động trả object về Pool khi hoàn tất.
