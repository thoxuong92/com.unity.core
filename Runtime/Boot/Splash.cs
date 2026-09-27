using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.Core.Logging;
using Unity.Core.Services.Ads;
using Unity.Core.Services.Analytics;
using Unity.Core.Services.RemoteConfig;

namespace Unity.Core.Boot
{
    /// <summary>
    /// Điều phối quá trình Splash Loading khi khởi động game.
    /// Quản lý thanh tiến trình mượt mà, đồng bộ cấu hình Remote Config, hiển thị App Open Ad đầu tiên,
    /// kiểm tra kết nối mạng và kích hoạt Banner khi hoàn tất.
    /// Không sử dụng Singleton, không scan Assembly, không Find Object.
    /// </summary>
    [HelpURL("https://github.com/thoxuong92/com.unity.core")]
    public class Splash : MonoBehaviour
    {
        /// <summary>
        /// Sự kiện tĩnh phát % loading (0f -> 1f) cho bất kỳ UI nào trong scene lắng nghe mà không cần Singleton.
        /// </summary>
        public static event Action<float> OnProgress;

        /// <summary>
        /// Sự kiện tĩnh thông báo khi toàn bộ quá trình loading hoàn tất.
        /// </summary>
        public static event Action OnCompleted;

        public static float CurrentPercent { get; private set; }
        public static bool IsLoadingCompleted { get; private set; }

        [Header("Settings")]
        [SerializeField]
        private bool isLoadingOnAwake = true;

        [Min(0.1f)]
        [Tooltip("Tổng thời gian loading dự kiến tối thiểu (giây).")]
        public float DurationLoading = 8f;

        [Tooltip("Thời gian tối đa chờ App Open Ad load (giây).")]
        public float TimeLoadAppOpen = 5f;

        [Tooltip("Thời gian tối đa chờ Remote Config fetch (giây).")]
        public float TimeWaitRemoteConfig = 3f;

        [Header("Banner")]
        [Tooltip("Tự động hiển thị Banner khi loading hoàn tất.")]
        public bool IsShowBannerOnComplete = true;

        [Header("Scene Transition (Tùy chọn)")]
        [Tooltip("Tự động chuyển sang Scene này sau khi loading hoàn tất (nếu để trống sẽ chỉ gọi OnComplete).")]
        [SerializeField]
        private string nextSceneName = "";

        [Header("Progress")]
        [ReadOnly, Range(0, 1f)]
        public float Percent = 0f;

        [Header("Events")]
        public UnityEvent<float> OnProgressPercent;
        public UnityEvent OnComplete;

        private bool _isCompleted = false;
        private bool _isCompletedAppOpen = false;
        private float _timeStart;
        private Coroutine _loadingCoroutine;

        public bool IsCompleted => _isCompleted;
        public bool IsCompletedAppOpen => _isCompletedAppOpen;
        public float TimeStart => _timeStart;

        public bool IsLoadingOnAwake
        {
            get => isLoadingOnAwake;
            set => isLoadingOnAwake = value;
        }

        public string NextSceneName
        {
            get => nextSceneName;
            set => nextSceneName = value;
        }

        protected virtual void Awake()
        {
            if (isLoadingOnAwake)
            {
                StartLoading();
            }
        }

        /// <summary>
        /// Bắt đầu quá trình Splash Loading.
        /// </summary>
        public void StartLoading()
        {
            if (_isCompleted) return;

            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
            }

            _loadingCoroutine = StartCoroutine(IE_Loading());
        }

        private IEnumerator IE_Loading()
        {
            _timeStart = Time.time;
            Time.timeScale = 1f;
            _isCompleted = false;
            _isCompletedAppOpen = false;
            IsLoadingCompleted = false;

            UpdatePercent(Percent);

            DurationLoading = Mathf.Max(0.1f, DurationLoading);

            yield return new WaitForEndOfFrame();

            // Giai đoạn 1: Chạy mồi ngẫu nhiên 25% -> 30%
            float firstTarget = UnityEngine.Random.Range(Mathf.Max(Percent, 0.25f), 0.30f);
            yield return IE_Progress(Percent, firstTarget);

            // Giai đoạn 2: Chờ Remote Config khởi tạo
            yield return IE_WaitRemoteConfig(TimeWaitRemoteConfig);

            // Giai đoạn 3: Chạy tiếp tới ngẫu nhiên 50% -> 80%
            float secondTarget = UnityEngine.Random.Range(Mathf.Max(Percent, 0.50f), 0.80f);
            yield return IE_Progress(Percent, secondTarget);

            yield return new WaitForEndOfFrame();

            // Giai đoạn 4: Tiến thêm một đoạn ngắn (10%) trong 3s
            float thirdTarget = Mathf.Min(Percent + 0.1f, 0.90f);
            yield return IE_Progress(Percent, thirdTarget, 3f);

            // Giai đoạn 5: Chờ nạp và kích hoạt App Open Ad
            yield return IE_WaitShowAppOpen(TimeLoadAppOpen);

            // Giai đoạn 6: Chạy tiếp tới 100% (tốc độ sẽ x10 nếu đã xem/bỏ qua App Open Ad)
            yield return IE_Progress(Percent, 1f);

            AppLogger.Log("[Splash] Loading Completed (100%).");
            Complete();
        }

