using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Terrain.Vegetation
{
    [Serializable]
    public class VegetationData
    {
        [SerializeField, HideInInspector]
        private int _radius;
        [SerializeField, HideInInspector]
        private int _height;
        [SerializeField, HideInInspector]
        private VoxelType[] _types;

        public int Radius => _radius;
        public int Height => _height;
        public IReadOnlyList<VoxelType> Types => _types;

        public VegetationData(int radius, int height, VoxelType[] types)
        {
            _radius = radius;
            _height = height;
            _types = types;
        }

        public void GetPosition(int i, ref Vector3Int position)
        {
            // TODO
        }

        public Vector3Int GetPosition(int i)
        {
            // TODO
            return default;
        }

        public VoxelType GetType(int i)
        {
            return _types[i];
        }

        public void ForEach(Action<int, int, int, int> action)
        {
            ForEach(_radius, _height, action);
        }

        public static void ForEach(
            int radius,
            int height,
            Action<int, int, int, int> action
          )
        {
            int r = radius - 1;

            int i = 0;
            for (int z = -r; z <= +r; ++z)
            for (int y = 0; y < height; ++y)
            for (int x = -r; x <= +r; ++x)
            {
                action(i, x, y, z);
                ++i;
            }
        }
    }
}
