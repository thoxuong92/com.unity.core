# Unity Core Framework (`com.unity.core`)

Framework nền tảng chuẩn hóa kiến trúc cho các dự án Unity (Mobile Android & iOS). Cung cấp giải pháp module hóa độc lập, phi tập trung (decoupled) để tích hợp các dịch vụ bên thứ ba (Firebase, AppLovin MAX, Adjust, AppsFlyer) với cơ chế tự động chuyển tiếp, fallback an toàn và không bị phụ thuộc chéo.

---

## 📦 Danh Sách Hệ Thống Package

| Package | Tên Đầy Đủ | Chức Năng Chính | Git Repository |
| :--- | :--- | :--- | :--- |
| **`com.unity.core`** | Unity Core Services Foundation | Service Registry, Event Bus, Secure Storage, Ads/Tracking/RemoteConfig Bridges | `https://github.com/thoxuong92/com.unity.core.git` |
| **`com.unity.firebase`** | Unity Firebase Service | Firebase Analytics, Remote Config Cloud Sync, OAuth2 Editor Tool | `https://github.com/thoxuong92/com.unity.firebase.git` |
| **`com.unity.applovin`** | Unity AppLovin MAX Service | AppLovin MAX Ads (Banner, Interstitial, Rewarded, App Open), ILRD Revenue | `https://github.com/thoxuong92/com.unity.applovin.git` |
| **`com.unity.adjust`** | Unity Adjust Service | Adjust SDK v5, Event Attribution, ILRD Ad Revenue Tracking | `https://github.com/thoxuong92/com.unity.adjust.git` |
| **`com.unity.appsflyer`** | Unity AppsFlyer Service | AppsFlyer SDK v6, Event Attribution, ILRD Ad Revenue Tracking | `https://github.com/thoxuong92/com.unity.appsflyer.git` |

---

## 🚀 Hướng Dẫn Cài Đặt

### Cách 1: Cài đặt tự động qua Hub (Khuyên Dùng)
1. Thêm gói `com.unity.core` vào dự án trước.
2. Trên thanh menu Unity Editor, chọn:  
   **`Unity Core > Package Manager Hub`**
3. Bấm **Install** vào các module dịch vụ bạn cần (Firebase, AppLovin, Adjust, AppsFlyer). Wizard sẽ tự động cấu hình và cập nhật file `Packages/manifest.json`.

### Cách 2: Thêm trực tiếp vào `Packages/manifest.json`
```json
{
  "dependencies": {
    "com.unity.core": "https://github.com/thoxuong92/com.unity.core.git#v1.0.0",
    "com.unity.firebase": "https://github.com/thoxuong92/com.unity.firebase.git#v1.0.0",
    "com.unity.applovin": "https://github.com/thoxuong92/com.unity.applovin.git#v1.0.0",
    "com.unity.adjust": "https://github.com/thoxuong92/com.unity.adjust.git#v1.0.0",
    "com.unity.appsflyer": "https://github.com/thoxuong92/com.unity.appsflyer.git#v1.0.0"
  }
}
```

---

## 💡 Nguyên Tắc Thiết Kế: Zero-Crash Fallback

Mọi dịch vụ (Ads, Tracking, Remote Config, Analytics) trong `com.unity.core` đều tuân thủ nguyên tắc **Mock Provider & Broadcaster**:
- Bạn có thể gọi code quảng cáo, tracking, remote config ở bất kỳ đâu trong gameplay/UI ngay từ ngày đầu tiên làm game.
- **Không bao giờ bị crash hoặc `NullReferenceException`**, kể cả khi dự án **chưa import** bất kỳ SDK hay package 3rd-party nào (hệ thống sẽ tự động in Console log an toàn).
- Khi cài thêm package (như `com.unity.applovin` hay `com.unity.firebase`), adapter thật sự sẽ tự động đăng ký vào `BeforeSceneLoad` và thay thế Mock một cách hoàn toàn trong suốt.

---

## 📖 Hướng Dẫn Sử Dụng Các Dịch Vụ

### 1. Quảng Cáo (Ads Service)

Thư viện hỗ trợ đầy đủ 4 định dạng quảng cáo chính: **Banner**, **Interstitial (Xen kẽ)**, **Rewarded (Trả thưởng)** và **App Open Ad (Mở ứng dụng)**.

