using System;
using System.Threading;
using UnityEngine;
using VoxelGame.Terrain.Vegetation;

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

            return new ChunkLoadTaskIn(
                ChunkManager.Instance.Seed,
                Chunk.Position, 
                _saveData
              );
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

            int poissonRadius = ChunkConfig.PoissonDiskRadius;

            for (int z = 0; z < ChunkConfig.HeightmapSizeZ; ++z)
            for (int x = 0; x < ChunkConfig.HeightmapSizeX; ++x)
            {
                int heightIndex = x + z * ChunkConfig.HeightmapSizeX;

                int worldX = chunkPosition.x + x - poissonRadius;
                int worldZ = chunkPosition.z + z - poissonRadius;

                float slider = BiomeLogic.GetSlider(worldX, worldZ);

                int height = BiomeLogic.GetHeight(worldX, worldZ, slider);
                pooledIn.Slidermap[heightIndex] = slider;
                pooledIn.Heightmap[heightIndex] = height;
                
                if (minHeight > height)
                {
                    minHeight = height;
                }
                if (maxHeight < height)
                {
                    maxHeight = height;
                }
            }

            // Add a conservative tree height estimate.
            maxHeight += 10;

            // VOXEL CALCULATION
            bool isMonotype = true;
            VoxelType? monotype = null;

            for (int z = 0; z < ChunkConfig.SizeZ; ++z)
            for (int x = 0; x < ChunkConfig.SizeX; ++x)
            {
                int heightIndex = (x + poissonRadius) + (z + poissonRadius) * ChunkConfig.HeightmapSizeX;
                int height = pooledIn.Heightmap[heightIndex];

                int worldX = chunkPosition.x + x;
                int worldZ = chunkPosition.z + z;
                
                for (int y = 0; y < ChunkConfig.SizeY; ++y)
                {
                    int worldY = chunkPosition.y + y;

                    int typeIndex = x + y * ChunkConfig.StrideY + z * ChunkConfig.StrideZ;
                    VoxelType type = BiomeLogic.GetVoxelType(worldX, worldY, worldZ, height);
                    pooledIn.PolytypeChunkData.Data[typeIndex] = type;

                    if (isMonotype)
                    {
                        if (monotype == null)
                        {
                            monotype = type;
                        } else
                        if (monotype != type)
                        {
                            isMonotype = false;
                        }
                    }
                }
            }

            // TREE PLACEMENT
            for (int z = 0; z < ChunkConfig.PoissonDiskSizeZ; ++z)
            for (int x = 0; x < ChunkConfig.PoissonDiskSizeX; ++x)
            {
                int i = x + z * ChunkConfig.PoissonDiskSizeX;
                int worldX = chunkPosition.x + x - poissonRadius;
                int worldZ = chunkPosition.z + z - poissonRadius;

                pooledIn.PoissonDisk[i] = GetTreeRangeHash(worldX, worldZ, input.Seed);
            }

            int endX = ChunkConfig.PoissonDiskSizeX - poissonRadius;
            int endZ = ChunkConfig.PoissonDiskSizeZ - poissonRadius;
            for (int z = poissonRadius; z < endZ; ++z)
            for (int x = poissonRadius; x < endX; ++x)
            {
                int heightX = x - poissonRadius;
                int heightZ = z - poissonRadius;
                int localX = heightX - poissonRadius;
                int localZ = heightZ - poissonRadius;
                int worldX = chunkPosition.x + localX;
                int worldZ = chunkPosition.z + localZ;

                int heightIndex = heightX + heightZ * ChunkConfig.HeightmapSizeX;
                float slider = input.PooledIn.Slidermap[heightIndex];
                if (!BiomeLogic.TryGetMinTreeDistance(worldX, worldZ, slider, out int radius))
                {
                    continue;
                }

                int outerIndex = x + z * ChunkConfig.PoissonDiskSizeX;
                int outerValue = pooledIn.PoissonDisk[outerIndex];

                for (int dz = -radius; dz <= +radius; ++dz)
                for (int dx = -radius; dx <= +radius; ++dx)
                {
                    if (dx == 0 && dz == 0)
                    {
                        continue; // Center
                    }

                    int innerIndexX = x + dx;
                    int innerIndexZ = z + dz;
                    int innerIndex = innerIndexX + innerIndexZ * ChunkConfig.PoissonDiskSizeX;
                    int innerValue = pooledIn.PoissonDisk[innerIndex];
                    if (innerValue >= outerValue)
                    {
                        goto end;
                    }
                }

                // Tree exists. See if any part crosses border.
                int localY = input.PooledIn.Heightmap[heightIndex] + 1 - chunkPosition.y;

                int treeIndex = Mathf.Abs(GetTreeIndexHash(worldX, worldZ, input.Seed)) % VegetationSystem.Instance.TreeCount;
                VegetationData vegetationData = VegetationSystem.Instance.GetTree(treeIndex);
                vegetationData.ForEach((i, x, y, z) =>
                {
                    int newLocalX = localX + x;
                    int newLocalY = localY + y;
                    int newLocalZ = localZ + z;

                    if (newLocalX < 0 || newLocalY < 0 || newLocalZ < 0 || newLocalX >= ChunkConfig.SizeX || newLocalY >= ChunkConfig.SizeY || newLocalZ >= ChunkConfig.SizeZ)
                    {
                        return; // Out of bounds.
                    }

                    // Place a tree.
                    int polytypeIndex = newLocalX + newLocalY * ChunkConfig.StrideY + newLocalZ * ChunkConfig.StrideZ;
                    if (pooledIn.PolytypeChunkData.Data[polytypeIndex] == VoxelType.AIR)
                    {
                        pooledIn.PolytypeChunkData.Data[polytypeIndex] = vegetationData.GetType(i);
                        isMonotype = false;
                    }
                });

                end:;
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

        private static int GetTreeRangeHash(int x, int z, int seed)
        {
            return Hash(x, z, seed, 1000);
        }

        private static int GetTreeIndexHash(int x, int z, int seed)
        {
            return Hash(x, z, seed, 2000);
        }

        private static int Hash(int x, int z, int seed, int salt)
        {
            uint h = (uint)x * 374761393u;
            h += (uint)z * 668265263u;
            h += (uint)seed * 1442695041u;
            h += (uint)salt * 2246822519u;

            h = (h ^ (h >> 13)) * 1274126177u;
            return (int)(h ^ (h >> 16));
        }

        private byte[] _saveData;
        private bool _isSaveDataInit;
        private readonly bool _isReload;
    }

    public class ChunkLoadTaskIn : IDisposable
    {
        public readonly int Seed;
        public readonly Vector3Int Position;
        public readonly byte[] SaveData;
        public readonly ChunkLoadTaskPooledIn PooledIn;

        public ChunkLoadTaskIn(int seed, Vector3Int position, byte[] saveData)
        {
            Seed = seed;
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
