using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Terrain.Meshing
{
    public sealed class GreedyMesherWorkspace
    {
        public readonly VoxelType[] Types;
        public readonly Quad[] GreedyQuads;
        public readonly int[] TopQuadIndices;

        public readonly List<Vector3> Vertices;
        public readonly List<Vector3> UV3s;
        public readonly List<Vector3> Normals;
        public readonly List<int> OpaqueQuads;
        public readonly List<int> CutoutQuads;

        public GreedyMesherWorkspace()
        {
            Vector3Int chunkSize = ChunkConfig.Size;

            int maxDimension = Mathf.Max(chunkSize.x, chunkSize.y, chunkSize.z);

            Types = new VoxelType[maxDimension * maxDimension];
            GreedyQuads = new Quad[maxDimension * maxDimension];
            TopQuadIndices = new int[maxDimension];

            // This is a lot of data but compared to per chunk data it is not that much. The
            // greedy mesher workspace is limited to the number of workers.
            const int QUADS_PER_VOXEL = 6;
            int initialListSize = ChunkConfig.Volume * QUADS_PER_VOXEL;
            Vertices = new List<Vector3>(initialListSize);
            UV3s = new List<Vector3>(initialListSize);
            Normals = new List<Vector3>(initialListSize);
            OpaqueQuads = new List<int>(initialListSize);
            CutoutQuads = new List<int>(initialListSize);
        }

        public void Clear()
        {
            // Arrays are overwritten completely by meshing.
            Vertices.Clear();
            UV3s.Clear();
            Normals.Clear();
            OpaqueQuads.Clear();
            CutoutQuads.Clear();
        }

        public void FillMesh(Mesh mesh)
        {
            mesh.Clear();
            mesh.subMeshCount = 2;
            mesh.SetVertices(Vertices);
            mesh.SetNormals(Normals);
            mesh.SetUVs(0, UV3s);
            mesh.SetIndices(OpaqueQuads, MeshTopology.Quads, 0);
            mesh.SetIndices(CutoutQuads, MeshTopology.Quads, 1);
            mesh.RecalculateBounds();
        }
    }
}
