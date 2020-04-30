using System.IO;
using System.Numerics;

namespace TpacTool.Lib
{
	public sealed class BoundingBox
	{
		public Vector3 Min { get; set; }
		public Vector3 Max { get; set; }
		public Vector3 Center { get; set; }
		public float BoundingSphereRadius { get; set; }

		public BoundingBox()
		{
			Min = new Vector3(0, 0, 0);
			Max = new Vector3(0, 0, 0);
			Center = new Vector3(0, 0, 0);
			BoundingSphereRadius = 0;
		}

		public BoundingBox(BinaryReader stream)
		{
			Min = stream.ReadVec4AsVec3();
			Max = stream.ReadVec4AsVec3();
			Center = stream.ReadVec4AsVec3();
			BoundingSphereRadius = stream.ReadSingle();
		}
	}
}