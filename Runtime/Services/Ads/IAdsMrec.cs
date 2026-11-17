using UnityEngine;

namespace Unity.Core.Services.Ads
{
    /// <summary>
    /// MREC (Medium Rectangle, 300x250 dp) cho các adapter hỗ trợ. Interface tùy chọn: adapter nào chưa
    /// có MREC vẫn chạy như cũ, AdsService coi như không có MREC.
    /// </summary>
    public interface IAdsMrec
    {
        bool CanShowMrec { get; }

        /// <summary>Hiện MREC tại vùng màn hình (pixel, gốc trên-trái như IMGUI). Gọi lại để dời chỗ.</summary>
        void ShowMrec(string placement, Rect screenRectPx);

        void HideMrec();
    }

    /// <summary>
    /// Kích thước thật của banner do adapter cho biết (pixel), để game chừa chỗ ở đáy màn hình.
    /// </summary>
    public interface IAdsBannerLayout
    {
        float BannerHeightPx { get; }
    }
}
