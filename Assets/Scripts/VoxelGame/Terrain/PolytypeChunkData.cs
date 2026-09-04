using System;
using System.Collections.Generic;
using VoxelGame.Terrain.Vegetation;

namespace VoxelGame.Terrain
{
    public sealed class PolytypeChunkData : IDisposable
    {
        public VoxelType[] Types => _pooledData.Types;
        public Dictionary<int, VoxelType> IndexToOriginalType 
            => _pooledData.IndexToOriginalType.IndexToT;
        public Dictionary<int, VegetationDataRef> IndexToVegetationDataRef 
            => _pooledData.IndexToVegetationDataRef.IndexToT;
        
        public PolytypeChunkData()
        {
            _pooledData = PolytypeChunkPooledData.Pool.Borrow();
        }

        public void Dispose()
        {
            if (_pooledData == null)
            {
                return;
            }
            PolytypeChunkPooledData.Pool.Return(_pooledData);
            _pooledData = null;
        }

        public void EnsureCapacityOfIndexToOriginalType(int targetCapacity)
        {
            _pooledData.EnsureCapacityOfIndexToOriginalType(targetCapacity);
        }

        public void EnsureCapacityOfIndexToVegetationDataRef(int targetCapacity)
        {
            _pooledData.EnsureCapacityOfIndexToVegetationDataRef(targetCapacity);
        }

        private PolytypeChunkPooledData _pooledData;
    }
}