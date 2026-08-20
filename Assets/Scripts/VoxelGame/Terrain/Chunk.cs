using System;
using System.Threading;
using UnityEngine;
using static VoxelGame.Terrain.VoxelData;

namespace VoxelGame.Terrain
{
	public class Chunk : IDisposable
    {
        public ChunkMono Mono { get; private set; }

        public int MeshVersion { get; set; } = -1;
        
        public int VoxelVersion { get; set; } = -1;

        public Vector3Int Position { get; private set; }
        
        public Vector3Int Id { get; private set; }

        public bool IsLoaded { get; private set; }

        public Chunk PosX => _posX;
        public Chunk PosY => _posY;
        public Chunk PosZ => _posZ;
        public Chunk NegX => _negX;
        public Chunk NegY => _negY;
        public Chunk NegZ => _negZ;

        public MonotypeChunkData MonoData => _monoData;
        public PolytypeChunkData PolyData { get; private set; }

        public Chunk(Vector3Int id)
        {
            Id = id;
            Position = new Vector3Int(
                id.x * ChunkConfig.SizeX,
                id.y * ChunkConfig.SizeY,
                id.z * ChunkConfig.SizeZ
              );
        }

        public CancellationToken GetCancellationToken()
        {
            return _cts.Token;
        }

        public bool IsUnmaterializedSolid => !IsMaterialized && _monoData.Data != VoxelType.AIR;

        public bool IsUnmaterializedEmpty => !IsMaterialized && _monoData.Data == VoxelType.AIR;

        public bool IsMaterialized { get; private set; }

        public bool IsMaterializedMonotype(out VoxelType type)
        {
            type = _monoData.Data;
            return IsMaterialized && PolyData == null;
        }

        public bool IsMaterializedPolytype => IsMaterialized && PolyData != null;

        public void InitUnmaterializedSolid()
        {
            IsMaterialized = false;
            _monoData = new MonotypeChunkData(VoxelType.DIRT);
            PolyData = null;
        }

        public void InitUnmaterializedEmpty()
        {
            IsMaterialized = false;
            _monoData = new MonotypeChunkData(VoxelType.AIR);
            PolyData = null;
        }

        public void InitMaterializedMonotype(MonotypeChunkData monoData)
        {
            IsMaterialized = true;
            _monoData = monoData;
            PolyData = null;
            VoxelVersion = 0;
        }

        public void InitMaterializedPolytype(PolytypeChunkData polyData)
        {
            IsMaterialized = true;
            _monoData = default; // Irrelevant
            PolyData = polyData;
            VoxelVersion = 0;
        }

        public void MarkLoaded()
        {
            IsLoaded = true;
        }

        public byte LoadedNeighborMask { get; private set; }

        public void SetLoadedNeighborBit(int faceIndex, bool isSet)
        {
            byte mask = (byte)(1 << faceIndex);
            if (isSet)
            {
                LoadedNeighborMask |= mask;
            }
            else
            {
                LoadedNeighborMask &= (byte)~mask;
            }
        }

        public void InitNeighbor(Chunk chunk, int faceIndex)
        {
            switch (faceIndex)
            {
                case 0: _posX = chunk; break;
                case 1: _posY = chunk; break;
                case 2: _posZ = chunk; break;
                case 3: _negX = chunk; break;
                case 4: _negY = chunk; break;
                case 5: _negZ = chunk; break;
            }
        }

        // TODO: Hook up when start unloading chunks.
        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();

            if (Mono != null)
            {
                // TODO: Return Mono to pool.
                Mono = null;
            }

            PolyData?.Dispose();
            PolyData = null;
        }

        private MonotypeChunkData _monoData;

        private Chunk _posX;
        private Chunk _posY;
        private Chunk _posZ;
        private Chunk _negX;
        private Chunk _negY;
        private Chunk _negZ;

        private readonly CancellationTokenSource _cts = new();
    }
}
