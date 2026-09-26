using UnityEngine;

namespace Unity.Core.Pool
{
    /// <summary>
    /// Các Extension methods giúp thao tác với ObjectPooler trực quan và ngắn gọn nhất.
    /// Cho phép gọi: gameObject.Despawn(), component.Despawn(), prefab.Spawn(), v.v.
    /// </summary>
    public static class ObjectPoolExtensions
    {
        /// <summary>
        /// Spawn một instance từ Prefab này qua ObjectPooler.
        /// </summary>
        public static GameObject Spawn(this GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            return ObjectPooler.Spawn(prefab, position, rotation, parent);
        }

        /// <summary>
        /// Spawn một Component từ Prefab này qua ObjectPooler.
        /// </summary>
        public static T Spawn<T>(this T prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null) where T : Component
        {
            return ObjectPooler.Spawn(prefab, position, rotation, parent);
        }

        /// <summary>
        /// Trả GameObject này về ObjectPooler (ngay lập tức hoặc sau delaySeconds).
        /// </summary>
        public static void Despawn(this GameObject instance, float delaySeconds = 0f)
        {
            ObjectPooler.Despawn(instance, delaySeconds);
        }

        /// <summary>
        /// Trả Component này về ObjectPooler (ngay lập tức hoặc sau delaySeconds).
        /// </summary>
        public static void Despawn(this Component instance, float delaySeconds = 0f)
        {
            if (instance != null)
            {
                ObjectPooler.Despawn(instance.gameObject, delaySeconds);
            }
        }

        /// <summary>
        /// Nạp sẵn (Prewarm) một số lượng object từ Prefab này vào Pool.
        /// </summary>
        public static void Prewarm(this GameObject prefab, int count)
        {
            ObjectPooler.Prewarm(prefab, count);
        }

        /// <summary>
        /// Nạp sẵn (Prewarm) một số lượng Component từ Prefab này vào Pool.
        /// </summary>
        public static void Prewarm<T>(this T prefab, int count) where T : Component
        {
            ObjectPooler.Prewarm(prefab, count);
        }
    }
}
