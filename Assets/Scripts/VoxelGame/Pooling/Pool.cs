using System;
using UnityEngine;
using UnityEngine.Pool;

namespace VoxelGame.Pooling
{
    public class Pool<T> where T : class, IPoolable
    {
        public int Count => _pool.CountInactive;

        public Pool(Func<T> create, int initialCount = 0)
        {
            Debug.Assert(create != null);
            _pool = new ObjectPool<T>(
                create,
                item => item.OnBorrowed(),
                item => item.OnReturned()
              );
            _create = create;

            for (int i = 0; i < initialCount; ++i)
            {
                PrewarmOne();
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

        public void PrewarmOne()
        {
            _pool.Release(_create());
        }

        private readonly ObjectPool<T> _pool;
        private readonly Func<T> _create;
    }
}
