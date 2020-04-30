using System;
using System.Collections.Generic;
using System.IO;

namespace TpacTool.Lib
{
	public class EditmodeMiscData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("644030f6-8a42-4c86-b935-9b9daa9391c2");

		public List<Tuple<string, string>> UnknownStringPairs { private set; get; }

		public EditmodeMiscData() : base(TYPE_GUID)
		{
			UnknownStringPairs = new List<Tuple<string, string>>();
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			int num = stream.ReadInt32();
			UnknownStringPairs.Clear();
			UnknownStringPairs.Capacity = num;
			for (int i = 0; i < num; i++)
			{
				var s1 = stream.ReadSizedString();
				var s2 = stream.ReadSizedString();
				UnknownStringPairs.Add(Tuple.Create(s1, s2));
			}
		}
	}
}