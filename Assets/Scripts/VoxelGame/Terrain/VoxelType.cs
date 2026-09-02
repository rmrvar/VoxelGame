using System;

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

        GRASS_BOT,
        GRASS_TOP
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
        public static VoxelMaterialType GetMaterialType(this VoxelType type)
        {
            return type switch
            {
                VoxelType.AIR         => VoxelMaterialType.TRANSPARENT,
                VoxelType.DIRT        => VoxelMaterialType.OPAQUE,
                VoxelType.GRASS       => VoxelMaterialType.OPAQUE,
                VoxelType.STONE       => VoxelMaterialType.OPAQUE,
                VoxelType.OAK_LOG     => VoxelMaterialType.OPAQUE,
                VoxelType.OAK_LEAVES  => VoxelMaterialType.CUTOUT,
                VoxelType.GRASS_SHORT => VoxelMaterialType.CUTOUT,
                VoxelType.GRASS_BOT   => VoxelMaterialType.CUTOUT,
                VoxelType.GRASS_TOP   => VoxelMaterialType.CUTOUT,

                _ => throw new NotImplementedException(
                    $"No {nameof(VoxelMaterialType)} is defined for {nameof(VoxelType)} '{type}'."
                  )
            };
        }

        public static VoxelGeometryType GetGeometryType(this VoxelType type)
        {
            return type switch
            {
                VoxelType.AIR         => VoxelGeometryType.CUBE,
                VoxelType.DIRT        => VoxelGeometryType.CUBE,
                VoxelType.GRASS       => VoxelGeometryType.CUBE,
                VoxelType.STONE       => VoxelGeometryType.CUBE,
                VoxelType.OAK_LOG     => VoxelGeometryType.CUBE,
                VoxelType.OAK_LEAVES  => VoxelGeometryType.CUBE,
                VoxelType.GRASS_SHORT => VoxelGeometryType.CROSS,
                VoxelType.GRASS_BOT   => VoxelGeometryType.CROSS,
                VoxelType.GRASS_TOP   => VoxelGeometryType.CROSS,

                _ => throw new NotImplementedException(
                    $"No {nameof(VoxelGeometryType)} is defined for {nameof(VoxelType)} '{type}'."
                  )
            };
        }

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
    }
}