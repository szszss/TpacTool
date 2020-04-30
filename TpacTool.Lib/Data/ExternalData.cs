using System;
using System.Collections.Generic;
using System.IO;

namespace TpacTool.Lib
{
	public class ExternalData
	{
		public Guid TypeGuid { internal protected set; get; }

		private byte[] _unknownRawData;

		public ExternalData()
		{
			this.TypeGuid = Guid.Empty;
		}

		public ExternalData(Guid typeGuid)
		{
			this.TypeGuid = typeGuid;
		}

		public virtual void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			_unknownRawData = stream.ReadBytes(totalSize);
		}

		public virtual byte[] SaveData()
		{
			throw new NotImplementedException();
		}
	}
}