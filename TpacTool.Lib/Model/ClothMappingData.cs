using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace TpacTool.Lib
{
	public class ClothMappingData : AbstractMeshData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("af41613e-bd4d-46ec-8284-debb56c042dc");

		public ClothMappingData() : base(TYPE_GUID)
		{
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			/*var indices = ReadStructArray<ushort>(stream);
			int max = indices.Max();
			var num = stream.ReadInt32();
			var pos = ReadStructArray<Vector3>(stream, num / 3); // TODO:*/
			base.ReadData(stream, userdata, totalSize);
		}
	}
}