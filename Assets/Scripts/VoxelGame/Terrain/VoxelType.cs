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

        GRASS_BOT,
        GRASS_TOP
    }

    public enum VoxelMaterialType : byte
    {
        OPAQUE,
        CUTOUT_TREE,
        CUTOUT_GRASS,
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
                VoxelType.AIR        => VoxelMaterialType.TRANSPARENT,
                VoxelType.DIRT       => VoxelMaterialType.OPAQUE,
                VoxelType.GRASS      => VoxelMaterialType.OPAQUE,
                VoxelType.STONE      => VoxelMaterialType.OPAQUE,
                VoxelType.OAK_LOG    => VoxelMaterialType.OPAQUE,
                VoxelType.OAK_LEAVES => VoxelMaterialType.CUTOUT_TREE,
                VoxelType.GRASS_BOT  => VoxelMaterialType.CUTOUT_GRASS,
                VoxelType.GRASS_TOP  => VoxelMaterialType.CUTOUT_GRASS,

                _ => throw new NotImplementedException(
                    $"No {nameof(VoxelMaterialType)} is defined for {nameof(VoxelType)} '{type}'."
                  )
            };
        }

        public static VoxelGeometryType GetGeometryType(this VoxelType type)
        {
            return type switch
            {
                VoxelType.AIR        => VoxelGeometryType.CUBE,
                VoxelType.DIRT       => VoxelGeometryType.CUBE,
                VoxelType.GRASS      => VoxelGeometryType.CUBE,
                VoxelType.STONE      => VoxelGeometryType.CUBE,
                VoxelType.OAK_LOG    => VoxelGeometryType.CUBE,
                VoxelType.OAK_LEAVES => VoxelGeometryType.CUBE,
                VoxelType.GRASS_BOT  => VoxelGeometryType.CROSS,
                VoxelType.GRASS_TOP  => VoxelGeometryType.CROSS,

                _ => throw new NotImplementedException(
                    $"No {nameof(VoxelGeometryType)} is defined for {nameof(VoxelType)} '{type}'."
                  )
            };
        }

        public static bool IsOpaque(this VoxelType type)
            => type.GetMaterialType() == VoxelMaterialType.OPAQUE;

        public static bool IsCutoutTree(this VoxelType type)
            => type.GetMaterialType() == VoxelMaterialType.CUTOUT_TREE;

        public static bool IsCutoutGrass(this VoxelType type)
            => type.GetMaterialType() == VoxelMaterialType.CUTOUT_GRASS;

        public static bool IsTransparent(this VoxelType type)
            => type.GetMaterialType() == VoxelMaterialType.TRANSPARENT;

        public static bool IsCube(this VoxelType type)
            => type.GetGeometryType() == VoxelGeometryType.CUBE;

        public static bool IsCross(this VoxelType type)
            => type.GetGeometryType() == VoxelGeometryType.CROSS;
    }
}