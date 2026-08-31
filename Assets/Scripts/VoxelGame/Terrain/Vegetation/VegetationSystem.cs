using System;
using System.Linq;
using UnityEngine;

namespace VoxelGame.Terrain.Vegetation
{
    [Serializable]
    public class VegetationSystem
    {
        [SerializeField]
        private VegetationDataSO[] _treeSOs;
        [SerializeField]
        private VegetationDataSO[] _grassSOs;

        public static VegetationSystem Instance { get; private set; }

        public int TreeCount => _treeSOs.Length;

        public VegetationData GetGrass(int i) => _grasses[i];

        public int GrassCount => _grassSOs.Length;

        public VegetationData GetTree(int i) => _trees[i];

        public void Init()
        {
            Debug.Assert(Instance == null);
            if (Instance != null)
            {
                return;
            }

            Instance = this;
            _trees = _treeSOs.Where(so => so != null).Select(so => so.Data).ToArray();
            _grasses = _grassSOs.Where(so => so != null).Select(so => so.Data).ToArray();
        }

        private VegetationData[] _trees;
        private VegetationData[] _grasses;
    }
}
