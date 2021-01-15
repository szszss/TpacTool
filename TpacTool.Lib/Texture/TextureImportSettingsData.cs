using System;
using System.Collections.Generic;
using System.IO;

namespace TpacTool.Lib
{
	public class TextureImportSettingsData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("7c475be2-7c8d-431d-b0e1-53218d047fb9");

		public string UnknownString { set; get; }

		public byte[] UnknownBytes { set; get; }

		public TextureImportSettingsData() : base(TYPE_GUID)
		{
			UnknownString = String.Empty;
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			UnknownString = stream.ReadSizedString();
			UnknownBytes = stream.ReadBytes(totalSize - 4);
		}
	}
}