# Unity Core Services Architecture (Pluggable SDK Bridge)

Kiến trúc **Dịch vụ Cắm-Rút Độc Lập (Pluggable Service Bridge & Null-Object Pattern)** cho phép code gameplay và UI gọi trực tiếp các hàm **Quảng cáo (Ads)**, **Phân tích (Analytics)**, **Theo dõi doanh thu (Tracking)** và **Cấu hình từ xa (Remote Config)** mà **KHÔNG BỊ LỖI** dù dự án chưa cài đặt bất kỳ SDK bên thứ 3 nào (như AppLovin MAX, AdMob, Firebase, Adjust, AppsFlyer).

---

## 🚀 Nguyên Lý Hoạt Động

```
  Gameplay / UI Code
        │
        ├── AdsService.ShowRewarded("revive", callback)
        ├── AnalyticsService.LogEvent("level_start")
        └── RemoteConfigService.GetValue("speed", 5)
        │
  ┌─────▼─────────────────────────────────────────────────┐
  │           Unity Core Services Layer                   │
  │  (Tự động Mock / Fallback an toàn khi chưa có SDK)    │
  └─────┬─────────────────────────────────────────────────┘
        │
        ├── Khi CHƯA cài SDK: Tự động Mock (In log Console, trả reward giả lập)
        │
        └── Khi CÀI SDK (Max/Firebase...): Tự động định tuyến sang SDK thật
```

---

## 🏛️ Cấu Trúc Thư Mục

```
Unity.Core/Runtime/Services/
├── Core/
│   ├── IService.cs                  // Base interface cho toàn bộ Service
│   ├── ServiceRegistry.cs           // Service Locator / Registry trung tâm
│   └── ServiceType.cs               // Enum các loại SDK bên thứ 3
├── Ads/
│   ├── IAdsService.cs               // Giao diện chuẩn cho Quảng cáo
│   ├── AdsService.cs                // Cổng gọi Ads (có sẵn Mock Provider tự động trả thưởng khi test)
│   └── Models/
│       ├── AdsSettings.cs           // Cấu hình tần suất, giãn cách quảng cáo
│       └── AdRevenueInfo.cs         // Dữ liệu doanh thu quảng cáo (Impression-level Revenue)
├── Analytics/
│   ├── IAnalyticsProvider.cs        // Giao diện chuẩn cho Analytics
│   └── AnalyticsService.cs          // Cổng phát đa điểm (Broadcaster) sang Firebase, GameAnalytics...
├── Tracking/
│   ├── ITrackingProvider.cs         // Giao diện chuẩn cho Attribution & Revenue Tracking
│   └── TrackingService.cs           // Cổng phát doanh thu sang Adjust, AppsFlyer...
└── RemoteConfig/
    ├── IRemoteConfigProvider.cs     // Giao diện chuẩn cho Remote Config
    └── RemoteConfigService.cs       // Cổng đọc cấu hình với giá trị mặc định an toàn
```

---

## 🛠️ Hướng Dẫn Sử Dụng Trong Game

### 1. Gọi Quảng Cáo (Ads)

```csharp
using UnityEngine;
using Unity.Core.Services.Ads;

public class RevivePopup : MonoBehaviour
{
    public void OnWatchAdsClicked()
    {
        // Khi chưa cài SDK: Tự động gọi callback(true) và in log Console
        // Khi đã cài SDK (MAX/AdMob): Hiển thị video quảng cáo thật
        AdsService.ShowRewarded("revive_hero", (isSuccess) =>
        {
            if (isSuccess)
            {
                Debug.Log("Hồi sinh thành công!");
            }
            else
            {
                Debug.Log("Quảng cáo bị tắt hoặc chưa hoàn tất.");
            }
        });
    }

    public void OnLevelFinished()
    {
        // Hiển thị quảng cáo chuyển cảnh
        AdsService.ShowInterstitial("level_finished", () =>
        {
            Debug.Log("Tiếp tục sang màn kế tiếp.");
        });
    }
}
```

---

### 2. Gửi Sự Kiện Phân Tích (Analytics)

```csharp
using System.Collections.Generic;
using Unity.Core.Services.Analytics;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // Gửi sự kiện có tham số (tự động phát tới mọi Provider đang đăng ký)
        AnalyticsService.LogEvent("level_start", new Dictionary<string, object>
        {
            ["level_index"] = 1,
            ["difficulty"] = "normal",
            ["hero_id"] = "warrior_01"
        });
    }
}
```

---

### 3. Đọc Cấu Hình Từ Xa (Remote Config)

```csharp
using Unity.Core.Services.RemoteConfig;

public class EnemySpawner : MonoBehaviour
{
    private void Start()
    {
        // Tự động trả về giá trị mặc định (5.0f) nếu chưa có SDK hoặc mất mạng
        float enemySpeed = RemoteConfigService.GetValue<float>("enemy_move_speed", 5.0f);
        int waveCount = RemoteConfigService.GetValue<int>("total_waves", 10);
    }
}
```

---

### 4. Tích Hợp Package SDK Mới (Ví dụ: Package AppLovin MAX / Firebase)

Khi bạn tạo một package con hoặc tải plugin SDK về dự án, chỉ cần viết một class Adapter triển khai Interface tương ứng và đăng ký lúc khởi động game:

```csharp
// Trong package com.unity.ads.max
public class MaxAdsAdapter : IAdsService
{
    public bool IsInitialized => MaxSdk.IsInitialized();
    public bool CanShowRewarded => MaxSdk.IsRewardedAdReady("ad_unit_id");
    // ... Triển khai các hàm SDK ...

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoRegister()
    {
        // Đăng ký vào hệ thống - Toàn bộ code game sẽ tự động dùng SDK MAX!
        AdsService.Register(new MaxAdsAdapter());
    }
}
```
*(Code gameplay và UI của bạn **không cần sửa một dòng nào**!)*
