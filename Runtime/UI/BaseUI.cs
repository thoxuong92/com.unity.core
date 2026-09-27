using System;
using System.Collections;
using UnityEngine;
using Unity.Core.Attributes;
using Unity.Core.Tick;

namespace Unity.Core.UI
{
    /// <summary>
    /// Lớp cơ sở cho tất cả các giao diện Panel / Screen trong game.
    /// Quản lý CanvasGroup, hiệu ứng mờ dần (Fade in/Fade out), và tương tác với UIManager.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseUI : TickBehaviour
    {
        [SerializeField]
        protected CanvasGroup group;
        public CanvasGroup Group => group != null ? group : (group = GetComponent<CanvasGroup>());

        protected override void Awake()
        {
            base.Awake();
            if (group == null) group = GetComponent<CanvasGroup>();
            OnInit();
        }

        public virtual void OnInit() { }
        public virtual void SetInfo() { }

        [Button("Show")]
        public virtual void OnShow()
        {
            Group.alpha = 1f;
            Group.blocksRaycasts = true;
            Group.interactable = true;
            gameObject.SetActive(true);
        }

        [Button("Hide")]
        public virtual void OnHide()
        {
            Group.alpha = 0f;
            Group.blocksRaycasts = false;
            Group.interactable = false;
            gameObject.SetActive(false);
        }

        public virtual void Show(float duration = 0.3f)
        {
            gameObject.SetActive(true);
            if (duration > 0.01f)
            {
                StopAllCoroutines();
                StartCoroutine(FadeRoutine(0f, 1f, duration, () => OnShow()));
            }
            else
            {
                OnShow();
            }
        }

        public virtual void Hide(float duration = 0.2f)
        {
            if (!gameObject.activeSelf)
            {
                OnHide();
                return;
            }

            if (duration > 0.01f)
            {
                StopAllCoroutines();
                StartCoroutine(FadeRoutine(Group.alpha, 0f, duration, () => OnHide()));
            }
            else
            {
                OnHide();
            }
        }

        private IEnumerator FadeRoutine(float from, float to, float duration, Action onFinish)
        {
            Group.alpha = from;
            Group.blocksRaycasts = to > 0.5f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                Group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            Group.alpha = to;
            onFinish?.Invoke();
        }
    }
}
