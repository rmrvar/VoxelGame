using UnityEngine;
using static VoxelGame.Terrain.VoxelData;

namespace VoxelGame.Terrain.Meshing
{
    public static class GrassMesher
    {
        public static void Generate(VoxelType[] voxels, GrassMesherWorkspace workspace)
        {
            for (int z = 0; z < ChunkConfig.SizeZ; ++z)
            for (int y = 0; y < ChunkConfig.SizeY; ++y)
            for (int x = 0; x < ChunkConfig.SizeX; ++x)
            {
                int i =
                    (x + 1) +
                    (y + 1) * ChunkConfig.PStrideY +
                    (z + 1) * ChunkConfig.PStrideZ;

                VoxelType type = voxels[i];
                if (!type.IsCross())
                {
                    continue;
                }

                CreateCross(x, y, z, type, workspace);
            }
        }

        private static void CreateCross(int x, int y, int z, VoxelType type, GrassMesherWorkspace workspace)
        {
            int uvOffset = ((int)type - 1) * 3;

            Vector3[] vertices = CrossQuadVertices;
            Vector4[] uvs = CrossQuadUVs4;

            Vector3 pos = new(x, y, z);

            for (int i = 0; i < 8; ++i)
            {
                workspace.Quads.Add(workspace.Vertices.Count);

                Vector3 v = vertices[i] + pos;
                Vector4 uv = uvs[i];
                uv.z = uvOffset;

                VoxelTintType tintType = type.GetTintType();
                uv.w = (int)tintType;

                workspace.Vertices.Add(v);
                workspace.UV4s.Add(uv);
            }
            workspace.Normals.AddRange(CrossQuadNormals);
        }
    }
}
