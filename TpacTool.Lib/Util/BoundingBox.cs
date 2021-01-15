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

		public void Write(BinaryWriter stream)
		{
			stream.WriteVec3AsVec4(Min, 1f);
			stream.WriteVec3AsVec4(Max, 1f);
			stream.WriteVec3AsVec4(Center, 1f);
			stream.Write(BoundingSphereRadius);
		}
	}
}