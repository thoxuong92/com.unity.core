using System;
using UnityEngine;

namespace Unity.Core.Data
{
    /// <summary>
    /// Model dữ liệu cài đặt hệ thống (Âm thanh, Đồ họa, Ngôn ngữ, Rung).
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        [Header("Audio")]
        public bool SoundEnabled = true;
        public bool MusicEnabled = true;
        public float MasterVolume = 1.0f;
        public float SfxVolume = 1.0f;
        public float MusicVolume = 0.8f;

        [Header("Display & Quality")]
        public int TargetFrameRate = 60;
        public int QualityLevel = 2; // 0: Low, 1: Medium, 2: High, 3: Ultra
        public bool VSync = false;

        [Header("Controls & Gameplay")]
        public bool HapticsEnabled = true;
        public string LanguageCode = "vi";

        public GameSettings()
        {
            SoundEnabled = true;
            MusicEnabled = true;
            MasterVolume = 1.0f;
            SfxVolume = 1.0f;
            MusicVolume = 0.8f;
            TargetFrameRate = 60;
            QualityLevel = 2;
            HapticsEnabled = true;
            LanguageCode = "vi";
        }
    }
}
