using UnityEngine;
using VoxelGame.Pooling;

namespace VoxelGame.Terrain
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class ChunkMono : MonoBehaviour, IPoolable
    {
        public Mesh Mesh { get; private set; }

        public bool CanCollide
        {
            get => _meshCollider.enabled;
            set => _meshCollider.enabled = value;
        }

        public bool IsVisible
        {
            get => _meshRenderer.enabled;
            set => _meshRenderer.enabled = value;
        }

        public void Refresh()
        {
            _meshCollider.sharedMesh = Mesh;
        }

        public void OnBorrowed()
        {
            _meshRenderer.enabled = false;
            _meshCollider.enabled = false;

            gameObject.SetActive(true);
        }

        public void OnReturned()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();

            Mesh = new Mesh();
            _meshFilter.sharedMesh = Mesh;
            _meshCollider.sharedMesh = Mesh;
        }

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
    }
}