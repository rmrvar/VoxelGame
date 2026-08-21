using System;
using System.Buffers;
using System.Threading;
using UnityEngine;
using VoxelGame.Terrain.Meshing;
using static VoxelGame.Terrain.VoxelData;

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

        public override bool TryLazyExecute()
        {
            return Chunk.IsMaterializedMonotype(out VoxelType voxelData) && voxelData == VoxelType.AIR;
        }

        protected override ChunkMeshTaskIn PrepareInput()
        {
            ChunkMeshTaskIn input = new();
            CopyVoxels(input.Voxels);
            return input;
        }

        protected override ChunkMeshTaskOut Execute(ChunkMeshTaskIn input, CancellationToken cancellationToken)
        {
            ChunkMeshTaskOut output = new() { Buffer = input.Buffer };

            GreedyMesher.Generate(input.Voxels, input.Buffer);

            return output;
        }

        protected override void HandleOutput(ChunkMeshTaskOut output, Exception exception)
        {
            if (exception != null)
            {
                return;
            }
            if (Chunk.MeshVersion >= MeshVersion)
            {
                return;
            }

            if (Chunk.Mono == null)
            {
                Chunk.InitMono();
            }

            GreedyMesher.GetMesh(output.Buffer, Chunk.Mono.Mesh);
            Chunk.Mono.Refresh();
            Chunk.Mono.IsVisible = true;
            Chunk.MeshVersion = MeshVersion;
        }

        private void CopyVoxels(VoxelType[] destination)
        {
            CopyInterior(destination, Chunk);
            FillFace(destination, Chunk.PosX, 0, 0);
            FillFace(destination, Chunk.PosY, 1, 0);
            FillFace(destination, Chunk.PosZ, 2, 0);
            FillFace(destination, Chunk.NegX, 0, 1);
            FillFace(destination, Chunk.NegY, 1, 1);
            FillFace(destination, Chunk.NegZ, 2, 1);
        }

        private void CopyInterior(
            VoxelType[] destination, 
            Chunk chunk
          )
        {
            Debug.Assert(chunk.IsMaterialized);

            bool isMonotype = chunk.IsMaterializedMonotype(out VoxelType monotype);

            for (int z = 0; z < ChunkConfig.SizeZ; ++z)
            for (int y = 0; y < ChunkConfig.SizeY; ++y)
            {
                int dstIndex = 1 + (y + 1) * ChunkConfig.PStrideY + (z + 1) * ChunkConfig.PStrideZ;

                if (isMonotype)
                {
                    Array.Fill(destination, monotype, dstIndex, ChunkConfig.SizeX);
                }
                else
                {
                    int srcIndex = y * ChunkConfig.StrideY + z * ChunkConfig.StrideZ;
                    Array.Copy(Chunk.PolyData.Data, srcIndex, destination, dstIndex, ChunkConfig.SizeX);
                }
            }
        }

        private void FillFace(
            VoxelType[] destination,
            Chunk neighbor,
            int axis,
            int sign
          )
        {
            if (neighbor == null)
            {
                // Very rare exception that we have unloaded a neighboring chunk (which requires you to move
                // very far away) and then reached it again and modified it in a way requiring a remesh but 
                // before the neighboring chunk was reloaded. In this case, just be conservative and show the
                // entire border.
                FillFace(
                    destination,
                    VoxelType.AIR,
                    axis,
                    sign
                  );
            } else
            if (neighbor.IsUnmaterializedSolid)
            {
                FillFace(
                    destination,
                    VoxelType.DIRT,
                    axis,
                    sign
                  );
            } else
            if (neighbor.IsUnmaterializedEmpty)
            {
                FillFace(
                    destination,
                    VoxelType.AIR,
                    axis,
                    sign
                  );
            } else
            if (neighbor.IsMaterializedMonotype(out VoxelType monotype))
            {
                FillFace(
                    destination, 
                    monotype, 
                    axis, 
                    sign
                  );
            } else
            {
                Debug.Assert(neighbor.IsMaterializedPolytype);
                CopyFace(
                    destination,
                    neighbor.PolyData.Data,
                    axis,
                    sign
                  );
            }
        }

        // Fills the specified border face of flat 3D array <destination> with <value>.
        // <axis> specifies the face axis and <sign> its direction: 1 for negative, 0 for positive.
        private void FillFace(
            VoxelType[] destination,
            VoxelType value,
            int axis,
            int sign
          )
        {
            Vector3Int size = ChunkConfig.Size;

            switch (axis)
            {
                case 0:
                {
                    int dstX = sign == 0 
                        ? size.x + 1
                        : 0;

                    for (int z = 0; z < size.z; ++z)
                    for (int y = 0; y < size.y; ++y)
                    {
                        int dstIndex = dstX
                            + (y + 1) * ChunkConfig.PStrideY
                            + (z + 1) * ChunkConfig.PStrideZ;

                        destination[dstIndex] = value;
                    }

                    break;
                }
                case 1:
                {
                    int dstY = sign == 0 
                        ? size.y + 1
                        : 0;

                    for (int z = 0; z < size.z; ++z)
                    {
                        int dstIndex = (z + 1) * ChunkConfig.PStrideZ
                            + dstY * ChunkConfig.PStrideY
                            + 1;

                        Array.Fill(
                            destination,
                            value,
                            dstIndex,
                            ChunkConfig.StrideY
                          );
                    }

                    break;
                }
                case 2:
                {
                    // Also fills the border edges, which are not needed but avoids multiple fill operations.
                    int dstZ = sign == 0 
                        ? size.z + 1
                        : 0;

                    int dstIndex = dstZ * ChunkConfig.PStrideZ;

                    Array.Fill(
                        destination,
                        value,
                        dstIndex,
                        ChunkConfig.PStrideZ
                      );

                    break;
                }
            }
        }

        // Fills the specified border face of flat 3D array <destination> with values copied from the opposite border face of flat 3D array <source>.
        // <axis> specifies the face axis and <sign> its direction: 1 for negative, 0 for positive.
        // <destination> is padded by one voxel on each side, while <source> is sized according to <size>.
        private void CopyFace(
            VoxelType[] destination,
            VoxelType[] source,
            int axis,
            int sign
          )
        {

            switch (axis)
            {
                case 0:
                {
                    int srcX = sign == 0
                        ? 0
                        : ChunkConfig.SizeX - 1;

                    int dstX = sign == 0
                        ? ChunkConfig.SizeX + 1
                        : 0;

                    for (int z = 0; z < ChunkConfig.SizeZ; ++z)
                    for (int y = 0; y < ChunkConfig.SizeY; ++y)
                    {
                        int srcIndex = srcX
                            + y * ChunkConfig.StrideY
                            + z * ChunkConfig.StrideZ;

                        int dstIndex = dstX
                            + (y + 1) * ChunkConfig.PStrideY
                            + (z + 1) * ChunkConfig.PStrideZ;

                        destination[dstIndex] = source[srcIndex];
                    }

                    break;
                }
                case 1:
                {
                    int srcY = sign == 0
                        ? 0
                        : ChunkConfig.SizeY - 1;

                    int dstY = sign == 0
                        ? ChunkConfig.SizeY + 1
                        : 0;

                    for (int z = 0; z < ChunkConfig.SizeZ; ++z)
                    {
                        int srcIndex = srcY * ChunkConfig.StrideY
                            + z * ChunkConfig.StrideZ;

                        int dstIndex = (z + 1) * ChunkConfig.PStrideZ
                            + dstY * ChunkConfig.PStrideY
                            + 1;

                        Array.Copy(
                            source,
                            srcIndex,
                            destination,
                            dstIndex,
                            ChunkConfig.StrideY
                          );
                    }

                    break;
                }
                default:
                {
                    Debug.Assert(axis == 2);
                    int srcZ = sign == 0
                        ? 0
                        : ChunkConfig.SizeZ - 1;

                    int dstZ = sign == 0
                        ? ChunkConfig.SizeZ + 1
                        : 0;

                    for (int y = 0; y < ChunkConfig.SizeY; ++y)
                    {
                        int srcIndex = srcZ * ChunkConfig.StrideZ
                            + y * ChunkConfig.StrideY;

                        int dstIndex = dstZ * ChunkConfig.PStrideZ
                            + (y + 1) * ChunkConfig.PStrideY
                            + 1;

                        Array.Copy(
                            source,
                            srcIndex,
                            destination,
                            dstIndex,
                            ChunkConfig.StrideY
                          );
                    }

                    break;
                }
            }
        }
    }

    public class ChunkMeshTaskIn : IDisposable
    {
        public VoxelType[] Voxels;
        public GreedyMesherBuffer Buffer;

        public ChunkMeshTaskIn()
        {
            Voxels = ArrayPool<VoxelType>.Shared.Rent(ChunkConfig.PVolume);
            Buffer = ChunkManager.Instance.GreedyMesherBufferPool.Borrow();
        }

        public void Dispose()
        {
            ArrayPool<VoxelType>.Shared.Return(Voxels);
            ChunkManager.Instance.GreedyMesherBufferPool.Return(Buffer);
        }
    }

    public class ChunkMeshTaskOut
    {
        public GreedyMesherBuffer Buffer;
    }
}