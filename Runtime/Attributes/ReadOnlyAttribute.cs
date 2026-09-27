using System;
using UnityEngine;

namespace Unity.Core
{
    /// <summary>
    /// Thuộc tính đánh dấu một trường serialized trong Inspector ở chế độ chỉ đọc (Read-Only).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
}
