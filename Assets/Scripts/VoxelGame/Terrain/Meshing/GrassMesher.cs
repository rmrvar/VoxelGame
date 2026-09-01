using UnityEngine;
using static VoxelGame.Terrain.VoxelData;

namespace VoxelGame.Terrain.Meshing
{
    public static class GrassMesher
    {
        public static void Generate(VoxelType[] voxels, MesherWorkspace workspace)
        {
            for (int z = 1; z < ChunkConfig.PSizeZ - 1; ++z)
            for (int y = 1; y < ChunkConfig.PSizeY - 1; ++y)
            for (int x = 1; x < ChunkConfig.PSizeX - 1; ++x)
            {
                int i = x + y * ChunkConfig.PStrideY + z * ChunkConfig.PStrideZ;

                VoxelType type = voxels[i];
                if (!type.IsCross())
                {
                    continue;
                }

                CreateCross(x, y, z, type, workspace);
            }
        }

        private static void CreateCross(int x, int y, int z, VoxelType type, MesherWorkspace workspace)
        {
            int uvOffset = ((int)type - 1) * 3;

            Vector3[] vertices = CrossQuadVertices;
            Vector3[] uvs = CrossQuadUVs3;

            Vector3 pos = new(x, y, z);

            for (int i = 0; i < 8; ++i)
            {
                workspace.Quads3.Add(workspace.Vertices.Count);

                Vector3 v = vertices[i] + pos;
                Vector3 uv = uvs[i];
                uv.z = uvOffset;

                workspace.Vertices.Add(v);
                workspace.UVs.Add(uv);
            }
            workspace.Normals.AddRange(CrossQuadNormals);
        }
    }
}
