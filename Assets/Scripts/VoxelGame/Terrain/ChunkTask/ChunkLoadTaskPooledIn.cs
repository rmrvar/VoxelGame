using VoxelGame.Pooling;

namespace VoxelGame.Terrain.ChunkTask
{
    public sealed class ChunkLoadTaskPooledIn : IPoolable
    {
        public readonly int[] Heights;
        public readonly int[] PoissonDisk;
        public PolytypeChunkData PolytypeChunkData;

        public static readonly Pool<ChunkLoadTaskPooledIn> Pool = new(
            () => new ChunkLoadTaskPooledIn(),
            10
          );

        public void OnBorrowed()
        {
            if (PolytypeChunkData == null)
            {
                // The PolytypeChunkData was transferred to the Chunk. Make a new one.
                PolytypeChunkData = new PolytypeChunkData();
            }
        }

        public void OnReturned()
        {
        }

        private ChunkLoadTaskPooledIn()
        {
            PolytypeChunkData = new PolytypeChunkData();
            Heights = new int[ChunkConfig.PSizeX * ChunkConfig.PSizeZ];
            // Why times 4? Need to guarantee the existance of neighbor at -R. Which needs info of -2R.
            int diskPSizeX = 4 * ChunkConfig.PoissonDiskRadius + ChunkConfig.SizeX;
            int diskPSizeZ = 4 * ChunkConfig.PoissonDiskRadius + ChunkConfig.SizeZ;
            PoissonDisk = new int[diskPSizeX * diskPSizeZ];
        }
    }
}
