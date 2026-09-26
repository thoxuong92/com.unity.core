using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Core.Pool
{
    [Serializable]
    public class PoolPrewarmItem
    {
        [Tooltip("Prefab cần nạp sẵn vào Pool")]
        public GameObject Prefab;

        [Tooltip("Số lượng instance cần khởi tạo sẵn")]
        [Min(1)]
        public int Count = 10;
    }

    /// <summary>
    /// Component đặt trong Scene (ví dụ trên GameManager / LevelManager) để tự động
    /// nạp sẵn (Prewarm) danh sách Prefab vào ObjectPooler khi Scene bắt đầu tải.
    /// </summary>
    [DisallowMultipleComponent]
    public class PoolPrewarmManager : MonoBehaviour
    {
        [Header("Prewarm Settings")]
        [SerializeField] private bool _prewarmOnAwake = true;
        [SerializeField] private List<PoolPrewarmItem> _prewarmList = new List<PoolPrewarmItem>();

        private void Awake()
        {
            if (_prewarmOnAwake)
            {
                ExecutePrewarm();
            }
        }

        /// <summary>
        /// Thực thi nạp sẵn toàn bộ danh sách Prefabs vào ObjectPooler.
        /// </summary>
        public void ExecutePrewarm()
        {
            if (_prewarmList == null || _prewarmList.Count == 0) return;

            foreach (var item in _prewarmList)
            {
                if (item != null && item.Prefab != null && item.Count > 0)
                {
                    ObjectPooler.Prewarm(item.Prefab, item.Count);
                }
            }
        }
    }
}
