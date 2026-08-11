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

    private void Start()
    {
        CreateDoubleTripleVoxelWithExtrusion();
    }
}
