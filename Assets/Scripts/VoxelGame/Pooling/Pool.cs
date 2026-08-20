using System;
using UnityEngine.Pool;

namespace VoxelGame.Pooling
{
    public class Pool<T> where T : class, IPoolable
    {
        public int Count => _pool.CountInactive;

        public Pool(Func<T> create, int initialCapacity = 0)
        {
            _pool = new ObjectPool<T>(
                create,
                item => item.OnBorrowed(),
                item => item.OnReturned()
              );

            for (int i = 0; i < initialCapacity; ++i)
            {
                _pool.Release(create());
            }
        }

        public T Borrow()
        {
            return _pool.Get();
        }

        public void Return(T item)
        {
            _pool.Release(item);
        }

        private readonly ObjectPool<T> _pool;
    }
}
