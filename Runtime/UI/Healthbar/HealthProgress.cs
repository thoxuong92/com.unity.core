using UnityEngine;
using UnityEngine.UI;
using Unity.Core.Tick;

namespace Unity.Core.UI.Healthbar
{
    public class UIProgress : TickBehaviour
    {
        [SerializeField] protected Slider slider;
        [SerializeField] protected Image imgFill;
        [SerializeField] protected Text textValue;
        [SerializeField] protected Color colorFill = Color.green;
        [SerializeField] protected Color colorBackground = Color.gray;

        [Range(0f, 1f)]
        [SerializeField] protected float progress = 0f;

        public float Progress
        {
            get => progress;
            set
            {
                progress = Mathf.Clamp01(value);
                UpdateProgress();
            }
        }

        public void SetColor(Color color)
        {
            if (imgFill != null) imgFill.color = color;
        }

        public void SetMin(float min = 0)
        {
            if (slider != null) slider.minValue = min;
        }

        public void SetMax(float max = 1)
        {
            if (slider != null) slider.maxValue = max;
        }

        public virtual void SetValue(float current, float max)
        {
            if (slider != null)
            {
                slider.minValue = 0;
                slider.maxValue = max;
                slider.value = current;
            }

            if (imgFill != null && max > 0.0001f)
            {
                imgFill.fillAmount = current / max;
            }

            if (textValue != null)
            {
                textValue.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }
        }

        protected virtual void UpdateProgress()
        {
            if (slider != null) slider.value = progress;
            if (imgFill != null) imgFill.fillAmount = progress;
            if (textValue != null) textValue.text = $"{Mathf.RoundToInt(progress * 100f)}%";
        }

        protected virtual void OnValidate()
        {
            if (imgFill != null) imgFill.color = colorFill;
            UpdateProgress();
        }
    }

    public class HealthProgress : UIProgress
    {
        [SerializeField] private Slider sliderDamage;
        [SerializeField] private Image imgFillDamage;
        [SerializeField] private Color colorDamage = Color.red;
        [SerializeField] private float timeShowDamage = 0.5f;
        [SerializeField] private float speedLerp = 5f;

        private float _timer;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (imgFillDamage != null)
            {
                imgFillDamage.color = colorDamage;
            }
        }

        public void SetFillDamage(float value)
        {
            _timer = Time.time + timeShowDamage;
            if (sliderDamage != null) sliderDamage.value = value;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (sliderDamage != null && slider != null && _timer <= Time.time)
            {
                sliderDamage.value = Mathf.Lerp(sliderDamage.value, slider.value, Time.deltaTime * speedLerp);
            }
        }
    }
}
