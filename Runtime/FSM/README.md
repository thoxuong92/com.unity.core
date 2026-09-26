# Unity Core Finite State Machine (FSM & HFSM)

Hệ thống Finite State Machine & Visual Graph Editor hoàn chỉnh, hiệu năng cao dành cho quản lý Game Flow, AI quái/nhân vật, và phân luồng trạng thái phức tạp trong Unity.

---

## 🚀 Các Tính Năng Cốt Lõi

1. **Full Lifecycle Hooks**: `OnEnter(prev)`, `OnUpdate(dt)`, `OnLateUpdate(dt)`, `OnFixedUpdate(fixedDt)`, `OnExit(next)`.
2. **State Duration Tracking**: Thuộc tính `StateTime` tự động đếm thời gian state đang hoạt động.
3. **Pure C# State Architecture (Không Gắn Component Rối GameObject)**:
   - Các State kế thừa `VisualStateNode` hoặc `VisualStateNode<T>` là các class `[Serializable]` thuần C#.
   - Chỉ duy nhất **1 component `AIBehaviour`** trên GameObject, quản lý danh sách State qua `[SerializeReference]`.
   - Truy cập trực tiếp thuộc tính `Actor`, `Transform`, `GameObject` mạnh kiểu (strongly-typed) mà không cần `GetComponent`.
4. **Hệ Thống Đa Điều Kiện Chuyển Đổi (Multi-Conditions & Logic AND / OR)**:
   - **Tất cả (AND)**: Tất cả các điều kiện con phải cùng thỏa mãn thì mới chuyển State.
   - **Bất kỳ (OR)**: Chỉ cần 1 trong các điều kiện con thỏa mãn là chuyển State.
   - Tích hợp sẵn các điều kiện thường dùng: `AlwaysTrue`, `TimerExpired`, `TargetDetected`, `InAttackRange`, `TargetLost`, `HealthZero`, `HealthBelowThreshold`, `DistanceLessThan`, `DistanceGreaterThan`, `CustomCondition`.
5. **Điều Kiện Tùy Chỉnh Thông Minh (`[FSMCondition]`) & Nhảy Tới Code (`✎ Edit Code`)**:
   - Đánh dấu hàm kiểm tra bằng attribute `[FSMCondition("Tên hiển thị")]`.
   - Inspector & Graph Editor tự động quét và hiển thị thành Dropdown danh sách thân thiện.
   - Bấm nút **`✎ Edit Code`** để mở ngay IDE (Visual Studio / Rider / VS Code) nhảy thẳng tới dòng định nghĩa hàm.
   - Tự động cache C# Delegate khi Awake, đạt hiệu suất tối đa (0 Garbage Collection Allocation).
6. **Animator-Style Visual Graph Editor**:
   - Mở qua menu `Unity Core > FSM Visual Node Graph` hoặc nút `OPEN VISUAL FSM GRAPH EDITOR`.
   - Giới hạn phạm vi (Scope): Tự động định vị thư mục chứa AIBehaviour và chỉ hiển thị các State trong thư mục đó.
   - Zoom mượt bằng con lăn chuột (40% - 200%), kéo thả Node, pan canvas.
   - Mũi tên chống chồng chéo, có **Huy hiệu (Badge)** hiển thị tóm tắt điều kiện trực tiếp trên đường nối.
   - Bảng **Transition Inspector Sidebar** bên phải Canvas cho phép chỉnh sửa toàn diện các điều kiện chuyển cảnh.
7. **Truyền Dữ Liệu Type-Safe Vào State**: Kế thừa `BaseStateWithData<TData>` để truyền payload khi chuyển state (`ChangeState<MyState, LevelData>(data)`).
8. **Hierarchical State Machine (HFSM)**: Bản thân `StateMachine` implement `IState`, cho phép lồng ghép State Machine con vào State Machine cha.
9. **ActionState**: Tạo nhanh state inline bằng Action / Lambda mà không cần tạo file class mới.

---

## 🛠️ Hướng Dẫn Sử Dụng