```csharp
using UnityEngine;
using Unity.Core.Services;
using Unity.Core.Services.Ads;

public class AdsExample : MonoBehaviour
{
    // ==========================================
    // 1. BANNER
    // ==========================================
    public void DisplayBanner()
    {
        // Hiển thị banner (vị trí mặc định ở dưới đáy màn hình)
        AdsService.ShowBanner("main_menu_banner");
    }

    public void CloseBanner()
    {
        AdsService.HideBanner();
    }

    // ==========================================
    // 2. INTERSTITIAL (Quảng cáo xen kẽ)
    // ==========================================
    public void DisplayInterstitial()
    {
        // Kiểm tra xem quảng cáo đã sẵn sàng và người chơi chưa mua Remove Ads
        if (AdsService.CanShowInterstitial)
        {
            AdsService.ShowInterstitial("end_level", () =>
            {
                Debug.Log("Quảng cáo Interstitial đã đóng -> Chuyển sang màn hình tiếp theo.");
                ProceedToNextLevel();
            });
        }
        else
        {
            // Nếu chưa sẵn sàng hoặc đã Remove Ads, thực hiện tiếp luồng game ngay lập tức
            ProceedToNextLevel();
        }
    }

    // ==========================================
    // 3. REWARDED (Quảng cáo trả thưởng)
    // ==========================================
    public void DisplayRewarded()
    {
        if (AdsService.CanShowRewarded)
        {
            AdsService.ShowRewarded("revive_player", isRewardEarned =>
            {
                if (isRewardEarned)
                {
                    Debug.Log("Người chơi xem hết video -> Nhận 100 Vàng!");
                    AddGold(100);
                }
                else
                {
                    Debug.Log("Người chơi tắt ngang hoặc lỗi tải video.");
                }
            });
        }
        else
        {
            Debug.LogWarning("Video trả thưởng chưa tải xong.");
        }
    }

    // ==========================================
    // 4. APP OPEN AD (Quảng cáo mở app)
    // ==========================================
    public void DisplayAppOpen()
    {
        if (AdsService.CanShowAppOpen)
        {
            AdsService.ShowAppOpen("app_open", () =>
            {
                Debug.Log("App Open Ad đã hoàn thành.");
            });
        }
    }

    // ==========================================
    // 5. CẤU HÌNH REMOVE ADS
    // ==========================================
    public void OnBuyRemoveAdsSuccess()
    {
        // Tắt toàn bộ Interstitial & Banner trong game
        AdsService.Settings.IsRemoveAds = true;
        AdsService.HideBanner();
    }

    private void ProceedToNextLevel() { }
    private void AddGold(int amount) { }
}
```

> **Cách gọi qua Adapter dùng chung:**
> ```csharp
> API.Get<GameAds>().ShowInterstitial("placement", () => { ... });
> API.Get<GameAds>().ShowRewarded("placement", success => { ... });
> ```

---

### 2. Đo Lường & Tracking Doanh Thu (Tracking Service)

Dịch vụ `TrackingService` đóng vai trò là Hub phát sóng (Broadcaster). Khi gọi lệnh track, dữ liệu sẽ tự động được gửi đồng thời tới toàn bộ các provider đang kích hoạt trong dự án (**Adjust**, **AppsFlyer**, **Firebase**).

```csharp
using UnityEngine;
using Unity.Core.Services;
using Unity.Core.Services.Ads;
using Unity.Core.Services.Tracking;

public class TrackingExample : MonoBehaviour
{
    // ==========================================
    // 1. TRACK SỰ KIỆN DOANH THU HOẶC TOKEN
    // ==========================================
    public void TrackInAppPurchase()
    {
        // Gửi sự kiện thanh toán IAP kèm doanh thu và loại tiền tệ
        // Tự động chuyển tiếp đến Adjust SDK và AppsFlyer SDK
        TrackingService.TrackEvent("iap_purchase_pack_1", revenue: 4.99, currency: "USD");
    }

    // ==========================================
    // 2. TRACK IMPRESSION-LEVEL AD REVENUE (ILRD)
    // ==========================================
    // Package 'com.unity.applovin' tự động gọi hàm này mỗi khi có quảng cáo được xem.
    // Bạn cũng có thể gọi thủ công nếu sử dụng hệ thống Ad Network ngoài:
    public void TrackCustomAdRevenue()
    {
        TrackingService.TrackRevenue(new AdRevenueInfo
        {
            Source = "Applovin",
            AdUnitId = "your_ad_unit_id",
            Format = "REWARDED",
            NetworkName = "Google AdMob",
            Placement = "daily_reward",
            Revenue = 0.0125,
            Currency = "USD"
        });
        // Lệnh trên tự động kích hoạt:
        // - Adjust.TrackAdRevenue (Adjust SDK v5)
        // - AppsFlyer.logAdRevenue (AppsFlyer SDK v6)
        // - FirebaseAnalytics.LogEvent("ad_impression", ...)
    }
}
```

