using System.Collections.Generic;
using UnityEngine;
using VoxelGame.Pooling;

namespace VoxelGame.Terrain.Meshing
{
    public class GreedyMesherBuffer : IPoolable
    {
        public readonly VoxelData.VoxelType[] Types;
        public readonly Quad[] GreedyQuads;
        public readonly int[] TopQuadIndices;

        public readonly List<Vector3> Vertices;
        public readonly List<Vector3> UVs;
        public readonly List<Vector3> Normals;
        public readonly List<int> Quads;

        public GreedyMesherBuffer()
        {
            Vector3Int chunkSize = ChunkConfig.Size;

            int maxDimension = Mathf.Max(chunkSize.x, chunkSize.y, chunkSize.z);

            Types = new VoxelData.VoxelType[maxDimension * maxDimension];
            GreedyQuads = new Quad[maxDimension * maxDimension];
            TopQuadIndices = new int[maxDimension];

            // For initial buffer list sizes, use superflat world.
            int initialListSize = chunkSize.x * chunkSize.z * 4;
            Vertices = new List<Vector3>(initialListSize);
            UVs = new List<Vector3>(initialListSize);
            Normals = new List<Vector3>(initialListSize);
            Quads = new List<int>(initialListSize);
        }

        public void OnBorrowed()
        {
            // Arrays don't need to be cleared (implementation detail of greedy mesher buffers).
            Vertices.Clear();
            UVs.Clear();
            Normals.Clear();
            Quads.Clear();
        }

        public void OnReturned()
        {
        }
    }
}
