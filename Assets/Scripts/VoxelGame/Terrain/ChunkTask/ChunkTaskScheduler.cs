using Priority_Queue;

namespace VoxelGame.Terrain.ChunkTask
{
    public class ChunkTaskScheduler
    {
        public ChunkTaskScheduler(int maxActiveTasks, int maxExecutesPerSecond)
        {
            _maxActiveTasks = maxActiveTasks;
            _timeBetweenExecutes = 1 / (float)maxExecutesPerSecond;
            _executeCountdown = _timeBetweenExecutes;
        }

        public int NumScheduledTasks => _chunkTasks.Count;

        public void Update(float deltaTime)
        {
            _executeCountdown -= deltaTime;

            if (_chunkTasks.Count <= 0)
            {
                return; // Nothing to do.
            }

            if (_executeCountdown > 0)
            {
                return; // Wait for the next execute time.
            }

            // Find the next task that needs a worker, skipping cancelled tasks and completing lazy tasks immediately.
            while (_chunkTasks.Count > 0)
            {
                ChunkTask task = _chunkTasks.First;

                if (task.IsCancelled())
                {
                    _chunkTasks.Dequeue();
                    continue;
                }

                if (task.TryLazyExecute())
                {
                    // TODO: Consider limiting this to _maxLazyExecutesPerFrame.
                    _chunkTasks.Dequeue();
                    continue;
                }

                if (_numActiveTasks >= _maxActiveTasks)
                {
                    return;
                }

                _chunkTasks.Dequeue();

                OnTaskStarted();
                _ = task.ExecuteAsync(OnTaskCompleted);
               _executeCountdown = _timeBetweenExecutes;
                return;
            }
        }

        public void Schedule(ChunkTask task, float priority)
        {
            _chunkTasks.Enqueue(task, priority);
        }

        public void Interrupt(ChunkTask task)
        {
            _ = task.ExecuteAsync();
        }

        private void OnTaskStarted()
        {
            ++_numActiveTasks;
        }

        private void OnTaskCompleted()
        {
            --_numActiveTasks;
        }

        private readonly FastPriorityQueue<ChunkTask> _chunkTasks = new(10000);

        private readonly int _maxActiveTasks;
        private readonly float _timeBetweenExecutes;
        private float _executeCountdown;
        private int _numActiveTasks;
    }
}
