using System;
using System.Collections.Generic;
using Unity.Core.Logging;

namespace Unity.Core.Services
{
    /// <summary>
    /// Service Registry trung tâm, quản lý và điều phối tất cả các Service trong game.
    /// Cho phép đăng ký adapter/provider của SDK bên thứ 3 (Ads, Analytics, Tracking, IAP) một cách linh hoạt.
    /// </summary>
    public static class ServiceRegistry
    {
        private static readonly Dictionary<Type, IService> Services = new Dictionary<Type, IService>();
        private static readonly List<IService> ServiceOrder = new List<IService>();

        public static event Action<Type, IService> OnServiceRegistered;
        public static event Action<Type> OnServiceUnregistered;

        /// <summary>
        /// Đăng ký một service instance vào registry.
        /// </summary>
        public static T Register<T>(T service) where T : class, IService
        {
            var type = typeof(T);
            if (Services.ContainsKey(type))
            {
                AppLogger.LogWarning($"[ServiceRegistry] Service kiểu {type.Name} đã tồn tại. Đang ghi đè.");
                Services[type].Shutdown();
                Services[type] = service;
            }
            else
            {
                Services.Add(type, service);
                ServiceOrder.Add(service);
            }

            service.Initialize();
            OnServiceRegistered?.Invoke(type, service);
            return service;
        }

        /// <summary>
        /// Lấy một service đã đăng ký.
        /// </summary>
        public static T Get<T>() where T : class, IService
        {
            var type = typeof(T);
            if (Services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            return null;
        }

        /// <summary>
        /// Thử lấy service nếu đã đăng ký.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class, IService
        {
            var type = typeof(T);
            if (Services.TryGetValue(type, out var foundService))
            {
                service = (T)foundService;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Kiểm tra xem service kiểu T đã được đăng ký hay chưa.
        /// </summary>
        public static bool IsRegistered<T>() where T : class, IService
        {
            return Services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Hủy đăng ký và giải phóng một service cụ thể.
        /// </summary>
        public static void Unregister<T>() where T : class, IService
        {
            var type = typeof(T);
            if (Services.TryGetValue(type, out var service))
            {
                service.Shutdown();
                Services.Remove(type);
                ServiceOrder.Remove(service);
                OnServiceUnregistered?.Invoke(type);
            }
        }

        /// <summary>
        /// Giải phóng toàn bộ các service khi thoát game hoặc reset.
        /// </summary>
        public static void ShutdownAll()
        {
            for (int i = ServiceOrder.Count - 1; i >= 0; i--)
            {
                try
                {
                    ServiceOrder[i]?.Shutdown();
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"[ServiceRegistry] Lỗi Shutdown service: {ex.Message}");
                }
            }

            Services.Clear();
            ServiceOrder.Clear();
        }
    }
}
