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
        OAK_LEAVES
    }

    public enum VoxelRenderType : byte
    {
        OPAQUE,
        CUTOUT,
        TRANSPARENT
    }

    public static class VoxelTypeExtensions
    {
        public static VoxelRenderType GetRenderType(this VoxelType type)
        {
            return type switch
            {
                VoxelType.AIR        => VoxelRenderType.TRANSPARENT,
                VoxelType.DIRT       => VoxelRenderType.OPAQUE,
                VoxelType.GRASS      => VoxelRenderType.OPAQUE,
                VoxelType.STONE      => VoxelRenderType.OPAQUE,
                VoxelType.OAK_LOG    => VoxelRenderType.OPAQUE,
                VoxelType.OAK_LEAVES => VoxelRenderType.CUTOUT,

                _ => throw new NotImplementedException(
                    $"No {nameof(VoxelRenderType)} is defined for {nameof(VoxelType)} '{type}'."
                )
            };
        }

        public static bool IsOpaque(this VoxelType type)
            => type.GetRenderType() == VoxelRenderType.OPAQUE;

        public static bool IsCutout(this VoxelType type)
            => type.GetRenderType() == VoxelRenderType.CUTOUT;

        public static bool IsTransparent(this VoxelType type)
            => type.GetRenderType() == VoxelRenderType.TRANSPARENT;
    }
}