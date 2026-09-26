using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Core.Data
{
    /// <summary>
    /// Model dữ liệu hồ sơ người chơi (Cài đặt âm thanh, tiền tệ, màn chơi, vật phẩm đã mở khóa).
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        [Header("Settings")]
        public bool Sound = true;
        public bool Music = true;
        public bool Light = true;
        public bool Haptics = true;
        public float MasterVolume = 1.0f;

        [Header("Progression")]
        public string PlayerName = "Player";
        public int Level = 1;
        public long Coins = 0;
        public int Gems = 0;
        public int HighScore = 0;

        [Header("Inventory & Unlocks")]
        public List<string> UnlockedItemIds = new List<string>();
        public string EquippedSkinId = "Default";

        public PlayerProfile()
        {
            Sound = true;
            Music = true;
            Light = true;
            Haptics = true;
            MasterVolume = 1.0f;
            Level = 1;
            Coins = 0;
            Gems = 0;
        }
    }

    /// <summary>
    /// Model dữ liệu vật phẩm trong game (tương thích reference framework).
    /// </summary>
    [Serializable]
    public class ItemInGameData
    {
        public string Id;
        public Vector3 Position;
        public int Amount = 1;

        public ItemInGameData()
        {
        }

        public ItemInGameData(string id, Vector3 position, int amount = 1)
        {
            Id = id;
            Position = position;
            Amount = amount;
        }
    }
}