> **Cách gọi qua Adapter dùng chung:**
> ```csharp
> API.Get<GameTracking>().TrackEvent("event_token", revenue: 1.99, currency: "USD");
> ```

---

### 3. Cấu Hình Từ Xa (Remote Config Service)

Tích hợp với Firebase Remote Config (qua `com.unity.firebase`). Hỗ trợ fallback cục bộ, cache mã hóa nội bộ và không bao giờ block game nếu thiết bị mất kết nối mạng.

```csharp
using UnityEngine;
using Unity.Core.Services;
using Unity.Core.Services.RemoteConfig;

public class RemoteConfigExample : MonoBehaviour
{
    private void Start()
    {
        // Lấy giá trị cấu hình an toàn (Tự động trả về giá trị mặc định nếu offline)
        int reviveCost = RemoteConfigService.GetValue<int>("revive_cost_gem", defaultValue: 50);
        float moveSpeed = RemoteConfigService.GetValue<float>("player_speed", defaultValue: 5.5f);
        string eventTheme = RemoteConfigService.GetValue<string>("current_event_theme", defaultValue: "Halloween");
        bool isEventActive = RemoteConfigService.GetValue<bool>("enable_pvp_event", defaultValue: false);

        Debug.Log($"Config nạp thành công: Revive={reviveCost}, Theme={eventTheme}");
    }

    // Tải cấu hình mới nhất từ Firebase Server
    public void RefreshConfigFromServer()
    {
        RemoteConfigService.Fetch(isSuccess =>
        {
            if (isSuccess)
            {
                Debug.Log("Đã nạp xong cấu hình mới từ máy chủ!");
                int newCost = RemoteConfigService.GetValue<int>("revive_cost_gem", 50);
            }
            else
            {
                Debug.LogWarning("Không thể tải cấu hình mới, giữ giá trị cache/mặc định.");
            }
        });
    }
}
```

> **Cách gọi qua Adapter dùng chung:**
> ```csharp
> int gem = API.Get<GameRemoteConfig>().GetValue<int>("revive_cost_gem", 50);
> ```

---

### 4. Phân Tích Dữ Liệu Game (Analytics Service)

Cung cấp cổng phân tích độc lập. Khi cài `com.unity.firebase`, toàn bộ sự kiện sẽ được đồng bộ trực tiếp lên Google Firebase Analytics Dashboard.

```csharp
using System.Collections.Generic;
using UnityEngine;
using Unity.Core.Services;
using Unity.Core.Services.Analytics;

public class AnalyticsExample : MonoBehaviour
{
    // Bắn sự kiện đơn giản
    public void LogGameStart()
    {
        AnalyticsService.LogEvent("game_started");
    }

    // Bắn sự kiện kèm tham số chi tiết
    public void LogLevelCompleted(int levelIndex, int stars, float playDurationSeconds)
    {
        var parameters = new Dictionary<string, object>
        {
            { "level", levelIndex },
            { "stars_earned", stars },
            { "time_spent", playDurationSeconds },
            { "platform", Application.platform.ToString() }
        };

        AnalyticsService.LogEvent("level_completed", parameters);
    }

    // Gán thông tin người chơi (User Property & ID)
    public void SetupUserProfile(string userId, string vipStatus)
    {
        AnalyticsService.SetUserId(userId);
        AnalyticsService.SetUserProperty("vip_rank", vipStatus);
    }
}
```

> **Cách gọi qua Adapter dùng chung:**
> ```csharp
> API.Get<GameAnalytics>().Log("event_name", parameters);
> ```

---

### 5. Lưu Trữ Dữ Liệu An Toàn Chống Hack (`SecurePlayerPrefs`)

