using System.Collections.Generic;
using VoxelGame.Pooling;

namespace VoxelGame.Terrain
{
    public sealed class PooledIndexToT<T> : IPoolable
    {
        public static int DefaultCapacity;

        public Dictionary<int, T> IndexToT;

        public static readonly Pool<PooledIndexToT<T>> Pool = new(() => new PooledIndexToT<T>(DefaultCapacity));

        public void OnBorrowed()
        {
        }

        public void OnReturned()
        {
        }

        public static PooledIndexToT<T> Create(int capacity)
        {
            return new PooledIndexToT<T>(capacity);
        }

        private PooledIndexToT(int capacity)
        {
            IndexToT = new Dictionary<int, T>(capacity);
        }
    }
}