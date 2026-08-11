using System;
using System.Threading;
using UnityEngine;

namespace VoxelGame.Terrain
{
	public class Chunk : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter _meshFilter;

        [SerializeField]
        private MeshCollider _meshCollider;

        public int MeshVersion { get; private set; } = -1;

        public int VoxelVersion { get; private set; } = -1;

        public Vector3Int Position { get; private set; }
        
        public Vector3Int Id { get; private set; }

        public bool IsLoaded => VoxelVersion >= 0;

        public void Init(Vector3Int id, Vector3Int position)
        {
            Id = id;
            Position = position;
        }

        public CancellationToken GetCancellationToken()
        {
            return _cts.Token;
        }

        public bool IsUniform(out VoxelData.VoxelType type)
        {
            if (_voxels == null)
            {
                type = _uniformVoxelType;
                return true;
            }

            type = default;
            return false;
        }

        public VoxelData.VoxelType GetVoxel(int x, int y, int z)
        {
            if (_voxels == null)
            {
                return _uniformVoxelType;
            }
            Vector3Int chunkSize = ChunkManager.Instance.ChunkSize;
            int yStride = chunkSize.x;
            int zStride = chunkSize.x * chunkSize.y;
            return _voxels[x + y * yStride + z * zStride];
        }

        public void SetVoxels(VoxelData.VoxelType[] voxels)
        {
            _voxels = voxels;
            VoxelVersion = 0;
        }

        public void SetUniform(VoxelData.VoxelType uniformVoxelType)
        {
            _voxels = null;
            _uniformVoxelType = uniformVoxelType;
            VoxelVersion = 0;
        }

        public Mesh GetMesh()
        {
            return _meshFilter.sharedMesh;
        }

        public void ApplyMesh(Mesh mesh, int meshVersion)
        {
            _meshCollider.sharedMesh = mesh;
            _meshFilter.sharedMesh = mesh;
            MeshVersion = meshVersion;
        }

        private void Awake()
        {
            Debug.Assert(_meshFilter != null, "Mesh filter is null!");
            Debug.Assert(_meshCollider != null, "Mesh collider is null!");
        }

        private void OnDestroy()
        {
            _cts.Cancel();
        }

        private VoxelData.VoxelType[] _voxels;
        private VoxelData.VoxelType _uniformVoxelType;

        private readonly CancellationTokenSource _cts = new();
    }
}
