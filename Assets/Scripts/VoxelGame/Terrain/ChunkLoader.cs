using Priority_Queue;

namespace VoxelGame.Terrain
{ 
    public class ChunkLoader
    {
        private readonly FastPriorityQueue<ChunkLoadTask> _chunkLoadQueue = new(10000);

        private readonly int _numLoadsPerSecond;
        private readonly float _timeBetweenLoads;
        private float _loadCountdown;

        public ChunkLoader(int numLoadsPerSecond)
        {
            _numLoadsPerSecond = numLoadsPerSecond;
            _timeBetweenLoads = 1 / (float) _numLoadsPerSecond;
            _loadCountdown = _timeBetweenLoads;
        }

        public void ScheduleForLoad(Chunk chunk, int priority)
        {
            _chunkLoadQueue.Enqueue(new ChunkLoadTask(chunk), priority);
        }

        public void Update(float deltaTime)
        {
            _loadCountdown -= deltaTime;

            if (_chunkLoadQueue.Count <= 0)
            {
                return; // Nothing to do.
            }

            if (_loadCountdown <= 0)
            {
                Chunk chunk;
                do
                {
                    chunk = _chunkLoadQueue.Dequeue().Chunk;
                } while (chunk == null && _chunkLoadQueue.Count > 0);

                chunk.Load(false, new System.Threading.CancellationToken());

                _loadCountdown = _timeBetweenLoads;
            }
        }
    }
}
