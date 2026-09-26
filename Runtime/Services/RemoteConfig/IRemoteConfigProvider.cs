using System;

namespace Unity.Core.Services.RemoteConfig
{
    /// <summary>
    /// Giao diện cho các nhà cung cấp cấu hình từ xa (Firebase Remote Config, Unity Remote Config, Mock).
    /// </summary>
    public interface IRemoteConfigProvider : IService
    {
        bool IsFetched { get; }
        T GetValue<T>(string key, T defaultValue = default);
        void FetchAsync(Action<bool> onComplete = null);
    }
}
