using System;

namespace Unity.Core.FSM.Visual
{
    /// <summary>
    /// Đánh dấu một hàm (method) không có tham số và trả về bool trên AIBehaviour là một điều kiện FSM tùy chỉnh.
    /// Giúp FSM Graph Editor và Inspector tự động quét và hiển thị thành danh sách Dropdown dễ chọn, dễ sửa.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class FSMConditionAttribute : Attribute
    {
        public string DisplayName { get; }

        public FSMConditionAttribute(string displayName = "")
        {
            DisplayName = displayName;
        }
    }
}
