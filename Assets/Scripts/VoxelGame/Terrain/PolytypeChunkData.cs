using System;
using System.Buffers;
using static VoxelGame.Terrain.VoxelData;

namespace VoxelGame.Terrain
{
    public class PolytypeChunkData : IDisposable
    {
        public VoxelType[] Data;

        public PolytypeChunkData()
        {
            Data = ArrayPool<VoxelType>.Shared.Rent(ChunkConfig.Volume);
        }

        public void Dispose()
        {
            if (Data != null)
            {
                ArrayPool<VoxelType>.Shared.Return(Data);
                Data = null;
            }
        }
    }
}