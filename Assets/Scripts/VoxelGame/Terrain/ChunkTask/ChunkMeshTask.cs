using System;
using System.Threading;
using Assets.Scripts.VoxelGame.Terrain;
using UnityEngine;
using VoxelGame.Terrain.Meshing;

namespace VoxelGame.Terrain.ChunkTask
{
    public class ChunkMeshTask
        : ChunkTask<ChunkMeshTaskIn, ChunkMeshTaskOut>
    {
        public int MeshVersion { get; }

        public ChunkMeshTask(
            Chunk chunk,
            CancellationToken token,
            bool shouldRunInBackground = true
          )
            : base(chunk, token, shouldRunInBackground)
        {
            MeshVersion = chunk.VoxelVersion;
        }

        public override bool IsCancelled() => Chunk.MeshVersion >= MeshVersion || base.IsCancelled();

        public override bool CanExecute() => ChunkManager.Instance.IsNeighborhoodLoaded(Chunk.Id);

        protected override ChunkMeshTaskIn PrepareInput()
        {
            // TODO: Check if even need to request buffer. If voxels == null => uniformVoxelType != null. Call a different mesher in that case.

            ChunkMeshTaskIn input = new(ChunkManager.Instance.ChunkSize);

            // TODO: Copy chunk and neighboring slices.

            return input;
        }


        protected override ChunkMeshTaskOut Execute(ChunkMeshTaskIn input, CancellationToken cancellationToken)
        {
            ChunkMeshTaskOut output = new() { Buffer = input.Buffer };

            GreedyMesher.Generate(input.Voxels, input.ChunkSize, input.Buffer);

            return output;
        }

        protected override void HandleOutput(ChunkMeshTaskOut output, Exception exception)
        {
            if (exception != null)
            {
                return;
            }
            if (Chunk.MeshVersion >= MeshVersion) // TODO: Make this a helper function.
            {
                return;
            }

            Mesh mesh = GreedyMesher.GetMesh(output.Buffer);
            Chunk.ApplyMesh(mesh, MeshVersion);

            // TODO: Potentially remove delta box colliders with <= MeshVersion here.
        }
    }

    public class ChunkMeshTaskIn : IDisposable
    {
        public VoxelData.VoxelType[] Voxels;
        public GreedyMesherBuffer Buffer;

        public ChunkMeshTaskIn(Vector3Int chunkSize)
        {
            Voxels = BufferPool.Borrow<VoxelData.VoxelType[]>(
                (chunkSize.x + 2) * (chunkSize.y + 2) * (chunkSize.z + 2)
              );
            Buffer = BufferPool.Borrow<GreedyMesherBuffer>();
        }

        public void Dispose()
        {
            BufferPool.Return(Voxels);
            BufferPool.Return(Buffer);
        }
    }

    public class ChunkMeshTaskOut
    {
        public GreedyMesherBuffer Buffer;
    }
}