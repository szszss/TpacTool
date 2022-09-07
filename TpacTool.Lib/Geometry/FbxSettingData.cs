using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class FbxSettingData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("e97d3df8-4257-40db-9f42-1436e339d581");

		internal protected int version = 2;

		public bool UnknownBool1 { set; get; }

		[Obsolete]
		public bool UnknownBool2 { set; get; }

		public string BlendShapeImport { set; get; }

		public string ConvertToUnit { set; get; }

		[NotNull]
		public GeometrySettingData GeometrySetting { set; get; }

		public FbxSettingData() : base(TYPE_GUID)
		{
			GeometrySetting = new GeometrySettingData();
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			/*UnknownBool1 = stream.ReadBoolean();
			if (version >= 2)
				UnknownBool2 = stream.ReadBoolean();*/
			BlendShapeImport = stream.ReadSizedString();
			UnknownBool1 = stream.ReadBoolean();
			ConvertToUnit = stream.ReadSizedString();
			GeometrySetting.version = stream.ReadInt32();
			GeometrySetting.ReadData(stream, userdata, totalSize);
		}
	}
}