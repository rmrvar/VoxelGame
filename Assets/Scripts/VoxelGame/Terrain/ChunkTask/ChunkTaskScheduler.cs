using System.Collections.Generic;
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

        public void Update(float deltaTime)
        {
            _executeCountdown -= deltaTime;

            if (_chunkTasks.Count <= 0)
            {
                return; // Nothing to do.
            }

            if (_numActiveTasks >= _maxActiveTasks)
            {
                return; // All executors are busy, wait for one to finish.
            }

            if (_executeCountdown > 0)
            {
                return; // Wait for the next execute time.
            }

            // Find the next task to execute, skipping cancelled tasks and tasks that cannot be executed.
            ChunkTask task = null;
            while (task == null && _chunkTasks.Count > 0)
            {
                task = _chunkTasks.Dequeue();
                if (task.IsCancelled())
                {
                    task = null;
                } else
                if (!task.CanExecute())
                {
                    _skippedTasks.Add(task);
                    task = null;
                }
            }

            // Re-add skipped tasks to the queue with the same priority.
            foreach (ChunkTask skippedTask in _skippedTasks)
            {
                _chunkTasks.Enqueue(skippedTask, skippedTask.Priority);
            }
            _skippedTasks.Clear();

            if (task == null)
            {
                // No task to execute, return early.
                return;
            }

            OnTaskStarted();
            _ = task.ExecuteAsync(OnTaskCompleted);

            _executeCountdown = _timeBetweenExecutes;
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
        private readonly List<ChunkTask> _skippedTasks = new();

        private readonly int _maxActiveTasks;
        private readonly float _timeBetweenExecutes;
        private float _executeCountdown;
        private int _numActiveTasks;
    }
}
