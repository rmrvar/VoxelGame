using System;
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
            bool shouldRunInBackground = true,
            bool isRemesh = false
          )
            : base(chunk, token, shouldRunInBackground)
        {
            MeshVersion = chunk.VoxelVersion;
            _isRemesh = isRemesh;
        }

        public bool IsMeshOutOfDate =>
            Chunk.MeshVersion > MeshVersion || (Chunk.MeshVersion == MeshVersion && !_isRemesh);

        public override bool IsCancelled() => IsMeshOutOfDate || base.IsCancelled();

        public override bool TryLazyExecute()
        {
            Debug.Assert(!_isRemesh);

            bool isMaterializedEmpty =
                Chunk.IsMaterializedMonotype(out VoxelType voxelData) && voxelData == VoxelType.AIR;
            return isMaterializedEmpty;
        }

        protected override ChunkMeshTaskIn PrepareInput()
        {
            ChunkMeshTaskIn input = new();
            CopyVoxels(input.PooledIn.Voxels);
            return input;
        }

        protected override ChunkMeshTaskOut Execute(ChunkMeshTaskIn input, CancellationToken cancellationToken)
        {
            GreedyMesher.Generate(input.PooledIn.Voxels, input.PooledIn.MesherWorkspace);
            GrassMesher.Generate(input.PooledIn.Voxels, input.PooledIn.MesherWorkspace);

            ChunkMeshTaskOut output = new()
            {
                MesherWorkspace = input.PooledIn.MesherWorkspace
            };
            return output;
        }

        protected override void HandleOutput(ChunkMeshTaskOut output, Exception exception)
        {
            Debug.Assert(exception == null);
            if (exception != null)
            {
                return;
            }

            if (IsCancelled())
            {
                return;
            }

            if (Chunk.Mono == null)
            {
                Chunk.InitMono();
            }

            output.MesherWorkspace.GetMesh(Chunk.Mono.Mesh);
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

        private readonly bool _isRemesh;
    }

    public class ChunkMeshTaskIn : IDisposable
    {
        public ChunkMeshTaskPooledIn PooledIn;

        public ChunkMeshTaskIn()
        {
            PooledIn = ChunkMeshTaskPooledIn.Pool.Borrow();
        }

        public void Dispose()
        {
            ChunkMeshTaskPooledIn.Pool.Return(PooledIn);
        }
    }

    public class ChunkMeshTaskOut
    {
        public MesherWorkspace MesherWorkspace;
    }
}