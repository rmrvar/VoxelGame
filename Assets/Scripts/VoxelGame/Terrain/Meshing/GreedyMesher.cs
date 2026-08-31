using System;
using System.Collections.Generic;
using UnityEngine;
using static VoxelGame.Terrain.VoxelData;

namespace VoxelGame.Terrain.Meshing
{
	public static class GreedyMesher
    {
        public static void Generate(VoxelType[] voxels, GreedyMesherWorkspace workspace)
        {
			for (int sliceNormalSign = 0; sliceNormalSign < 2; ++sliceNormalSign)
            for (int sliceNormalAxis = 0; sliceNormalAxis < 3; ++sliceNormalAxis)
            {
				// This gives you essentially what kind of faces you are merging.
				GenerateSlice(sliceNormalAxis, sliceNormalSign, voxels, workspace);
            }
        }

        private static void GenerateSlice(
            int normalAxis, 
            int normalSign,
            VoxelType[] voxels, 
            GreedyMesherWorkspace workspace
          )
        {
            int faceIndex = GetFaceIndex(normalAxis, normalSign);

            Vector3Int sliceSize = ToSliceSpace(ChunkConfig.Size, normalAxis);
            Vector3Int slicePSize = ToSliceSpace(ChunkConfig.PSize, normalAxis);

            int xStride;
            int yStride;
            int dStride;

            switch (normalAxis)
            {
                case 0: // X axis slice: (Y, Z, X)
                {
                    xStride = ChunkConfig.PStrideY;
                    yStride = ChunkConfig.PStrideZ;
                    dStride = ChunkConfig.PStrideX;
                    break;
                }
                case 1: // Y axis slice: (X, Z, Y)
                {
                    xStride = ChunkConfig.PStrideX;
                    yStride = ChunkConfig.PStrideZ;
                    dStride = ChunkConfig.PStrideY;
                    break;
                }
                default: // Z axis slice: (X, Y, Z)
                {
                    Debug.Assert(normalAxis == 2);
                    xStride = ChunkConfig.PStrideX;
                    yStride = ChunkConfig.PStrideY;
                    dStride = ChunkConfig.PStrideZ;
                    break;
                }
            }

            VoxelType[] types = workspace.Types;
            int[] topQuadIndices = workspace.TopQuadIndices;
            Quad[] quads = workspace.GreedyQuads;

			for (int d = 1; d < slicePSize.z - 1; ++d)
			{
				// Write types
				for (int y = 1; y < slicePSize.y - 1; ++y)
                for (int x = 1; x < slicePSize.x - 1; ++x)
                {
                    int i1 = x * xStride + y * yStride + d * dStride;

                    int xm1 = x - 1;
                    int ym1 = y - 1;

                    int i2 = xm1 + ym1 * sliceSize.x;
                    VoxelType backType = voxels[i1 + dStride * (normalSign == 0 ? +1 : -1)];
                    if (backType.IsTransparent() || backType.IsCutout())
                    {
						// The back voxel is see-through so this quad can potentially be drawn.
					    types[i2] = voxels[i1];
                    }
                    else
                    {
						types[i2] = VoxelType.AIR;
                    }
                }

                Array.Fill(topQuadIndices, -1);

                // Grow quads
                int numQuads = 0;
                VoxelType lftType = VoxelType.AIR;
                for (int y = 0; y < sliceSize.y; ++y)
                for (int x = 0; x < sliceSize.x; ++x)
                {
                    int i = x + y * sliceSize.x;

                    VoxelType type = types[i];

                    if (type == VoxelType.AIR)
                    {
                        // Air voxels do not have quads.
                        topQuadIndices[x] = -1;
                        continue;
                    }

                    ref Quad quad = ref quads[numQuads];
                    
                    if (lftType != type)
                    {
						// New quad type.
                        quad.Type = type;
                        quad.MinX = (byte)x;
                        quad.MaxX = (byte)(x + 1);
                        quad.MinY = (byte)y;
                        quad.MaxY = (byte)(y + 1);
                        lftType = type;
                    }
                    else
                    {
						// Old quad type.
                        ++quad.MaxX;
                    }

                    if (x < sliceSize.x - 1 && types[i + 1] == type)
                    {
						// Quad keeps going.
                        topQuadIndices[x] = -1;
                        continue;
                    }

                    int topQuadIndex = topQuadIndices[x];
                    if (topQuadIndex < 0)
                    {
						// Has no top quad. This quad stays.
                        topQuadIndices[x] = numQuads;
                        ++numQuads;
                    }
                    else
                    {
						// Has a top quad.
                        ref Quad topQuad = ref quads[topQuadIndex];
                        if (topQuad.MinX == quad.MinX && topQuad.Type == quad.Type)
                        {
							// This quad gets taken over.
                            ++topQuad.MaxY;
                        }
                        else
                        {
							// This quad stays.
                            topQuadIndices[x] = numQuads;
                            ++numQuads;
                        }
                    }
                    lftType = VoxelType.AIR;
                }

                for (int i = 0; i < numQuads; ++i)
                {
                    ref Quad quad = ref quads[i];
					CreateQuad(faceIndex, normalAxis, normalSign, d, quad, workspace);
                }
            }
        }

