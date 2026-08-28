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
    public class VegetationRoot : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int _radius;
        [SerializeField]
        private int _height;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _radius = new Vector2Int(Mathf.Max(0, _radius.x), Mathf.Max(0, _radius.y));
            _height = Mathf.Max(1, _height);

            transform.position = GetClampedPosition();
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                Debug.LogAssertion(
                    $"{nameof(VegetationRoot)} is meant to be used only in the Editor and not in Play Mode!"
                  );
                return;
            }

            transform.position = GetClampedPosition();
        }

        private IEnumerable<Vector3Int> GetPoints()
        {
            Vector3Int currPos = GetClampedPosition();
            for (int y = 0; y < _height; ++y)
            for (int z = -_radius.y; z <= +_radius.y; ++z)
            for (int x = -_radius.x; x <= +_radius.x; ++x)
            {
                Vector3Int pos = currPos + new Vector3Int(x, y, z);
                yield return pos;
            }
        }

        private void OnDrawGizmos()
        {
            Vector3Int currPos = GetClampedPosition();
            Gizmos.color = Color.blue;
            foreach (Vector3Int point in GetPoints())
            {
                Vector3 color = point + new Vector3Int(_radius.x, 0, _radius.y) - currPos;
                color = new Vector3(color.x / (2 * _radius.x), color.y / _height, color.z / (2 * _radius.y));

                Gizmos.color = new Color(color.x, color.y, color.z, 0.7F);
                Gizmos.DrawSphere(point + new Vector3(0.5F, 0.5F, 0.5F), 0.05F);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                transform.position + new Vector3(0.5F, _height * 0.5F, 0.5F),
                new Vector3(_radius.x * 2 + 1, _height, _radius.y * 2 + 1)
              );
        }

        private Vector3Int GetClampedPosition()
        {
            return new(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y),
                Mathf.FloorToInt(transform.position.z)
              );
        }
#endif
    }
}
