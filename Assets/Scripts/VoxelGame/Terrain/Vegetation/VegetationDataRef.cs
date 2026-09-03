namespace VoxelGame.Terrain.Vegetation
{
    public struct VegetationDataRef
    {
        // Index into VegetationSystem.VegetationDatas.
        public byte VegetationDataIndex;

        // Index into VegetationData.Types.
        public ushort TypeIndex;

        // Reserved for per-reference metadata.
        // Potential uses:
        // - 1 bit: reflection across X
        // - 2 bits: rotation around Y
        // - X bits: distance from vegetation root (might be inferrable from TypeIndex)
        // - X bits: group ID
        public byte Reserved;
    }
}
