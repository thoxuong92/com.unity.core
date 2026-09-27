using System;

namespace Unity.Core.Protected
{
    public interface IRandomProvider
    {
        int Next();
        int Next(int maxValue);
        int Next(int minValue, int maxValue);
        double NextDouble();
    }

    /// <summary>
    /// Bộ sinh số giả ngẫu nhiên có thể gieo seed (Deterministic Pseudo-Random Generator)
    /// dựa trên thuật toán XorShift128+ siêu tốc và an toàn, chống cheat drop rate.
    /// </summary>
    public class PseudoRandom : IRandomProvider
    {
        private ulong _s0;
        private ulong _s1;

        public PseudoRandom() : this((ulong)DateTime.UtcNow.Ticks) { }

        public PseudoRandom(ulong seed)
        {
            SetSeed(seed);
        }

        public void SetSeed(ulong seed)
        {
            _s0 = seed == 0 ? 0x8a5cd789635d2dffUL : seed;
            _s1 = _s0 ^ 0x4f5d2f7823901bcaUL;
        }

        public ulong NextULong()
        {
            ulong s1 = _s0;
            ulong s0 = _s1;
            _s0 = s0;
            s1 ^= s1 << 23;
            _s1 = s1 ^ s0 ^ (s1 >> 17) ^ (s0 >> 26);
            return _s1 + s0;
        }

        public int Next()
        {
            return (int)(NextULong() & 0x7FFFFFFF);
        }

        public int Next(int maxValue)
        {
            if (maxValue <= 0) return 0;
            return (int)(NextULong() % (ulong)maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            if (minValue >= maxValue) return minValue;
            return minValue + Next(maxValue - minValue);
        }

        public double NextDouble()
        {
            return (NextULong() >> 11) * (1.0 / (1UL << 53));
        }

        public float NextFloat(float min = 0f, float max = 1f)
        {
            return min + (float)NextDouble() * (max - min);
        }
    }
}
