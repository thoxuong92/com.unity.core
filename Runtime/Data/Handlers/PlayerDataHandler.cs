using System;
using UnityEngine;

namespace Unity.Core.Data
{
    /// <summary>
    /// Handler xử lý logic nghiệp vụ và tiến trình người chơi (Coins, Gems, Level, Settings).
    /// Truy cập tập trung qua: DataManager.Get<PlayerDataHandler>().AddCoins(100);
    /// </summary>
    public class PlayerDataHandler : DataHandler<PlayerProfile>
    {
        public const string DefaultKey = "UnityCore_PlayerProfile";

        public PlayerDataHandler() : base(DefaultKey)
        {
        }

        public PlayerDataHandler(string key, IDataStorage<PlayerProfile> storage = null) 
            : base(key, storage)
        {
        }

        #region Convenience Progression Methods

        public bool IsSoundOn => Data.Sound;
        public bool IsMusicOn => Data.Music;
        public long Coins => Data.Coins;
        public int Gems => Data.Gems;
        public int Level => Data.Level;

        public void AddCoins(long amount)
        {
            if (amount <= 0) return;
            Data.Coins += amount;
            Save();
        }

        public bool SpendCoins(long amount)
        {
            if (amount <= 0 || Data.Coins < amount) return false;
            Data.Coins -= amount;
            Save();
            return true;
        }

        public void AddGems(int amount)
        {
            if (amount <= 0) return;
            Data.Gems += amount;
            Save();
        }

        public bool SpendGems(int amount)
        {
            if (amount <= 0 || Data.Gems < amount) return false;
            Data.Gems -= amount;
            Save();
            return true;
        }

        public void SetLevel(int level)
        {
            Data.Level = Mathf.Max(1, level);
            Save();
        }

        public void SetSound(bool enabled)
        {
            Data.Sound = enabled;
            Save();
        }

        public void SetMusic(bool enabled)
        {
            Data.Music = enabled;
            Save();
        }

        #endregion
    }
}
