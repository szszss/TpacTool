using System.Numerics;

namespace TpacTool.Lib
{
	public struct Transform
	{
		public Vector4 V1;

		public Vector4 V2;

		public Transform(Vector4 v1, Vector4 v2)
		{
			V1 = v1;
			V2 = v2;
		}
	}
}