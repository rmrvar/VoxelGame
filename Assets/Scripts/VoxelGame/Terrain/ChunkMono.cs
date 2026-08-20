using UnityEngine;

namespace VoxelGame.Terrain
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class ChunkMono : MonoBehaviour
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
            _meshCollider.sharedMesh = null;
            _meshCollider.sharedMesh = Mesh;
        }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();

            Mesh = new Mesh();
            _meshFilter.sharedMesh = Mesh;
            _meshCollider.sharedMesh = Mesh;
            // TODO: Make this class poolable and have everything after this happen in OnBorrowed.
            // Mesh.Clear(); // Uncomment when pooling added.
            // Refresh(); // Maybe uncomment when pooling added???
            _meshRenderer.enabled = false;
            _meshCollider.enabled = false;
        }

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
    }
}