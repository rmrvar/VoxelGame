using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using VoxelGame.Terrain;
using VoxelGame.Terrain.Meshing;

namespace VoxelGame.Vegetation
{
    [ExecuteInEditMode()]
    public class VegetationVoxel : MonoBehaviour
    {
        [SerializeField]
        private VoxelType _voxelType;

#if UNITY_EDITOR
        private void OnValidate()
        {
            transform.localPosition = GetClampedLocalPosition();
            UpdateGraphics();
        }

        // Veeeery hacky...
        private void UpdateGraphics()
        {
            if (_voxelType != _oldVoxelType || !_hasVoxelType)
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
                    _voxelType,
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
                var workspace = new GreedyMesherWorkspace();
                GreedyMesher.Generate(types, workspace);

                MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
                meshFilter.sharedMesh = new Mesh();

                GreedyMesher.GetMesh(workspace, meshFilter.sharedMesh);

                // Finish the hacky stuff.
                ChunkConfig.Reset();

                _oldVoxelType = _voxelType;
                _hasVoxelType = true;
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                Debug.LogAssertion(
                    $"{nameof(VegetationVoxel)} is meant to be used only in the Editor and not in Play Mode!"
                  );
                return;
            }

            transform.localPosition = GetClampedLocalPosition();
            UpdateGraphics();
        }
        private Vector3Int GetClampedLocalPosition()
        {
            return new(
                Mathf.FloorToInt(transform.localPosition.x),
                Mathf.FloorToInt(transform.localPosition.y),
                Mathf.FloorToInt(transform.localPosition.z)
              );
        }

        private VoxelType _oldVoxelType;
        private bool _hasVoxelType;
#endif
    }
}
