using System;
using System.Collections.Generic;
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
            VoxelType[] types = pooledIn.PolytypeChunkData.Types;
            Dictionary<int, VegetationDataRef> indexToVegetationDataRef = pooledIn.PolytypeChunkData.IndexToVegetationDataRef;
            Vector3Int chunkPosition = input.Position;
            float[] slidermap = pooledIn.Slidermap;
            int[] heightmap = pooledIn.Heightmap;
            uint[] poissonDisk = pooledIn.PoissonDisk;

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

                float slider = BiomeLogic.GetSlider(worldX, worldZ, input.Seed);

                int height = BiomeLogic.GetHeight(worldX, worldZ, slider, input.Seed);
                slidermap[heightIndex] = slider;
                heightmap[heightIndex] = height;
                
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
                int height = heightmap[heightIndex];

                int worldX = chunkPosition.x + x;
                int worldZ = chunkPosition.z + z;
                
                for (int y = 0; y < ChunkConfig.SizeY; ++y)
                {
                    int worldY = chunkPosition.y + y;

                    int typeIndex = x + y * ChunkConfig.StrideY + z * ChunkConfig.StrideZ;
                    VoxelType type = BiomeLogic.GetVoxelType(worldX, worldY, worldZ, height, input.Seed);
                    types[typeIndex] = type;

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

            // VEGETATION DISK
            for (int z = 0; z < ChunkConfig.PoissonDiskSizeZ; ++z)
            for (int x = 0; x < ChunkConfig.PoissonDiskSizeX; ++x)
            {
                int i = x + z * ChunkConfig.PoissonDiskSizeX;
                int worldX = chunkPosition.x + x - poissonRadius;
                int worldZ = chunkPosition.z + z - poissonRadius;

                poissonDisk[i] = Hash(worldX, worldZ, input.Seed, 0);
            }

            // VEGETATION PLACEMENT
            VegetationSystem vegetationSystem = VegetationSystem.Instance;

            int endX2 = ChunkConfig.PoissonDiskSizeX - poissonRadius;
            int endZ2 = ChunkConfig.PoissonDiskSizeZ - poissonRadius;
            for (int z = poissonRadius; z < endZ2; ++z)
            for (int x = poissonRadius; x < endX2; ++x)
            {
                int heightX = x - poissonRadius;
                int heightZ = z - poissonRadius;
                int localX = heightX - poissonRadius;
                int localZ = heightZ - poissonRadius;
                int worldX = chunkPosition.x + localX;
                int worldZ = chunkPosition.z + localZ;

                int heightIndex = heightX + heightZ * ChunkConfig.HeightmapSizeX;
                float slider = slidermap[heightIndex];

                int vegetationDataIndex = -1;
                VegetationData vegetationData = null;

                int outerIndex = x + z * ChunkConfig.PoissonDiskSizeX;
                uint outerValue = poissonDisk[outerIndex];

                uint probabilityValue = Hash(worldX, worldZ, input.Seed, 10000);

                float treeProbability = BiomeLogic.GetTreeProbability(worldX, worldZ, slider, input.Seed);
                if (probabilityValue > treeProbability * uint.MaxValue)
                {
                    goto placeGrass; // No tree here.
                }

                for (int dz = -ChunkConfig.PoissonDiskRadius; dz <= +ChunkConfig.PoissonDiskRadius; ++dz)
                for (int dx = -ChunkConfig.PoissonDiskRadius; dx <= +ChunkConfig.PoissonDiskRadius; ++dx)
                {
                    if (dx == 0 && dz == 0)
                    {
                        continue; // Center
                    }

                    int innerIndexX = x + dx;
                    int innerIndexZ = z + dz;
                    int innerIndex = innerIndexX + innerIndexZ * ChunkConfig.PoissonDiskSizeX;
                    uint innerValue = poissonDisk[innerIndex];
                    if (innerValue >= outerValue)
                    {
                        goto placeGrass;
                    }
                }

                int treeIndex = (int)(probabilityValue % VegetationSystem.Instance.TreeCount);
                vegetationData = vegetationSystem.GetTree(treeIndex);
                vegetationDataIndex = vegetationSystem.GetCombinedIndex(treeIndex, isTree: true);
                goto placeVegetation;

                placeGrass:
                float grassProbability = BiomeLogic.GetGrassProbability(worldX, worldZ, slider, input.Seed);
                if (probabilityValue > grassProbability * uint.MaxValue)
                {
                    goto placeNothing; // No grass here.
                }

                int grassIndex = (int)(probabilityValue % VegetationSystem.Instance.GrassCount);
                vegetationData = vegetationSystem.GetGrass(grassIndex);
                vegetationDataIndex = vegetationSystem.GetCombinedIndex(grassIndex, isTree: false);

                placeVegetation:
                // Vegetation exists. See if any part crosses border.
                int localY = heightmap[heightIndex] + 1 - chunkPosition.y;

                vegetationData.ForEach((i, x, y, z) =>
                {
                    VoxelType type = vegetationData.GetType(i);
                    if (type.Is(VoxelType.AIR))
                    {
                        return; // Not a voxel.
                    }

                    int newLocalX = localX + x;
                    int newLocalY = localY + y;
                    int newLocalZ = localZ + z;

                    if (newLocalX < 0 || newLocalY < 0 || newLocalZ < 0 || newLocalX >= ChunkConfig.SizeX || newLocalY >= ChunkConfig.SizeY || newLocalZ >= ChunkConfig.SizeZ)
                    {
                        return; // Out of bounds.
                    }

                    // Place the vegetation voxel.
                    int polytypeIndex = newLocalX + newLocalY * ChunkConfig.StrideY + newLocalZ * ChunkConfig.StrideZ;
                    if (types[polytypeIndex].Is(VoxelType.AIR))
                    {
                        types[polytypeIndex] = vegetationData.GetType(i);
                        indexToVegetationDataRef[polytypeIndex] = new VegetationDataRef()
                        {
                            VegetationDataIndex = (byte)vegetationDataIndex,
                            TypeIndex = (ushort)i
                        };
                        isMonotype = false;
                    }
                });

                placeNothing: ;
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
            ChunkLoadTaskPooledIn pooledIn = input.PooledIn;
            VoxelType[] types = pooledIn.PolytypeChunkData.Types;
            byte[] saveData = input.SaveData;

            for (int i = 0; i < saveData.Length; ++i)
            {
                types[i] = (VoxelType)saveData[i];
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

        private static uint Hash(int x, int z, int seed, int salt)
        {
            uint h = (uint)x * 374761393u;
            h += (uint)z * 668265263u;
            h += (uint)seed * 1442695041u;
            h += (uint)salt * 2246822519u;

            h = (h ^ (h >> 13)) * 1274126177u;
            return h ^ (h >> 16);
        }

        private byte[] _saveData;
        private bool _isSaveDataInit;
        private readonly bool _isReload;
    }

    public readonly struct ChunkLoadTaskIn : IDisposable
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

    public struct ChunkLoadTaskOut
    {
        public ChunkLoadTaskIn Input;
        public bool IsMonotype;
        public MonotypeChunkData MonotypeChunkData;
        public bool HasHeight;
        public int MinHeight;
        public int MaxHeight;
    }
}
