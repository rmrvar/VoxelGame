using UnityEngine;
using VoxelGame.Pooling;

namespace VoxelGame.Terrain
{
    public class ChunkPooledMono : MonoBehaviour, IPoolable
    {
        [SerializeField]
        private MeshFilter _triggerMeshFilter;
        [SerializeField]
        private MeshRenderer _triggerMeshRenderer;
        [SerializeField]
        private MeshCollider _triggerMeshCollider;

        [SerializeField]
        private MeshFilter _colliderMeshFilter;
        [SerializeField]
        private MeshRenderer _colliderMeshRenderer;
        [SerializeField]
        private MeshCollider _colliderMeshCollider;

        public Mesh TriggerMesh { get; private set; }
        public Mesh ColliderMesh { get; private set; }

        public bool CanCollide
        {
            get => _colliderMeshCollider.enabled;
            set
            {
                _triggerMeshCollider.enabled = value;
                _colliderMeshCollider.enabled = value;
            }
        }

        public bool IsVisible
        {
            get => _colliderMeshRenderer.enabled;
            set 
            {
                _triggerMeshRenderer.enabled = value;
                _colliderMeshRenderer.enabled = value;
            }
        }

        public void Refresh()
        {
            _triggerMeshCollider.sharedMesh = TriggerMesh;
            _colliderMeshCollider.sharedMesh = ColliderMesh;
        }

        public void OnBorrowed()
        {
            _triggerMeshRenderer.enabled = false;
            _triggerMeshCollider.enabled = false;
            _colliderMeshRenderer.enabled = false;
            _colliderMeshCollider.enabled = false;

            gameObject.SetActive(true);
        }

        public void OnReturned()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            Debug.Assert(_triggerMeshFilter != null);
            Debug.Assert(_triggerMeshRenderer != null);
            Debug.Assert(_triggerMeshCollider != null);
            Debug.Assert(_colliderMeshFilter != null);
            Debug.Assert(_colliderMeshRenderer != null);
            Debug.Assert(_colliderMeshCollider != null);

            TriggerMesh = new Mesh();
            _triggerMeshFilter.sharedMesh = TriggerMesh;
            _triggerMeshCollider.sharedMesh = TriggerMesh;
            ColliderMesh = new Mesh();
            _colliderMeshFilter.sharedMesh = ColliderMesh;
            _colliderMeshCollider.sharedMesh = ColliderMesh;
        }
    }
}