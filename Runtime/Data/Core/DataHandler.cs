using System;
using UnityEngine;
using Unity.Core.Logging;

namespace Unity.Core.Data
{
    /// <summary>
    /// Base class quản lý dữ liệu cho một Data Model T cụ thể.
    /// Tự động kết nối cơ chế lưu trữ (IDataStorage) và vòng đời tự động lưu (Auto-Save on Focus/Quit).
    /// </summary>
    public class DataHandler<T> where T : class, new()
    {
        public T Data;

        public string Key { get; }
        public IDataStorage<T> Storage { get; protected set; }

        public event Action<T> OnLoaded;
        public event Action<T> OnSaved;

        public DataHandler() : this($"Data_{typeof(T).Name}")
        {
        }

        public DataHandler(string key, IDataStorage<T> customStorage = null)
        {
            Key = key;
            Storage = customStorage ?? new PlayerPrefsStorage<T>(key);
            Init();
        }

        public virtual void Init()
        {
            LoadData();

            // Đăng ký tự động lưu dữ liệu theo vòng đời Unity
            Application.focusChanged += OnApplicationFocusChanged;
            Application.quitting += OnApplicationQuitting;
        }

        public virtual void LoadData()
        {
            Data = Storage.LoadData() ?? new T();
            OnLoaded?.Invoke(Data);
            AppLogger.Log($"[DataHandler<{typeof(T).Name}>] Loaded data for key '{Key}'.");
        }

        public virtual void Save()
        {
            if (Data != null)
            {
                Storage.Save(Data);
                OnSaved?.Invoke(Data);
                AppLogger.Log($"[DataHandler<{typeof(T).Name}>] Saved data for key '{Key}'.");
            }
        }

        /// <summary>
        /// Xóa sạch dữ liệu đã lưu và reset về trạng thái ban đầu.
        /// </summary>
        public virtual void Delete()
        {
            Storage.Delete();
            Data = new T();
            AppLogger.Log($"[DataHandler<{typeof(T).Name}>] Deleted data for key '{Key}'.");
        }

        /// <summary>
        /// Reset dữ liệu về mặc định và lưu lại.
        /// </summary>
        public virtual void ResetToDefault()
        {
            Data = new T();
            Save();
        }

        protected virtual void OnApplicationFocusChanged(bool isFocus)
        {
            if (!isFocus)
            {
                Save();
            }
        }

        protected virtual void OnApplicationQuitting()
        {
            Save();
        }
    }
}
