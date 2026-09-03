using System;
using System.Collections.Generic;
using VoxelGame.Terrain.Vegetation;

namespace VoxelGame.Terrain
{
    public sealed class PolytypeChunkData : IDisposable
    {
        public VoxelType[] Types 
            => _pooledData.Types;
        public Dictionary<int, VegetationDataRef> IndexToVegetationDataRef 
            => _pooledData.PooledVegetationData.IndexToVegetationDataRef;
        
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

        public void EnsureDictionaryCapacity(int targetCapacity)
        {
            _pooledData.EnsureDictionaryCapacity(targetCapacity);
        }

        private PolytypeChunkPooledData _pooledData;
    }
}