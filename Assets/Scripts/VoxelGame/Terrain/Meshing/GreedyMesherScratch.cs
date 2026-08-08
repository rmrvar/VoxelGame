using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Terrain.Meshing
{
    public class GreedyMesherScratch
    {
        public readonly List<Vector3Int> SortedPositions = new(EstimatedNumVoxelsPerChunk);

        public GreedyMesherScratch(int numVoxels = -1)
        {
            if (numVoxels > 0)
            {
                EnsureCapacity(numVoxels);
            }
        }

        public void Clear()
        {
            SortedPositions.Clear();
        }

        public void EnsureCapacity(int voxelCount)
        {
            SortedPositions.Capacity = Mathf.Max(SortedPositions.Capacity, voxelCount);
        }

        private const int EstimatedNumVoxelsPerChunk = 32 * 32 * 5; // Chunk W * Chunk H * Random D
    }
}
