using UnityEngine;

namespace Unity.Core.Pool
{
    /// <summary>
    /// Component tiện ích tự động trả GameObject về ObjectPooler sau một khoảng thời gian
    /// hoặc khi ParticleSystem / AudioSource hoàn tất phát.
    /// Thích hợp cho Đạn (Bullets), Hiệu ứng kỹ năng (FX/VFX), Floating Damage Text, v.v.
    /// </summary>
    [DisallowMultipleComponent]
    public class AutoDespawn : MonoBehaviour, IPoolable
    {
        [Header("Timing")]
        [Tooltip("Tự động despawn sau khoảng thời gian này (giây). Đặt <= 0 nếu chỉ dùng Particle hoặc Audio.")]
        [SerializeField] private float _lifetime = 2.0f;

        [Header("Auto Triggers")]
        [Tooltip("Tự động despawn khi ParticleSystem trên GameObject này dừng phát.")]
        [SerializeField] private bool _despawnOnParticleStopped = false;

        [Tooltip("Tự động despawn khi AudioSource trên GameObject này phát xong âm thanh.")]
        [SerializeField] private bool _despawnOnAudioFinished = false;

        private float _elapsedTime;
        private ParticleSystem _particleSystem;
        private AudioSource _audioSource;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _audioSource = GetComponent<AudioSource>();
        }

        public void OnSpawn()
        {
            _elapsedTime = 0f;

            if (_particleSystem != null)
            {
                _particleSystem.Play();
            }

            if (_audioSource != null && _audioSource.clip != null && _audioSource.playOnAwake)
            {
                _audioSource.Play();
            }
        }

        public void OnDespawn()
        {
            _elapsedTime = 0f;

            if (_particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;

            // 1. Lifetime check
            if (_lifetime > 0f && _elapsedTime >= _lifetime)
            {
                gameObject.Despawn();
                return;
            }

            // 2. Particle check
            if (_despawnOnParticleStopped && _particleSystem != null && !_particleSystem.IsAlive(true))
            {
                gameObject.Despawn();
                return;
            }

            // 3. Audio check
            if (_despawnOnAudioFinished && _audioSource != null && !_audioSource.isPlaying && _elapsedTime > 0.1f)
            {
                gameObject.Despawn();
                return;
            }
        }
    }
}
