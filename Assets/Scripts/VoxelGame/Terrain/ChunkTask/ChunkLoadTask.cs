using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace VoxelGame.Terrain.ChunkTask
{
    public class ChunkLoadTask
        : ChunkTask<ChunkLoadTaskIn, ChunkLoadTaskOut>
    {
        public ChunkLoadTask(
            Chunk chunk, 
            ChunkTaskScheduler scheduler, 
            int priority, 
            CancellationToken token
          ) 
            : base(chunk, scheduler, priority, token)
        {
        }

        protected override ChunkLoadTaskIn PrepareInput()
        {
            return new ChunkLoadTaskIn(); // TODO
        }

        protected override ChunkLoadTaskOut Execute(ChunkLoadTaskIn input, CancellationToken cancellationToken)
        {
            _in = input;
            _out = new ChunkLoadTaskOut();

            if (ShouldLoad())
            {
                LoadVoxels(); // TODO: Consider moving into its own task (ShouldLoad is known when scheduling already).
            }
            else
            {
                GenerateBiomesmap();
                Token.ThrowIfCancellationRequested();
                GenerateHeightmap();
                Token.ThrowIfCancellationRequested();
                GenerateVoxels();
            }

            return _out;
        }

        protected override void HandleOutput(ChunkLoadTaskOut output)
        {
            Chunk.Status = Chunk.LoadStatus.FINISHED_LOADING;
            Chunk.Voxels = output.Voxels;
        }

        private bool ShouldLoad()
        {
            return false; // TODO
        }

        private void LoadVoxels()
        {
            // TODO
        }

        private void GenerateBiomesmap()
        {
        }

        private void GenerateHeightmap()
        {
            for (int z = 0; z < _in.ChunkSize.y; ++z)
            for (int x = 0; x < _in.ChunkSize.x; ++x)
            {
                Token.ThrowIfCancellationRequested();
                Vector3 position = _in.Position + new Vector3(x, 0, z);
                _in.Heightmap[x, z] = BiomeLogic.GetHeight(position);
            }
        }

        private void GenerateVoxels()
        {
            var voxels = new Dictionary<Vector3Int, Voxel>();
            
            for (int z = 0; z < _in.ChunkSize.y; ++z)
            for (int x = 0; x < _in.ChunkSize.x; ++x)
            {
                Token.ThrowIfCancellationRequested();

                var biome = 0; // TODO: Set the biome type.
                var height = _in.Heightmap[x + 1, z + 1];

                var neighboringHeights = GetNeighboringHeights(x, z);
                var minHeight = height - 1; // One less than the coordinate of the lowest ground block at this x and z coord..
                var maxHeight = height + 1; // Y coordinate of the highest air block with this x and z coord..
                foreach (var h in neighboringHeights)
                {
                    minHeight = Math.Min(minHeight, h);
                    maxHeight = Math.Max(maxHeight, h);
                }

                if (height >= _maxHeight)
                {
                    _maxHeight = height;
                }
                if (height < _minHeight)
                {
                    _minHeight = height;
                }

                // Create the Air Blocks.
                for (var y = maxHeight; y > height; --y)
                {
                    var pos = new Vector3Int(x, y, z);
                    var voxel = new Voxel(pos, VoxelData.VoxelType.AIR, biome);

                    // Using the assumption that the map is always a heightmap, we can simplify the mesh
                    // generation process by a lot. We know that each air block has a connection in any
                    // direction if that neighboring height is equal to its height. And only the bottom
                    // air block has a connection downwards.
                    foreach (var h in neighboringHeights)
                    {
                        if (y == h)
                        {
                            ++voxel.NumOfExposedFaces;
                        }
                    }
                    if (y == height + 1)
                    {
                        ++voxel.NumOfExposedFaces;
                    }

                    voxels.Add(pos, voxel);
                }

                // Create the ground blocks.
                for (var y = height; y > minHeight; --y)
                {
                    var pos = new Vector3Int(x, y, z);
                    var worldPos = _in.Position + pos;
                    var voxelType = BiomeLogic.GetVoxelType(worldPos, _in.Heightmap[pos.x, pos.z]);
                    var voxel = new Voxel(pos, voxelType, biome);

                    voxels.Add(pos, voxel);
                }
            }

            _out.Voxels = voxels;
        }

        public IEnumerable<int> GetNeighboringHeights(int x, int z)
        {
            int[,] heightmap = _in.Heightmap;
            if (x < heightmap.GetLength(0))
            {
                yield return heightmap[x + 1, z];
            }
            if (z < heightmap.GetLength(1))
            {
                yield return heightmap[x, z + 1];
            }
            if (x > 0)
            {
                yield return heightmap[x - 1, z];
            }
            if (z > 0)
            {
                yield return heightmap[x, z - 1];
            }
        }

        private ChunkLoadTaskIn _in;
        private ChunkLoadTaskOut _out;
        private int _minHeight;
        private int _maxHeight;
    }

    public class ChunkLoadTaskIn : IDisposable
    {
        public Vector2Int ChunkSize;
        public Vector3Int Position;

        public readonly int[,] Heightmap;

        public ChunkLoadTaskIn()
        {
            Heightmap = new int[ChunkSize.x, ChunkSize.y]; // TODO: Get from pool.
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }

    public class ChunkLoadTaskOut
    {
        public Dictionary<Vector3Int, Voxel> Voxels;
    }
}
