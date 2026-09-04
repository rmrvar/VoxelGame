using VoxelGame.Pooling;
using VoxelGame.Terrain.Vegetation;

namespace VoxelGame.Terrain
{
    public sealed class PolytypeChunkPooledData : IPoolable
    {
        public readonly VoxelType[] Types;
        public PooledIndexToT<VoxelType> IndexToOriginalType;
        public PooledIndexToT<VegetationDataRef> IndexToVegetationDataRef;
        
        public static readonly Pool<PolytypeChunkPooledData> Pool = new(() => new PolytypeChunkPooledData());

        public void OnBorrowed()
        {
            IndexToVegetationDataRef.IndexToT.Clear();
            IndexToOriginalType.IndexToT.Clear();
        }

        public void OnReturned()
        {
        }

        public void EnsureCapacityOfIndexToOriginalType(int targetCapacity)
        {
            int capacity = IndexToOriginalType.IndexToT.EnsureCapacity(0);
            if (capacity < targetCapacity)
            {
                PooledIndexToT<VoxelType>.Pool.Return(IndexToOriginalType);
                IndexToOriginalType = PooledIndexToT<VoxelType>.Create(targetCapacity);
            }
        }

        public void EnsureCapacityOfIndexToVegetationDataRef(int targetCapacity)
        {
            int capacity = IndexToVegetationDataRef.IndexToT.EnsureCapacity(0);
            if (capacity < targetCapacity)
            {
                PooledIndexToT<VegetationDataRef>.Pool.Return(IndexToVegetationDataRef);
                IndexToVegetationDataRef = PooledIndexToT<VegetationDataRef>.Create(targetCapacity);
            }
        }

        private PolytypeChunkPooledData()
        {
            Types = new VoxelType[ChunkConfig.Volume];
            IndexToOriginalType = PooledIndexToT<VoxelType>.Pool.Borrow();
            IndexToVegetationDataRef = PooledIndexToT<VegetationDataRef>.Pool.Borrow();
        }
    }
}