using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VoxelGame.Terrain.ChunkTask;

namespace VoxelGame.Terrain
{
	public class ChunkManager : MonoBehaviour
	{
		[SerializeField]
        private int _seed = 0;

		[SerializeField]
        private Chunk _chunkPrefab = null;

        [field: SerializeField] 
        public Vector2Int ChunkSize { get; private set; } = new(32, 32);

		[SerializeField] 
        private Transform _generationOrigin = null;
		[SerializeField] 
        private float _generationRadius = 100;

        [SerializeField]
        private float _chunkRefreshCooldown = 1;
        [SerializeField]
        private int _numTaskExecutors = 8;
        [SerializeField]
        private int _numTaskExecutesPerSecond = 8;

		public static ChunkManager Instance { get; private set; }

		private void Awake()
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if (Instance != null)
			{
				Debug.LogAssertion($"ChunkManager.Awake: Attempted to create multiple instances of type {typeof(ChunkManager)}!");
				Destroy(this.gameObject);
				return;
			}
#endif

			Instance = this;

            Random.InitState(_seed);
            _chunks = new Dictionary<Vector2Int, Chunk>();
            _chunkTaskScheduler = new ChunkTaskScheduler(_numTaskExecutors, _numTaskExecutesPerSecond);
		}


		private void Update()
		{
            _chunkTaskScheduler.Update(Time.deltaTime);

			_chunkRefreshTimer -= Time.deltaTime;
			if (_chunkRefreshTimer <= 0)
			{
				//Debug.Log("Loading chunks!");
				ShowChunksWithinView();
				_chunkRefreshTimer = _chunkRefreshCooldown;
			}
        }

		public Vector2Int GetChunkID(Vector3 pos)
		{
			pos.Scale(new Vector3(1.0F / ChunkSize.x, 0, 1.0F / ChunkSize.y));
			return new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
		}

		// Get the Chunk containing this position.
		public Chunk GetChunk(Vector3 pos)
		{
			_chunks.TryGetValue(GetChunkID(pos), out var chunk);

			return chunk;
		}

		private void ShowChunksWithinView()
		{
			int ratio = Mathf.CeilToInt(_generationRadius / ChunkSize.x);

			//Debug.Log(ratio);

			// Get the Chunks enveloping the player.
			for (int z = -ratio; z < +ratio; ++z)
			for (int x = -ratio; x < +ratio; ++x)
			{
				var chunkId = GetChunkID(new Vector3(x * ChunkSize.x, 0, z * ChunkSize.y) + _generationOrigin.position);
				var chunkPos = new Vector3Int(chunkId.x * ChunkSize.x, 0, chunkId.y * ChunkSize.y);

				if (_chunks.TryGetValue(chunkId, out var chunk))
				{
					if (!chunk.gameObject.activeInHierarchy)
					{
						//Debug.Log("Reactivating chunk!");
						chunk.gameObject.SetActive(true);
					}	
				}
				else
				{  // We have to create this Chunk.
					//Debug.Log("Spawning Chunk " + chunkId);
					chunk = Instantiate(_chunkPrefab, chunkPos, Quaternion.identity, this.transform);
                    _chunks.Add(chunkId, chunk);

					var sqrDistance = (chunkPos - _generationOrigin.position).sqrMagnitude;
                    int priority = (int) sqrDistance;

					// Schedule the chunk's tasks.
                    _chunkTaskScheduler.Schedule(new ChunkLoadTask(chunk, _chunkTaskScheduler, priority, CancellationToken.None));
                    _chunkTaskScheduler.Schedule(new ChunkMeshTask(chunk, _chunkTaskScheduler, priority, CancellationToken.None));
				}
			}
		}

        private Dictionary<Vector2Int, Chunk> _chunks;
        private ChunkTaskScheduler _chunkTaskScheduler;
		private float _chunkRefreshTimer;
    }
}
