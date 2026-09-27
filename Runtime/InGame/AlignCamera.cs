using UnityEngine;
using Unity.Core.Tick;

namespace Unity.Core.InGame
{
    /// <summary>
    /// Tự động xoay Transform hướng về phía Camera (Billboard effect) trong mỗi frame.
    /// Thích hợp cho thanh máu, nameplate, icon chỉ hướng trong không gian 3D.
    /// </summary>
    [AddComponentMenu("Unity Core/InGame/Align Camera")]
    public class AlignCamera : TickBehaviour
    {
        private Camera _mainCamera;
        protected Camera MainCamera
        {
            get
            {
                if (_mainCamera == null) _mainCamera = Camera.main;
                return _mainCamera;
            }
        }

        [SerializeField] private bool isAlignWithCamera = true;
        public bool IsAlignWithCamera { get => isAlignWithCamera; set => isAlignWithCamera = value; }

        public override void OnUpdate()
        {
            if (!isAlignWithCamera || MainCamera == null) return;
            transform.forward = MainCamera.transform.forward;
        }
    }
}
