using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class GeometrySettingData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("3e67a08f-c480-4e33-99e3-4c849aa20307");

		internal protected int version = 3;

		public bool UnknownBool1 { set; get; }

		public bool UnknownBool2 { set; get; }

		public bool UnknownBool3 { set; get; }

		public bool UnknownBool4 { set; get; }

		public bool UnknownBool5 { set; get; }

		public bool UnknownBool6 { set; get; }

		public List<GeometrySettingItem> UnknownItems1 { private set; get; }

		public List<Tuple<string, bool>> UnknownItems2 { private set; get; }

		public GeometrySettingData() : base(TYPE_GUID)
		{
			UnknownItems1 = new List<GeometrySettingItem>();
			UnknownItems2 = new List<Tuple<string, bool>>();
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			UnknownBool1 = stream.ReadBoolean();
			UnknownBool2 = stream.ReadBoolean();
			UnknownBool3 = stream.ReadBoolean();
			UnknownBool4 = stream.ReadBoolean();
			UnknownBool5 = stream.ReadBoolean();
			UnknownBool6 = stream.ReadBoolean();

			int num = stream.ReadInt32();
			UnknownItems1.Capacity = num;
			for (int i = 0; i < num; i++)
			{
				UnknownItems1.Add(new GeometrySettingItem(stream, version));
			}

			num = stream.ReadInt32();
			UnknownItems2.Capacity = num;
			for (int i = 0; i < num; i++)
			{
				UnknownItems2.Add(Tuple.Create(stream.ReadSizedString(), stream.ReadBoolean()));
			}
		}

		public class GeometrySettingItem
		{
			[NotNull]
			public string UnknownString1 { set; get; }

			public bool UnknownBool1 { set; get; }

			public bool UnknownBool2 { set; get; }

			public bool UnknownBool3 { set; get; }

			public bool UnknownBool4 { set; get; }

			public bool UnknownBool5 { set; get; }

			public bool UnknownBool6 { set; get; }

			[NotNull]
			public string UnknownString2 { set; get; }

			public Vector4 UnknownVec { set; get; }

			public GeometrySettingItem()
			{
				UnknownString1 = String.Empty;
				UnknownString2 = String.Empty;
			}

			public GeometrySettingItem(BinaryReader stream, int version = 3)
			{
				UnknownString1 = stream.ReadSizedString();
				UnknownBool1 = stream.ReadBoolean();
				UnknownBool2 = stream.ReadBoolean();
				UnknownBool3 = stream.ReadBoolean();
				UnknownBool4 = stream.ReadBoolean();
				UnknownBool5 = stream.ReadBoolean();

				if (version >= 1)
					UnknownBool6 = stream.ReadBoolean();

				if (version >= 2)
					UnknownString2 = stream.ReadSizedString();
				else
					UnknownString2 = String.Empty;

				if (version >= 1)
					UnknownVec = stream.ReadVec4();
			}
		}
	}
}