using System;
using System.IO;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class ProceduralVectorField : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("cc373411-785f-4ef0-95a2-d5cf54b24536");

		[NotNull]
		public string Method { set; get; }

		public int UnknownInt1 { set; get; }

		public int UnknownInt2 { set; get; }

		public int UnknownInt3 { set; get; }

		public float UnknownFloat { set; get; }

		public int UnknownInt4 { set; get; }

		public int UnknownInt5 { set; get; }

		public int UnknownInt6 { set; get; }

		public int UnknownInt7 { set; get; }

		public int UnknownInt8 { set; get; }

		public ProceduralVectorField() : base(TYPE_GUID)
		{
			Method = String.Empty;
		}

		public override void ReadMetadata(BinaryReader stream, int totalSize)
		{
			var version = stream.ReadUInt32();
			Method = stream.ReadSizedString();
			UnknownInt1 = stream.ReadInt32();
			UnknownInt2 = stream.ReadInt32();
			UnknownInt3 = stream.ReadInt32();
			if (Method.Length > 0)
			{
				UnknownFloat = stream.ReadSingle();
				UnknownInt4 = stream.ReadInt32();
				UnknownInt5 = stream.ReadInt32();
				UnknownInt6 = stream.ReadInt32();
				UnknownInt7 = stream.ReadInt32();
				UnknownInt8 = stream.ReadInt32();
			}
		}
	}
}