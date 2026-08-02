using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VoxelGame.Terrain.Meshing;

namespace VoxelGame.Terrain
{
	[RequireComponent(typeof(Mesh))]
	public class Chunk : MonoBehaviour
	{
		public enum LoadStatus
		{
			LOADING,
			FINISHED_LOADING
		}

		public LoadStatus Status { get; set; }
		
		public Vector3Int Position { get; private set; }

		public Dictionary<Vector3Int, Voxel> Voxels { get; set; }

		// The height densities of solid blocks.
		private Dictionary<int, int> _heightDensities;
		private int[,] _heightmap;
		public int MinHeight { get; private set; } = int.MaxValue;
		public int MaxHeight { get; private set; } = int.MinValue;

		public void AddHeight(int height)
		{
			// Valid values for height should be between MinHeight - 1 and MaxHeight + 1. Though it 
			// may be possible that removing a block causes RemoveHeight to be called, incrementing 
			// the MinHeight, and then a new block is generated underneath, calling AddHeight with 
			// a value of MinHeight - 2.

			// Cover the case that you created a block below the min height. This block is the new min height.
			for (int i = MinHeight; i >= height; --i)
			{
				if (!_heightDensities.ContainsKey(i))
				{
					_heightDensities[i] = 0;
				}
			}

			// Cover the case that you created a block above the max height. This block is the new max height.
			for (int i = MaxHeight; i <= height; ++i)
			{
				if (!_heightDensities.ContainsKey(i))
				{
					_heightDensities[i] = 0;
				}
			}

			if (height < MinHeight)
			{
				MinHeight = height;
			} else
			if (height > MaxHeight)
			{
				MaxHeight = height;
			}

			++_heightDensities[height];
		}

		public void RemoveHeight(int height)
		{
			// Valid values for height are between MinHeight and MaxHeight.

			--_heightDensities[height];
			if (height == MinHeight)
			{
				// Set MinHeight to the first nonzero height, removing any zero heights along the way.
				for (int i = height; i <= MaxHeight; ++i)
				{
					if (_heightDensities[height] <= 0)
					{
						_heightDensities.Remove(height);
					}
					else
					{  // This is guaranteed to happen because _heightDensities[MaxHeight] > 0.
						MinHeight = i;
						break;
					}
				}
			}
			if (height == MaxHeight)
			{
				// Set MaxHeight to the first nonzero height, removing any zero heights along the way.
				for (int i = height; i >= MinHeight; --i)
				{
					if (_heightDensities[height] <= 0)
					{
						_heightDensities.Remove(height);
					}
					else
					{  // This is guaranteed to happen because _heightDensities[MinHeight] > 0.
						MaxHeight = i;
						break;
					}
				}
			}
		}

		public bool HasBeenModified { get; private set; }
		
		private MeshFilter _meshFilter;
		public GreedyMesher Mesher { get; private set; }
		public bool ShouldRedraw { get; set; }

		public bool ShouldCalculateCollisions { get; set; }
		private MeshCollider _meshCollider;

		private void Awake()
		{
			_meshFilter = GetComponent<MeshFilter>();
		}

		private void Start()
		{

		}

		private void Update()
		{
			if (Status != LoadStatus.FINISHED_LOADING)
			{
				return;
			}

			if (ShouldRedraw)
			{
				Mesher.ShowMesh(_meshFilter);
				ShouldRedraw = false;
			}

			if (ShouldCalculateCollisions)
			{
				CalculateCollisions();
				ShouldCalculateCollisions = false;
			}

			if (Mesher.DirtyCount > Mesher.MaxDirtyCountBeforeRegenerate)
			{
				Mesher.GenerateMesh();
				ShouldRedraw = true;
				ShouldCalculateCollisions = true;
			}
		}

		public async Task Load(bool shouldLoadFromFile, CancellationToken destroyRequestedToken)
		{
			Position = Vector3Int.FloorToInt(transform.position);

			//var stopwatch = new System.Diagnostics.Stopwatch();
			//stopwatch.Start();

			Status = LoadStatus.LOADING;

			await Task.Run(() =>
			{
				//if (Position != Vector3Int.zero) return;  // TODO: Remove!

				if (shouldLoadFromFile)
				{
					LoadMapsAndMesh();
				}
				else
				{
					Initialize();
					GenerateBiomesmap();
					GenerateHeightmap();
					GenerateVoxels();
					Mesher = new GreedyMesher(this);
					Mesher.GenerateMesh();
				}
			}, destroyRequestedToken);

			Status = LoadStatus.FINISHED_LOADING;

			Mesher.ShowMesh(_meshFilter);
			CalculateCollisions();

			//stopwatch.Stop();
			//Debug.Log($"Chunk took {stopwatch.ElapsedMilliseconds} milliseconds to create!");
		}

		private void LoadMapsAndMesh()
		{

		}

		private void OnDrawGizmosSelected()
		{
			if (Status != LoadStatus.FINISHED_LOADING)
			{
				return;
			}

			Gizmos.color = Color.blue;
			Gizmos.DrawWireMesh(Mesher.Mesh, 0, Position);
		}

		public Voxel AddVoxelStub(Vector3Int position, VoxelData.VoxelType voxelType = VoxelData.VoxelType.AIR, int biomeId = -1)
		{
			var voxel = new Voxel(position, voxelType, biomeId);
			Voxels.Add(position, voxel);
			return voxel;
		}

		// Should only be called when the actual voxel at this position has no visible faces.
		public void RemoveVoxel(Vector3Int position)
		{
			Voxels.Remove(position);
		}

		public Voxel GetVoxel(Vector3Int pos)
		{
			Voxels.TryGetValue(pos, out var voxel);

			return voxel;
		}

		public bool IsInChunk(Vector3Int position)
		{
			var x = position.x;
			var y = position.y;
			return x < 0 || x >= _heightmap.GetLength(0) || y < 0 || y >= _heightmap.GetLength(1);
		}

		private void CalculateCollisions()
		{
			if (_meshCollider == null)
			{
				_meshCollider = gameObject.AddComponent<MeshCollider>();
			}
			_meshCollider.sharedMesh = Mesher.Mesh;
		}
	}
}
