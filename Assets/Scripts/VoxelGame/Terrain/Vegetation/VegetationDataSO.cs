using UnityEngine;

namespace VoxelGame.Terrain.Vegetation
{
    public class VegetationDataSO : ScriptableObject
    {
        [field: SerializeField]
        public VegetationData Data { get; private set; }

#if UNITY_EDITOR
        public void SetData(VegetationData data)
        {
            Data = data;
        }
#endif
    }
}
