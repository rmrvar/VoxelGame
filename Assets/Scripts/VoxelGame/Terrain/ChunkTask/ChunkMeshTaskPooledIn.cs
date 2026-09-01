using VoxelGame.Pooling;
using VoxelGame.Terrain.Meshing;

namespace VoxelGame.Terrain.ChunkTask
{
    public sealed class ChunkMeshTaskPooledIn : IPoolable
    {
        public readonly VoxelType[] Voxels;
        public readonly MesherWorkspace MesherWorkspace;

        public static readonly Pool<ChunkMeshTaskPooledIn> Pool = new(
            () => new ChunkMeshTaskPooledIn(),
            10
          );

        public void OnBorrowed()
        {
            // Voxels are overwritten completely by ChunkMeshTask.
            MesherWorkspace.Clear();
        }

        public void OnReturned()
        {
        }

        private ChunkMeshTaskPooledIn()
        {
            Voxels = new VoxelType[ChunkConfig.PVolume];
            MesherWorkspace = new MesherWorkspace();
        }
    }
}
