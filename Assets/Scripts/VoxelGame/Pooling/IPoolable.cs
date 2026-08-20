namespace VoxelGame.Pooling
{
    public interface IPoolable
    {
        void OnBorrowed();
        void OnReturned();
    }
}
