using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelGame.Terrain;

namespace VoxelGame.Player
{
	public class PlaceAndPickupBlocks : MonoBehaviour
	{
		[SerializeField] 
        private Transform _lookRoot;

		private void Update()
		{
            if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				InteractWithVoxel(placeOrPickup: false);
			} else
			if (Input.GetKeyDown(KeyCode.Mouse1))
			{
				InteractWithVoxel(placeOrPickup: true);
			}
		}

        private void InteractWithVoxel(bool placeOrPickup)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var layerMask = 
                (1 << LayerMask.NameToLayer("Chunk")) |
                (1 << LayerMask.NameToLayer("ChunkTrigger"));

            Debug.DrawLine(_lookRoot.position, _lookRoot.position + ray.direction.normalized * 7f, Color.blue, 1f);

            bool old = Physics.queriesHitBackfaces;
            try
            {
                Physics.queriesHitBackfaces = true;
                if (Physics.Raycast(ray, out var hitInfo, 7, layerMask))
                {
                    Debug.Log("HIT CHUNK!");

                    Vector3 point = hitInfo.point;

                    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Chunk"))
                    {
                        point += ((placeOrPickup ? +1 : -1) / 2.0F) * hitInfo.normal.normalized;
                    }
                    else
                    {
                        if (placeOrPickup)
                        {
                            return; // We don't want to place blocks on chunk triggers.
                        }
                    }

                    var posToAffect = Vector3Int.FloorToInt(point);
                    var chunkToAffect = ChunkManager.Instance.GetChunkByPos(posToAffect);
                    var indexToAffect = chunkToAffect.GetVoxelIndexFromWorldPosition(posToAffect);

                    Debug.Log($"Affected voxel {posToAffect} in {chunkToAffect.Id}");
                    DrawCube(posToAffect);

                    ConvertToPolytypeChunk(chunkToAffect);

                    UpdateVoxel(chunkToAffect, indexToAffect, placeOrPickup ? VoxelType.DIRT : VoxelType.AIR);

                    UpdateMeshes(chunkToAffect, posToAffect);
                }
            }
            finally
            {
                Physics.queriesHitBackfaces = old;
            }
        }

        private void ConvertToPolytypeChunk(Chunk chunk)
        {
            if (!chunk.IsMaterializedPolytype)
            {
                ChunkManager.Instance.ScheduleImmediateReloadTask(chunk);
            }
        }

        private void UpdateVoxel(Chunk chunk, int i, VoxelType type)
        {
            chunk.PolyData.Data[i] = type;
            ++chunk.VoxelVersion;
            ChunkManager.Instance.SaveSystem.MarkDirty(chunk.Id);
        }
        
        private void UpdateMeshes(Chunk chunk, Vector3Int position)
        {
            ChunkManager.Instance.ScheduleImmediateRemeshTask(chunk);
            foreach (Vector3Int neighborPosition in GetNeighboringPositions(position))
            {
                Vector3Int neighborChunkId = ChunkManager.Instance.GetChunkId(neighborPosition);

                Chunk neighborChunk = ChunkManager.Instance.GetChunkById(neighborChunkId);
                if (neighborChunk == chunk)
                {
                    continue; // We have already meshed this chunk.
                }

                ChunkManager.Instance.SaveSystem.MarkDirty(neighborChunkId);

                if (neighborChunk == null)
                {
                    // Only happens with really fast player and unloading, but shouldn't be
                    // an issue with dirtiness.
                    continue;
                }

                if (!neighborChunk.IsMaterializedPolytype)
                {
                    // We could technically render a dirty materialized monotype chunk, but
                    // the save system wants all dirty chunks to be polytypes.
                    ChunkManager.Instance.ScheduleImmediateReloadTask(neighborChunk);
                }
                ChunkManager.Instance.ScheduleImmediateRemeshTask(neighborChunk);
            }
        }

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
	