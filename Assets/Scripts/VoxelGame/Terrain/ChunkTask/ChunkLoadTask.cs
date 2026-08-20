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
            CancellationToken token,
            bool shouldRunInBackground = true
          ) 
            : base(chunk, token, shouldRunInBackground)
        {
        }

        public override bool TryLazyExecute()
        {
            if (!ChunkManager.Instance.GetChunkHeightRange(Chunk.Id, out int minHeight, out int maxHeight))
            {
                return false;
            }

            // TODO: Return false if ChunkManager.Instance.SaveSystem has an entry for this chunk ID (includes if neighbor made change on border).

            // Check if there is no visible face in this chunk, so we can skip materializing it until needed.
            float minChunkY = Chunk.Position.y;
            float maxChunkY = Chunk.Position.y + ChunkConfig.SizeY - 1;
            if (maxHeight < minChunkY)
            {
                Chunk.InitUnmaterializedEmpty();
                FinishLoad(isMaterialized: false);
                return true;
            }

            if (minHeight > maxChunkY)
            {
                Chunk.InitUnmaterializedSolid(); 
                FinishLoad(isMaterialized: false);
                return true;
            }

            return false;
        }

        protected override ChunkLoadTaskIn PrepareInput()
        {
            return new ChunkLoadTaskIn(Chunk.Position);
        }

        protected override ChunkLoadTaskOut Execute(ChunkLoadTaskIn input, CancellationToken cancellationToken)
        {
            Vector3Int chunkPosition = input.Position;

            int minHeight = int.MaxValue;
            int maxHeight = int.MinValue;

            for (int z = 0; z < ChunkConfig.SizeZ; ++z)
            for (int x = 0; x < ChunkConfig.SizeX; ++x)
            {
                int heightIndex = x + z * ChunkConfig.SizeX;
                int height = BiomeLogic.GetHeight(
                    chunkPosition.x + x, 
                    chunkPosition.z + z
                  );
                input.Heights[heightIndex] = height;
                minHeight = Mathf.Min(minHeight, height);
                maxHeight = Mathf.Max(maxHeight, height);
            }

            bool isUniform = true;
            VoxelData.VoxelType? monotype = null;

            for (int z = 0; z < ChunkConfig.SizeZ; ++z)
            for (int y = 0; y < ChunkConfig.SizeY; ++y)
            {
                int heightIndex0 = z * ChunkConfig.SizeX;

                for (int x = 0; x < ChunkConfig.SizeX; ++x)
                {
                    int heightIndex = heightIndex0 + x;
                    int height = input.Heights[heightIndex];

                    Vector3Int position = chunkPosition + new Vector3Int(x, y, z);
                    int voxelTypeIndex = x + y * ChunkConfig.StrideY + z * ChunkConfig.StrideZ;
                    VoxelData.VoxelType voxelType = BiomeLogic.GetVoxelType(position, height);

                    input.PolytypeChunkData.Data[voxelTypeIndex] = voxelType;

                    if (isUniform)
                    {
                        if (monotype == null)
                        {
                            monotype = voxelType;
                        } else 
                        if (monotype != voxelType)
                        {
                            isUniform = false;
                        }
                    }
                }
            }

            ChunkLoadTaskOut output = new()
            {
                Input = input,
                IsMonotype = isUniform,
                MonotypeChunkData = new MonotypeChunkData(monotype.GetValueOrDefault()),
                MinHeight = minHeight,
                MaxHeight = maxHeight
            };
            return output;
        }

        protected override void HandleOutput(ChunkLoadTaskOut output, Exception exception)
        {
            if (exception != null)
            {
                return; // Something went wrong.
            }

            ChunkManager.Instance.SetChunkHeightRange(Chunk.Id, output.MinHeight, output.MaxHeight);

            if (output.IsMonotype)
            {
                Chunk.InitMaterializedMonotype(output.MonotypeChunkData);
            }
            else
            {
                Chunk.InitMaterializedPolytype(output.Input.PolytypeChunkData);
                output.Input.PolytypeChunkData = null; // This transfers ownership to Chunk.
            }

            FinishLoad(isMaterialized: true);
        }

        private void FinishLoad(bool isMaterialized)
        {
            Chunk.MarkLoaded();

            NotifyNeighbor(Chunk.PosX, 3);
            NotifyNeighbor(Chunk.PosY, 4);
            NotifyNeighbor(Chunk.PosZ, 5);
            NotifyNeighbor(Chunk.NegX, 0);
            NotifyNeighbor(Chunk.NegY, 1);
            NotifyNeighbor(Chunk.NegZ, 2);

            if (isMaterialized && IsNeighborhoodLoaded(Chunk))
            {
                ChunkManager.Instance.ScheduleMeshTask(Chunk);
            }
        }

        private static void NotifyNeighbor(Chunk neighbor, int faceIndex)
        {
            if (neighbor == null)
            {
                return;
            }

            neighbor.SetLoadedNeighborBit(faceIndex, true);

            if (neighbor.IsLoaded && neighbor.IsMaterialized && IsNeighborhoodLoaded(neighbor))
            {
                ChunkManager.Instance.ScheduleMeshTask(neighbor);
            }
        }

        private static bool IsNeighborhoodLoaded(Chunk chunk)
        {
            return chunk.LoadedNeighborMask == 0b111111;
        }
    }

    public class ChunkLoadTaskIn : IDisposable
    {
        public int[] Heights;
        public PolytypeChunkData PolytypeChunkData;
        public Vector3Int Position;

        public ChunkLoadTaskIn(Vector3Int position)
        {
            Position = position;
            PolytypeChunkData = new PolytypeChunkData();
            Heights = ArrayPool<int>.Shared.Rent(ChunkConfig.SizeX * ChunkConfig.SizeZ);
        }

        public void Dispose()
        {
            if (PolytypeChunkData != null)
            {
                PolytypeChunkData.Dispose();
                PolytypeChunkData = null;
            }
            ArrayPool<int>.Shared.Return(Heights);
        }
    }

    public class ChunkLoadTaskOut
    {
        public ChunkLoadTaskIn Input;
        public bool IsMonotype;
        public MonotypeChunkData MonotypeChunkData;
        public int MinHeight;
        public int MaxHeight;
    }
}
