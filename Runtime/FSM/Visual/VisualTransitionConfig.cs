using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Core.FSM.Visual
{
    /// <summary>
    /// Các loại điều kiện chuyển trạng thái FSM rõ ràng, tường minh.
    /// </summary>
    public enum TransitionConditionType
    {
        [InspectorName("Luôn luôn chuyển (Always True)")]
        AlwaysTrue = 0,

        [InspectorName("Hết thời gian chờ (Timer Expired)")]
        TimerExpired = 1,

        [InspectorName("Phát hiện mục tiêu (Target Detected)")]
        TargetDetected = 2,

        [InspectorName("Trong tầm đánh (In Attack Range)")]
        InAttackRange = 3,

        [InspectorName("Mất dấu mục tiêu (Target Lost)")]
        TargetLost = 4,

        [InspectorName("Hết máu (Health <= 0)")]
        HealthZero = 5,

        [InspectorName("Máu dưới mức ngưỡng (Health < Threshold)")]
        HealthBelowThreshold = 6,

        [InspectorName("Khoảng cách gần hơn ngưỡng (Distance < Threshold)")]
        DistanceLessThan = 7,

        [InspectorName("Khoảng cách xa hơn ngưỡng (Distance > Threshold)")]
        DistanceGreaterThan = 8,

        [InspectorName("Điều kiện tùy chỉnh (Custom Condition)")]
        CustomCondition = 9
    }

    /// <summary>
    /// Chế độ kết hợp nhiều điều kiện chuyển đổi trạng thái.
    /// </summary>
    public enum ConditionLogicMode
    {
        [InspectorName("Tất cả (AND)")]
        All = 0, // Tất cả các điều kiện phải đúng

        [InspectorName("Bất kỳ (OR)")]
        Any = 1  // Chỉ cần 1 trong các điều kiện đúng
    }

    /// <summary>
    /// Từng điều kiện con trong một Transition.
    /// </summary>
    [Serializable]
    public class TransitionConditionItem
    {
        [Tooltip("Loại điều kiện")]
        public TransitionConditionType ConditionType = TransitionConditionType.TargetDetected;

        [Tooltip("Thời gian chờ (giây) khi chọn điều kiện TimerExpired")]
        public float DurationThreshold = 1.0f;

        [Tooltip("Giá trị ngưỡng số (khoảng cách hoặc lượng máu)")]
        public float NumericThreshold = 5.0f;

        [Tooltip("Tên điều kiện tùy chỉnh khi chọn CustomCondition")]
        public string CustomConditionName = "";

        public TransitionConditionItem()
        {
        }

        public TransitionConditionItem(TransitionConditionType type, float duration = 1.0f, float numeric = 5.0f, string custom = "")
        {
            ConditionType = type;
            DurationThreshold = duration;
            NumericThreshold = numeric;
            CustomConditionName = custom;
        }

        public string GetSummary()
        {
            switch (ConditionType)
            {
                case TransitionConditionType.AlwaysTrue:
                    return "Always";
                case TransitionConditionType.TimerExpired:
                    return $"Timer >= {DurationThreshold:0.#}s";
                case TransitionConditionType.TargetDetected:
                    return "Target Detected";
                case TransitionConditionType.InAttackRange:
                    return "In Attack Range";
                case TransitionConditionType.TargetLost:
                    return "Target Lost";
                case TransitionConditionType.HealthZero:
                    return "Health <= 0";
                case TransitionConditionType.HealthBelowThreshold:
                    return $"Health < {NumericThreshold:0.#}";
                case TransitionConditionType.DistanceLessThan:
                    return $"Dist < {NumericThreshold:0.#}m";
                case TransitionConditionType.DistanceGreaterThan:
                    return $"Dist > {NumericThreshold:0.#}m";
                case TransitionConditionType.CustomCondition:
                    return string.IsNullOrEmpty(CustomConditionName) ? "Custom" : CustomConditionName;
                default:
                    return ConditionType.ToString();
            }
        }

        public string GetDetailedDescription()
        {
            switch (ConditionType)
            {
                case TransitionConditionType.AlwaysTrue:
                    return "Chuyển trạng thái ngay lập tức mà không cần điều kiện.";
                case TransitionConditionType.TimerExpired:
                    return $"Tự động chuyển khi State hiện tại đã chạy đủ {DurationThreshold:F1} giây.";
                case TransitionConditionType.TargetDetected:
                    return "Chuyển khi mục tiêu nằm trong tầm phát hiện (Distance <= DetectRange).";
                case TransitionConditionType.InAttackRange:
                    return "Chuyển khi mục tiêu nằm trong tầm đánh (Distance <= AttackRange).";
                case TransitionConditionType.TargetLost:
                    return "Chuyển khi mục tiêu ra khỏi tầm phát hiện (Distance > DetectRange).";
                case TransitionConditionType.HealthZero:
                    return "Chuyển khi lượng máu giảm về 0 hoặc nhỏ hơn.";
                case TransitionConditionType.HealthBelowThreshold:
                    return $"Chuyển khi lượng máu hiện tại nhỏ hơn mức ngưỡng {NumericThreshold:F1}.";
                case TransitionConditionType.DistanceLessThan:
                    return $"Chuyển khi khoảng cách đến mục tiêu nhỏ hơn {NumericThreshold:F1}m.";
                case TransitionConditionType.DistanceGreaterThan:
                    return $"Chuyển khi khoảng cách đến mục tiêu lớn hơn {NumericThreshold:F1}m.";
                case TransitionConditionType.CustomCondition:
                    return $"Kiểm tra qua hàm C# hoặc cờ tùy chỉnh: '{CustomConditionName}'.";
                default:
                    return "Điều kiện chuyển đổi trạng thái FSM.";
            }
        }
    }

    /// <summary>
    /// Cấu hình quy tắc chuyển trạng thái hỗ trợ kết hợp nhiều điều kiện (AND / OR).
    /// </summary>
    [Serializable]
    public class VisualTransitionConfig
    {
        [Tooltip("Tên State nguồn (để trống nếu chuyển từ [Any State])")]
        public string FromState;

        [Tooltip("Tên State đích sẽ chuyển tới")]
        public string ToState;

        [Tooltip("Chế độ kết hợp điều kiện: Thỏa mãn TẤT CẢ (AND) hoặc Thỏa mãn BẤT KỲ (OR)")]
        public ConditionLogicMode LogicMode = ConditionLogicMode.All;

        [Tooltip("Danh sách các điều kiện cần kiểm tra để chuyển trạng thái")]
        public List<TransitionConditionItem> Conditions = new List<TransitionConditionItem>();

        // Properties hỗ trợ truy cập nhanh / tương thích ngược
        public TransitionConditionType ConditionType
        {
            get => (Conditions != null && Conditions.Count > 0) ? Conditions[0].ConditionType : TransitionConditionType.AlwaysTrue;
            set
            {
                EnsureConditionExists();
                Conditions[0].ConditionType = value;
            }
        }

        public float DurationThreshold
        {
            get => (Conditions != null && Conditions.Count > 0) ? Conditions[0].DurationThreshold : 1.0f;
            set
            {
                EnsureConditionExists();
                Conditions[0].DurationThreshold = value;
            }
        }

        public float NumericThreshold
        {
            get => (Conditions != null && Conditions.Count > 0) ? Conditions[0].NumericThreshold : 5.0f;
            set
            {
                EnsureConditionExists();
                Conditions[0].NumericThreshold = value;
            }
        }

        public string CustomConditionName
        {
            get => (Conditions != null && Conditions.Count > 0) ? Conditions[0].CustomConditionName : string.Empty;
            set
            {
                EnsureConditionExists();
                Conditions[0].CustomConditionName = value;
            }
        }

        public VisualTransitionConfig()
        {
        }

        public VisualTransitionConfig(string fromState, string toState, TransitionConditionType condition, float duration = 1.0f, float numeric = 5.0f)
        {
            FromState = fromState;
            ToState = toState;
            LogicMode = ConditionLogicMode.All;
            Conditions = new List<TransitionConditionItem>
            {
                new TransitionConditionItem(condition, duration, numeric)
            };
        }

        public void EnsureConditionExists()
        {
            if (Conditions == null) Conditions = new List<TransitionConditionItem>();
            if (Conditions.Count == 0) Conditions.Add(new TransitionConditionItem());
        }

        /// <summary>
        /// Chuỗi tóm tắt ngắn gọn hiển thị trực tiếp trên mũi tên Node Graph.
        /// </summary>
        public string GetSummary()
        {
            if (Conditions == null || Conditions.Count == 0)
                return "Always";

            if (Conditions.Count == 1)
                return Conditions[0] != null ? Conditions[0].GetSummary() : "Always";

            string separator = LogicMode == ConditionLogicMode.All ? " & " : " | ";
            var list = new List<string>();
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (Conditions[i] != null) list.Add(Conditions[i].GetSummary());
            }

            return list.Count > 0 ? string.Join(separator, list) : "Always";
        }

        /// <summary>
        /// Mô tả chi tiết toàn bộ các điều kiện và cách kết hợp logic (AND / OR).
        /// </summary>
        public string GetDetailedDescription()
        {
            if (Conditions == null || Conditions.Count == 0)
                return "Chuyển trạng thái ngay lập tức.";

            if (Conditions.Count == 1)
                return Conditions[0] != null ? Conditions[0].GetDetailedDescription() : "Chuyển trạng thái ngay lập tức.";

            string modeText = LogicMode == ConditionLogicMode.All
                ? "TẤT CẢ (AND) các điều kiện sau:"
                : "BẤT KỲ (OR) 1 trong các điều kiện sau:";

            var sb = new System.Text.StringBuilder(modeText);
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (Conditions[i] != null)
                {
                    sb.Append($"\n• [{i + 1}] {Conditions[i].GetDetailedDescription()}");
                }
            }
            return sb.ToString();
        }
    }
}
