using Priority_Queue;

namespace VoxelGame.Terrain
{
    public class ChunkLoadTask : FastPriorityQueueNode
    {
        public ChunkLoadTask(Chunk chunk)
        {
            Chunk = chunk;
        }

        public Chunk Chunk { get; private set; }
    }
}