        private static void CreateQuad(
            int faceIndex, 
            int normalAxis,
            int normalSign,
            int depth,
            Quad quad, 
            GreedyMesherWorkspace workspace
          )
        {
            int w = quad.MaxX - quad.MinX;
            int h = quad.MaxY - quad.MinY;

            int uvOffset = ((int)quad.Type - 1) * 3 + TextureFaceOffsets[faceIndex];

            Vector3[] vertices = Vertices[faceIndex];
            Vector3[] uvs = UVs3[faceIndex];

            workspace.Normals.AddRange(Normals[faceIndex]);

            List<int> quads = quad.Type.IsOpaque()
                ? workspace.Quads1
                : workspace.Quads2;

            for (int i = 0; i < 4; ++i)
            {
                quads.Add(workspace.Vertices.Count);

                Vector3 vertex = vertices[i];
                Vector3 sliceVertex = ToSliceSpace(vertex, normalAxis);

                sliceVertex.x = sliceVertex.x > 0 ? quad.MaxX : quad.MinX;
                sliceVertex.y = sliceVertex.y > 0 ? quad.MaxY : quad.MinY;
                sliceVertex.z = depth - normalSign;

                Vector3 newVertex = ToLocalSpace(sliceVertex, normalAxis);
                workspace.Vertices.Add(newVertex);

                Vector3 uv = uvs[i];
                if (faceIndex is 0 or 3)
                {
                    // Exception for +/-X for some reason I forgot about.
                    uv.x *= h;
                    uv.y *= w;
                }
                else
                {
                    uv.x *= w;
                    uv.y *= h;
                }
                uv.z += uvOffset;
                workspace.UVs.Add(uv);
            }
        }

        public static Mesh GetMesh(GreedyMesherWorkspace workspace, Mesh mesh = null)
        {
            if (mesh == null)
            {
                mesh = new Mesh();
            }
            else
            {
                mesh.Clear();
            }

            mesh.subMeshCount = 2;
            mesh.SetVertices(workspace.Vertices);
            mesh.SetNormals(workspace.Normals);
            mesh.SetUVs(0, workspace.UVs);
            mesh.SetIndices(workspace.Quads1, MeshTopology.Quads, 0);
            mesh.SetIndices(workspace.Quads2, MeshTopology.Quads, 1);
            mesh.RecalculateBounds();
            return mesh;
        }

        // Transforms from local space to slice space.
        public static Vector3 ToSliceSpace(Vector3 position, int axis)
        {
            return ToSliceSpace(position.x, position.y, position.z, axis);
        }

        // Transforms from local space to slice space.
        public static Vector3Int ToSliceSpace(Vector3Int position, int axis)
        {
            return ToSliceSpace(position.x, position.y, position.z, axis);
        }

        // Transforms from local space to slice space.
        public static Vector3Int ToSliceSpace(int x, int y, int z, int axis)
        {
            return axis switch
            {
                // X axis slice: (Y, Z, X)
                0 => new Vector3Int(y, z, x),
                // Y axis slice: (X, Z, Y)
                1 => new Vector3Int(x, z, y),
                // Z axis slice: (X, Y, Z)
                _ => new Vector3Int(x, y, z)
            };
        }

        // Transforms from local space to slice space.
        public static Vector3 ToSliceSpace(float x, float y, float z, int axis)
        {
            return axis switch
            {
                // X axis slice: (Y, Z, X)
                0 => new Vector3(y, z, x),
                // Y axis slice: (X, Z, Y)
                1 => new Vector3(x, z, y),
                // Z axis slice: (X, Y, Z)
                _ => new Vector3(x, y, z)
            };
        }

        // Transforms from slice space to local space.
        public static Vector3 ToLocalSpace(Vector3 position, int axis)
        {
            return ToLocalSpace(position.x, position.y, position.z, axis);
        }

        // Transforms from slice space to local space.
        public static Vector3Int ToLocalSpace(Vector3Int position, int axis)
        {
            return ToLocalSpace(position.x, position.y, position.z, axis);
        }

        // Transforms from slice space to local space.
        public static Vector3Int ToLocalSpace(int x, int y, int z, int axis)
        {
            return axis switch
            {
                // X axis slice: (Z, X, Y)
                0 => new Vector3Int(z, x, y),
                // Y axis slice: (X, Z, Y)
                1 => new Vector3Int(x, z, y),
                // Z axis slice: (X, Y, Z)
                _ => new Vector3Int(x, y, z)
            };
        }

        // Transforms from slice space to local space.
        public static Vector3 ToLocalSpace(float x, float y, float z, int axis)
        {
            return axis switch
            {
                // X axis slice: (Z, X, Y)
                0 => new Vector3(z, x, y),
                // Y axis slice: (X, Z, Y)
                1 => new Vector3(x, z, y),
                // Z axis slice: (X, Y, Z)
                _ => new Vector3(x, y, z)
            };
        }
    }
}
