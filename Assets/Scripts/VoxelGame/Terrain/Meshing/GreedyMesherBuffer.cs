using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Vector3 = UnityEngine.Vector3;

namespace VoxelGame.Terrain.Meshing
{
    public class GreedyMesherBuffer
    {
        public readonly VoxelData.VoxelType[] Types;
        public readonly Quad[] GreedyQuads;
        public readonly int[] TopQuadIndices;

        public readonly List<Vector3> Vertices;
        public readonly List<Vector3> UVs;
        public readonly List<Vector3> Normals;
        public readonly List<int> Quads;

        public static GreedyMesherBuffer Borrow()
        {
            return _pool.Get();
        }

        public static void Return(GreedyMesherBuffer buffer)
        {
            _pool.Release(buffer);
        }

        private GreedyMesherBuffer()
        {
            Vector3Int chunkSize = ChunkManager.Instance.ChunkSize;

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

        private void Clear()
        {
            // Arrays don't need to be cleared (implementation detail of greedy mesher buffers).
            Vertices.Clear();
            UVs.Clear();
            Normals.Clear();
            Quads.Clear();
        }

        private static readonly ObjectPool<GreedyMesherBuffer> _pool =
            new(
                () => new GreedyMesherBuffer(),
                buffer => buffer.Clear()
              );
    }
}
