using System.Collections.Generic;
using UnityEngine;
using VoxelGame.Pooling;
using VoxelGame.Saving;
using VoxelGame.Terrain.ChunkTask;
using Random = UnityEngine.Random;

namespace VoxelGame.Terrain
{
	public class ChunkManager : MonoBehaviour
	{
		[SerializeField]
        private int _seed;

        [SerializeField] 
        private bool _shouldLoad;
        [SerializeField] 
        private bool _shouldSave;

        [SerializeField]
        private Vector3Int _chunkSize = new(32, 32, 32);

        [SerializeField] 
        private Transform _loadOrigin;
		[SerializeField] 
        private float _loadRadiusXZ = 300;
        [SerializeField]
        private float _loadRadiusY = 100;
        [SerializeField]
        private float _collisionRadius = 64;

        [SerializeField]
        private float _chunkRefreshCooldown = 1;
        [SerializeField]
        private int _maxActiveTasks = 8;
        [SerializeField]
        private int _maxTaskExecutesPerSecond = 8;
        [SerializeField]
        private int _maxLazyExecutesPerFrame = 50;

        [SerializeField]
        private ChunkMono _chunkMonoPrefab;

        [SerializeField]
        private int _chunkMonoPoolRefillThreshold = 1000;
        [SerializeField]
        private int _chunkMonoPoolRefillRate = 20;

        public static ChunkManager Instance { get; private set; }

        public SaveSystem SaveSystem { get; private set; }
        public Pool<ChunkMono> ChunkMonoPool { get; private set; }

        public void ScheduleLoadTask(Chunk chunk)
        {
            _scheduler.Schedule(
                new ChunkLoadTask(chunk, chunk.GetCancellationToken()),
                priority: GetChunkPriority(chunk)
              );
        }

        public void ScheduleMeshTask(Chunk chunk)
        {
            _scheduler.Schedule(
                new ChunkMeshTask(chunk, chunk.GetCancellationToken()),
                priority: GetChunkPriority(chunk)
              );
        }

        public void ScheduleImmediateReloadTask(Chunk chunk)
        {
            _scheduler.Interrupt(
                new ChunkLoadTask(
                    chunk,
                    chunk.GetCancellationToken(),
                    shouldRunInBackground: false,
                    isReload: true
                  )
              );
        }

        public void ScheduleImmediateRemeshTask(Chunk chunk)
        {
            _scheduler.Interrupt(
                new ChunkMeshTask(
                    chunk,
                    chunk.GetCancellationToken(),
                    shouldRunInBackground: false,
                    isRemesh: true
                  )
              );
        }

        public bool GetChunkHeightRange(Vector3Int id, out int minHeight, out int maxHeight)
        {
            Vector2Int id2 = new(id.x, id.z);
            if (_chunkIdXZToHeightRange.TryGetValue(id2, out Vector2Int heightRange))
            {
                minHeight = heightRange.x;
                maxHeight = heightRange.y;
                return true;
            }

            minHeight = 0;
            maxHeight = 0;
            return false;
        }

        public void SetChunkHeightRange(Vector3Int id, int minHeight, int maxHeight)
        {
            Vector2Int id2 = new Vector2Int(id.x, id.z);
            _chunkIdXZToHeightRange[id2] = new Vector2Int(minHeight, maxHeight);
        }

        public Vector3Int GetChunkId(Vector3 pos)
        {
            pos.Scale(new Vector3(1.0F / ChunkConfig.SizeX, 1.0F / ChunkConfig.SizeY, 1.0F / ChunkConfig.SizeZ));
            return new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
        }

        public Chunk GetChunkByPos(Vector3 pos)
        {
            _chunkIdToChunk.TryGetValue(GetChunkId(pos), out Chunk chunk);
            return chunk;
        }

        public Chunk GetChunkById(Vector3Int id)
        {
            _chunkIdToChunk.TryGetValue(id, out Chunk chunk);
            return chunk;
        }

        private void Awake()
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Assert(Instance == null);
			if (Instance != null)
            {
				Destroy(this.gameObject);
				return;
			}
#endif
			Instance = this;

            Application.targetFrameRate = 60;

            SaveSystem = new SaveSystem();
            if (_shouldLoad)
            {
                SaveSystem.Load();
            }

            Random.InitState(_seed);
            BiomeLogic.Init();
			ChunkConfig.Init(_chunkSize);

            _collisionRadiusSqr = _collisionRadius * _collisionRadius;

            _ratioX = Mathf.CeilToInt(_loadRadiusXZ / ChunkConfig.SizeX);
            _ratioY = Mathf.CeilToInt( _loadRadiusY / ChunkConfig.SizeY);
            _ratioZ = Mathf.CeilToInt(_loadRadiusXZ / ChunkConfig.SizeZ);
            int sizeX = _ratioX * 2 + 1;
            int sizeY = _ratioY * 2 + 1;
            _neighborY = new Chunk[sizeX];
            _neighborZ = new Chunk[sizeY, sizeX];

