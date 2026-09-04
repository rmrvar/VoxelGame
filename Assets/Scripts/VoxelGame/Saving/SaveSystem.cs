using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VoxelGame.Terrain;

namespace VoxelGame.Saving
{
	public class SaveSystem
	{
		private static string _version = "0.0.0.1";

        public bool IsDirty(Vector3Int chunkId)
        {
            return _dirtyChunkIds.Contains(chunkId);
        }

        public void MarkClean(Vector3Int chunkId)
        {
            _dirtyChunkIds.Remove(chunkId);
        }

        public void MarkDirty(Vector3Int chunkId)
        {
            _dirtyChunkIds.Add(chunkId);
        }

        public bool TryGetSaveData(Vector3Int chunkId, out byte[] saveData)
        {
            return _chunkIdToSaveData.TryGetValue(chunkId, out saveData);
        }

		public void Load()
        {
            bool hasSave = PlayerPrefs.GetInt("HAS_SAVE") == 1;
            if (!hasSave)
            {
                return;
            }

            string version = PlayerPrefs.GetString("VERSION");
            if (version != _version)
            {
                Debug.LogError("Failed to load because save version unsupported!");
                return;
            }

            string dirtyChunkIdsText = PlayerPrefs.GetString("DIRTY_CHUNK_IDS");
            List<Vector3Int> dirtyChunkIds = DeserializeVectorList(dirtyChunkIdsText);
            if (dirtyChunkIds.Count <= 0)
            {
                return;
            }

            foreach (Vector3Int chunkId in dirtyChunkIds)
            {
                string saveKey = $"CHUNK_{SerializeVector(chunkId, "_")}";
                byte[] saveData = Convert.FromBase64String(
                    PlayerPrefs.GetString(saveKey)
                  );
                _chunkIdToSaveData.Add(chunkId, saveData);
                _dirtyChunkIds.Add(chunkId);
            }
		}

		public void Save()
		{
            PlayerPrefs.DeleteAll();

			PlayerPrefs.SetInt("HAS_SAVE", 1);

            PlayerPrefs.SetString("VERSION", _version);

            string dirtyChunkIdsText = SerializeVectorList(_dirtyChunkIds.ToList());
            PlayerPrefs.SetString("DIRTY_CHUNK_IDS", dirtyChunkIdsText);

            foreach (Vector3Int chunkId in _dirtyChunkIds)
            {
                string saveKey = $"CHUNK_{SerializeVector(chunkId, "_")}";

                Chunk chunk = ChunkManager.Instance.GetChunkById(chunkId);
                if (chunk == null)
                {
                    continue; // Chunk already unloaded. Should write changes there. Based on voxel version.
                }

                Debug.Assert(chunk.IsMaterializedPolytype);

                VoxelType[] types = chunk.PolyData.Types;
                byte[] bytes = new byte[ChunkConfig.Volume];
                for (int i = 0; i < bytes.Length; ++i)
                {
                    bytes[i] = (byte)types[i];
                }
                string saveData = Convert.ToBase64String(bytes);
                PlayerPrefs.SetString(saveKey, saveData);
            }

            PlayerPrefs.Save();
        }

        private static string SerializeVectorList(List<Vector3Int> list)
        {
            StringBuilder sb = new();
            for (int i = 0; i < list.Count; ++i)
            {
                sb.Append(SerializeVector(list[i], ","));
                if (i < list.Count - 1)
                {
                    sb.Append(";");
                }
            }
            return sb.ToString();
        }

        private static List<Vector3Int> DeserializeVectorList(string text)
        {
            List<Vector3Int> list = new();
            if (string.IsNullOrEmpty(text))
            {
                return list;
            }
            string[] splits = text.Split(";");
            for (int i = 0; i < splits.Length; ++i)
            {
                Vector3Int v = DeserializeVector(splits[i], ",");
                list.Add(v);
            }
            return list;
        }

        private static string SerializeVector(Vector3Int v, string separator)
        {
            return $"{v.x}{separator}{v.y}{separator}{v.z}";
        }

        private static Vector3Int DeserializeVector(string text, string separator)
        {
            Vector3Int v = default;
            string[] splits = text.Split(separator);
            Debug.Assert(splits.Length == 3);
            for (int i = 0; i < splits.Length; ++i)
            {
                v[i] = int.Parse(splits[i]);
            }
            return v;
        }

        private readonly Dictionary<Vector3Int, byte[]> _chunkIdToSaveData = new();
        private readonly HashSet<Vector3Int> _dirtyChunkIds = new();
    }
}
