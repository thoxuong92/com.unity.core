using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Core.Logging;
using Object = UnityEngine.Object;

namespace Unity.Core.Pool
{
    /// <summary>
    /// Hệ thống Object Pooler hiệu năng cao, tối ưu tuyệt đối bộ nhớ và chống giật lag (GC Spikes) cho Unity (Mobile / PC).
    /// Hỗ trợ Prewarm nạp sẵn, Despawn trì hoãn, phân nhóm Hierarchy gọn gàng, và gọi tự động IPoolable.
    /// </summary>
    public static class ObjectPooler
    {
        private class PoolData
        {
            public GameObject Prefab;
            public Transform GroupTransform;
            public Queue<GameObject> InactiveQueue = new Queue<GameObject>();
            public HashSet<GameObject> ActiveSet = new HashSet<GameObject>();
        }

        private static readonly Dictionary<GameObject, PoolData> Pools = new Dictionary<GameObject, PoolData>();
        private static readonly Dictionary<GameObject, GameObject> InstanceToPrefabMap = new Dictionary<GameObject, GameObject>();

        private static Transform _poolRoot;
        private static PoolCoroutineRunner _coroutineRunner;

        #region Initialization & Root Management

        private static Transform GetPoolRoot()
        {
            if (_poolRoot == null)
            {
                var go = new GameObject("[UnityCore_PoolRoot]");
                Object.DontDestroyOnLoad(go);
                _poolRoot = go.transform;
                _coroutineRunner = go.AddComponent<PoolCoroutineRunner>();
            }
            return _poolRoot;
        }

        private static PoolData GetOrCreatePool(GameObject prefab)
        {
            if (prefab == null) return null;

            if (!Pools.TryGetValue(prefab, out var poolData))
            {
                var groupGo = new GameObject($"Pool_{prefab.name}");
                groupGo.transform.SetParent(GetPoolRoot());

                poolData = new PoolData
                {
                    Prefab = prefab,
                    GroupTransform = groupGo.transform
                };
                Pools.Add(prefab, poolData);
            }

            return poolData;
        }

        #endregion

        #region Prewarm

        /// <summary>
        /// Nạp sẵn (Prewarm) một số lượng object vào Pool từ trước để tránh giật lag khung hình khi spawn lần đầu.
        /// </summary>
        /// <param name="prefab">Prefab gốc cần nạp sẵn</param>
        /// <param name="count">Số lượng cần khởi tạo</param>
        public static void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0) return;

            var poolData = GetOrCreatePool(prefab);
            for (int i = 0; i < count; i++)
            {
                var instance = Object.Instantiate(prefab, poolData.GroupTransform);
                instance.name = $"{prefab.name} (Pooled)";
                instance.SetActive(false);

                poolData.InactiveQueue.Enqueue(instance);
                InstanceToPrefabMap[instance] = prefab;
            }

