using System.Collections.Generic;
using VoxelGame.Terrain.Vegetation;
using VoxelGame.Pooling;

namespace VoxelGame.Terrain
{
    public sealed class PolytypeChunkPooledVegetationData : IPoolable
    {
        public Dictionary<int, VegetationDataRef> IndexToVegetationDataRef;

        public static readonly Pool<PolytypeChunkPooledVegetationData> Pool = new(
            () => new PolytypeChunkPooledVegetationData(),
            5000
          );

        public void OnBorrowed()
        {
        }

        public void OnReturned()
        {
        }

        public static PolytypeChunkPooledVegetationData Create(int capacity)
        {
            return new PolytypeChunkPooledVegetationData(capacity);
        }

        private PolytypeChunkPooledVegetationData()
            : this(ChunkConfig.SizeX * ChunkConfig.SizeZ)
        {
        }

        private PolytypeChunkPooledVegetationData(int capacity)
        {
            IndexToVegetationDataRef = new Dictionary<int, VegetationDataRef>(capacity);
        }
    }
}