#if UNITY_EDITOR

using UnityEngine;

namespace VoxelGame.Terrain.Vegetation
{
    public class VegetationDataAuthoringRoot : MonoBehaviour
    {
        [field: SerializeField]
        public int Radius { get; private set; }
        [field: SerializeField]
        public int Height { get; private set; }

        private void OnValidate()
        {
            Radius = Mathf.Max(1, Radius);
            Height = Mathf.Max(1, Height);
        }

        private void OnDrawGizmos()
        {
            VegetationData.ForEach(Radius, Height, DrawGizmoAt);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                transform.position + new Vector3(0.5F, Height * 0.5F, 0.5F),
                new Vector3(2 * Radius - 1, Height, 2 * Radius - 1)
              );
        }

        private void DrawGizmoAt(int _, int x, int y, int z)
        {
            int r = Radius - 1;

            Color color = new Color(
                (float)(x + r) / (2 * r), 
                (float)y / Height, 
                (float)(x + r) / (2 * r), 
                0.7F
              );
            Vector3 point = transform.TransformPoint(new Vector3(x + 0.5F, y + 0.5F, z + 0.5F));

            Gizmos.color = color;
            Gizmos.DrawSphere(point, 0.05F);
        }
    }
}

#endif
