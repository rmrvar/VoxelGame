using VoxelGame.Pooling;

namespace VoxelGame.Terrain
{
    public sealed class PolytypeChunkPooledData : IPoolable
    {
        public readonly VoxelType[] Types;
        public PolytypeChunkPooledVegetationData PooledVegetationData;
        
        public static readonly Pool<PolytypeChunkPooledData> Pool = new(
            () => new PolytypeChunkPooledData(),
            5000
          );

        public void OnBorrowed()
        {
            PooledVegetationData.IndexToVegetationDataRef.Clear();
        }

        public void OnReturned()
        {
        }

        public void EnsureDictionaryCapacity(int targetCapacity)
        {
            int capacity = PooledVegetationData.IndexToVegetationDataRef.EnsureCapacity(0);
            if (capacity < targetCapacity)
            {
                PolytypeChunkPooledVegetationData.Pool.Return(PooledVegetationData);
                PooledVegetationData = PolytypeChunkPooledVegetationData.Create(targetCapacity);
            }
        }

        private PolytypeChunkPooledData()
        {
            Types = new VoxelType[ChunkConfig.Volume];
            PooledVegetationData = PolytypeChunkPooledVegetationData.Pool.Borrow();
        }
    }
}