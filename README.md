# Unity Core Framework (`com.unity.core`)

Gói framework nền tảng dành cho các dự án game Unity, hỗ trợ cả Android và iOS.

## Tính Năng Chính
1. **`ServiceRegistry`**: Service Locator tốc độ cao, hỗ trợ giải phóng bộ nhớ và vòng đời `IService`.
2. **`EventBus`**: Message hub phi tập trung theo chuẩn Type-Safe (hỗ trợ struct/class implementing `IEvent`), triệt tiêu phụ thuộc chéo giữa các module.
3. **`GameStateMachine`**: Quản lý State Flow của game (Boot -> Splash -> MainMenu -> Gameplay -> GameOver).
4. **`ObjectPooler`**: Pool tái sử dụng GameObject & Component chống rác bộ nhớ (GC Allocation Spikes) trên thiết bị di động.
5. **`AppLogger`**: Hệ thống logging có điều kiện `[Conditional("ENABLE_UNITY_CORE_LOG")]`, tự động xóa sạch log khi build Release để an toàn trên Store.
6. **`PackageInstallerWizard`** (Editor Window): Tích hợp trực tiếp tại `Unity Core > Package Manager Hub` để cài/gỡ các module qua Git URL.
7. **`AccountSafetyScaffolder`** (Editor Window): Quét hardcoded API Key, kiểm tra IL2CPP Stripping, cấu hình an toàn cho nhiều tài khoản Google Play / App Store.
8. **`Splash` & Bootstrapping**: Quản lý quy trình nạp khởi đầu (Progress Bar, Remote Config, App Open Ad first open, Internet connectivity check, Auto Banner & Scene Transition) tương thích hoàn toàn với hệ thống WASD.

## Hướng Dẫn Cài Đặt Qua UPM (Unity Package Manager)
Thêm vào file `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.unity.core": "https://github.com/thoxuong92/com.unity.core.git#v1.0.0"
  }
}
```

## Ví Dụ Sử Dụng Nhanh

### 1. Đăng ký và gọi Service
```csharp
using Unity.Core.Services;

// Đăng ký Service
ServiceRegistry.Register<IMyService>(new MyService());

// Gọi Service từ bất kỳ đâu
var myService = ServiceRegistry.Get<IMyService>();
```

### 2. Bắn & Lắng nghe Event qua EventBus
```csharp
using Unity.Core.Events;

// Định nghĩa Event
public struct ScoreChangedEvent : IEvent
{
    public int NewScore;
}

// Lắng nghe Event
EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);

private void OnScoreChanged(ScoreChangedEvent evt)
{
    Debug.Log($"Score: {evt.NewScore}");
}

// Bắn Event
EventBus.Publish(new ScoreChangedEvent { NewScore = 100 });

// Hủy lắng nghe
EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
```

### 3. GameBootstrapper & Splash Loading
`GameBootstrapper` là lớp thuần C# tự động khởi chạy tại `BeforeSceneLoad` (không kế thừa MonoBehaviour, không dùng Singleton, không scan Assembly, không Find Object).

- **`GameBootstrapper`**: Cấu hình `targetFrameRate = 60`, `multiTouchEnabled = true`, khởi tạo `GameStateMachine` và tự động cập nhật qua Unity PlayerLoop.
- **`Splash`**: Component MonoBehaviour điều phối quá trình loading, Remote Config, App Open Ad đầu tiên và hiển thị Banner khi hoàn tất.

Mọi script UI ở bất kỳ scene nào đều có thể lắng nghe tiến trình qua Static Events:
```csharp
using UnityEngine;
using UnityEngine.UI;
using Unity.Core.Boot;

public class GameSplashUI : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;

    private void OnEnable()
    {
        // Lắng nghe trực tiếp từ Splash qua Static Event (không cần Singleton)
        Splash.OnProgress += OnProgressChanged;
        Splash.OnCompleted += OnSplashCompleted;
    }

    private void OnDisable()
    {
        Splash.OnProgress -= OnProgressChanged;
        Splash.OnCompleted -= OnSplashCompleted;
    }

    private void OnProgressChanged(float percent)
    {
        if (progressSlider != null) progressSlider.value = percent;
    }

    private void OnSplashCompleted()
    {
        Debug.Log("Loading & khởi tạo các package đã hoàn tất!");
    }
}
```
*(Tương thích 100% với code cũ `using WASD; public class Splash : TickBehaviour`).*

