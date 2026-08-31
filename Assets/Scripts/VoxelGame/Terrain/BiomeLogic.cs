using UnityEngine;

namespace VoxelGame.Terrain
{ 
	public static class BiomeLogic
    {
        public static VoxelType GetVoxelType(int x, int y, int z, int h)
        {
            int dirtDepth = Mathf.FloorToInt(
                EvaluatePerlin(x, z, 0.1F, 0.1F, 300000, 300000, 2, 4)
              );
            int depth = h - y;
            if (depth < 0)
            {
                return VoxelType.AIR;
            }
            if (depth == 0)
            {
                return VoxelType.GRASS;
            }
            if (depth < dirtDepth)
            {
                return VoxelType.DIRT;
            }
            else
            {
                return VoxelType.STONE;
            }
        }

        // Super important that this is smooth. Also, ideally [0, 1] is equally common.
        public static float GetSlider(int x, int z)
        {
            float p1 = EvaluatePerlin(x, z, 0.007F, 0.007F, 1_000, 1_000, 0, 1);
            float p2 = EvaluatePerlin(x, z, 0.006F, 0.008F, 2_000, 2_000, 0, 1);
            float p3 = EvaluatePerlin(x, z, 0.010F, 0.002F, 3_000, 3_000, 0, 1);
            float p4 = EvaluatePerlin(x, z, 0.015F, 0.010F, 4_000, 4_000, 0, 1);
            return (Mathf.Min(p1, p3) + Mathf.Max(p2, p4)) * 0.5F;
        }

        public static bool TryGetMinTreeDistance(int x, int z, float t, out int minTreeDistance)
        {
            if (t > 0.8)
            {
                minTreeDistance = 0;
                return false;
            }

            // Tree biome
            float p1 = EvaluatePerlin(x, z, 0.005F, 0.003F, 5_000, 5_000, 0, 1);
            float p2 = EvaluatePerlin(x, z, 0.002F, 0.007F, 6_000, 6_000, 0, 1);
            float p = (p1 + p2) * 0.5F;

            if (p < 0.5F)
            {
                minTreeDistance = 0;
                return false; // Biome decided there is no tree here.
            }

            float t2 = 1 - (p - 0.5F);
            float combinedT = Mathf.Min(t, t2);

            // Remap combinedT.
            float newT = Mathf.Clamp01(Mathf.InverseLerp(0.1F, 0.4F, combinedT));

            minTreeDistance = Mathf.FloorToInt(Mathf.Lerp(5, 10, newT));
            return true;
        }

        public static int GetHeight(int x, int z, float t)
        {
            Debug.Assert(t is >= 0 and <= 1);

            //return Mathf.FloorToInt(t * 100);

            float height1 = 0;
            float height2 = 0;
            float heightStrength = -1;

            if (t > _PLATEAU_T + _PLATEAU_R)
            {
                height1 = GetPlateauHeight(x, z);
            } else 
            if (t > _PLATEAU_T - _PLATEAU_R)
            {
                height1 = GetHiHillsHeight(x, z);
                height2 = GetPlateauHeight(x, z);
                heightStrength = EvaluateLine(t, _PLATEAU_T, _PLATEAU_R);
            } else 
            if (t > _HI_HILLS_T + _HI_HILLS_R)
            {
                height1 = GetHiHillsHeight(x, z);
            } else
            if (t > _HI_HILLS_T - _HI_HILLS_R)
            {
                height1 = GetFlatlandsHeight(x, z);
                height2 = GetHiHillsHeight(x, z);
                heightStrength = EvaluateLine(t, _HI_HILLS_T, _HI_HILLS_R);
            } else
            if (t > _FLATLANDS_T + _FLATLANDS_R)
            {
                height1 = GetFlatlandsHeight(x, z);
            } else
            if (t > _FLATLANDS_T - _FLATLANDS_R)
            {
                height1 = GetLoHillsHeight(x, z);
                height2 = GetFlatlandsHeight(x, z);
                heightStrength = EvaluateLine(t, _FLATLANDS_T, _FLATLANDS_R);
            }
            else
            {
                height1 = GetLoHillsHeight(x, z);
            }

            if (heightStrength < 0)
            {
                return Mathf.RoundToInt(height1);
            }
            else
            {
                return Mathf.RoundToInt(Mathf.Lerp(height1, height2, heightStrength));
            }
        }

        private static float GetFlatlandsHeight(int x, int z)
        {
            float p1 = EvaluatePerlin(x, z, 0.01F, 0.02F, 0, 0, 0, 4);
            float p2 = EvaluatePerlin(x, z, 0.01F, 0.01F, 1000, 1000, 0, 4);

            return (p1 + p2) * 0.5F;
        }

        private static float GetLoHillsHeight(int x, int z)
        {
            float p1 = EvaluatePerlin(x, z, 0.03F, 0.01F, 50000, 50000, 0, 15);
            float p2 = EvaluatePerlin(x, z, 0.05F, 0.03F, 51000, 51000, 0, 15);

            return (p1 + p2) * 0.5F;
        }

        private static float GetHiHillsHeight(int x, int z)
        {
            float p1 = EvaluatePerlin(x, z, 0.04F, 0.01F, 100000, 100000, 15, 40);
            float p2 = EvaluatePerlin(x, z, 0.03F, 0.04F, 101000, 101000, 1, 15);

            return p1 + p2;
        }

        private static float GetPlateauHeight(int x, int z)
        {
            float p1 = EvaluatePerlin(x, z, 0.01F, 0.03F, 150000, 150000, 40, 60);
            float p2 = EvaluatePerlin(x, z, 0.07F, 0.05F, 151000, 153000, 1, 10);

            return p1 + p2;
        }

        private static float EvaluatePerlin(
            int x, 
            int z, 
            float xF, 
            float zF,
            float xOffset,
            float zOffset,
            float minY,
            float maxY
          )
        {
            // Perlin noise is symmetric around the origin, so offset the sample space.
            xOffset += _PERLIN_OFFSET;
            zOffset += _PERLIN_OFFSET;
            return Mathf.Lerp(minY, maxY, Mathf.PerlinNoise(x * xF + xOffset, z * zF + zOffset));
        }

        // Evaluates a sigmoid centered on c where 99% of y occurs within c +/- r.
        private static float EvaluateSigmoid(float x, float c, float r)
        {
            float dx = x - c;
            return 1 / (1 + Mathf.Exp(-_SIGMOID99_K * dx / r));
        }

        // Evaluates a line centered on c where +/-1 occurs on c +/- r.
        private static float EvaluateLine(float x, float c, float r)
        {
            float dx = x - c;
            return Mathf.Clamp(dx / r, -1, +1) * 0.5F + 0.5F;
        }

        private const float _FLATLANDS_T = 0.2F;
        private const float _HI_HILLS_T = 0.6F;
        private const float _PLATEAU_T = 0.8F;

        private const float _FLATLANDS_R = 0.05F;
        private const float _HI_HILLS_R = 0.15F;
        private const float _PLATEAU_R = 0.05F;

        private const float _PERLIN_OFFSET = 10000.11F;

        private const float _SIGMOID99_K = 5.293305F;
    }
}
