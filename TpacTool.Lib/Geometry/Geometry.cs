using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	// never appear in tpac
	public class Geometry : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("3eba3679-debd-4c7a-8634-f121f6325e33");

		[NotNull]
		public string ResourceFile { set; get; } = String.Empty;

		public ulong Checksum { set; get; }

		/// <summary>
		/// Assets contained in this file. The first item is type guid. The second is asset guid.
		/// </summary>
		public List<Tuple<Guid, Guid>> AssetsUsingThis { get; } = new List<Tuple<Guid, Guid>>(0);

		/// <summary>
		/// Only used by speedtree resource
		/// </summary>
		public List<string> ReferencedTextures { get; } = new List<string>(0);

		[CanBeNull]
		public ExternalLoader<FbxSettingData> FbxSetting { set; get; }

		public Geometry() : base(TYPE_GUID)
		{
		}

		public override void ReadMetadata(BinaryReader stream, int totalSize)
		{
			var pos = stream.BaseStream.Position;
			var version = stream.ReadInt32(); // 1 for 1.6.0
			ResourceFile = stream.ReadSizedString();
			Checksum = stream.ReadUInt64();
			var resCount = stream.ReadInt32();
			AssetsUsingThis.Clear();
			AssetsUsingThis.Capacity = resCount;
			for (int i = 0; i < resCount; i++)
			{
				var type = stream.ReadGuid();
				var res = stream.ReadGuid();
				AssetsUsingThis.Add(Tuple.Create(type, res));
			}

			resCount = stream.ReadInt32();
			ReferencedTextures.Clear();
			ReferencedTextures.Capacity = resCount;
			for (int i = 0; i < resCount; i++)
			{
				ReferencedTextures.Add(stream.ReadSizedString());
			}
			var daze = stream.ReadBytes(totalSize - (int) (stream.BaseStream.Position - pos));
		}

		public override void WriteMetadata(BinaryWriter stream)
		{
			stream.Write(1);
			stream.WriteSizedString(ResourceFile);
			stream.Write(Checksum);
			stream.Write(AssetsUsingThis.Count);
			for (var i = 0; i < AssetsUsingThis.Count; i++)
			{
				var tuple = AssetsUsingThis[i];
				stream.Write(tuple.Item1);
				stream.Write(tuple.Item2);
			}
			stream.Write(ReferencedTextures.Count);
			for (var i = 0; i < ReferencedTextures.Count; i++)
			{
				stream.WriteSizedString(ReferencedTextures[i]);
			}
		}

		public override void ConsumeDataSegments(AbstractExternalLoader[] externalData)
		{
			foreach (var externalLoader in externalData)
			{
				if (externalLoader is ExternalLoader<FbxSettingData> fbxSetting)
					FbxSetting = fbxSetting;
			}

			base.ConsumeDataSegments(externalData);
		}
	}
}