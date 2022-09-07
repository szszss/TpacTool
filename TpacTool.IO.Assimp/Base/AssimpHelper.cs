using System.Numerics;
using Assimp;
using TpacTool.Lib;
using Matrix4x4 = Assimp.Matrix4x4;
using Quaternion = Assimp.Quaternion;

namespace TpacTool.IO.Assimp
{
	static class AssimpHelper
	{
		public static Vector3D ToAssimpVec(this Vector4 vec4)
		{
			return new Vector3D(vec4.X, vec4.Y, vec4.Z);
		}

		public static Vector3D ToAssimpVec(this Vector3 vec3)
		{
			return new Vector3D(vec3.X, vec3.Y, vec3.Z);
		}

		public static Vector3D ToAssimpVec3(this Vector2 vec2)
		{
			return new Vector3D(vec2.X, vec2.Y, 0);
		}

		public static Color4D ToAssimpColor(this AbstractMeshData.Color color)
		{
			return new Color4D(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
		}

		public static Matrix4x4 ToAssimpMatrix(this System.Numerics.Matrix4x4 matrix)
		{
			matrix = System.Numerics.Matrix4x4.Transpose(matrix);
			return new Matrix4x4(matrix.M11, matrix.M12, matrix.M13, matrix.M14,
				matrix.M21, matrix.M22, matrix.M23, matrix.M24,
				matrix.M31, matrix.M32, matrix.M33, matrix.M34,
				matrix.M41, matrix.M42, matrix.M43, matrix.M44);
		}

		public static Quaternion ToAssimpQuaternion(this System.Numerics.Quaternion quaternion)
		{
			return new Quaternion(quaternion.W, quaternion.X, quaternion.Y, quaternion.Z);
		}

		/*public static Bone ToAssimpBone(this MeshEditData.Bone bone)
		{
			return new Bone()
		}*/
	}
}