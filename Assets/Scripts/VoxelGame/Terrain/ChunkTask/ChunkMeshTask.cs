using System;
using System.Threading;

namespace VoxelGame.Terrain.ChunkTask
{
    public class ChunkMeshTask
        : ChunkTask<ChunkMeshTaskIn, ChunkMeshTaskOut>
    {
        public ChunkMeshTask(
            Chunk chunk,
            ChunkTaskScheduler scheduler,
            int priority,
            CancellationToken token
          )
            : base(chunk, scheduler, priority, token)
        {
        }

        protected override ChunkMeshTaskIn PrepareInput()
        {
            return new ChunkMeshTaskIn(); // TODO
        }


        protected override ChunkMeshTaskOut Execute(ChunkMeshTaskIn input, CancellationToken cancellationToken)
        {
            return new ChunkMeshTaskOut(); // TODO
        }

        protected override void HandleOutput(ChunkMeshTaskOut output)
        {
            // TODO
        }
    }

    public class ChunkMeshTaskIn : IDisposable
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }
    }

    public class ChunkMeshTaskOut
    {

    }
}