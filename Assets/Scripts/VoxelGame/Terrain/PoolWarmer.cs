using VoxelGame.Terrain.ChunkTask;
using VoxelGame.Terrain.Vegetation;

namespace VoxelGame.Terrain
{
    public static class PoolWarmer
    {
        public static void Warm()
        {
            PooledIndexToT<VoxelType>.DefaultCapacity = 10;
            PooledIndexToT<VegetationDataRef>.DefaultCapacity = ChunkConfig.SizeX * ChunkConfig.SizeZ;

            PolytypeChunkPooledData.Pool.Warm(5000);
            PooledIndexToT<VoxelType>.Pool.Warm(5000);
            PooledIndexToT<VegetationDataRef>.Pool.Warm(5000);
            ChunkLoadTaskPooledIn.Pool.Warm(10);
            ChunkMeshTaskPooledIn.Pool.Warm(10);
            ChunkManager.Instance.ChunkMonoPool.Warm(5000);
        }
    }
}
