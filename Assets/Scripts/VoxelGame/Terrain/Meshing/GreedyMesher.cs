using UnityEngine;
using static VoxelGame.Terrain.VoxelData;

namespace VoxelGame.Terrain.Meshing
{
	public static class GreedyMesher
    {
        public static void Generate(VoxelType[] voxels, Vector3Int size, GreedyMesherBuffer buffer)
        {
			for (int sliceNormalSign = 0; sliceNormalSign < 2; ++sliceNormalSign)
            for (int sliceNormalAxis = 0; sliceNormalAxis < 3; ++sliceNormalAxis)
            {
				// This gives you essentially what kind of faces you are merging.
				GenerateSlice(sliceNormalAxis, sliceNormalSign, size, voxels, buffer);
            }
        }

        private static void GenerateSlice(
            int normalAxis, 
            int normalSign,
			Vector3Int size,
            VoxelType[] voxels, 
            GreedyMesherBuffer buffer
          )
        {
            int faceIndex = GetFaceIndex(normalAxis, normalSign);

            Vector3Int pSize = new(2 + size.x, 2 + size.y, 2 + size.z);
            Vector3Int sliceSize = ToSliceSpace(size, normalAxis);
            Vector3Int slicePSize = ToSliceSpace(pSize, normalAxis);

            int xStride;
            int yStride;
            int dStride;

            switch (normalAxis)
            {
                case 0: // X axis slice: (Y, Z, X)
                {
                    xStride = pSize.x;
                    yStride = pSize.x * pSize.y;
                    dStride = 1;
                    break;
                }
                case 1: // Y axis slice: (X, Z, Y)
                {
                    xStride = 1;
                    yStride = pSize.x * pSize.y;
                    dStride = pSize.x;
                    break;
                }
                default: // Z axis slice: (X, Y, Z)
                {
                    xStride = 1;
                    yStride = pSize.x;
                    dStride = pSize.x * pSize.y;
                    break;
                }
            }

            VoxelType[] types = buffer.Types;
            int[] topQuadIndices = buffer.TopQuadIndices;
            Quad[] quads = buffer.GreedyQuads;

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
                    if (voxels[i1 - dStride] == VoxelType.AIR)
                    {
						// The back voxel is see-through so this quad can potentially be drawn.
					    types[i2] = voxels[i1];
                    }
                    else
                    {
						types[i2] = VoxelType.AIR;
                    }
                }

                topQuadIndices[^1] = -1; // Only the last element actually needs to be reset.

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
                        if (topQuad.MinX == quad.MinX)
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
					CreateQuad(faceIndex, normalAxis, normalSign, d, quad, buffer);
                }
            }
        }

        private static void CreateQuad(
            int faceIndex, 
            int normalAxis,
            int normalSign,
            int depth,
            Quad quad, 
            GreedyMesherBuffer buffer
          )
        {
            int w = quad.MaxX - quad.MinX;
            int h = quad.MaxY - quad.MinY;

            int uvOffset = ((int)quad.Type - 1) * 3 + TextureFaceOffsets[faceIndex];

            Vector3[] vertices = Vertices[faceIndex];
            Vector3[] uvs = UVs3[faceIndex];

            buffer.Normals.AddRange(Normals[faceIndex]);

            for (int i = 0; i < 4; ++i)
            {
				buffer.Quads.Add(buffer.Vertices.Count);

                Vector3 vertex = vertices[i];
                Vector3Int sliceVertex = ToSliceSpace((int)vertex.x, (int)vertex.y, (int)vertex.z, normalAxis);

                sliceVertex.x = sliceVertex.x > 0 ? quad.MaxX : quad.MinX;
                sliceVertex.y = sliceVertex.y > 0 ? quad.MaxY : quad.MinY;
                sliceVertex.z = depth - normalSign;

                Vector3 newVertex = ToLocalSpace(sliceVertex, normalAxis);
                buffer.Vertices.Add(newVertex);

                Vector3 uv = uvs[i];
                uv.x *= w;
                uv.y *= h;
                uv.z += uvOffset;
                buffer.UVs.Add(uv);
            }
        }

        // TODO: Figure out cleanup/pooling.
        public static Mesh GetMesh(GreedyMesherBuffer buffer)
        {
            Mesh mesh = new();
            mesh.SetVertices(buffer.Vertices);
            mesh.SetNormals(buffer.Normals);
            mesh.SetUVs(0, buffer.UVs);
            mesh.SetIndices(buffer.Quads, MeshTopology.Quads, 0);
            return mesh;
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
    }
}
