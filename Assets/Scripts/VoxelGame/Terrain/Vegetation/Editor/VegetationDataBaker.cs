using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VoxelGame.Terrain.Vegetation.Editor
{
    public static class VegetationDataBaker
    {
        private const string InputPath = "Assets/Terrain/Vegetation/Authoring";
        private const string OutputPath = "Assets/Terrain/Vegetation/Data";

        [MenuItem("Terrain/Bake Vegetation")]
        private static void Bake()
        {
            Debug.Log("Started vegetation baking.");

            EnsureOutputDirectoryExists();

            string[] guids = AssetDatabase.FindAssets(
                "t:Prefab",
                new[] { InputPath }
              );

            List<VegetationDataAuthoringRoot> authoringRoots = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogError(
                        $"Failed to load prefab at '{path}'."
                      );
                    continue;
                }

                VegetationDataAuthoringRoot authoringRoot =
                    prefab.GetComponent<VegetationDataAuthoringRoot>();

                if (authoringRoot == null)
                {
                    Debug.LogWarning(
                        $"Skipping '{path}': " +
                        $"missing {nameof(VegetationDataAuthoringRoot)}."
                      );
                    continue;
                }

                authoringRoots.Add(authoringRoot);
            }

            Debug.Log(
                $"Collected {authoringRoots.Count} " +
                $"{nameof(VegetationDataAuthoringRoot)}s."
              );

            Dictionary<VegetationDataAuthoringRoot, VegetationData> bakedData = new();
            Dictionary<Vector3Int, VoxelType> positionToVoxelType = new();

            foreach (VegetationDataAuthoringRoot authoringRoot in authoringRoots)
            {
                Debug.Log($"Processing '{authoringRoot.name}'.");

                if (!TryBake(
                        authoringRoot,
                        positionToVoxelType,
                        out VegetationData vegetationData))
                {
                    Debug.LogError(
                        $"Failed to bake '{authoringRoot.name}'."
                      );

                    return;
                }

                bakedData.Add(authoringRoot, vegetationData);
            }

            foreach (KeyValuePair<VegetationDataAuthoringRoot, VegetationData> pair in bakedData)
            {
                SaveVegetationData(pair.Key, pair.Value);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Vegetation baking finished. " +
                $"Baked {bakedData.Count} assets."
              );
        }

        private static bool TryBake(
            VegetationDataAuthoringRoot authoringRoot,
            Dictionary<Vector3Int, VoxelType> positionToVoxelType,
            out VegetationData vegetationData)
        {
            vegetationData = null;
            positionToVoxelType.Clear();

            VegetationDataAuthoringNode[] authoringNodes =
                authoringRoot.GetComponentsInChildren<VegetationDataAuthoringNode>(
                    true
                  );

            foreach (VegetationDataAuthoringNode authoringNode in authoringNodes)
            {
                Vector3Int position = authoringNode.LocalPosition;
                VoxelType type = authoringNode.Type;

                if (positionToVoxelType.TryGetValue(position, out VoxelType existingType))
                {
                    if (existingType != type)
                    {
                        Debug.LogError(
                            $"Conflicting voxel types in '{authoringRoot.name}' " +
                            $"at position {position}: " +
                            $"{existingType} vs {type}."
                          );

                        return false;
                    }

                    // Same position and same type - nothing else to do.
                    continue;
                }

                positionToVoxelType.Add(position, type);
            }

            if (authoringRoot.Radius <= 0)
            {
                Debug.LogError(
                    $"Invalid radius on '{authoringRoot.name}': " +
                    $"{authoringRoot.Radius}."
                  );

                return false;
            }

            if (authoringRoot.Height <= 0)
            {
                Debug.LogError(
                    $"Invalid height on '{authoringRoot.name}': " +
                    $"{authoringRoot.Height}."
                  );

                return false;
            }

            int side = 2 * (authoringRoot.Radius - 1) + 1;
            int voxelCount = side * side * authoringRoot.Height;

            VoxelType[] types = new VoxelType[voxelCount];

            VegetationData.ForEach(
                authoringRoot.Radius,
                authoringRoot.Height,
                (int index, int x, int y, int z) =>
                {
                    Vector3Int position = new(x, y, z);

                    // Missing positions remain Air.
                    if (positionToVoxelType.TryGetValue(
                            position,
                            out VoxelType type))
                    {
                        types[index] = type;
                    }
                    else
                    {
                        types[index] = VoxelType.AIR;
                    }
                }
              );

            vegetationData = new VegetationData(
                authoringRoot.Radius,
                authoringRoot.Height,
                types
              );

            return true;
        }

        private static void SaveVegetationData(
            VegetationDataAuthoringRoot authoringRoot,
            VegetationData vegetationData)
        {
            string outputPath =
                $"{OutputPath}/{authoringRoot.name}.asset";

            VegetationDataSO vegetationDataSO =
                AssetDatabase.LoadAssetAtPath<VegetationDataSO>(outputPath);

            if (vegetationDataSO != null)
            {
                vegetationDataSO.SetData(vegetationData);
                EditorUtility.SetDirty(vegetationDataSO);
            }
            else
            {
                vegetationDataSO =
                    ScriptableObject.CreateInstance<VegetationDataSO>();

                vegetationDataSO.SetData(vegetationData);

                AssetDatabase.CreateAsset(
                    vegetationDataSO,
                    outputPath
                );
            }
        }

        private static void EnsureOutputDirectoryExists()
        {
            if (AssetDatabase.IsValidFolder(OutputPath))
            {
                return;
            }

            string parent = "Assets/Terrain/Vegetation";
            string folderName = "Data";

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}