using System;
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
            bool shouldRunInBackground = true,
            bool isReload = false
          ) 
            : base(chunk, token, shouldRunInBackground)
        {
            _isReload = isReload;
        }

        public override bool TryLazyExecute()
        {
            Debug.Assert(!_isReload);

            ChunkManager.Instance.SaveSystem.TryGetSaveData(Chunk.Id, out _saveData);
            _isSaveDataInit = true;

            if (_saveData != null)
            {
                return false;
            }

            if (!ChunkManager.Instance.GetChunkHeightRange(Chunk.Id, out int minHeight, out int maxHeight))
            {
                return false;
            }

            // Check if there is no visible face in this chunk, so we can skip materializing it until needed.
            float minChunkY = Chunk.Position.y;
            float maxChunkY = Chunk.Position.y + ChunkConfig.SizeY - 1;
            if (maxHeight < minChunkY)
            {
                Chunk.InitUnmaterializedEmpty();
                FinishLoading();
                return true;
            }

            if (minHeight > maxChunkY)
            {
                Chunk.InitUnmaterializedSolid(); 
                FinishLoading();
                return true;
            }

            return false;
        }

        protected override ChunkLoadTaskIn PrepareInput()
        {
            Debug.Assert(Chunk.PolyData == null); // This task is for loading chunks for the first time or reloading them as PolyData.

            if (!_isSaveDataInit)
            {
                ChunkManager.Instance.SaveSystem.TryGetSaveData(Chunk.Id, out _saveData);
                _isSaveDataInit = true;
            }

            return new ChunkLoadTaskIn(Chunk.Position, _saveData);
        }

        protected override ChunkLoadTaskOut Execute(ChunkLoadTaskIn input, CancellationToken cancellationToken)
        {
            if (input.SaveData == null)
            {
                return Generate(input, cancellationToken);
            }
            else
            {
                return Parse(input, cancellationToken);
            }
        }

        private ChunkLoadTaskOut Generate(ChunkLoadTaskIn input, CancellationToken cancellationToken)
        {
            ChunkLoadTaskPooledIn pooledIn = input.PooledIn;

            Vector3Int chunkPosition = input.Position;

            // HEIGHTMAP CALCULATION
            int minHeight = int.MaxValue;
            int maxHeight = int.MinValue;

            for (int z = 0; z < ChunkConfig.PSizeZ; ++z)
            for (int x = 0; x < ChunkConfig.PSizeX; ++x)
            {
                int heightIndex = x + z * ChunkConfig.PSizeX;
                int height = BiomeLogic.GetHeight(
                    chunkPosition.x + x - 1,
                    chunkPosition.z + z - 1
                  );
                pooledIn.Heights[heightIndex] = height;
                minHeight = Mathf.Min(minHeight, height);
                maxHeight = Mathf.Max(maxHeight, height);
            }

            // VOXEL CALCULATION
            bool isMonotype = true;
            VoxelType? monotype = null;

            for (int z = 0; z < ChunkConfig.SizeZ; ++z)
            for (int y = 0; y < ChunkConfig.SizeY; ++y)
            {
                int heightIndex0 = (z + 1) * ChunkConfig.PSizeX;

                for (int x = 0; x < ChunkConfig.SizeX; ++x)
                {
                    int heightIndex = heightIndex0 + (x + 1);
                    int height = pooledIn.Heights[heightIndex];

                    Vector3Int position = chunkPosition + new Vector3Int(x, y, z);
                    int voxelTypeIndex = x + y * ChunkConfig.StrideY + z * ChunkConfig.StrideZ;
                    VoxelType voxelType = BiomeLogic.GetVoxelType(position, height);

                    pooledIn.PolytypeChunkData.Data[voxelTypeIndex] = voxelType;

                    if (isMonotype)
                    {
                        if (monotype == null)
                        {
                            monotype = voxelType;
                        } else
                        if (monotype != voxelType)
                        {
                            isMonotype = false;
                        }
                    }
                }
            }

            ChunkLoadTaskOut output = new()
            {
                Input = input,
                IsMonotype = isMonotype,
                MonotypeChunkData = new MonotypeChunkData(monotype.GetValueOrDefault()),
                HasHeight = true,
                MinHeight = minHeight,
                MaxHeight = maxHeight
            };
            return output;
        }

        private ChunkLoadTaskOut Parse(ChunkLoadTaskIn input, CancellationToken cancellationToken)
        {
            byte[] saveData = input.SaveData;
            for (int i = 0; i < saveData.Length; ++i)
            {
                input.PooledIn.PolytypeChunkData.Data[i] = (VoxelType)saveData[i];
            }

            ChunkLoadTaskOut output = new()
            {
                Input = input,
                IsMonotype = false, // We're always poly type if loaded from file.
                MonotypeChunkData = default,
                HasHeight = false
            };
            return output;
        }

        protected override void HandleOutput(ChunkLoadTaskOut output, Exception exception)
        {
            Debug.Assert(exception == null);
            if (exception != null)
            {
                return; // Something went wrong.
            }

            if (IsCancelled())
            {
                return;
            }

            if (output.HasHeight)
            {
                ChunkManager.Instance.SetChunkHeightRange(Chunk.Id, output.MinHeight, output.MaxHeight);
            }

            if (output.IsMonotype && !_isReload)
            {
                Chunk.InitMaterializedMonotype(output.MonotypeChunkData);
            }
            else
            {
                Chunk.InitMaterializedPolytype(output.Input.PooledIn.PolytypeChunkData);
                output.Input.PooledIn.PolytypeChunkData = null; // This transfers ownership to Chunk.
            }

            if (!_isReload)
            {
                FinishLoading();
            }
        }

        private void FinishLoading()
        {
            Chunk.MarkLoaded();

            SyncLoadedNeighborBits(Chunk, Chunk.PosX, 0, 3);
            SyncLoadedNeighborBits(Chunk, Chunk.PosY, 1, 4);
            SyncLoadedNeighborBits(Chunk, Chunk.PosZ, 2, 5);
            SyncLoadedNeighborBits(Chunk, Chunk.NegX, 3, 0);
            SyncLoadedNeighborBits(Chunk, Chunk.NegY, 4, 1);
            SyncLoadedNeighborBits(Chunk, Chunk.NegZ, 5, 2);

            TryToScheduleChunkMeshTask(Chunk);
        }

        private static void SyncLoadedNeighborBits(
            Chunk thisChunk,
            Chunk thatChunk, 
            int thisFaceIndex, 
            int thatFaceIndex
          )
        {
            if (thatChunk == null)
            {
                return;
            }

            thisChunk.SetLoadedNeighborBit(thisFaceIndex, thatChunk.IsLoaded);
            thatChunk.SetLoadedNeighborBit(thatFaceIndex, true);

            TryToScheduleChunkMeshTask(thatChunk);
        }

        private static void TryToScheduleChunkMeshTask(Chunk chunk)
        {
            if (!chunk.IsLoaded)
            {
                return;
            }

            if (!chunk.IsMaterialized)
            {
                return;
            }

            if (chunk.LoadedNeighborMask != 0b111111)
            {
                return;
            }

            ChunkManager.Instance.ScheduleMeshTask(chunk);
        }

        private byte[] _saveData;
        private bool _isSaveDataInit;
        private readonly bool _isReload;
    }

    public class ChunkLoadTaskIn : IDisposable
    {
        public Vector3Int Position;
        public byte[] SaveData;
        public ChunkLoadTaskPooledIn PooledIn;

        public ChunkLoadTaskIn(Vector3Int position, byte[] saveData)
        {
            SaveData = saveData;
            Position = position;
            PooledIn = ChunkLoadTaskPooledIn.Pool.Borrow();
        }

        public void Dispose()
        {
            ChunkLoadTaskPooledIn.Pool.Return(PooledIn);
        }
    }

    public class ChunkLoadTaskOut
    {
        public ChunkLoadTaskIn Input;
        public bool IsMonotype;
        public MonotypeChunkData MonotypeChunkData;
        public bool HasHeight;
        public int MinHeight;
        public int MaxHeight;
    }
}
