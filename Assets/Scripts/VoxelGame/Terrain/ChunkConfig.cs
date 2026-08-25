using UnityEngine;

namespace VoxelGame.Terrain
{
    public static class ChunkConfig
    {
        public static Vector3Int Size => _size;
        public static int SizeX => _sizeX;
        public static int SizeY => _sizeY;
        public static int SizeZ => _sizeZ;

        public static int Volume => _volume;
        public static int StrideX => _strideX;
        public static int StrideY => _strideY;
        public static int StrideZ => _strideZ;

        public static Vector3Int PSize => _pSize;
        public static int PSizeX => _pSizeX;
        public static int PSizeY => _pSizeY;
        public static int PSizeZ => _pSizeZ;

        public static int PVolume => _pVolume;
        public static int PStrideX => _pStrideX;
        public static int PStrideY => _pStrideY;
        public static int PStrideZ => _pStrideZ;

        public static int PoissonDiskRadius = 5;

        public static void Init(Vector3Int size)
        {
            Debug.Assert(!_isInitialized);
            if (_isInitialized)
            {
                return;
            }

            _size = size;
            _sizeX = size.x;
            _sizeY = size.y;
            _sizeZ = size.z;

            _volume = _sizeX * _sizeY * _sizeZ;
            _strideX = 1;
            _strideY = _sizeX;
            _strideZ = _sizeX * _sizeY;

            _pSizeX = _sizeX + 2;
            _pSizeY = _sizeY + 2;
            _pSizeZ = _sizeZ + 2;
            _pSize = new Vector3Int(_pSizeX, _pSizeY, _pSizeZ);

            _pVolume = _pSizeX * _pSizeY * _pSizeZ;
            _pStrideX = 1;
            _pStrideY = _pSizeX;
            _pStrideZ = _pSizeX * _pSizeY;

            _isInitialized = true;
        }

        private static Vector3Int _size;
        private static int _sizeX;
        private static int _sizeY;
        private static int _sizeZ;

        private static int _volume;
        private static int _strideX;
        private static int _strideY;
        private static int _strideZ;

        private static Vector3Int _pSize;
        private static int _pSizeX;
        private static int _pSizeY;
        private static int _pSizeZ;

        private static int _pVolume;
        private static int _pStrideX;
        private static int _pStrideY;
        private static int _pStrideZ;

        private static bool _isInitialized;
    }
}