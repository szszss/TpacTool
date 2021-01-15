using System;
using System.Collections.Generic;
using System.IO;

namespace TpacTool.Lib
{
	public class TextureSourceInfoData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("8ab981a7-6ba0-4908-9606-91ad341d19a9");

		public int Width { set; get; }

		public int Height { set; get; }

		public int Depth { set; get; }

		public int Array { set; get; }

		public int Mipmap { set; get; }

		public string Format { set; get; }

		public ulong SourceSize { set; get; }

		public TextureSourceInfoData() : base(TYPE_GUID)
		{
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			Width = stream.ReadInt32();
			Height = stream.ReadInt32();
			Depth = stream.ReadInt32();
			Array = stream.ReadInt32();
			Mipmap = stream.ReadInt32();
			Format = stream.ReadSizedString();
			SourceSize = stream.ReadUInt64();
		}
	}
}