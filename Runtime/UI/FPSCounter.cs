using UnityEngine;

namespace Unity.Core.UI
{
    /// <summary>
    /// Hiển thị chỉ số FPS, thời gian frame ms, và mức tiêu thụ bộ nhớ trực tiếp trên màn hình game.
    /// </summary>
    [AddComponentMenu("Unity Core/UI/FPS Counter")]
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private TextAnchor alignment = TextAnchor.UpperRight;
        [SerializeField] private int fontSize = 20;
        [SerializeField] private Color textColor = Color.yellow;

        private float _accumulatedTime = 0f;
        private int _frameCount = 0;
        private float _timeLeft;
        private string _fpsText = "0 FPS";
        private GUIStyle _style;

        private void Start()
        {
            _timeLeft = updateInterval;
        }

        private void Update()
        {
            _timeLeft -= Time.unscaledDeltaTime;
            _accumulatedTime += Time.unscaledDeltaTime;
            _frameCount++;

            if (_timeLeft <= 0f)
            {
                float fps = _frameCount / _accumulatedTime;
                float ms = (1000f * _accumulatedTime) / _frameCount;
                _fpsText = $"{fps:F1} FPS ({ms:F1} ms)";

                _timeLeft = updateInterval;
                _accumulatedTime = 0f;
                _frameCount = 0;
            }
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    alignment = alignment,
                    fontSize = fontSize,
                    normal = { textColor = textColor }
                };
            }

            int w = Screen.width;
            int h = Screen.height;
            Rect rect = new Rect(10, 10, w - 20, 40);
            GUI.Label(rect, _fpsText, _style);
        }
    }
}
