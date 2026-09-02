#if UNITY_EDITOR

using UnityEngine;
using VoxelGame.Terrain.Meshing;

namespace VoxelGame.Terrain.Vegetation
{
    [ExecuteInEditMode]
    public class VegetationDataAuthoringNode : MonoBehaviour
    {
        [field: SerializeField]
        public VoxelType Type { get; private set; }

        [SerializeField]
        private MeshFilter _triggerMeshFilter;
        [SerializeField]
        private MeshFilter _colliderMeshFilter;

        public Vector3Int LocalPosition => new(
            Mathf.FloorToInt(transform.localPosition.x),
            Mathf.FloorToInt(transform.localPosition.y),
            Mathf.FloorToInt(transform.localPosition.z)
          );

        private void OnValidate()
        {
            UpdateGraphics();
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                Debug.LogAssertion(
                    $"{nameof(VegetationDataAuthoringNode)} is meant to be used only in the Editor and not in Play Mode!"
                  );
                return;
            }

            UpdateGraphics();
        }

        // Veeeery hacky...
        private void UpdateGraphics()
        {
            transform.localPosition = LocalPosition;
            if (Type != _oldVoxelType || !_hasVoxelType)
            {
                // Start hacky stuff.
                ChunkConfig.Reset();
                ChunkConfig.Init(Vector3Int.one);

                var types = new VoxelType[]
                {
                    // Y = y - 1
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    // Y = y
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    Type,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    // Y = y + 1
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                    VoxelType.AIR,
                };

                MeshFilter filter = Type.IsCube() 
                    ? _colliderMeshFilter 
                    : _triggerMeshFilter;
                if (filter.sharedMesh == null)
                {
                    filter.sharedMesh = new Mesh();
                }

                if (filter == _colliderMeshFilter)
                {
                    var ws = new GreedyMesherWorkspace();
                    GreedyMesher.Generate(types, ws);
                    ws.FillMesh(filter.sharedMesh);

                    _triggerMeshFilter.sharedMesh = null;
                }
                else
                {   
                    var ws = new GrassMesherWorkspace();
                    GrassMesher.Generate(types, ws);
                    ws.FillMesh(filter.sharedMesh);

                    _colliderMeshFilter.sharedMesh = null;
                }

                // Finish the hacky stuff.
                ChunkConfig.Reset();

                _oldVoxelType = Type;
                _hasVoxelType = true;
            }
        }

        private VoxelType _oldVoxelType;
        private bool _hasVoxelType;
    }
}

#endif
