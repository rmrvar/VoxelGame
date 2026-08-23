using System;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

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
            if (_executeCountdown > 0)
            {
                return; // Wait for the next execute time.
            }

            FlushPendingTasks();

            if (_chunkTasks.Count <= 0)
            {
                return; // Nothing to do.
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
            task.Priority = priority;
            _pendingTasks.Add(task);
        }

        public void Interrupt(ChunkTask task)
        {
            _ = task.ExecuteAsync();
        }

        public void Reprioritize(Func<Chunk, float> getPriority)
        {
            Debug.Assert(getPriority != null);
            foreach (ChunkTask chunkTask in _pendingTasks)
            {
                chunkTask.Priority = getPriority(chunkTask.Chunk);
            }
            _chunkTasks.RefreshPriorities(chunkTask => getPriority(chunkTask.Chunk));
        }

        private void OnTaskStarted()
        {
            ++_numActiveTasks;
        }

        private void OnTaskCompleted()
        {
            --_numActiveTasks;
        }

        private void FlushPendingTasks()
        {
            foreach (var pendingTask in _pendingTasks)
            {
                _chunkTasks.Enqueue(pendingTask, pendingTask.Priority);
            }
            _pendingTasks.Clear();
        }

        private readonly List<ChunkTask> _pendingTasks = new(1000);
        private readonly FastPriorityQueue<ChunkTask> _chunkTasks = new(50000);

        private readonly int _maxActiveTasks;
        private readonly float _timeBetweenExecutes;
        private float _executeCountdown;
        private int _numActiveTasks;
    }
}
