using System;
using System.Collections.Generic;
using Unity.Core.Logging;

namespace Unity.Core.Data
{
    /// <summary>
    /// Trung tâm điều phối dữ liệu (Data Service / Registry) của game.
    /// Quản lý tập trung toàn bộ DataHandler và Data Model mà không cần viết Singleton phân tán.
    /// </summary>
    public static class DataManager
    {
        private static readonly Dictionary<Type, object> _registry = new Dictionary<Type, object>();

        /// <summary>
        /// Lấy hoặc tự động khởi tạo DataHandler chuyên biệt (ví dụ: DataManager.Get<PlayerDataHandler>()).
        /// </summary>
        public static THandler Get<THandler>() where THandler : class
        {
            var type = typeof(THandler);
            if (!_registry.TryGetValue(type, out var instance))
            {
                try
                {
                    instance = Activator.CreateInstance(type);
                    _registry[type] = instance;
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"[DataManager] Không thể khởi tạo '{type.Name}': {ex.Message}. Hãy đảm bảo class có parameterless constructor.");
                    return null;
                }
            }
            return (THandler)instance;
        }

        /// <summary>
        /// Lấy đối tượng Data Model T từ DataHandler tương ứng (ví dụ: DataManager.Data<PlayerProfile>()).
        /// </summary>
        public static TModel Data<TModel>() where TModel : class, new()
        {
            return Handler<TModel>().Data;
        }

        /// <summary>
        /// Lấy đối tượng DataHandler cơ bản quản lý Data Model T (ví dụ: DataManager.Handler<PlayerProfile>()).
        /// </summary>
        public static DataHandler<TModel> Handler<TModel>(string key = null) where TModel : class, new()
        {
            var type = typeof(DataHandler<TModel>);
            if (!_registry.TryGetValue(type, out var instance))
            {
                string finalKey = !string.IsNullOrEmpty(key) ? key : $"Data_{typeof(TModel).Name}";
                instance = new DataHandler<TModel>(finalKey);
                _registry[type] = instance;
            }
            return (DataHandler<TModel>)instance;
        }

        /// <summary>
        /// Lưu dữ liệu của một Model cụ thể.
        /// </summary>
        public static void Save<TModel>() where TModel : class, new()
        {
            Handler<TModel>().Save();
        }

        /// <summary>
        /// Lưu toàn bộ các DataHandler đang hoạt động trong bộ nhớ.
        /// </summary>
        public static void SaveAll()
        {
            foreach (var kvp in _registry)
            {
                var saveMethod = kvp.Value.GetType().GetMethod("Save", Type.EmptyTypes);
                saveMethod?.Invoke(kvp.Value, null);
            }
            AppLogger.Log("[DataManager] All active data handlers saved.");
        }

        /// <summary>
        /// Đăng ký một instance custom vào container (tiện lợi cho Unit Test, Mocking, hoặc cấu hình lưu trữ riêng).
        /// </summary>
        public static void Register<THandler>(THandler instance) where THandler : class
        {
            if (instance != null)
            {
                _registry[typeof(THandler)] = instance;
            }
        }

        /// <summary>
        /// Xóa sạch registry bộ nhớ tạm (dùng khi đăng xuất / reset tài khoản).
        /// </summary>
        public static void Clear()
        {
            _registry.Clear();
        }
    }
}
