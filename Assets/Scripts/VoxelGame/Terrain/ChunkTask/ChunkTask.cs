using System;
using System.Threading;
using System.Threading.Tasks;
using Priority_Queue;

namespace VoxelGame.Terrain.ChunkTask
{
    public abstract class ChunkTask 
        : FastPriorityQueueNode
    {
        public Chunk Chunk { get; }
        public ChunkTaskScheduler Scheduler { get; set; }
        public int Priority { get; private set; }
        public CancellationToken Token { get; }

        public ChunkTask(
            Chunk chunk,
            ChunkTaskScheduler scheduler,
            int priority,
            CancellationToken token
          )
        {
            Chunk = chunk;
            Scheduler = scheduler;
            Priority = priority;
            Token = token;
        }

        public bool IsCancelled() => Token.IsCancellationRequested;
        public virtual bool CanExecute() => true;

        public async Task ExecuteAsync()
        {
            try
            {
                await RunTaskAsync();
            }
            catch (OperationCanceledException)
            {
                // Don't crash the game if the task was cancelled, just ignore it.
            }
            finally
            {
                Scheduler.OnTaskCompleted(this);
            }
        }

        protected virtual async Task RunTaskAsync()
        {
            await Task.Run(() => Execute(Token), Token);
        }

        protected abstract void Execute(CancellationToken cancellationToken);
    }

    public abstract class ChunkTask<TIn, TOut> 
        : ChunkTask
        where TIn : IDisposable
    {
        public ChunkTask(
            Chunk chunk,
            ChunkTaskScheduler scheduler,
            int priority,
            CancellationToken token
          )
            : base(chunk, scheduler, priority, token)
        {
        }

        protected sealed override async Task RunTaskAsync()
        {
            using TIn input = PrepareInput();

            TOut output = await Task.Run(() => Execute(input, Token), Token);

            if (!Token.IsCancellationRequested)
            {
                HandleOutput(output);
            }
        }

        protected sealed override void Execute(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Use Execute(TIn, CancellationToken).");
        }

        protected abstract TIn PrepareInput();
        protected abstract void HandleOutput(TOut output);
        protected abstract TOut Execute(TIn input, CancellationToken cancellationToken);
    }
}