            _scheduler = new ChunkTaskScheduler(_maxActiveTasks, _maxTaskExecutesPerSecond, _maxLazyExecutesPerFrame);

            ChunkMonoPool = new Pool<ChunkMono>(
                () => Instantiate(_chunkMonoPrefab, transform),
                _chunkMonoPoolRefillThreshold
              );

            _chunkMonoPoolRefillCooldown = 1f / _chunkMonoPoolRefillRate;
            _chunkMonoPoolRefillTimer = _chunkMonoPoolRefillCooldown;
        }

        private void OnDestroy()
        {
            if (_shouldSave)
            {
                SaveSystem.Save();
            }

            foreach (Chunk chunk in _chunkIdToChunk.Values)
            {
                chunk.Dispose();
            }
        }

        private void Update()
		{
            _currChunkId = GetChunkId(_loadOrigin.position);
            if (_currChunkId != _prevChunkId)
            {
                _scheduler.Reprioritize(GetChunkPriority);
            }

			_scheduler.Update(Time.deltaTime);

            RefillPools();

            LoadChunks();

            _prevChunkId = _currChunkId;
        }

        private void RefillPools()
        {
            if (ChunkMonoPool.Count >= _chunkMonoPoolRefillThreshold)
            {
                return;
            }

            _chunkMonoPoolRefillTimer -= Time.deltaTime;
            if (_chunkMonoPoolRefillTimer > 0)
            {
                return;
            }

            ChunkMonoPool.Warm(1);
            _chunkMonoPoolRefillTimer = _chunkMonoPoolRefillCooldown;
        }

        private void LoadChunks()
        {
            _chunkRefreshTimer -= Time.deltaTime;
            if (_chunkRefreshTimer > 0)
            {
                return;
            }

            ShowChunksWithinView();
            _chunkRefreshTimer = _chunkRefreshCooldown;
        }

        private void ShowChunksWithinView()
        {
            for (int z = -_ratioZ; z <= _ratioZ; ++z)
            for (int y = -_ratioY; y <= _ratioY; ++y)
            {
                int iy = y + _ratioY;

                _neighborX = null;

                Vector3Int chunkId = _currChunkId + new Vector3Int(-_ratioX, y, z);

                for (int x = -_ratioX; x <= _ratioX; ++x)
                {
                    int ix = x + _ratioX;

                    if (_chunkIdToChunk.TryGetValue(chunkId, out var chunk))
                    {
                        if (chunk.Mono != null)
                        {
                            float sqrDistance = (chunk.Center - _loadOrigin.position).sqrMagnitude;
                            bool shouldCollide = sqrDistance <= _collisionRadiusSqr;
                            chunk.Mono.CanCollide = shouldCollide;
                        }
                    }
                    else
                    {
                        chunk = new Chunk(chunkId);
                        _chunkIdToChunk.Add(chunkId, chunk);

                        ScheduleLoadTask(chunk);
                    }

                    Chunk neighborNegX = x > -_ratioX
                        ? _neighborX
                        : null;
                    Chunk neighborNegY = y > -_ratioY
                        ? _neighborY[ix]
                        : null;
                    Chunk neighborNegZ = z > -_ratioZ
                        ? _neighborZ[iy, ix]
                        : null;

                    neighborNegX?.InitNeighbor(chunk, 0);
                    neighborNegY?.InitNeighbor(chunk, 1);
                    neighborNegZ?.InitNeighbor(chunk, 2);
                    chunk.InitNeighbor(neighborNegX, 3);
                    chunk.InitNeighbor(neighborNegY, 4);
                    chunk.InitNeighbor(neighborNegZ, 5);

                    _neighborX = chunk;
                    _neighborY[ix] = chunk;
                    _neighborZ[iy, ix] = chunk;

                    ++chunkId.x;
                }
            }
        }

        private float GetChunkPriority(Chunk chunk)
        {
            return (chunk.Center - _loadOrigin.position).sqrMagnitude;
        }

        private float _collisionRadiusSqr;

        private int _ratioX;
        private int _ratioY;
        private int _ratioZ;

        private Chunk _neighborX;
        private Chunk[] _neighborY;
        private Chunk[,] _neighborZ;

        private readonly Dictionary<Vector3Int, Chunk> _chunkIdToChunk = new(10000);
        private readonly Dictionary<Vector2Int, Vector2Int> _chunkIdXZToHeightRange = new(1000);
		private float _chunkRefreshTimer;

        private ChunkTaskScheduler _scheduler;

        private float _chunkMonoPoolRefillTimer;
        private float _chunkMonoPoolRefillCooldown;

        private Vector3Int _currChunkId;
        private Vector3Int _prevChunkId;
    }
}