        private IEnumerator IE_Progress(float start, float end)
        {
            UpdatePercent(start);
            while (true)
            {
                yield return new WaitForEndOfFrame();
                float speed = AdsService.ShowFirstOpenDone ? 10f : 1f;

                float nextValue = Mathf.MoveTowards(Percent, end, speed * Time.deltaTime / DurationLoading);
                UpdatePercent(nextValue);

                if (Percent >= end)
                {
                    break;
                }
            }
        }

        private IEnumerator IE_Progress(float start, float end, float duration)
        {
            UpdatePercent(start);
            float timeWait = Time.time + duration;

            while (Time.time <= timeWait)
            {
                yield return new WaitForEndOfFrame();

                float nextValue = Mathf.MoveTowards(Percent, end, Time.deltaTime / duration);
                UpdatePercent(nextValue);

                if (Percent >= end)
                {
                    break;
                }
            }
        }

        private void UpdatePercent(float value)
        {
            Percent = value;
            CurrentPercent = value;
            OnProgressPercent?.Invoke(value);
            OnProgress?.Invoke(value);
        }

        private IEnumerator IE_WaitRemoteConfig(float duration = 3f)
        {
            float timeWait = Time.time + duration;

            RemoteConfigService.Fetch();

            while (!RemoteConfigService.IsFetched && Time.time < timeWait)
            {
                yield return new WaitForEndOfFrame();
            }

            if (RemoteConfigService.IsFetched)
            {
                TimeLoadAppOpen = RemoteConfigService.GetValue("time_load_app_open", AdsService.Settings.TimeLoadAppOpen);
                AdsService.Settings.TimeLoadAppOpen = TimeLoadAppOpen;
            }
            else
            {
                TimeLoadAppOpen = AdsService.Settings.TimeLoadAppOpen;
            }
        }

        private IEnumerator IE_WaitShowAppOpen(float duration = 5f)
        {
            yield return new WaitForEndOfFrame();
            float timeWait = Time.time + duration;
            float percentTarget = (Percent + 0.95f) / 2f;
            percentTarget = Mathf.Clamp(percentTarget, Percent, 0.95f);

            AppLogger.Log($"[Splash] Wait Show App Open {Percent:P0} -> {percentTarget:P0} trong {duration}s");
            var coroutine = StartCoroutine(IE_Progress(Percent, percentTarget, duration));

            while (!_isCompletedAppOpen && Time.time < timeWait)
            {
                yield return new WaitForEndOfFrame();

                if (!AdsService.ShowedFirstOpen && AdsService.CanShowAppOpen)
                {
                    AdsService.OnAppOpenLoaded();
                    _isCompletedAppOpen = true;
                    if (coroutine != null)
                    {
                        StopCoroutine(coroutine);
                    }
                    break;
                }
            }

            if (!_isCompletedAppOpen)
            {
                AdsService.ShowFirstOpenDone = true;
            }

            AppLogger.Log($"[Splash] Wait Show App Open Completed: {_isCompletedAppOpen}");
        }

        /// <summary>
        /// Hoàn tất splash loading: kiểm tra internet, gửi telemetry, bật banner và kích hoạt event/chuyển scene.
        /// </summary>
        public void Complete()
        {
            if (_isCompleted) return;
            _isCompleted = true;
            IsLoadingCompleted = true;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                AnalyticsService.LogEvent("internet_not_reachable");
            }

            UpdatePercent(1f);

            OnComplete?.Invoke();
            OnCompleted?.Invoke();

            if (IsShowBannerOnComplete)
            {
                AdsService.ShowBanner();
            }

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                AppLogger.Log($"[Splash] Đang chuyển sang Scene: {nextSceneName}");
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
