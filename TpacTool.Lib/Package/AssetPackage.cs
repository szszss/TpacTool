using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class AssetPackage
	{
		private Guid _guid;

		public Guid Guid
		{
			set
			{
				if (IsGuidLocked)
					throw new Exception("The guid of this Asset Package is already locked");
				_guid = value;
			}
			get => _guid;
		}

		public bool IsGuidLocked { internal set; get; }

		public bool HeaderLoaded { private set; get; }

		public bool DataLoaded { private set; get; }

		[CanBeNull] public FileInfo File { private set; get; }

		public bool Invalid => false;

		public List<AssetItem> Items { private set; get; }

		public AssetPackage()
		{
			this.Guid = Guid.Empty;
			this.File = null;
			HeaderLoaded = true;
			DataLoaded = true;
			Items = new List<AssetItem>();
		}

		public AssetPackage(Guid guid) : this()
		{
			this.Guid = guid;
		}

		public AssetPackage([NotNull] BinaryReader reader)
		{
			this.File = null;
			Items = new List<AssetItem>();
			Load(reader, true);
		}

		public AssetPackage([NotNull] byte[] data)
		{
			this.File = null;
			Items = new List<AssetItem>();
			using (var reader = data.CreateBinaryReader())
			{
				Load(reader, true);
			}
		}

		public AssetPackage([NotNull] string filePath, bool loadHeaderNow = true, bool loadDataNow = false)
		{
			//if (!System.IO.File.Exists(filePath))
			//	throw new FileNotFoundException("Tpac file is not found:", filePath);
			File = new FileInfo(filePath);
			HeaderLoaded = false;
			DataLoaded = false;
			Items = new List<AssetItem>();
			if (loadHeaderNow)
			{
				Load(loadDataNow);
			}
		}

		public void Load(bool loadDataIntoMemory = false)
		{
			if (File == null)
				throw new InvalidOperationException("This tpac is not created from file");
			File.CheckFileExist();
			if (HeaderLoaded && DataLoaded)
				throw new Exception("This tpac is already loaded");
			if (!HeaderLoaded)
			{
				using (var stream = File.OpenBinaryReader())
				{
					Load(stream, loadDataIntoMemory);
				}
			}
			else
			{
				// TODO:
			}
		}

		protected virtual void Load(BinaryReader stream, bool loadDataIntoMemory)
		{
			if (loadDataIntoMemory && !stream.BaseStream.CanSeek)
				throw new IOException("When load a whole Tpac file into memory, " +
									"the base stream must support random access (seek).");
			HeaderLoaded = true;

			uint version;
			if (stream.ReadUInt32() != 0x43415054)
				throw new IOException("Not a Tpac file: " + File.FullName);
			if ((version = stream.ReadUInt32()) != 1)
				throw new Exception("Unsupported Tpac version: " + version);
			this.Guid = stream.ReadGuid();

			uint resourceNum = stream.ReadUInt32();
			uint dataOffset = stream.ReadUInt32();
			stream.ReadUInt32(); // skip reserve

			for (int i = 0; i < resourceNum; i++)
			{
				var typeGuid = stream.ReadGuid();
				var resGuid = stream.ReadGuid();
				var resName = stream.ReadSizedString();
				TypedAssetFactory.CreateTypedAsset(typeGuid, out var assetItem);
				assetItem.Guid = resGuid;
				assetItem.Name = resName;

				var metadataSize = stream.ReadUInt64();
				stream.RecordPosition();
				assetItem.ReadMetadata(stream, (int)metadataSize);
				stream.AssertLength((long)metadataSize);
				var unknownMetadataChecknum = stream.ReadInt64();

				var dataSegmentNum = stream.ReadInt32();
				var segments = new AbstractExternalLoader[dataSegmentNum];
				for (int j = 0; j < dataSegmentNum; j++)
				{
					var segOffset = stream.ReadUInt64();
					var segActualSize = stream.ReadUInt64();
					var segStorageSize = stream.ReadUInt64();
					var segGuid = stream.ReadGuid();
					var segTypeGuid = stream.ReadGuid();
					TypedDataFactory.CreateTypedLoader(segTypeGuid, File, out var result);
					var segment = result;
					segment._offset = segOffset;
					segment._actualSize = segActualSize;
					segment._storageSize = segStorageSize;
					segment.OwnerGuid = segGuid;
					segment._unknownUlong = stream.ReadUInt64();
					segment._unknownUint = stream.ReadUInt32();
					segment._storageFormat = (AbstractExternalLoader.StorageFormat) stream.ReadByte();
					segments[j] = segment;
				}
				assetItem.ConsumeDataSegments(segments);
				if (loadDataIntoMemory)
				{
					foreach (var segment in segments)
					{
						segment.ForceLoad(); // TODO: use current stream
						segment.MarkLongLive();
					}
				}

				var depNum = stream.ReadInt32();
				for (int j = 0; j < depNum; j++)
				{
					var guid1 = stream.ReadGuid(); // asset item (metamesh)
					var guid2 = stream.ReadGuid(); // mesh
					var guid3 = stream.ReadGuid();
				}

				Items.Add(assetItem);
			}

			if (loadDataIntoMemory)
			{
				DataLoaded = true;
			}
		}
	}
}