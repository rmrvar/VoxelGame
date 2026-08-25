namespace VoxelGame.Terrain
{
    // Provides symmetry with PolytypeChunkData.
    public struct MonotypeChunkData
    {
        public VoxelType Data;

        public MonotypeChunkData(VoxelType data)
        {
            Data = data;
        }
    }
}