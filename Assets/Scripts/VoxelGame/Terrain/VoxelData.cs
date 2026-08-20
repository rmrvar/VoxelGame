using UnityEngine;

namespace VoxelGame.Terrain
{
	public static class VoxelData
	{
        // Maps normalAxis (0-2) and normalSign (0-1) to a face index (0-5).
        public static int GetFaceIndex(int normalAxis, int normalSign)
        {
            return normalSign * 3 + normalAxis;
        }

		public static Vector3[][] Vertices { get; } = new Vector3[6][]
		{
			// +X
			new Vector3[4] { new(1,0,0), new(1,1,0), new(1,1,1), new(1,0,1) },
			// +Y
			new Vector3[4] { new(0,1,0), new(0,1,1), new(1,1,1), new(1,1,0) },
			// +Z
			new Vector3[4] { new(0,0,1), new(1,0,1), new(1,1,1), new(0,1,1) },
			// -X
			new Vector3[4] { new(0,0,0), new(0,0,1), new(0,1,1), new(0,1,0) },
			// -Y
			new Vector3[4] { new(0,0,0), new(1,0,0), new(1,0,1), new(0,0,1) },
			// -Z
			new Vector3[4] { new(0,0,0), new(0,1,0), new(1,1,0), new(1,0,0) },
		};

		public static Vector3[][] UVs3 { get; } = new Vector3[6][]
		{
			// +X
			new Vector3[4] { new(0, 0), new(0, 1), new(1, 1), new(1, 0), },
			// +Y
			new Vector3[4] { new(0, 1), new(0, 0), new(1, 0), new(1, 1), },
			// +Z
			new Vector3[4] { new(1, 0), new(0, 0), new(0, 1), new(1, 1), },
			// -X
			new Vector3[4] { new(1, 0), new(0, 0), new(0, 1), new(1, 1), },
			// -Y
			new Vector3[4] { new(0, 0), new(1, 0), new(1, 1), new(0, 1), },
			// -Z
			new Vector3[4] { new(0, 0), new(0, 1), new(1, 1), new(1, 0), },
		};
	
        public static Vector3[][] Normals { get; } = new Vector3[6][]
		{
			// +X
			new Vector3[4] { new(+1,  0,  0), new(+1,  0,  0), new(+1,  0,  0), new(+1,  0,  0), },
			// +Y
			new Vector3[4] { new( 0, +1,  0), new( 0, +1,  0), new( 0, +1,  0), new( 0, +1,  0), },
			// +Z
			new Vector3[4] { new( 0,  0, +1), new( 0,  0, +1), new( 0,  0, +1), new( 0,  0, +1), },
			// -X
			new Vector3[4] { new(-1,  0,  0), new(-1,  0,  0), new(-1,  0,  0), new(-1,  0,  0), },
			// -Y
			new Vector3[4] { new( 0, -1,  0), new( 0, -1,  0), new( 0, -1,  0), new( 0, -1,  0), },
			// -Z
			new Vector3[4] { new( 0,  0, -1), new( 0,  0, -1), new( 0,  0, -1), new( 0,  0, -1), },
		};

		public static int[] TextureFaceOffsets { get; } = new int[6]
		{
			0,
			1,
			0,
			0,
			2,
			0
		};

		public enum VoxelType : byte
		{ 
			AIR = 0,
			DIRT,
			GRASS,
			STONE,
		}
	}
}
