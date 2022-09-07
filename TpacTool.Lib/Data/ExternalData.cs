using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

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

		public virtual void WriteData(BinaryWriter stream, IDictionary<object, object> userdata)
		{
			stream.Write(_unknownRawData);
		}

		public virtual ExternalData Clone()
		{
			throw new NotImplementedException();
		}

		protected void CloneDo(ExternalData parent)
		{
			this.TypeGuid = parent.TypeGuid;
			this._unknownRawData = parent._unknownRawData;
		}

		protected bool HasUserdata(IDictionary<object, object> userdata, [NotNull] object key, [CanBeNull] object value)
		{
			if (userdata == null)
				return false;
			if (userdata.TryGetValue(key, out object result))
			{
				if (value == null)
					return result == null;
				return value.Equals(result);
			}

			return false;
		}

		protected bool HasUserdataTag(IDictionary<object, object> userdata, [NotNull] object key)
		{
			return HasUserdata(userdata, key, true);
		}
	}
}