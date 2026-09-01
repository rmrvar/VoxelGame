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
                var workspace = new MesherWorkspace();
                GreedyMesher.Generate(types, workspace);

                MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
                meshFilter.sharedMesh = new Mesh();

                workspace.GetMesh(meshFilter.sharedMesh);

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
