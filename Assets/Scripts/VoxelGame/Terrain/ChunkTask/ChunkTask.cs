#define DISABLE_BACKGROUND_EXECUTION
using System;
using System.Threading;
using System.Threading.Tasks;
using Priority_Queue;
using UnityEngine;

namespace VoxelGame.Terrain.ChunkTask
{
    public abstract class ChunkTask 
        : FastPriorityQueueNode
    {
        public Chunk Chunk { get; }
        public CancellationToken Token { get; }
        public bool ShouldRunInBackground { get; }

        protected ChunkTask(
            Chunk chunk,
            CancellationToken token,
            bool shouldRunInBackground = true
          )
        {
            Chunk = chunk;
            Token = token;
            ShouldRunInBackground = shouldRunInBackground;

#if DISABLE_BACKGROUND_EXECUTION
            ShouldRunInBackground = false;
#endif
        }

        public virtual bool IsCancelled() => Token.IsCancellationRequested;
        public virtual bool CanExecute() => true;

        public async Task ExecuteAsync(Action onCompleted = null)
        {
            try
            {
                await RunTaskAsync();
            }
            catch (OperationCanceledException)
            {
                // Don't crash the game if the task was cancelled, just ignore it.
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            finally
            {
                onCompleted?.Invoke();
            }
        }

        protected virtual Task RunTaskAsync()
        {
            if (ShouldRunInBackground)
            {
                return Task.Run(() => Execute(Token), Token);
            }
            else
            {
                Execute(Token);
                return Task.CompletedTask;
            }
        }

        protected abstract void Execute(CancellationToken cancellationToken);
    }

    public abstract class ChunkTask<TIn, TOut> 
        : ChunkTask
        where TIn : IDisposable
    {
        protected ChunkTask(
            Chunk chunk,
            CancellationToken token,
            bool shouldRunInBackground = true
          )
            : base(chunk, token, shouldRunInBackground)
        {
        }

        protected sealed override async Task RunTaskAsync()
        {
            using TIn input = PrepareInput();

            TOut output = default;
            Exception exception = null;
            try
            {
                if (ShouldRunInBackground)
                {
                    output = await Task.Run(() => Execute(input, Token), Token);
                }
                else
                {
                    output = Execute(input, Token);
                }
            }
            catch (Exception e)
            {
                exception = e;
                throw;
            }
            finally
            {
                HandleOutput(output, exception);
            }
        }

        protected sealed override void Execute(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Use Execute(TIn, CancellationToken).");
        }

        protected abstract TIn PrepareInput();
        protected abstract void HandleOutput(TOut output, Exception exception);
        protected abstract TOut Execute(TIn input, CancellationToken cancellationToken);
    }
}
