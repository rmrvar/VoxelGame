using VoxelGame.Pooling;

namespace VoxelGame.Terrain.ChunkTask
{
    public sealed class ChunkLoadTaskPooledIn : IPoolable
    {
        public readonly int[] Heightmap;
        public readonly float[] Slidermap;
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

            int heightmapVolume = ChunkConfig.HeightmapVolume;
            Heightmap = new int[heightmapVolume];
            Slidermap = new float[heightmapVolume];

            int poissonDiskVolume = ChunkConfig.PoissonDiskVolume;
            PoissonDisk = new int[poissonDiskVolume];
        }
    }
}
