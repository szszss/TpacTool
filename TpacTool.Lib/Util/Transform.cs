using System;
using System.Numerics;

namespace TpacTool.Lib
{
	[Obsolete]
	public struct Transform
	{
		public Quaternion Rotation;

		public Vector4 Position;

		public Transform(Quaternion v1, Vector4 v2)
		{
			Rotation = v1;
			Position = v2;
		}
	}
}