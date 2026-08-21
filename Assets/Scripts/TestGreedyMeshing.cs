using System;
using UnityEngine;
using VoxelGame.Pooling;
using VoxelGame.Terrain;
using VoxelGame.Terrain.Meshing;

[RequireComponent(typeof(MeshRenderer))]
public class TestGreedyMeshing : MonoBehaviour
{
    private void CreateSingleVoxel()
    {
        VoxelData.VoxelType[] types = 
        {
            // Z = 0
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 1
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 2
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
        };
        Vector3Int size = new(1, 1, 1);
        DoTest(size, types);
    }

    private void CreateDoubleVoxel()
    {
        VoxelData.VoxelType[] types =
        {
            // Z = 0
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 1
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 2
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.STONE,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 3
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
        };
        Vector3Int size = new(1, 1, 2);
        DoTest(size, types);
    }

    private void CreateTripleVoxel()
    {
        VoxelData.VoxelType[] types =
        {
            // Z = 0
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 1
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 2
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 3
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.STONE,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 4
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
        };
        Vector3Int size = new(1, 1, 3);
        DoTest(size, types);
    }

    private void CreateDoubleTripleVoxel()
    {
        VoxelData.VoxelType[] types =
        {
            // Z = 0
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 1
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 2
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 3
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.STONE,
            VoxelData.VoxelType.STONE,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 4
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
        };
        Vector3Int size = new(2, 1, 3);
        DoTest(size, types);
    }

    private void CreateDoubleTripleVoxelWithExtrusion()
    {
        VoxelData.VoxelType[] types =
        {
            // Z = 0
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 1
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 2
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 3
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.STONE,
            VoxelData.VoxelType.STONE,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 4
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
        };
        Vector3Int size = new(2, 2, 3);
        DoTest(size, types);
    }

    private void CreateDoubleTripleVoxelWithExtrusionAndIsland()
    {
        VoxelData.VoxelType[] types =
        {
            // Z = 0
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 1
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 2
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.GRASS,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 3
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.STONE,
            VoxelData.VoxelType.STONE,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 4
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.DIRT,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            // Z = 5
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
            VoxelData.VoxelType.AIR,
        };
        Vector3Int size = new(3, 2, 4);
        DoTest(size, types);
    }

    private void CreateGiantVoxel()
    {
        int n = 10;
        VoxelData.VoxelType[] types = new VoxelData.VoxelType[(n + 2) * (n + 2) * (n + 2)];
        Array.Fill(types, VoxelData.VoxelType.AIR);
        for (int z = 1; z <= n; ++z)
        for (int y = 1; y <= n; ++y)
        for (int x = 1; x <= n; ++x)
        {
            int i = x + y * (n + 2) + z * (n + 2) * (n + 2);
            types[i] = VoxelData.VoxelType.DIRT;
        }
        Vector3Int size = new(n, n, n);
        DoTest(size, types);
    }

    private void CreateCheckerChunk()
    {
        int n = 3;
        VoxelData.VoxelType[] types = new VoxelData.VoxelType[(n + 2) * (n + 2) * (n + 2)];
        Array.Fill(types, VoxelData.VoxelType.AIR);
        int c = 0;
        for (int z = 1; z <= n; ++z)
        for (int y = 1; y <= n; ++y)
        for (int x = 1; x <= n; ++x)
        {
            int i = x + y * (n + 2) + z * (n + 2) * (n + 2);

            var solidType = z == 1
                ? VoxelData.VoxelType.STONE
                : (z == 2
                    ? VoxelData.VoxelType.DIRT
                    : VoxelData.VoxelType.GRASS);

            types[i] = c++ % 2 == 0
                ? VoxelData.VoxelType.AIR
                : solidType;
        }
        Vector3Int size = new(n, n, n);
        DoTest(size, types);
    }

    private void CreateChunk()
    {
        int n = 32;
        VoxelData.VoxelType[] types = new VoxelData.VoxelType[(n + 2) * (n + 2) * (n + 2)];
        Array.Fill(types, VoxelData.VoxelType.AIR);
        for (int z = 1; z <= n; ++z)
        for (int y = 1; y <= n; ++y)
        for (int x = 1; x <= n; ++x)
        {
            Vector2 pos = new Vector2(x, z) * 0.1F;
            int height = Mathf.FloorToInt(Mathf.PerlinNoise(pos.x + 123.41F, pos.y - 1455.211F) * 16 + 2);
            VoxelData.VoxelType type;
            if (y > height)
            {
                type = VoxelData.VoxelType.AIR;
            } else 
            if (y == height)
            {
                type = VoxelData.VoxelType.GRASS;
            }
            else
            {
                type = VoxelData.VoxelType.DIRT;
            }

            int i = x + y * (n + 2) + z * (n + 2) * (n + 2);
            types[i] = type;
        }
        Vector3Int size = new(n, n, n);
        DoTest(size, types);
    }

    private void Start()
    {
        _greedyMesherBufferPool = new Pool<GreedyMesherBuffer>(
            () => new GreedyMesherBuffer(),
            1
          );
        CreateChunk();
    }

    private void DoTest(Vector3Int size, VoxelData.VoxelType[] types)
    {
        GreedyMesherBuffer buffer = _greedyMesherBufferPool.Borrow();
        try
        {
            ChunkConfig.Init(size);
            GreedyMesher.Generate(types, buffer);
            Mesh mesh = GreedyMesher.GetMesh(buffer);
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter != null)
            {
                filter.sharedMesh = mesh;
            }
        }
        finally
        {
            if (buffer != null)
            {
                _greedyMesherBufferPool.Return(buffer);
            }
        }
    }

    private Pool<GreedyMesherBuffer> _greedyMesherBufferPool;
}
