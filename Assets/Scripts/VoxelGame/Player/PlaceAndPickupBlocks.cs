using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelGame.Terrain;

namespace VoxelGame
{
	public class PlaceAndPickupBlocks : MonoBehaviour
	{
		[SerializeField] private Transform _lookRoot = null;

		private void Awake()
		{
			Cursor.lockState = CursorLockMode.Locked;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Cursor.lockState = CursorLockMode.None;
			}

			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				Cursor.lockState = CursorLockMode.Locked;
				InteractWithBlock(placeOrPickup: false);
			} else
			if (Input.GetKeyDown(KeyCode.Mouse1))
			{
				Cursor.lockState = CursorLockMode.Locked;
				InteractWithBlock(placeOrPickup: true);
			}
		}

		private void InteractWithBlock(bool placeOrPickup)
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			var layerMask = 1 << LayerMask.NameToLayer("Chunk");  // We only want to intersect the Chunk.

			Debug.DrawLine(_lookRoot.position, _lookRoot.position + ray.direction.normalized * 7f, Color.blue, 1f);

			if (Physics.Raycast(ray, out var hitInfo, 7, layerMask))
			{
				Debug.Log("HIT CHUNK!");

				var posToAffect = Vector3Int.FloorToInt(hitInfo.point + (placeOrPickup ? +1 : -1) * hitInfo.normal.normalized / 2f);
				var chunkToAffect = ChunkManager.Instance.GetChunkByPos(posToAffect);
                var indexToAffect = chunkToAffect.GetVoxelIndexFromWorldPosition(posToAffect);

				Debug.Log($"Hit voxel {posToAffect} in {chunkToAffect.Id}");

                if (!chunkToAffect.IsMaterializedPolytype)
                {
					// We need voxels to work with.
					ChunkManager.Instance.ScheduleImmediateLoadPolytypeTask(chunkToAffect);
                }

				DrawCube(posToAffect);

				// We are now working with a polytype chunk.

				if (placeOrPickup)
				{
                    chunkToAffect.PolyData.Data[indexToAffect] = VoxelData.VoxelType.DIRT; // TODO: Add support for different voxel types.
				}
                else
                {
                    chunkToAffect.PolyData.Data[indexToAffect] = VoxelData.VoxelType.AIR;
                }

                ++chunkToAffect.VoxelVersion;

                ChunkManager.Instance.ScheduleImmediateMeshTask(chunkToAffect);

                foreach (Vector3Int neighborPosition in GetNeighboringPositions(posToAffect))
                {
                    Vector3Int neighborChunkId = ChunkManager.Instance.GetChunkId(neighborPosition);

                    Chunk neighborChunk = ChunkManager.Instance.GetChunkById(neighborChunkId);
                    if (neighborChunk == chunkToAffect)
                    {
                        continue; // We have already meshed this chunk.
                    }

                    ChunkManager.Instance.MarkChunkIdDirty(neighborChunkId);

                    if (neighborChunk == null)
                    {
                        // Only happens with really fast player and unloading, but shouldn't be an issue with dirtiness.
                        continue;
                    }

                    if (!neighborChunk.IsMaterialized)
                    {
                        ChunkManager.Instance.ScheduleImmediateLoadPolytypeTask(neighborChunk);
                    }
                    ChunkManager.Instance.ScheduleImmediateMeshTask(neighborChunk);

                    ChunkManager.Instance.MarkChunkIdClean(neighborChunkId);
                }

				//if (placeOrPickup)
				//{ 
				//	PushOutAllItemDropsInBlock(chunkToAffect, posToAffect);
				//}
			}
		}

        //private void PushOutAllItemDropsInBlock(Chunk chunk, Vector3Int localPositionToPlace)
        //{
        //	var neighboringPositions = chunk.GetNeighboringPositions(localPositionToPlace);
        //
        //	var chunkPos = chunk.gameObject.transform.position;
        //	var globalBlockOrigin = chunkPos + localPositionToPlace + new Vector3(0.5F, 0.5F, 0.5F);
        //
        //	var colliders = Physics.OverlapBox(globalBlockOrigin, Vector3.one * 0.5F, Quaternion.identity, LayerMask.GetMask("Item Drop"));
        //	foreach (var collider in colliders)
        //	{
        //		var itemPos = collider.attachedRigidbody.position;
        //
        //		var smallestDir = Vector3.zero;
        //		var smallestDeltaMag = float.MaxValue;
        //		foreach (var neighboringPos in neighboringPositions)
        //		{
        //			Vector3 neighborDir = neighboringPos - localPositionToPlace;
        //			var neighbor = chunk.GetVoxel(neighboringPos);
        //
        //			if (neighbor != null)
        //			{ 
        //				continue;  // The neighboring block is occupied. We want to find an unoccupied one to push this item to. Skip.
        //			}
        //
        //			// Calculates the distance of the item to the neighboring edge.
        //			// Here we rely on the alternate definition of the dot product
        //			// a dot b = a.x * b.x + a.y * b.y + a.z * b.z
        //			// to eliminate any the two axes not in the direction of the neighbor.
        //			var delta = (globalBlockOrigin + neighborDir * 0.5F) - itemPos;
        //			var deltaMag = Vector3.Dot(delta, neighborDir);
        //			if (smallestDeltaMag > deltaMag)
        //			{
        //				smallestDir = neighborDir;
        //				smallestDeltaMag = deltaMag;
        //			}
        //		}
        //
        //		// Add a small constant to the smallestDeltaMag to represent the size of the ItemDrop.
        //		collider.transform.position += smallestDir * (smallestDeltaMag + 0.2F);
        //	}
        //}

        private static IEnumerable<Vector3Int> GetNeighboringPositions(Vector3Int position)
        {
        	yield return position + new Vector3Int(+1,  0,  0);
        	yield return position + new Vector3Int( 0, +1,  0);
        	yield return position + new Vector3Int( 0,  0, +1);
        	yield return position + new Vector3Int(-1,  0,  0);
        	yield return position + new Vector3Int( 0, -1,  0);
        	yield return position + new Vector3Int( 0,  0, -1);
        }

        private void DrawCube(Vector3 position)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }
            _coroutine = StartCoroutine(IE_DrawCube(position));
        }

		private IEnumerator IE_DrawCube(Vector3 pos)
		{
			_cubeCenter = pos + Vector3.one * 0.5F;
			yield return new WaitForSeconds(0.1F);
			_cubeCenter = null;
            _coroutine = null;
        }

		private Vector3? _cubeCenter;
		private Coroutine _coroutine;
		private void OnDrawGizmos()
		{
			if (_cubeCenter != null)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawCube(_cubeCenter.Value, Vector3.one);
			}
		}
	}
}
	