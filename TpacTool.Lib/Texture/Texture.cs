using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class Texture : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("c974cbcb-5f1c-49f6-9a32-2b5b6c92c2e8");

		[NotNull]
		public AssetDependence<Material> BillboardMaterial { set; get; }

		public uint UnknownUint1 { set; get; } // always 0

		[NotNull]
		public string Source { set; get; }

		public ulong UnknownUlong { set; get; } // may be CRC checksum

		public bool UnknownBool { set; get; } // false for almost all textures, only 2 snow textures are true

		public uint UnknownUint2 { set; get; } // always 0

		public List<string> Flags { set; get; }
		/*
		 dont_degrade 0x2
		 dont_delay_loading 0x80
		 is_envmap 0x1000000
		 is_specularmap 0x800000
		 is_bumpmap 0x400000
		 for_terrain 0x200000
		 for_colorgrade 0x100000
		 for_skybox_background 0x4000000
		 for_skybox_cloud 0x8000000
		 for_skybox_sun 0x10000000
		 dont_compress 0x20
		 ignore_alpha 0x80000
		 dont_resize_in_atlas 0x100
		 */

		public uint UnknownUint3 { set; get; } // always 0

		public byte UnknownByte { set; get; } // always 2

		public uint Width { set; get; }

		public uint Height { set; get; }

		public uint UnknownUint4 { set; get; } // always 1

		public byte MipmapCount { set; get; }

		public ushort ArrayCount { set; get; }

		public TextureFormat Format { set; get; }

		[CanBeNull]
		private string _rawFormat;

		public uint UnknownUint5 { set; get; } // always 0

		public List<string> SystemFlags { set; get; }
		/*
		 has_alpha 0x8000
		 is_cubemap 0x2000
		 */

		public List<Tuple<Guid, Guid>> UnknownGuidPairs { set; get; }

		[CanBeNull]
		public ExternalLoader<TexturePixelData> TexturePixels { set; get; }

		public bool HasPixelData
		{
			get => TexturePixels != null;
		}

		public Texture() : base(TYPE_GUID)
		{
			BillboardMaterial = AssetDependence<Material>.CreateEmpty();
			Source = String.Empty;
			UnknownByte = 2;
			UnknownUint4 = 1;
		}

		public override void ReadMetadata(BinaryReader stream, int totalSize)
		{
			var pos = stream.BaseStream.Position;
			var version = stream.ReadUInt32();
			BillboardMaterial = new AssetDependence<Material>(stream.ReadGuid());
			UnknownUint1 = stream.ReadUInt32();
			Source = stream.ReadSizedString();
			UnknownUlong = stream.ReadUInt64();
			UnknownBool = stream.ReadBoolean();
			UnknownUint2 = stream.ReadUInt32();
			Flags = stream.ReadStringList();
			UnknownUint3 = stream.ReadUInt32();

			UnknownByte = stream.ReadByte();
			Width = stream.ReadUInt32();
			Height = stream.ReadUInt32();
			UnknownUint4 = stream.ReadUInt32();
			MipmapCount = stream.ReadByte();
			ArrayCount = stream.ReadUInt16();
			_rawFormat = stream.ReadSizedString();
			if (TextureFormat.TryParse(_rawFormat, true, out TextureFormat format))
			{
				Format = format;
			}
			else
			{
				Format = TextureFormat.UNKNOWN;
			}
			UnknownUint5 = stream.ReadUInt32();
			SystemFlags = stream.ReadStringList();

			// dirty hack for 1.5.0
			// TW introduced a new field for the metadata of texture since 1.5.0
			// but they didn't bump the version of metadata
			UnknownGuidPairs = new List<Tuple<Guid, Guid>>();
			if (version >= 1 || totalSize - (stream.BaseStream.Position - pos) == 4)
			{
				var numPair = stream.ReadUInt32();
				for (int i = 0; i < numPair; i++)
				{
					UnknownGuidPairs.Add(Tuple.Create(stream.ReadGuid(), stream.ReadGuid()));
				}
			}
		}

		public override void ConsumeDataSegments(AbstractExternalLoader[] externalData)
		{
			foreach (var externalLoader in externalData)
			{
				var pixelData = externalLoader as ExternalLoader<TexturePixelData>;
				if (pixelData != null)
				{
					var ud = pixelData.UserData;
					ud[TexturePixelData.KEY_WIDTH] = (int) Width;
					ud[TexturePixelData.KEY_HEIGHT] = (int) Height;
					ud[TexturePixelData.KEY_ARRAY] = (int) ArrayCount;
					ud[TexturePixelData.KEY_MIPMAP] = (int) MipmapCount;
					ud[TexturePixelData.KEY_FORMAT] = Format;
					TexturePixels = pixelData;
				}
			}
			base.ConsumeDataSegments(externalData);
		}
	}
}