namespace Unity.Core.Pool
{
    /// <summary>
    /// Giao diện cho các GameObject / Component cần reset hoặc khởi tạo khi được Spawn / Despawn từ ObjectPool.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Được gọi ngay sau khi object được lấy ra từ Pool và kích hoạt (SetActive = true).
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Được gọi ngay trước khi object bị trả về Pool và vô hiệu hóa (SetActive = false).
        /// </summary>
        void OnDespawn();
    }
}
