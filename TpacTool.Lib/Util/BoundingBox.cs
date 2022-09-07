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

		public float Width => Max.X - Min.X;

		public float Depth => Max.Y - Min.Y;

		public float Height => Max.Z - Min.Z;

		public BoundingBox()
		{
			Min = new Vector3(0, 0, 0);
			Max = new Vector3(0, 0, 0);
			Center = new Vector3(0, 0, 0);
			BoundingSphereRadius = 0;
		}

		public BoundingBox(Vector3 min, Vector3 max)
		{
			Min = Vector3.Min(min, max);
			Max = Vector3.Max(min, max);
			Recompute();
		}

		public BoundingBox(BinaryReader stream)
		{
			Min = stream.ReadVec4AsVec3();
			Max = stream.ReadVec4AsVec3();
			Center = stream.ReadVec4AsVec3();
			BoundingSphereRadius = stream.ReadSingle();
		}

		public void Recompute()
		{
			Center = (Min + Max) / 2;
			BoundingSphereRadius = Vector3.Distance(Max, Center);
		}

		public void Write(BinaryWriter stream)
		{
			stream.WriteVec3AsVec4(Min, 1f);
			stream.WriteVec3AsVec4(Max, 1f);
			stream.WriteVec3AsVec4(Center, 1f);
			stream.Write(BoundingSphereRadius);
		}

		public static BoundingBox Merge(BoundingBox b1, BoundingBox b2)
		{
			return new BoundingBox(Vector3.Min(b1.Min, b2.Min), Vector3.Max(b1.Max, b2.Max));
		}
	}
}