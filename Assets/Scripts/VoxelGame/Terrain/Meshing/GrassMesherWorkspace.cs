using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Terrain.Meshing
{
    public sealed class GrassMesherWorkspace
    {
        public readonly List<Vector3> Vertices;
        public readonly List<Vector4> UV4s;
        public readonly List<Vector3> Normals;
        public readonly List<int> Quads;

        public GrassMesherWorkspace()
        {
            Vector3Int chunkSize = ChunkConfig.Size;
            const int QUADS_PER_COL = 4;
            const int SIZE_PER_QUAD = 4;
            int initialListSize = chunkSize.x * chunkSize.z * QUADS_PER_COL * SIZE_PER_QUAD;
            Vertices = new List<Vector3>(initialListSize);
            UV4s = new List<Vector4>(initialListSize);
            Normals = new List<Vector3>(initialListSize);
            Quads = new List<int>(initialListSize);
        }

        public void Clear()
        {
            Vertices.Clear();
            UV4s.Clear();
            Normals.Clear();
            Quads.Clear();
        }

        public void FillMesh(Mesh mesh)
        {
            mesh.Clear();
            mesh.subMeshCount = 1;
            mesh.SetVertices(Vertices);
            mesh.SetNormals(Normals);
            mesh.SetUVs(0, UV4s);
            mesh.SetIndices(Quads, MeshTopology.Quads, 0);
            mesh.RecalculateBounds();
        }
    }
}