            AppLogger.Log($"[ObjectPooler] Prewarmed {count} instances of '{prefab.name}'. Total Inactive: {poolData.InactiveQueue.Count}");
        }

        /// <summary>
        /// Nạp sẵn (Prewarm) một số lượng component vào Pool.
        /// </summary>
        public static void Prewarm<T>(T prefab, int count) where T : Component
        {
            if (prefab != null)
            {
                Prewarm(prefab.gameObject, count);
            }
        }

        #endregion

        #region Spawn

        /// <summary>
        /// Lấy một object từ Pool (hoặc tạo mới nếu Pool trống), gán vị trí/góc xoay và kích hoạt (SetActive = true).
        /// </summary>
        public static GameObject Spawn(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            if (prefab == null) return null;

            var poolData = GetOrCreatePool(prefab);
            GameObject instance = null;

            while (poolData.InactiveQueue.Count > 0)
            {
                var candidate = poolData.InactiveQueue.Dequeue();
                if (candidate != null)
                {
                    instance = candidate;
                    break;
                }
            }

            if (instance == null)
            {
                instance = Object.Instantiate(prefab);
                instance.name = $"{prefab.name} (Pooled)";
                InstanceToPrefabMap[instance] = prefab;
            }

            // Setup transform
            instance.transform.SetParent(parent != null ? parent : poolData.GroupTransform);
            instance.transform.position = position;
            instance.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            instance.transform.localScale = prefab.transform.localScale;

            // Track active
            poolData.ActiveSet.Add(instance);

            // Activate
            instance.SetActive(true);

            // Notify IPoolable
            var poolables = instance.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < poolables.Length; i++)
            {
                try
                {
                    poolables[i].OnSpawn();
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"[ObjectPooler] Exception in OnSpawn on '{instance.name}': {ex.Message}");
                }
            }

            return instance;
        }

        /// <summary>
        /// Lấy một component từ Pool và trả về kiểu dữ liệu T mạnh kiểu.
        /// </summary>
        public static T Spawn<T>(T prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null) where T : Component
        {
            if (prefab == null) return null;
            var go = Spawn(prefab.gameObject, position, rotation, parent);
            return go != null ? go.GetComponent<T>() : null;
        }

        #endregion

        #region Despawn

        /// <summary>
        /// Trả một GameObject đã spawn về Pool và vô hiệu hóa (SetActive = false).
        /// </summary>
        public static void Despawn(GameObject instance)
        {
            if (instance == null) return;

            if (!InstanceToPrefabMap.TryGetValue(instance, out var prefab))
            {
                // Nếu object này không được spawn từ Pool, phá hủy an toàn
                Object.Destroy(instance);
                return;
            }

            var poolData = GetOrCreatePool(prefab);

            // Notify IPoolable
            var poolables = instance.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < poolables.Length; i++)
            {
                try
                {
                    poolables[i].OnDespawn();
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"[ObjectPooler] Exception in OnDespawn on '{instance.name}': {ex.Message}");
                }
            }

            // Deactivate and reset parent
            instance.SetActive(false);
            if (poolData.GroupTransform != null)
            {
                instance.transform.SetParent(poolData.GroupTransform);
            }

            poolData.ActiveSet.Remove(instance);
            poolData.InactiveQueue.Enqueue(instance);
        }

        /// <summary>
        /// Trả một Component đã spawn về Pool.
        /// </summary>
        public static void Despawn<T>(T instance) where T : Component
        {
            if (instance != null)
            {
                Despawn(instance.gameObject);
            }
        }

        /// <summary>
        /// Trả GameObject về Pool sau một khoảng thời gian chờ (giây).
        /// </summary>
        public static void Despawn(GameObject instance, float delaySeconds)
        {
            if (instance == null) return;

            if (delaySeconds <= 0f)
            {
                Despawn(instance);
                return;
            }

            GetPoolRoot(); // Ensure runner is initialized
            _coroutineRunner.StartCoroutine(DelayedDespawnRoutine(instance, delaySeconds));
        }

        /// <summary>
        /// Trả Component về Pool sau một khoảng thời gian chờ (giây).
        /// </summary>
        public static void Despawn<T>(T instance, float delaySeconds) where T : Component
        {
            if (instance != null)
            {
                Despawn(instance.gameObject, delaySeconds);
            }
        }

        private static IEnumerator DelayedDespawnRoutine(GameObject instance, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (instance != null && instance.activeSelf)
            {
                Despawn(instance);
            }
        }

        #endregion

        #region Pool Query & Clear

        /// <summary>
        /// Lấy số lượng instance đang hoạt động ngoài Scene của prefab này.
        /// </summary>
        public static int GetActiveCount(GameObject prefab)
        {
            if (prefab != null && Pools.TryGetValue(prefab, out var poolData))
            {
                return poolData.ActiveSet.Count;
            }
            return 0;
        }

        /// <summary>
        /// Lấy số lượng instance đang nhàn rỗi trong Pool sẵn sàng tái sử dụng.
        /// </summary>
        public static int GetInactiveCount(GameObject prefab)
        {
            if (prefab != null && Pools.TryGetValue(prefab, out var poolData))
            {
                return poolData.InactiveQueue.Count;
            }
            return 0;
        }

        /// <summary>
        /// Lấy tổng số instance (cả active và inactive) của prefab này trong bộ nhớ.
        /// </summary>
        public static int GetTotalCount(GameObject prefab)
        {
            if (prefab != null && Pools.TryGetValue(prefab, out var poolData))
            {
                return poolData.ActiveSet.Count + poolData.InactiveQueue.Count;
            }
            return 0;
        }

        /// <summary>
        /// Xóa và giải phóng bộ nhớ của một Pool cụ thể.
        /// </summary>
        public static void ClearPool(GameObject prefab)
        {
            if (prefab == null || !Pools.TryGetValue(prefab, out var poolData)) return;

            while (poolData.InactiveQueue.Count > 0)
            {
                var inst = poolData.InactiveQueue.Dequeue();
                if (inst != null)
                {
                    InstanceToPrefabMap.Remove(inst);
                    Object.Destroy(inst);
                }
            }

            foreach (var inst in poolData.ActiveSet)
            {
                if (inst != null)
                {
                    InstanceToPrefabMap.Remove(inst);
                    Object.Destroy(inst);
                }
            }

            if (poolData.GroupTransform != null)
            {
                Object.Destroy(poolData.GroupTransform.gameObject);
            }

            Pools.Remove(prefab);
        }

        /// <summary>
        /// Xóa sạch tất cả các Pool và reset toàn bộ hệ thống.
        /// </summary>
        public static void ClearAllPools()
        {
            Pools.Clear();
            InstanceToPrefabMap.Clear();

            if (_poolRoot != null)
            {
                Object.Destroy(_poolRoot.gameObject);
                _poolRoot = null;
                _coroutineRunner = null;
            }

            AppLogger.Log("[ObjectPooler] All pools cleared.");
        }

        #endregion

        /// <summary>
        /// Internal runner MonoBehaviour to execute delayed Coroutines for ObjectPooler.
        /// </summary>
        private class PoolCoroutineRunner : MonoBehaviour
        {
        }
    }
}
