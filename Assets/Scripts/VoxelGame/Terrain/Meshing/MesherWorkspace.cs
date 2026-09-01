using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Terrain.Meshing
{
    public sealed class MesherWorkspace
    {
        public readonly VoxelType[] Types;
        public readonly Quad[] GreedyQuads;
        public readonly int[] TopQuadIndices;

        public readonly List<Vector3> Vertices;
        public readonly List<Vector3> UVs;
        public readonly List<Vector3> Normals;
        public readonly List<int> Quads1; // Opaque
        public readonly List<int> Quads2; // Cutout Tree
        public readonly List<int> Quads3; // Cutout Grass

        public MesherWorkspace()
        {
            Vector3Int chunkSize = ChunkConfig.Size;

            int maxDimension = Mathf.Max(chunkSize.x, chunkSize.y, chunkSize.z);

            Types = new VoxelType[maxDimension * maxDimension];
            GreedyQuads = new Quad[maxDimension * maxDimension];
            TopQuadIndices = new int[maxDimension];

            // For initial buffer list sizes, use N times superflat world.
            int N = 5;
            int initialListSize = (chunkSize.x * chunkSize.z * 10) * N;
            Vertices = new List<Vector3>(initialListSize);
            UVs = new List<Vector3>(initialListSize);
            Normals = new List<Vector3>(initialListSize);
            Quads1 = new List<int>(initialListSize);
            Quads2 = new List<int>(initialListSize);
            Quads3 = new List<int>(initialListSize);
        }

        public void Clear()
        {
            // Arrays are overwritten completely by meshing.
            Vertices.Clear();
            UVs.Clear();
            Normals.Clear();
            Quads1.Clear();
            Quads2.Clear();
            Quads3.Clear();
        }

        public Mesh GetMesh(Mesh mesh = null)
        {
            if (mesh == null)
            {
                mesh = new Mesh();
            }
            else
            {
                mesh.Clear();
            }

            mesh.subMeshCount = 3;
            mesh.SetVertices(Vertices);
            mesh.SetNormals(Normals);
            mesh.SetUVs(0, UVs);
            mesh.SetIndices(Quads1, MeshTopology.Quads, 0);
            mesh.SetIndices(Quads2, MeshTopology.Quads, 1);
            mesh.SetIndices(Quads3, MeshTopology.Quads, 2);
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
