using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class AssetPackage
	{
		public const uint TPAC_MAGIC_NUMBER = 0x43415054;

		private const int TPAC_LATEST_VERSION = 2;

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

		private static HashSet<string> tempStrs = new HashSet<string>();

		protected virtual void Load(BinaryReader stream, bool loadDataIntoMemory)
		{
			if (loadDataIntoMemory && !stream.BaseStream.CanSeek)
				throw new IOException("When load a whole Tpac file into memory, " +
									"the base stream must support random access (seek).");
			HeaderLoaded = true;

			if (stream.ReadUInt32() != TPAC_MAGIC_NUMBER)
				throw new IOException("Not a Tpac file: " + File.FullName);
			uint version = stream.ReadUInt32();
			switch (version)
			{
				case 1: // 1.0.0~1.4.2
				case 2: // since 1.4.3
					break;
				default:
					throw new Exception("Unsupported Tpac version: " + version);
			}
			this.Guid = stream.ReadGuid();

			uint resourceNum = stream.ReadUInt32();
			uint dataOffset = stream.ReadUInt32();
			stream.ReadUInt32(); // skip reserve

			for (int i = 0; i < resourceNum; i++)
			{
				var typeGuid = stream.ReadGuid();
				TypedAssetFactory.CreateTypedAsset(typeGuid, out var assetItem);
				assetItem.Guid = stream.ReadGuid();

				uint assetVersion = 0;
				if (version > 1)
					assetVersion = stream.ReadUInt32();
				assetItem.Version = assetVersion;
				assetItem.Name = stream.ReadSizedString();

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
				assetItem.UnknownDependences.Capacity = depNum;
				for (int j = 0; j < depNum; j++)
				{
					var guid1 = stream.ReadGuid(); // asset item (metamesh)
					var guid2 = stream.ReadGuid(); // mesh
					var guid3 = stream.ReadGuid();
					var deps = new AssetItem.UnknownDependence()
					{
						UnknownGuid1 = guid1,
						UnknownGuid2 = guid2,
						UnknownGuid3 = guid3
					};
					assetItem.UnknownDependences.Add(deps);
				}

				Items.Add(assetItem);
			}

			if (loadDataIntoMemory)
			{
				DataLoaded = true;
			}
		}

		// not finished yet
		public void Save([CanBeNull] string newFilePath = null /*, bool resetFilePtrAfterSave = true*/, 
			int tpacVersion = TPAC_LATEST_VERSION)
		{
			if (File == null && newFilePath == null)
				throw new ArgumentNullException();
			if (File == null && string.IsNullOrWhiteSpace(newFilePath))
				throw new ArgumentException();

			var saveTargetPath = string.IsNullOrWhiteSpace(newFilePath) ? File.FullName : newFilePath;
			var saveTargetFi = new FileInfo(saveTargetPath + ".tmp");
			Directory.CreateDirectory(saveTargetFi.DirectoryName);

			using (var stream = new BinaryWriter(saveTargetFi.OpenWrite()))
			{
				Save(stream, tpacVersion);
			}

#if NETSTANDARD1_3
			if (System.IO.File.Exists(saveTargetPath))
				System.IO.File.Delete(saveTargetPath);
			saveTargetFi.MoveTo(saveTargetPath);
#else
			if (System.IO.File.Exists(saveTargetPath))
				saveTargetFi.Replace(saveTargetPath, null);
			else
				saveTargetFi.MoveTo(saveTargetPath);
#endif
		}

		public virtual void Save(BinaryWriter stream, int tpacVersion = TPAC_LATEST_VERSION)
		{
			var shouldExportAssetVersion = tpacVersion > 1;
			var sizeOfVersion = shouldExportAssetVersion ? sizeof(uint) : 0;

			stream.Write(TPAC_MAGIC_NUMBER);
			stream.Write(tpacVersion);
			stream.Write(Guid);
			stream.Write(Items.Count);

			ulong totalSize = sizeof(uint) // magic number
							+ sizeof(int) // version
							+ 16 // guid
							+ sizeof(uint) // res num
							+ sizeof(uint) // data segment offset
							+ sizeof(int) // reserve
				;
			var metadataQueue = new Queue<byte[]>();
			var segmentQueue = new Queue<Segment>();
			
			for (int i = 0; i < Items.Count; i++)
			{
				var asset = Items[i];
				var metadata = asset.WriteMetadata();
				metadataQueue.Enqueue(metadata);
				var assetSize = 16 // res type guid
							+ 16 // res guid
							+ sizeOfVersion // version
							+ Utils.GetStringSize(asset.Name, true) // name
							+ sizeof(ulong) // length of metadata
							+ metadata.Length // metadata
							+ sizeof(long) // unknown checksum
							+ sizeof(uint) // segment num
							+ asset.TypelessDataSegments.Count * (
								sizeof(ulong) // offset
								+ sizeof(ulong) // actual size
								+ sizeof(ulong) // storage size
								+ 16 // seg guid
								+ 16 // seg type guid
								+ sizeof(ulong) // unknown ulong
								+ sizeof(uint) // unknown uint
								+ sizeof(byte) // storage format
							)
							+ sizeof(uint) // dep num
							+ asset.UnknownDependences.Count * (
								16 * 3	// dep size
							)
					; // TODO: we don't export the dep for now
				totalSize += (ulong) assetSize;


				foreach (var dataSegment in asset.TypelessDataSegments)
				{
					var segment = new Segment();
					segment.Data = dataSegment.SaveTo(
						out segment.ActualSize, 
						out segment.StorageSize,
						out segment.Format);
					segment.Info = dataSegment;
					segmentQueue.Enqueue(segment);
				}
			}

			var padding = totalSize % 8;
			if (padding != 0)
				padding = 8 - padding;
			padding = 0; // disable padding for now
			totalSize += padding;

			stream.Write((uint) totalSize - 36);
			stream.Write((int) 0);

			for (int i = 0; i < Items.Count; i++)
			{
				var asset = Items[i];
				stream.Write(asset.Type);
				stream.Write(asset.Guid);
				if (shouldExportAssetVersion)
					stream.Write(asset.Version);
				stream.WriteSizedString(asset.Name);

				var metadata = metadataQueue.Dequeue();
				stream.Write((ulong) metadata.Length);
				stream.Write(metadata);
				stream.Write((ulong) 0); // wtf checksum

				stream.Write((uint) asset.TypelessDataSegments.Count);
				for (int j = 0; j < asset.TypelessDataSegments.Count; j++)
				{
					var seg = segmentQueue.Dequeue();
					stream.Write(totalSize);
					stream.Write(seg.ActualSize);
					stream.Write(seg.StorageSize);
					stream.Write(seg.Info.OwnerGuid);
					stream.Write(seg.Info.TypeGuid);
					stream.Write(seg.Info._unknownUlong);
					stream.Write(seg.Info._unknownUint);
					stream.Write((byte) seg.Format);

					totalSize += seg.StorageSize;
					seg.Padding = totalSize % 8;
					if (seg.Padding != 0)
						seg.Padding = 8 - seg.Padding;
					seg.Padding = 0; // disable padding for now
					totalSize += seg.Padding;
					segmentQueue.Enqueue(seg);
				}

				stream.Write(asset.UnknownDependences.Count);
				foreach (var dependence in asset.UnknownDependences)
				{
					stream.Write(dependence.UnknownGuid1);
					stream.Write(dependence.UnknownGuid2);
					stream.Write(dependence.UnknownGuid3);
				}
			}

			for (int i = 0, j = (int) padding; i < j; i++)
			{
				stream.Write((byte) 0);
			}

			for (int i = 0; i < Items.Count; i++)
			{
				var asset = Items[i];
				for (int j = 0; j < asset.TypelessDataSegments.Count; j++)
				{
					var seg = segmentQueue.Dequeue();
					stream.Write(seg.Data);
					for (int j1 = 0, j2 = (int) seg.Padding; j1 < j2; j1++)
					{
						stream.Write((byte) 0);
					}
				}
			}
		}

		protected sealed class Segment
		{
			public AbstractExternalLoader Info;
			public AbstractExternalLoader.StorageFormat Format;
			public ulong ActualSize;
			public ulong StorageSize;
			public byte[] Data;
			public ulong Padding;
		}
	}
}