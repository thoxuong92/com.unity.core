# Unity Core Game Data & Save System

Hệ thống quản lý và lưu trữ dữ liệu game an toàn, đa nền tảng, thiết kế chuẩn mực theo mô hình **Service Registry / Data Handler Pattern** giúp **loại bỏ hoàn toàn Singleton phân tán** trong dự án.

---

## 🏛️ Cấu Trúc Thư Mục Chuẩn Hóa

```
Unity.Core/Runtime/Data/
├── Core/
│   ├── IDataStorage.cs              // Interface chuẩn IDataStorage<T>
│   ├── DataHandler.cs               // Base DataHandler<T> (vòng đời Auto-Save, quản lý Model)
│   ├── DataManager.cs               // Service Registry trung tâm (Get<THandler>, Data<TModel>, SaveAll)
│   └── DataCrypto.cs                // Tiện ích mã hóa bảo mật AES-256 (Anti-Cheat)
├── Storages/
│   ├── PlayerPrefsStorage.cs        // Lưu trữ PlayerPrefs (nhanh, nhẹ)
│   └── FileStorage.cs               // Lưu trữ file JSON (Application.persistentDataPath, Atomic Write & Backup)
├── Models/
│   ├── PlayerProfile.cs             // Model hồ sơ người chơi (Coins, Gems, Level, Sound...)
│   └── GameSettings.cs              // Model cài đặt game (Volume, Graphic, Quality, Language...)
└── Handlers/
    └── PlayerDataHandler.cs         // Handler xử lý nghiệp vụ người chơi (AddCoins, SpendGems, SetSound...)
```

---

## 🚀 Các Tính Năng Cốt Lõi

1. **Chuẩn Hóa Tên Gọi & Logic Nghiệp Vụ**:
   - **`DataHandler<T>`**: Đối tượng nắm giữ và xử lý nghiệp vụ dữ liệu cho Model `T`.
   - **`DataManager`**: Service Registry trung tâm quản lý mọi Handler và Data trong game mà không cần Singleton phân tán.
   - **`IDataStorage<T>`**: Interface trừu tượng hóa phương thức lưu trữ (PlayerPrefs, File, Cloud...).
2. **Không Dùng Singleton Phân Tán**:
   - `DataManager.Get<PlayerDataHandler>().AddCoins(100);`
   - `DataManager.Get<PlayerDataHandler>().SetSound(true);`
3. **Truy Cập Nhanh Trực Tiếp Cho Mọi Model C#**:
   - `PlayerProfile profile = DataManager.Data<PlayerProfile>();`
   - `profile.Coins += 100;`
   - `DataManager.Save<PlayerProfile>();`
4. **Lưu Trữ File JSON An Toàn (`FileStorage<T>`)**:
   - Ghi file nguyên tử (Atomic Write qua file `.tmp`) chống corrupt dữ liệu khi crash/tắt nguồn đột ngột.
   - Tự động tạo bản sao lưu phục hồi (`.bak`).
5. **Mã Hóa AES-256 Chống Gian Lận (`DataCrypto`)**:
   - Bảo vệ file save trước các phần mềm can thiệp/hack cheat trên Android, iOS, PC.
6. **Cửa Sổ Quản Lý Trong Unity Editor (`GameDataEditorWindow`)**:
   - Menu: **`Unity Core > Game Data Manager`**
   - Xem và chỉnh sửa trực tiếp các thông số save game trong Editor.

---

## 🛠️ Hướng Dẫn Sử Dụng

### 1. Thao Tác Nghiệp Vụ Người Chơi Qua `PlayerDataHandler`

```csharp
using UnityEngine;
using Unity.Core.Data;

public class GameFlowController : MonoBehaviour
{
    private void Start()
    {
        // Đọc dữ liệu người chơi
        long coins = DataManager.Get<PlayerDataHandler>().Coins;
        int level = DataManager.Get<PlayerDataHandler>().Level;

        Debug.Log($"Level: {level}, Coins: {coins}");
    }

    public void OnLevelFinished()
    {
        // Thêm thưởng & tự động lưu
        DataManager.Get<PlayerDataHandler>().AddCoins(500);
        DataManager.Get<PlayerDataHandler>().AddGems(10);
        DataManager.Get<PlayerDataHandler>().SetLevel(2);
    }
}
```

---

### 2. Truy Cập Nhanh Cho Bất Kỳ Model Nào

```csharp
// Đọc & Ghi Model trực tiếp
var profile = DataManager.Data<PlayerProfile>();
profile.Coins += 200;
profile.Sound = false;

// Lưu lại
DataManager.Save<PlayerProfile>();
```

---

### 3. Tạo Thêm Data Model & Handler Mới

#### Bước 1: Tạo Model dữ liệu (trong thư mục `Models/`)
```csharp
using System;
using System.Collections.Generic;

namespace MyGame.Data
{
    [Serializable]
    public class InventoryData
    {
        public List<string> ItemIds = new List<string>();
        public int SlotCapacity = 20;
    }
}
```

#### Bước 2: Tạo Handler xử lý nghiệp vụ (trong thư mục `Handlers/`)
```csharp
using Unity.Core.Data;

namespace MyGame.Data
{
    public class InventoryDataHandler : DataHandler<InventoryData>
    {
        public InventoryDataHandler() : base("Inventory", new FileStorage<InventoryData>("Inventory.json", encrypt: true))
        {
        }

        public bool AddItem(string itemId)
        {
            if (Data.ItemIds.Count >= Data.SlotCapacity) return false;
            Data.ItemIds.Add(itemId);
            Save();
            return true;
        }
    }
}
```

#### Bước 3: Sử dụng ở bất kỳ đâu trong game
```csharp
DataManager.Get<InventoryDataHandler>().AddItem("sword_01");
```

---

## 📂 Quản Lý Save Game Trong Unity Editor

1. Mở menu **`Unity Core > Game Data Manager`**.
2. Chọn loại Storage (`PlayerPrefs` hoặc `PersistentFile`).
3. Chỉnh sửa các trường dữ liệu và bấm **`💾 Lưu Thay Đổi (Save)`**.
4. Bấm **`📂 Mở Thư Mục Persistent Data`** để mở ngay thư mục chứa file save trên máy tính.
