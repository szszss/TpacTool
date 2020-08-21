using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class AssetItem : IDependence
	{
		private Guid _resourceGuid;

		public Guid Type { private set; get; }

		public bool Removed { private set; get; }

		public List<AbstractExternalLoader> TypelessDataSegments { private set; get; }

		public uint Version { protected internal set; get; }

		public Guid Guid
		{
			internal protected set => _resourceGuid = value;
			get
			{
				if (_resourceGuid == Guid.Empty)
					_resourceGuid = Guid.NewGuid();
				return _resourceGuid;
			}
		}

		[NotNull]
		public string Name { set; get; }

		public bool Invalid { internal set; get; }

		public AssetItem(Guid type)
		{
			this.Type = type;
			this.Guid = Guid.Empty;
			this.Name = String.Empty;
			this.TypelessDataSegments = new List<AbstractExternalLoader>(0);
			Removed = false;
		}

		private byte[] _temp_metadata;
		public virtual void ReadMetadata([NotNull] BinaryReader stream, int totalSize)
		{
			// skip metadata without processing
			_temp_metadata = stream.ReadBytes(totalSize);
		}

		public virtual void ConsumeDataSegments(AbstractExternalLoader[] externalData)
		{
			TypelessDataSegments.Capacity = externalData.Length;
			TypelessDataSegments.AddRange(externalData);
		}
	}
}