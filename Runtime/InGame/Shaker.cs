using UnityEngine;
using Unity.Core.Attributes;
using Unity.Core.Tick;

namespace Unity.Core.InGame
{
    /// <summary>
    /// Hiệu ứng rung màn hình / rung đối tượng (Screen Shake / Transform Shake) dựa trên Perlin Noise mượt mà.
    /// </summary>
    [AddComponentMenu("Unity Core/InGame/Shaker")]
    public class Shaker : TickBehaviour
    {
        public Transform ShakeTarget;
        [Range(0, 60)] public float MaxAngle = 5f;
        [Range(0, 1)] public float ShakeIntensity = 1f;
        [Range(0, 20)] public float ShakeStartIntensity = 3f;
        [Range(0, 20)] public float ShakeEndIntensity = 3f;
        [Range(0, 20)] public float ShakeSpeed = 2f;
        public float ShakeDuration = 0.5f;
        public bool AlwaysShaking;

        private float _currentTime;
        private float _currentShakeIntensity;
        public bool IsShaking { get; private set; }

        private float _coordX, _coordY, _coordZ;
        private Vector3 _shakingEulerRotation;

        public Vector3 ShakeLocalEulerRotation => _shakingEulerRotation;
        public Quaternion ShakeLocalRotation => Quaternion.Euler(_shakingEulerRotation);

        private void Start()
        {
            _coordX = Random.Range(-1000f, 1000f);
            _coordY = Random.Range(-1000f, 1000f);
            _coordZ = Random.Range(-1000f, 1000f);

            if (ShakeTarget == null) ShakeTarget = transform;
        }

        public override void OnUpdate()
        {
            if (ShakeTarget == null) return;

            _currentShakeIntensity = Mathf.Clamp01(_currentShakeIntensity);
            float intensityQuadratic = _currentShakeIntensity * _currentShakeIntensity;
            float time = Time.time * ShakeSpeed;

            float rotX = (ShakeIntensity * intensityQuadratic) * MaxAngle * PerlinNoise(_coordX, time);
            float rotY = (ShakeIntensity * intensityQuadratic) * MaxAngle * PerlinNoise(_coordY, time);
            float rotZ = (ShakeIntensity * intensityQuadratic) * MaxAngle * PerlinNoise(_coordZ, time);

            _shakingEulerRotation.Set(rotX, rotY, rotZ);
            ShakeTarget.localEulerAngles = _shakingEulerRotation;

            if (!AlwaysShaking)
            {
                if (IsShaking)
                {
                    _currentShakeIntensity += ShakeStartIntensity * Time.deltaTime;
                }
                else
                {
                    _currentShakeIntensity -= ShakeEndIntensity * Time.deltaTime;
                }
            }
            else
            {
                _currentShakeIntensity += ShakeStartIntensity * Time.deltaTime;
            }

            if (_currentTime < ShakeDuration)
            {
                _currentTime += Time.deltaTime;
                IsShaking = true;
            }
            else
            {
                IsShaking = false;
            }
        }

        [Button("Test Shake")]
        public void Shake()
        {
            Shake(ShakeSpeed, ShakeDuration, ShakeStartIntensity, ShakeEndIntensity, MaxAngle, ShakeIntensity);
        }

        public void Shake(float speed = 3f, float duration = 0.5f, float startIntensity = 15f, float endIntensity = 3f, float maxAngle = 5f, float intensity = 1f)
        {
            _currentTime = 0f;
            ShakeSpeed = speed;
            ShakeDuration = duration;
            ShakeStartIntensity = startIntensity;
            ShakeEndIntensity = endIntensity;
            MaxAngle = maxAngle;
            ShakeIntensity = intensity;
            IsShaking = true;
        }

        private float PerlinNoise(float coordinate, float time)
        {
            return 1f - 2f * Mathf.PerlinNoise(coordinate + time, coordinate + time);
        }
    }
}
