using VoxelGame.Pooling;
using VoxelGame.Terrain.Meshing;

namespace VoxelGame.Terrain.ChunkTask
{
    public sealed class ChunkMeshTaskPooledIn : IPoolable
    {
        public readonly VoxelType[] Voxels;
        public readonly GreedyMesherWorkspace GreedyMesherWorkspace;
        public readonly GrassMesherWorkspace GrassMesherWorkspace;

        public static readonly Pool<ChunkMeshTaskPooledIn> Pool = new(() => new ChunkMeshTaskPooledIn());

        public void OnBorrowed()
        {
            // Voxels are overwritten completely by ChunkMeshTask.
            GrassMesherWorkspace.Clear();
            GreedyMesherWorkspace.Clear();
        }

        public void OnReturned()
        {
        }

        private ChunkMeshTaskPooledIn()
        {
            Voxels = new VoxelType[ChunkConfig.PVolume];
            GreedyMesherWorkspace = new GreedyMesherWorkspace();
            GrassMesherWorkspace = new GrassMesherWorkspace();
        }
    }
}
