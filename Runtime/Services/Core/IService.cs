namespace Unity.Core.Services
{
    /// <summary>
    /// Base interface cho toàn bộ các Service trong framework.
    /// Hỗ trợ vòng đời khởi tạo (Initialize) và giải phóng tài nguyên (Shutdown).
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Được gọi khi Service được đăng ký hoặc khởi chạy lúc boot.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Được gọi khi game tắt hoặc khi Service bị hủy bỏ.
        /// </summary>
        void Shutdown();
    }
}
