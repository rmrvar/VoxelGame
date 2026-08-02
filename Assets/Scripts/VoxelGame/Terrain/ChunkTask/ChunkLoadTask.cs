using System;
using System.Threading;

namespace VoxelGame.Terrain.ChunkTask
{
    public class ChunkLoadTask
        : ChunkTask<ChunkLoadTaskIn, ChunkLoadTaskOut>
    {
        public ChunkLoadTask(ChunkTaskScheduler scheduler, int priority, CancellationToken token) : base(scheduler, priority, token)
        {
        }

        protected override ChunkLoadTaskIn PrepareInput()
        {
            return new ChunkLoadTaskIn(); // TODO
        }


        protected override ChunkLoadTaskOut Execute(ChunkLoadTaskIn input, CancellationToken cancellationToken)
        {
            return new ChunkLoadTaskOut(); // TODO
        }

        protected override void HandleOutput(ChunkLoadTaskOut output)
        {
            // TODO
        }
    }

    public class ChunkLoadTaskIn : IDisposable
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }
    }

    public class ChunkLoadTaskOut
    {

    }
}
