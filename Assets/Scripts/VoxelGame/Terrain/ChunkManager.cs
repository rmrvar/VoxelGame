using System.Collections.Generic;
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
        public Vector3Int ChunkSize { get; set; } = new(32, 32, 32);

		[SerializeField] 
        private Transform _generationOrigin = null;
		[SerializeField] 
        private float _generationRadius = 100;

        [SerializeField]
        private float _chunkRefreshCooldown = 1;
        [SerializeField]
        private int _maxActiveTasks = 8;
        [SerializeField]
        private int _maxTaskExecutesPerSecond = 8;

		public static ChunkManager Instance { get; private set; }

		public ChunkTaskScheduler Scheduler { get; private set; }


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
            _chunks = new Dictionary<Vector3Int, Chunk>();
            Scheduler = new ChunkTaskScheduler(_maxActiveTasks, _maxTaskExecutesPerSecond);
            BiomeLogic.Init();
        }


		//private void Update()
		//{
        //    Scheduler.Update(Time.deltaTime);
		//
		//	_chunkRefreshTimer -= Time.deltaTime;
		//	if (_chunkRefreshTimer <= 0)
		//	{
		//		//Debug.Log("Loading chunks!");
		//		ShowChunksWithinView();
		//		_chunkRefreshTimer = _chunkRefreshCooldown;
		//	}
        //}

		public Vector3Int GetChunkId(Vector3 pos)
		{
			pos.Scale(new Vector3(1.0F / ChunkSize.x, 1.0F / ChunkSize.y, 1.0F / ChunkSize.z));
			return new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		}

		public Chunk GetChunkByPos(Vector3 pos)
		{
			_chunks.TryGetValue(GetChunkId(pos), out Chunk chunk);
			return chunk;
		}

        public Chunk GetChunkById(Vector3Int id)
        {
            _chunks.TryGetValue(id, out Chunk chunk);
            return chunk;
        }

		private void ShowChunksWithinView()
		{
			int ratio = Mathf.CeilToInt(_generationRadius / ChunkSize.x);

			//Debug.Log(ratio);

			// Get the Chunks enveloping the player.
			for (int z = -ratio; z < +ratio; ++z)
            for (int y = -ratio; y < +ratio; ++y)
			for (int x = -ratio; x < +ratio; ++x)
            {
                Vector3 offset = new(x * ChunkSize.x, y * ChunkSize.y, z * ChunkSize.z);
                Vector3 position = _generationOrigin.position + offset;

				var chunkId = GetChunkId(position);
				var chunkPos = new Vector3Int(chunkId.x * ChunkSize.x, chunkId.y * ChunkSize.y, chunkId.z * ChunkSize.z);

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

                    // Schedule the chunk's tasks.
                    Scheduler.Schedule(new ChunkLoadTask(chunk, chunk.GetCancellationToken()), priority: sqrDistance);
				}
            }
		}

        private Dictionary<Vector3Int, Chunk> _chunks;
		private float _chunkRefreshTimer;
    }
}
