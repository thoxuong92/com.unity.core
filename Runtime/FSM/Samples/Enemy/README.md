# Hướng Dẫn Ví Dụ: Enemy AIBehaviour & Pure C# States

Ví dụ mẫu hoàn chỉnh minh họa kiến trúc **AIBehaviour** và **Pure C# State Nodes** (không gắn component riêng rẽ lên GameObject, tránh rối Inspector/Hierarchy).

---

## 🎮 Cách Thiết Lập Enemy Trong Unity Editor

### Cách 1: Thiết lập tự động 1 Click từ Inspector
1. Tạo một GameObject mới trong Scene (đặt tên là `Enemy`).
2. Gắn component `EnemyAIBehaviour` vào GameObject `Enemy` (chỉ duy nhất 1 component này trên GameObject!).
3. Trong Inspector của `EnemyAIBehaviour`, nhấn nút:
   👉 **`Auto-Setup Enemy AI Preset`** (hoặc mở Visual Graph Editor và bấm `⚡ Setup Enemy Preset`).
4. Xong! Hệ thống sẽ tự động khởi tạo 4 Pure C# States (`EnemyIdleState`, `EnemyMoveState`, `EnemyAttackState`, `EnemyDeadState`), cấu hình các điều kiện chuyển cảnh mặc định và lưu trữ qua `[SerializeReference]`.

---

### Cách 2: Quản lý & Tạo Node bằng Visual Graph Editor
1. Chọn GameObject `Enemy`.
2. Mở cửa sổ **Unity Core > FSM Visual Node Graph** (hoặc bấm `OPEN VISUAL FSM GRAPH EDITOR` trong Inspector).
3. Chuột phải lên Canvas để tạo State mới hoặc kéo thả kết nối các đường chuyển cảnh.
4. Bấm vào đường mũi tên bất kỳ để mở **Transition Inspector** bên phải, tùy chỉnh điều kiện chuyển cảnh hoặc đổi chế độ `Tất cả (AND)` / `Bất kỳ (OR)`.
5. Mọi thay đổi đều tự động đồng bộ và lưu vào component `EnemyAIBehaviour` với đầy đủ Undo/Redo.

---

## 🔍 Điều Kiện Tùy Chỉnh & Nhảy Tới Code (`✎ Edit Code`)

Trong file [`EnemyAIBehaviour.cs`](EnemyAIBehaviour.cs), các hàm kiểm tra được đánh dấu với attribute `[FSMCondition]`:
```csharp
[FSMCondition("Mục tiêu bị choáng (IsStunned)")]
public bool CheckIsStunned() => _isStunned;

[FSMCondition("Nhìn thấy người chơi rõ ràng (HasClearSight)")]
public bool CheckHasClearSight() => Target != null && DistanceToTarget <= _detectRange;
```
- Khi chọn loại `Điều kiện tùy chỉnh (Custom Condition)` trong Inspector / Graph, các hàm này sẽ tự động xuất hiện trong Dropdown.
- Bấm nút **`✎ Edit Code`** cạnh Dropdown để nhảy thẳng đến đúng dòng định nghĩa hàm trong Visual Studio / Rider / VS Code.

---

## ⚡ Live Debug Trong Play Mode

Khi bấm **Play** trong Unity:
1. Inspector và Visual Graph của `EnemyAIBehaviour` sáng trạng thái Active State:
   `● ACTIVE STATE: Idle (2.3s)`
2. Hiển thị sơ đồ luồng chuyển cảnh trực quan:
   ```
   [Idle]        ──(TargetDetected)──►  [Move]
   [Move]        ──(InAttackRange)──►   [Attack]
   [Attack]      ──(TargetDetected)──►  [Move]
   [Move]        ──(TargetLost)──►      [Idle]
   [Any State *] ──(HealthZero)──►      [Dead]
   ```
3. Nút tiện ích debug trong Inspector:
   - **Bấm vào tên State bất kỳ**: Ép chuyển State tức thì.
   - **Damage Enemy (-25 HP)**: Trừ máu để kiểm tra điều kiện phát hiện / chiến đấu.
   - **Kill Enemy (0 HP)**: Ép quái về 0 máu để kiểm tra chuyển sang `EnemyDeadState`.