### 1. Tạo Nhanh Script Bằng Menu Chuột Phải
- Chuột phải trong cửa sổ **Project** ➔ `Create > FSM > State` để tạo class State kế thừa `VisualStateNode`.
- Chuột phải trong cửa sổ **Project** ➔ `Create > FSM > AIBehaviour` để tạo AI Behaviour component.
- Tên class C# sẽ tự động cập nhật khớp với tên file bạn nhập.

---

### 2. Định Nghĩa State Kế Thừa `VisualStateNode<T>`
```csharp
using UnityEngine;
using Unity.Core.FSM.Visual;

public class EnemyMoveState : VisualStateNode<EnemyAIBehaviour>
{
    public override void OnEnter(IState previousState)
    {
        base.OnEnter(previousState);
        Debug.Log($"{Actor.name} bắt đầu di chuyển về phía mục tiêu.");
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);

        if (Actor != null && Actor.Target != null)
        {
            Vector3 dir = (Actor.Target.position - Transform.position).normalized;
            Transform.position += dir * (Actor.MoveSpeed * deltaTime);
        }
    }
}
```

---

### 3. Tạo Điều Kiện Tùy Chỉnh Với `[FSMCondition]`
Trong component AI (ví dụ `EnemyAIBehaviour.cs`):
```csharp
public class EnemyAIBehaviour : AIBehaviour
{
    [SerializeField] private bool _isStunned = false;

    // Đánh dấu hàm kiểm tra điều kiện
    [FSMCondition("Mục tiêu bị choáng (IsStunned)")]
    public bool CheckIsStunned()
    {
        return _isStunned;
    }

    [FSMCondition("Nhìn thấy người chơi rõ ràng (HasClearSight)")]
    public bool CheckHasClearSight()
    {
        return Target != null && !Physics.Linecast(transform.position, Target.position);
    }
}
```
- Khi chọn `Điều kiện tùy chỉnh (Custom Condition)` trên Inspector hoặc Graph, Dropdown sẽ tự động xuất hiện các hàm này.
- Bấm nút **`✎ Edit Code`** cạnh Dropdown để nhảy thẳng đến code của hàm.

---

### 4. Kết Hợp Nhiều Điều Kiện (Multi-Conditions & Logic AND / OR)
Trong Graph Editor hoặc Inspector:
- Chuyển **Chế độ kết hợp**:
  - `Tất cả (AND)`: Thỏa mãn đồng thời tất cả điều kiện con (ví dụ: `Target Detected` VÀ `Timer >= 1.5s`).
  - `Bất kỳ (OR)`: Thỏa mãn 1 trong các điều kiện (ví dụ: `Target Lost` HOẶC `Health <= 0`).
- Bấm **`+ Thêm Điều Kiện`** để bổ sung thêm các điều kiện con.

---

### 5. Sử Dụng C# StateMachine Thuần Túy (Không Visual)
```csharp
using UnityEngine;
using Unity.Core.FSM;

public class CustomAI : MonoBehaviour
{
    private StateMachine _fsm;
    private float _health = 100f;
    private bool _hasTarget = false;

    private void Awake()
    {
        _fsm = new StateMachine();

        var patrol = new PatrolState();
        var chase = new ChaseState();
        var dead = new DeadState();

        // Chuyển có điều kiện
        _fsm.AddTransition(patrol, chase, () => _hasTarget);
        _fsm.AddTransition(chase, patrol, () => !_hasTarget);
        _fsm.AddAnyTransition(dead, () => _health <= 0);

        _fsm.DefaultState = patrol;
    }

    private void Update()
    {
        _fsm.OnUpdate(Time.deltaTime);
    }
}
```

---

### 6. Truyền Dữ Liệu Type-Safe (`IStateWithData<T>`)
```csharp
public class GameOverData
{
    public int FinalScore;
    public bool IsNewHighscore;
}

public class GameOverState : BaseStateWithData<GameOverData>
{
    public override void OnEnter(IState previousState, GameOverData data)
    {
        Debug.Log($"Game Over! Điểm: {data.FinalScore}, Kỷ lục mới: {data.IsNewHighscore}");
    }
}

// Chuyển state kèm payload
fsm.ChangeState<GameOverState, GameOverData>(new GameOverData 
{ 
    FinalScore = 1500, 
    IsNewHighscore = true 
});
```
