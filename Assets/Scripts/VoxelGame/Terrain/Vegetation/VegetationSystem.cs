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

        public int CombinedCount => _combined.Length;
        public int TreeCount => _trees.Length;
        public int GrassCount => _grasses.Length;

        public VegetationData Get(int i) => _combined[i];
        public VegetationData GetGrass(int i) => _grasses[i];
        public VegetationData GetTree(int i) => _trees[i];

        public int GetCombinedIndex(int i, bool isTree)
        {
            if (isTree)
            {
                return i;
            }
            else
            {
                return _trees.Length + i;
            }
        }

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
            _combined = _trees.Concat(_grasses).ToArray();
        }

        private VegetationData[] _trees;
        private VegetationData[] _grasses;

        private VegetationData[] _combined;
    }
}
