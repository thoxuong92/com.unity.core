using System;
using System.Collections;
using UnityEngine;

namespace Unity.Core.InGame
{
    /// <summary>
    /// Điều khiển hiệu ứng chuyển động chậm (Bullet Time / Slow Motion) mượt mà không làm đơ main thread.
    /// Tự động cập nhật fixedDeltaTime tương ứng để duy trì độ mượt cho engine vật lý.
    /// </summary>
    public class Slowmotion : MonoBehaviour
    {
        private static Slowmotion _instance;
        private static Slowmotion Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[Unity.Core.Slowmotion]");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<Slowmotion>();
                }
                return _instance;
            }
        }

        private const float DefaultFixedDeltaTime = 0.02f;
        private Coroutine _currentRoutine;

        public static void Do(float targetTimeScale = 0.1f, float duration = 1.0f, Action onComplete = null)
        {
            Instance.StartSlowMotionInternal(targetTimeScale, duration, onComplete);
        }

        public static void ResetTimeScale()
        {
            if (_instance != null)
            {
                _instance.StopAllCoroutines();
            }
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = DefaultFixedDeltaTime;
        }

        private void StartSlowMotionInternal(float targetScale, float duration, Action onComplete)
        {
            if (_currentRoutine != null)
            {
                StopCoroutine(_currentRoutine);
            }
            _currentRoutine = StartCoroutine(SlowMotionRoutine(targetScale, duration, onComplete));
        }

        private IEnumerator SlowMotionRoutine(float targetScale, float duration, Action onComplete)
        {
            Time.timeScale = targetScale;
            Time.fixedDeltaTime = DefaultFixedDeltaTime * targetScale;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = DefaultFixedDeltaTime;
            _currentRoutine = null;
            onComplete?.Invoke();
        }
    }
}
