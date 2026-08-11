using System;
using System.Buffers;
using System.Threading;
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

        public override bool CanExecute() => IsNeighborhoodLoaded();

        protected override ChunkMeshTaskIn PrepareInput()
        {
            // TODO: Check if even need to request buffer. If voxels == null => uniformVoxelType != null. Call a different mesher in that case.

            ChunkMeshTaskIn input = new(ChunkManager.Instance.ChunkSize);
            CopyVoxels(input.Voxels, input.ChunkSize);
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

            Mesh oldMesh = Chunk.GetMesh();
            Mesh newMesh = GreedyMesher.GetMesh(output.Buffer, oldMesh);
            Chunk.ApplyMesh(newMesh, MeshVersion);

            // TODO: Potentially remove delta box colliders with <= MeshVersion here.
        }

        private bool IsNeighborhoodLoaded()
        {
            Chunk negX = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int(-1,  0,  0));
            Chunk posX = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int(+1,  0,  0));
            Chunk negY = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int( 0, -1,  0));
            Chunk posY = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int( 0, +1,  0));
            Chunk negZ = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int( 0,  0, -1));
            Chunk posZ = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int( 0,  0, +1));
            return
                  Chunk != null && Chunk.IsLoaded
               &&  negX != null && negX.IsLoaded
               &&  posX != null && posX.IsLoaded
               &&  negY != null && negY.IsLoaded
               &&  posY != null && posY.IsLoaded
               &&  negZ != null && negZ.IsLoaded
               &&  posZ != null && posZ.IsLoaded;
        }

        private void CopyVoxels(VoxelData.VoxelType[] voxels, Vector3Int size)
        {
            Chunk negX = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int(-1,  0,  0));
            Chunk posX = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int(+1,  0,  0));
            Chunk negY = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int( 0, -1,  0));
            Chunk posY = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int( 0, +1,  0));
            Chunk negZ = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int( 0,  0, -1));
            Chunk posZ = ChunkManager.Instance.GetChunkById(Chunk.Id + new Vector3Int( 0,  0, +1));

            int yStride = size.x + 2;
            int zStride = (size.x + 2) * (size.y + 2);

            for (int z = 0; z < size.z + 2; ++z)
            for (int y = 0; y < size.y + 2; ++y)
            for (int x = 0; x < size.x + 2; ++x)
            {
                int i = x + y * yStride + z * zStride;
                voxels[i] = GetVoxel(x, y, z, size, negX, posX, negY, posY, negZ, posZ);
            }
        }

        private VoxelData.VoxelType GetVoxel(
            int x,
            int y,
            int z,
            Vector3Int size,
            Chunk negX,
            Chunk posX,
            Chunk negY,
            Chunk posY,
            Chunk negZ,
            Chunk posZ
          )
        {
            x--;
            y--;
            z--;


            int outside = 0;

            if (x < 0 || x >= size.x)
            {
                ++outside;
            }
            if (y < 0 || y >= size.y)
            {
                ++outside;
            }
            if (z < 0 || z >= size.z)
            {
                ++outside;
            }

            // Corner chunk. Doesn't affect greedy meshing so just take air.
            if (outside >= 2)
            {
                return VoxelData.VoxelType.AIR;
            }

            if (x < 0)
            {
                return negX.GetVoxel(x + size.x, y, z);
            }
            if (x >= size.x)
            {
                return posX.GetVoxel(x - size.x, y, z);
            }
            if (y < 0)
            {
                return negY.GetVoxel(x, y + size.y, z);
            }
            if (y >= size.y)
            {
                return posY.GetVoxel(x, y - size.y, z);
            }
            if (z < 0)
            {
                return negZ.GetVoxel(x, y, z + size.z);
            }
            if (z >= size.z)
            {
                return posZ.GetVoxel(x, y, z - size.z);
            }
            return Chunk.GetVoxel(x, y, z);
        }
    }

    public class ChunkMeshTaskIn : IDisposable
    {
        public VoxelData.VoxelType[] Voxels;
        public GreedyMesherBuffer Buffer;
        public Vector3Int ChunkSize;

        public ChunkMeshTaskIn(Vector3Int chunkSize)
        {
            ChunkSize = chunkSize;
            Voxels = ArrayPool<VoxelData.VoxelType>.Shared.Rent(
                (chunkSize.x + 2) * (chunkSize.y + 2) * (chunkSize.z + 2)
              );
            Buffer = GreedyMesherBuffer.Borrow();
        }

        public void Dispose()
        {
            ArrayPool<VoxelData.VoxelType>.Shared.Return(Voxels);
            GreedyMesherBuffer.Return(Buffer);
        }
    }

    public class ChunkMeshTaskOut
    {
        public GreedyMesherBuffer Buffer;
    }
}