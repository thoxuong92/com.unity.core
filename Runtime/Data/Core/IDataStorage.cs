namespace Unity.Core.Data
{
    /// <summary>
    /// Giao diện chuẩn cho các cơ chế lưu trữ dữ liệu (PlayerPrefs, File Json, Encrypted Binary, Cloud).
    /// </summary>
    public interface IDataStorage<T> where T : class
    {
        /// <summary>
        /// Tải dữ liệu từ bộ nhớ lưu trữ. Nếu chưa có dữ liệu, trả về instance mặc định mới.
        /// </summary>
        T LoadData();

        /// <summary>
        /// Lưu đối tượng dữ liệu vào bộ nhớ lưu trữ.
        /// </summary>
        void Save(T data);

        /// <summary>
        /// Kiểm tra xem key/file dữ liệu đã tồn tại hay chưa.
        /// </summary>
        bool HasData();

        /// <summary>
        /// Xóa dữ liệu đã lưu.
        /// </summary>
        void Delete();
    }
}
