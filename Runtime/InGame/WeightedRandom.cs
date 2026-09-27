using System;
using System.Collections.Generic;

namespace Unity.Core.InGame
{
    /// <summary>
    /// Bảng xác suất có trọng số (Weighted Probability Picker).
    /// Hỗ trợ gacha, drop tỷ lệ vật phẩm, spawn quái vật theo xác suất phần trăm.
    /// </summary>
    public class WeightedRandom<T>
    {
        private class WeightedItem
        {
            public T Item { get; }
            public double Weight { get; }

            public WeightedItem(T item, double weight)
            {
                Item = item;
                Weight = weight;
            }
        }

        private readonly List<WeightedItem> _items = new List<WeightedItem>();
        private double _totalWeight = 0;
        private readonly Random _random = new Random();

        public int Count => _items.Count;
        public double TotalWeight => _totalWeight;

        public void Add(T item, double weight)
        {
            if (weight <= 0)
                throw new ArgumentException("Trọng số phải lớn hơn 0.", nameof(weight));

            _items.Add(new WeightedItem(item, weight));
            _totalWeight += weight;
        }

        public void Clear()
        {
            _items.Clear();
            _totalWeight = 0;
        }

        public T GetValue()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Danh sách WeightedRandom trống.");

            double randomValue = _random.NextDouble() * _totalWeight;
            double cumulativeWeight = 0;

            foreach (var weightedItem in _items)
            {
                cumulativeWeight += weightedItem.Weight;
                if (randomValue <= cumulativeWeight)
                {
                    return weightedItem.Item;
                }
            }

            return _items[^1].Item;
        }
    }
}
