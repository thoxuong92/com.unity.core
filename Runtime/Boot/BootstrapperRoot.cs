using UnityEngine;

namespace Unity.Core.Boot
{
    /// <summary>
    /// Root GameObject dùng chung cho các core runtime component (GameBootstrapper, Splash).
    /// Đảm bảo duy nhất 1 GameObject trong DontDestroyOnLoad để quản lý vòng đời ứng dụng.
    /// </summary>
    internal static class BootstrapperRoot
    {
        private static GameObject _root;

        public static GameObject GetOrCreate()
        {
            if (_root == null)
            {
                _root = GameObject.Find("[Unity.Core.Bootstrapper]");
                if (_root == null)
                {
                    _root = new GameObject("[Unity.Core.Bootstrapper]");
                    Object.DontDestroyOnLoad(_root);
                }
            }
            return _root;
        }
    }
}
