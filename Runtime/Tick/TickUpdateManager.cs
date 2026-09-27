using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Core.Tick
{
    /// <summary>
    /// Bộ quản lý vòng lặp cập nhật tập trung (Centralized Tick Loop).
    /// Giảm thiểu chi phí gọi hàng ngàn hàm Update/LateUpdate/FixedUpdate từ native Unity C++ sang managed C#.
    /// Hỗ trợ SlowUpdate (chu kỳ định kỳ tiết kiệm CPU cho UI, AI, check trạng thái).
    /// </summary>
    [DefaultExecutionOrder(-5000)]
    public class TickUpdateManager : MonoBehaviour
    {
        private static TickUpdateManager _instance;
        public static TickUpdateManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[Unity.Core.TickUpdateManager]");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<TickUpdateManager>();
                }
                return _instance;
            }
        }

        public static Action<bool> OnPause;
        public static Action OnQuit;

        public static float SlowUpdateInterval = 0.2f;

        private static readonly List<TickBehaviour> RegularList = new List<TickBehaviour>(128);
        private static readonly List<TickBehaviour> FixedList = new List<TickBehaviour>(64);
        private static readonly List<TickBehaviour> LateList = new List<TickBehaviour>(64);
        private static readonly List<TickBehaviour> SlowList = new List<TickBehaviour>(64);

        private float _lastSlowCall;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (_instance == null)
            {
                var go = new GameObject("[Unity.Core.TickUpdateManager]");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<TickUpdateManager>();
            }
        }

        public static void Register(TickBehaviour behaviour)
        {
            if (behaviour == null) return;

            if (behaviour.HasUpdate && !RegularList.Contains(behaviour)) RegularList.Add(behaviour);
            if (behaviour.HasFixedUpdate && !FixedList.Contains(behaviour)) FixedList.Add(behaviour);
            if (behaviour.HasLateUpdate && !LateList.Contains(behaviour)) LateList.Add(behaviour);
            if (behaviour.HasSlowUpdate && !SlowList.Contains(behaviour)) SlowList.Add(behaviour);
        }

        public static void Unregister(TickBehaviour behaviour)
        {
            if (behaviour == null) return;

            if (behaviour.HasUpdate) RegularList.Remove(behaviour);
            if (behaviour.HasFixedUpdate) FixedList.Remove(behaviour);
            if (behaviour.HasLateUpdate) LateList.Remove(behaviour);
            if (behaviour.HasSlowUpdate) SlowList.Remove(behaviour);
        }

        private void Update()
        {
            int count = RegularList.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (i < RegularList.Count)
                {
                    var item = RegularList[i];
                    if (item != null && item.isActiveAndEnabled)
                    {
                        item.OnUpdate();
                    }
                }
            }

            if (Time.time - _lastSlowCall >= SlowUpdateInterval)
            {
                _lastSlowCall = Time.time;
                int slowCount = SlowList.Count;
                for (int i = slowCount - 1; i >= 0; i--)
                {
                    if (i < SlowList.Count)
                    {
                        var item = SlowList[i];
                        if (item != null && item.isActiveAndEnabled)
                        {
                            item.OnSlowUpdate();
                        }
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            int count = FixedList.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (i < FixedList.Count)
                {
                    var item = FixedList[i];
                    if (item != null && item.isActiveAndEnabled)
                    {
                        item.OnFixedUpdate();
                    }
                }
            }
        }

        private void LateUpdate()
        {
            int count = LateList.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (i < LateList.Count)
                {
                    var item = LateList[i];
                    if (item != null && item.isActiveAndEnabled)
                    {
                        item.OnLateUpdate();
                    }
                }
            }
        }

        private void OnApplicationPause(bool pause)
        {
            OnPause?.Invoke(pause);
        }

        private void OnApplicationQuit()
        {
            OnQuit?.Invoke();
        }
    }

    /// <summary>
    /// Base class thay thế MonoBehaviour tiêu chuẩn cho các đối tượng cần update liên tục.
    /// Tự động đăng ký với TickUpdateManager để tối ưu hóa CPU cache và loại bỏ overhead native-to-managed.
    /// </summary>
    public abstract class TickBehaviour : MonoBehaviour
    {
        private Transform _cachedTransform;
        public Transform CachedTransform
        {
            get
            {
                if (_cachedTransform == null) _cachedTransform = transform;
                return _cachedTransform;
            }
        }

        private bool _isRegistered;
        public Action<bool> OnRegisterCallback;

        public virtual bool HasUpdate => true;
        public virtual bool HasFixedUpdate => false;
        public virtual bool HasLateUpdate => false;
        public virtual bool HasSlowUpdate => false;

        protected virtual void Awake()
        {
            RegisterTick();
        }

        protected virtual void OnEnable()
        {
            RegisterTick();
        }

        protected virtual void OnDisable()
        {
            UnregisterTick();
        }

        protected virtual void OnDestroy()
        {
            UnregisterTick();
        }

        private void RegisterTick()
        {
            if (!_isRegistered)
            {
                TickUpdateManager.Register(this);
                _isRegistered = true;
                OnRegisterCallback?.Invoke(true);
            }
        }

        private void UnregisterTick()
        {
            if (_isRegistered)
            {
                TickUpdateManager.Unregister(this);
                _isRegistered = false;
                OnRegisterCallback?.Invoke(false);
            }
        }

        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnSlowUpdate() { }
    }
}
