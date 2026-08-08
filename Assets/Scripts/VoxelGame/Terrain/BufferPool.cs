namespace Assets.Scripts.VoxelGame.Terrain
{
    public static class BufferPool
    {
        public static T Borrow<T>(int size) where T : class
        {
            return null;
        }

        public static void Return<T>(T buffer)
        {

        }
    }
}