Thay thế cho `PlayerPrefs` thông thường để bảo vệ các chỉ số nhạy cảm (Tiền, Vàng, Level, Thời gian) khỏi các ứng dụng can thiệp bộ nhớ hoặc chỉnh sửa file Save trên Android/iOS:
- Mã hóa dữ liệu bằng thuật toán mã hóa khóa động.
- Kiểm tra tính toàn vẹn (Integrity Check MD5 Tamper-Proof). Nếu dữ liệu bị chỉnh sửa từ bên ngoài, hệ thống sẽ phát hiện và tự động trả về giá trị mặc định.

```csharp
using UnityEngine;
using Unity.Core.Protected;

public class StorageExample : MonoBehaviour
{
    public void SaveGameData()
    {
        SecurePlayerPrefs.SetInt("PlayerCoins", 9999);
        SecurePlayerPrefs.SetFloat("HighScore", 1250.5f);
        SecurePlayerPrefs.SetString("AuthToken", "secure_secret_token_123");
        SecurePlayerPrefs.SetBool("SoundEnabled", true);

        // Lưu xuống đĩa (nếu SecurePlayerPrefs.AutoSave = true thì không cần gọi Save thủ công)
        SecurePlayerPrefs.Save();
    }

    public void LoadGameData()
    {
        int coins = SecurePlayerPrefs.GetInt("PlayerCoins", defaultValue: 0);
        float score = SecurePlayerPrefs.GetFloat("HighScore", defaultValue: 0f);
        string token = SecurePlayerPrefs.GetString("AuthToken", defaultValue: "");
        bool sound = SecurePlayerPrefs.GetBool("SoundEnabled", defaultValue: true);

        Debug.Log($"Data đã nạp: Coins = {coins}, Score = {score}");
    }
}
```

---

### 6. Khởi Động Game & Splash Flow (`Splash.cs`)

Script `Unity.Core.Boot.Splash` (hoặc `GameFramework.Splash`) điều phối toàn bộ quy trình khởi chạy game chuẩn hóa:
1. Kiểm tra kết nối Internet.
2. Tải Firebase Remote Config.
3. Kích hoạt App Open Ad đầu tiên (First-Open Ad) nếu được bật.
4. Tự động hiển thị Banner quảng cáo sau khi nạp xong.
5. Chuyển scene sang màn hình chính (`MainMenu`).

Gắn component `Splash` vào GameObject trong scene khởi đầu và lắng nghe sự kiện từ UI Slider / Text:

```csharp
using UnityEngine;
using UnityEngine.UI;
using Unity.Core.Boot;

public class GameSplashUI : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text statusText;

    private void OnEnable()
    {
        // Lắng nghe sự kiện nạp tĩnh từ Splash (không phụ thuộc Singleton)
        Splash.OnProgress += UpdateProgressBar;
        Splash.OnCompleted += OnLoadFinished;
    }

    private void OnDisable()
    {
        Splash.OnProgress -= UpdateProgressBar;
        Splash.OnCompleted -= OnLoadFinished;
    }

    private void UpdateProgressBar(float percent)
    {
        if (progressBar != null) progressBar.value = percent;
        if (statusText != null) statusText.text = $"Đang tải... {(int)(percent * 100)}%";
    }

    private void OnLoadFinished()
    {
        Debug.Log("Splash hoàn tất! Chuẩn bị vào game.");
    }
}
```

---

## 🛠 Hỗ Trợ Công Cụ Editor
- **Package Manager Hub**: Truy cập tại menu `Unity Core > Package Manager Hub` để quản lý các package dịch vụ.
- **Firebase OAuth2 Remote Config Sync**: Truy cập tại menu `Unity Firebase > Remote Config Tool` để chỉnh sửa và đồng bộ cấu hình từ xa trực tiếp từ Unity Editor.
- **AppLovin Settings Window**: Truy cập tại menu `Unity AppLovin > Settings` để cấu hình Ad Unit ID và mạng quảng cáo trung gian.
- **Adjust Window**: Truy cập tại menu `Unity Adjust > Settings` để cấu hình App Token và môi trường (Sandbox / Production).
- **AppsFlyer Window**: Truy cập tại menu `Unity AppsFlyer > Settings` để nhập Dev Key và App ID.

---

## 📄 Bản Quyền
Phát triển và bảo trì bởi **thoxuong92**. Phát hành dưới giấy phép MIT.
