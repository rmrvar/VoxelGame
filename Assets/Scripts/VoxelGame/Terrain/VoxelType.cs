namespace VoxelGame.Terrain
{
    public enum VoxelType : byte
    {
        AIR = 0,

        DIRT,
        GRASS,
        STONE,
        OAK_LOG,
        OAK_LEAVES,

        GRASS_SHORT,

        GRASS_POPPY,
        GRASS_DAISY,
        GRASS_DANDELION,
     
        GRASS_BOT,
        GRASS_TOP,

        // IMPORTANT!!! For any new type, must add mapping to MapVoxelTypeToVoxelMaterialType and MapVoxelTypeToVoxelGeometryType!
    }

    public enum VoxelMaterialType : byte
    {
        OPAQUE,
        CUTOUT,
        TRANSPARENT
    }

    public enum VoxelGeometryType : byte
    {
        CUBE,
        CROSS
    }

    public static class VoxelTypeExtensions
    {
        static VoxelTypeExtensions()
        {
            for (int i = 0; i < 256; ++i)
            {
                VoxelType type = (VoxelType)i;
                VoxelType cleanType = type.Clean();
                _materialTypes[i] = MapVoxelTypeToVoxelMaterialType(cleanType);
                _geometryTypes[i] = MapVoxelTypeToVoxelGeometryType(cleanType);
            }
        }

        public static bool Is(this VoxelType type, VoxelType other)
        {
            return ((byte)type & CLEAN_MASK) == (byte)other;
        }

        public static VoxelType Dirty(this VoxelType type)
        {
            return (VoxelType)((byte)type | DIRTY_MASK);
        }

        public static VoxelType Clean(this VoxelType type)
        {
            return (VoxelType)((byte)type & CLEAN_MASK);
        }

        public static bool IsDirty(this VoxelType type)
        {
            return ((byte)type & DIRTY_MASK) != 0;
        }

        public static bool IsClean(this VoxelType type)
        {
            return ((byte)type & DIRTY_MASK) == 0;
        }

        public static VoxelMaterialType GetMaterialType(this VoxelType type)
            => _materialTypes[(byte)type];

        public static VoxelGeometryType GetGeometryType(this VoxelType type)
            => _geometryTypes[(byte)type];

        public static bool IsOpaque(this VoxelType type)
            => type.GetMaterialType() == VoxelMaterialType.OPAQUE;

        public static bool IsCutout(this VoxelType type)
            => type.GetMaterialType() == VoxelMaterialType.CUTOUT;

        public static bool IsTransparent(this VoxelType type)
            => type.GetMaterialType() == VoxelMaterialType.TRANSPARENT;

        public static bool IsSeeThrough(this VoxelType type)
        {
            VoxelMaterialType materialType = type.GetMaterialType();
            return materialType is VoxelMaterialType.TRANSPARENT or VoxelMaterialType.CUTOUT;
        }

        public static bool IsCube(this VoxelType type)
            => type.GetGeometryType() == VoxelGeometryType.CUBE;

        public static bool IsCross(this VoxelType type)
            => type.GetGeometryType() == VoxelGeometryType.CROSS;

        private static VoxelMaterialType MapVoxelTypeToVoxelMaterialType(VoxelType type)
        {
            return type switch
            {
                VoxelType.AIR             => VoxelMaterialType.TRANSPARENT,
                VoxelType.DIRT            => VoxelMaterialType.OPAQUE,
                VoxelType.GRASS           => VoxelMaterialType.OPAQUE,
                VoxelType.STONE           => VoxelMaterialType.OPAQUE,
                VoxelType.OAK_LOG         => VoxelMaterialType.OPAQUE,
                VoxelType.OAK_LEAVES      => VoxelMaterialType.CUTOUT,
                VoxelType.GRASS_SHORT     => VoxelMaterialType.CUTOUT,
                VoxelType.GRASS_BOT       => VoxelMaterialType.CUTOUT,
                VoxelType.GRASS_TOP       => VoxelMaterialType.CUTOUT,
                VoxelType.GRASS_POPPY     => VoxelMaterialType.CUTOUT,
                VoxelType.GRASS_DAISY     => VoxelMaterialType.CUTOUT,
                VoxelType.GRASS_DANDELION => VoxelMaterialType.CUTOUT,

                // Add new mappings above.
                _ => default
            };
        }

        private static VoxelGeometryType MapVoxelTypeToVoxelGeometryType(VoxelType type)
        {
            return type switch
            {
                VoxelType.AIR             => VoxelGeometryType.CUBE,
                VoxelType.DIRT            => VoxelGeometryType.CUBE,
                VoxelType.GRASS           => VoxelGeometryType.CUBE,
                VoxelType.STONE           => VoxelGeometryType.CUBE,
                VoxelType.OAK_LOG         => VoxelGeometryType.CUBE,
                VoxelType.OAK_LEAVES      => VoxelGeometryType.CUBE,
                VoxelType.GRASS_SHORT     => VoxelGeometryType.CROSS,
                VoxelType.GRASS_BOT       => VoxelGeometryType.CROSS,
                VoxelType.GRASS_TOP       => VoxelGeometryType.CROSS,
                VoxelType.GRASS_POPPY     => VoxelGeometryType.CROSS,
                VoxelType.GRASS_DAISY     => VoxelGeometryType.CROSS,
                VoxelType.GRASS_DANDELION => VoxelGeometryType.CROSS,

                // Add new mappings above.
                _ => default
            };
        }

        private const byte DIRTY_MASK = 0B1000_0000;
        private const byte CLEAN_MASK = 0B0111_1111;

        private static readonly VoxelMaterialType[] _materialTypes = new VoxelMaterialType[256];
        private static readonly VoxelGeometryType[] _geometryTypes = new VoxelGeometryType[256];
    }
}