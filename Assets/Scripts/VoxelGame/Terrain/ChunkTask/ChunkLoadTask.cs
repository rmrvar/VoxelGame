using System;
using System.Buffers;
using System.Threading;
using UnityEngine;

namespace VoxelGame.Terrain.ChunkTask
{
    public class ChunkLoadTask
        : ChunkTask<ChunkLoadTaskIn, ChunkLoadTaskOut>
    {
        public ChunkLoadTask(
            Chunk chunk, 
            CancellationToken token
          ) 
            : base(chunk, token, shouldRunInBackground: true)
        {
        }

        protected override ChunkLoadTaskIn PrepareInput()
        {
            return new ChunkLoadTaskIn(Chunk.Position, ChunkManager.Instance.ChunkSize);
        }

        protected override ChunkLoadTaskOut Execute(ChunkLoadTaskIn input, CancellationToken cancellationToken)
        {
            Vector3Int chunkPosition = input.Position;
            Vector3Int chunkSize = input.Size;

            for (int z = 0; z < chunkSize.z; ++z)
            for (int x = 0; x < chunkSize.x; ++x)
            {
                int heightIndex = x + z * chunkSize.x;
                int height = BiomeLogic.GetHeight(
                    chunkPosition.x + x, 
                    chunkPosition.z + z
                  );
                input.Heights[heightIndex] = height;
            }

            bool isUniform = true;
            VoxelData.VoxelType? uniformVoxelType = null;

            int strideY = chunkSize.x;
            int strideZ = chunkSize.x * chunkSize.z;

            for (int z = 0; z < chunkSize.z; ++z)
            for (int y = 0; y < chunkSize.y; ++y)
            {
                int heightIndex0 = z * chunkSize.x;

                for (int x = 0; x < chunkSize.x; ++x)
                {
                    int heightIndex = heightIndex0 + x;
                    int height = input.Heights[heightIndex];

                    Vector3Int position = chunkPosition + new Vector3Int(x, y, z);
                    int voxelTypeIndex = x + y * strideY + z * strideZ;
                    VoxelData.VoxelType voxelType = BiomeLogic.GetVoxelType(position, y - height);

                    input.Voxels[voxelTypeIndex] = voxelType;

                    if (isUniform)
                    {
                        if (uniformVoxelType == null)
                        {
                            uniformVoxelType = voxelType;
                        } else 
                        if (uniformVoxelType != voxelType)
                        {
                            isUniform = false;
                        }
                    }
                }
            }

            ChunkLoadTaskOut output = new()
            {
                Input = input,
                IsUniform = isUniform,
                UniformVoxelType = uniformVoxelType.GetValueOrDefault(),
            };
            return output;
        }

        protected override void HandleOutput(ChunkLoadTaskOut output, Exception exception)
        {
            if (exception != null)
            {
                return; // Something went wrong.
            }

            if (!output.IsUniform)
            {
                Chunk.SetVoxels(output.Input.Voxels);
                output.Input.DetachBuffers();
            }
            else
            {
                Chunk.SetUniform(output.UniformVoxelType);
            }

            float diagonalSqDist = 
                output.Input.Size.x * output.Input.Size.x + output.Input.Size.y * output.Input.Size.y + output.Input.Size.z * output.Input.Size.z;

            ChunkManager.Instance.Scheduler.Schedule(
                new ChunkMeshTask(Chunk, Chunk.GetCancellationToken()),
                Priority + diagonalSqDist
              );
        }
    }

    public class ChunkLoadTaskIn : IDisposable
    {
        public int[] Heights;
        public VoxelData.VoxelType[] Voxels;
        public Vector3Int Size;
        public Vector3Int Position;

        public ChunkLoadTaskIn(Vector3Int position, Vector3Int size)
        {
            Position = position;
            Size = size;
            Voxels = ArrayPool<VoxelData.VoxelType>.Shared.Rent(size.x * size.y * size.z);
            Heights = ArrayPool<int>.Shared.Rent(size.x * size.z);
        }

        public void DetachBuffers()
        {
            Voxels = null;
        }

        public void Dispose()
        {
            if (Voxels != null)
            {
                ArrayPool<VoxelData.VoxelType>.Shared.Return(Voxels);
                Voxels = null;
            }
            ArrayPool<int>.Shared.Return(Heights);
        }
    }

    public class ChunkLoadTaskOut
    {
        public ChunkLoadTaskIn Input;
        public bool IsUniform;
        public VoxelData.VoxelType UniformVoxelType;
    }
}
