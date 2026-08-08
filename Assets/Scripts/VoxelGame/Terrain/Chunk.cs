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

        public int VoxelVersion { get; private set; }

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

        public void SetVoxels(VoxelData.VoxelType[] voxels)
        {
            _voxels = voxels;
        }

        public void SetUniform(VoxelData.VoxelType uniformVoxelType)
        {
            _voxels = null;
            _uniformVoxelType = uniformVoxelType;
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
