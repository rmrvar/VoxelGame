using System;
using UnityEngine;
using VoxelGame.Terrain;
using VoxelGame.Terrain.Meshing;

[RequireComponent(typeof(MeshRenderer))]
public class TestGreedyMeshing : MonoBehaviour
{
    private void CreateSingleVoxel()
    {
        VoxelData.VoxelType[] voxels = 
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
        ChunkManager.Instance.ChunkSize = size;
        GreedyMesherBuffer buffer = GreedyMesherBuffer.Borrow();
        GreedyMesher.Generate(voxels, size, buffer);
        Mesh mesh = GreedyMesher.GetMesh(buffer);
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
    }

    private void CreateDoubleVoxel()
    {
        VoxelData.VoxelType[] voxels =
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
        ChunkManager.Instance.ChunkSize = size;
        GreedyMesherBuffer buffer = GreedyMesherBuffer.Borrow();
        GreedyMesher.Generate(voxels, size, buffer);
        Mesh mesh = GreedyMesher.GetMesh(buffer);
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
    }

    private void CreateTripleVoxel()
    {
        VoxelData.VoxelType[] voxels =
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
        ChunkManager.Instance.ChunkSize = size;
        GreedyMesherBuffer buffer = GreedyMesherBuffer.Borrow();
        GreedyMesher.Generate(voxels, size, buffer);
        Mesh mesh = GreedyMesher.GetMesh(buffer);
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
    }

    private void CreateDoubleTripleVoxel()
    {
        VoxelData.VoxelType[] voxels =
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
        ChunkManager.Instance.ChunkSize = size;
        GreedyMesherBuffer buffer = GreedyMesherBuffer.Borrow();
        GreedyMesher.Generate(voxels, size, buffer);
        Mesh mesh = GreedyMesher.GetMesh(buffer);
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
    }

    private void CreateDoubleTripleVoxelWithExtrusion()
    {
        VoxelData.VoxelType[] voxels =
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
        ChunkManager.Instance.ChunkSize = size;
        GreedyMesherBuffer buffer = GreedyMesherBuffer.Borrow();
        GreedyMesher.Generate(voxels, size, buffer);
        Mesh mesh = GreedyMesher.GetMesh(buffer);
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
    }

    private void CreateDoubleTripleVoxelWithExtrusionAndIsland()
    {
        VoxelData.VoxelType[] voxels =
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
        ChunkManager.Instance.ChunkSize = size;
        GreedyMesherBuffer buffer = GreedyMesherBuffer.Borrow();
        GreedyMesher.Generate(voxels, size, buffer);
        Mesh mesh = GreedyMesher.GetMesh(buffer);
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
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
        ChunkManager.Instance.ChunkSize = size;
        GreedyMesherBuffer buffer = GreedyMesherBuffer.Borrow();
        GreedyMesher.Generate(types, size, buffer);
        Mesh mesh = GreedyMesher.GetMesh(buffer);
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
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
        ChunkManager.Instance.ChunkSize = size;
        GreedyMesherBuffer buffer = GreedyMesherBuffer.Borrow();
        GreedyMesher.Generate(types, size, buffer);
        Mesh mesh = GreedyMesher.GetMesh(buffer);
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
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
        ChunkManager.Instance.ChunkSize = size;
        GreedyMesherBuffer buffer = GreedyMesherBuffer.Borrow();
        GreedyMesher.Generate(types, size, buffer);
        Mesh mesh = GreedyMesher.GetMesh(buffer);
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
    }

    private void Start()
    {
        CreateChunk();
    }
}
