using System;
using System.IO;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public class DdsExporter : AbstractTextureExporter
	{
		private const int DDSD_CAPS = 0x1;
		private const int DDSD_HEIGHT = 0x2;
		private const int DDSD_WIDTH = 0x4;
		private const int DDSD_PITCH = 0x8;
		private const int DDSD_PIXELFORMAT = 0x1000;
		private const int DDSD_MIPMAPCOUNT = 0x20000;
		private const int DDSD_LINEARSIZE = 0x80000;
		private const int DDSD_DEPTH = 0x800000;
		private const int DDSD_MIN = DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT;

		private const int DDPF_ALPHAPIXELS = 0x1;
		private const int DDPF_ALPHA = 0x2;
		private const int DDPF_FOURCC = 0x4;
		private const int DDPF_RGB = 0x40;
		private const int DDPF_YUV = 0x200;
		private const int DDPF_LUMINANCE = 0x20000;

		private const int DDSCAPS_COMPLEX = 0x8;
		private const int DDSCAPS_MIPMAP = 0x400000;
		private const int DDSCAPS_TEXTURE = 0x1000;

		private const int DDSCAPS2_CUBEMAP = 0x200;
		private const int DDSCAPS2_CUBEMAP_ALLFACES = 0x400 | 0x800 | 0x1000 | 0x2000 | 0x4000 | 0x8000;

		public DdsExporter()
		{
		}

		public override string Extension => "dds";

		public override void Export(Stream writeStream)
		{
			CheckStreamAndTexture(writeStream);

			bool ignoreArray = IgnoreArray || SpecifyArray >= 0;
			bool hasMipmap = Texture.MipmapCount > 1 && !IgnoreMipmap;
			bool hasArray = Texture.ArrayCount > 1 && !ignoreArray;
			int dwFlags = DDSD_MIN;
			if (hasMipmap)
				dwFlags |= DDSD_MIPMAPCOUNT;

			int width = (int)Texture.Width;
			int height = (int)Texture.Height;
			var format = Texture.Format;
			int pitch = 0;
			if (format.IsBlockCompression())
			{
				var bytesPerBlock = format.GetBitsPerPixel() * 16 / 8;
				pitch = Math.Max(1, ((width + 3) / 4)) * bytesPerBlock;
			}
			else
			{
				pitch = (width * format.GetBitsPerPixel() + 7) / 8;
			}

			int mode = 0;
			switch (format)
			{
				case TextureFormat.B8G8R8A8_UNORM:
				case TextureFormat.B8G8R8X8_UNORM:
				case TextureFormat.A8_UNORM:
				case TextureFormat.R8G8B8A8_UNORM:
				case TextureFormat.L8_UNORM:
				case TextureFormat.R8_UNORM:
				case TextureFormat.R8G8B8:
				case TextureFormat.B8G8R8:
				case TextureFormat.R8G8_UNORM:
					break;
				case TextureFormat.DXT1:
				case TextureFormat.DXT2:
				case TextureFormat.DXT3:
				case TextureFormat.DXT4:
				case TextureFormat.DXT5:
					mode = 1;
					break;
				default:
					mode = 2;
					break;
			}

			byte[] standardHeader = new byte[128];
			using (var headerWriter = new BinaryWriter(new MemoryStream(standardHeader, true)))
			{
				headerWriter.Write(0x20534444); // magic number
				headerWriter.Write(124); // size
				headerWriter.Write(dwFlags); // flags
				headerWriter.Write(height); // height
				headerWriter.Write(width); // width
				headerWriter.Write(pitch); // dwPitchOrLinearSize
				headerWriter.Write(0); // dwDepth, no volume texture supported
				headerWriter.Write(hasMipmap ? (int)Texture.MipmapCount : 0); // mipmap
				headerWriter.Seek(11 * 4, SeekOrigin.Current); // reservedInt[11]
				/*for (int i = 0; i < 11; i++)
					headerWriter.Write((byte)0); // reserved[11]*/

				// DDPIXELFORMAT
				headerWriter.Write(32); // size

				int flags = 0; // dwFlags and FourCC
				switch (mode)
				{
					case 0:
						switch (format)
						{
							case TextureFormat.B8G8R8A8_UNORM:
							case TextureFormat.R8G8B8A8_UNORM:
								flags |= DDPF_ALPHAPIXELS | DDPF_RGB;
								break;
							case TextureFormat.B8G8R8X8_UNORM:
							case TextureFormat.R8_UNORM:
							case TextureFormat.R8G8B8:
							case TextureFormat.B8G8R8:
							case TextureFormat.R8G8_UNORM:
								flags |= DDPF_RGB;
								break;
							case TextureFormat.A8_UNORM:
								flags |= DDPF_ALPHAPIXELS | DDPF_ALPHA;
								break;
							case TextureFormat.L8_UNORM:
								flags |= DDPF_LUMINANCE;
								break;
						}
						headerWriter.Write(flags);
						headerWriter.Write(0); // no FourCC
						break;
					case 1:
						headerWriter.Write(DDPF_FOURCC);
						headerWriter.Write(format.GetFourCC());
						break;
					case 2:
						headerWriter.Write(DDPF_FOURCC);
						headerWriter.Write(TextureFormat.BC4.GetFourCC());
						break;
				}

				int bpp = 0;
				uint maskR = 0;
				uint maskG = 0;
				uint maskB = 0;
				uint maskA = 0;
				if (mode == 0)
				{
					bpp = format.GetBitsPerPixel();
					switch (format)
					{
						case TextureFormat.B8G8R8A8_UNORM:
							maskB = 0x000000FF;
							maskG = 0x0000FF00;
							maskR = 0x00FF0000;
							maskA = 0xFF000000;
							break;
						case TextureFormat.B8G8R8X8_UNORM:
							maskB = 0x000000FF;
							maskG = 0x0000FF00;
							maskR = 0x00FF0000;
							break;
						case TextureFormat.A8_UNORM:
							maskA = 0xFF;
							break;
						case TextureFormat.R8G8B8A8_UNORM:
							maskR = 0x000000FF;
							maskG = 0x0000FF00;
							maskB = 0x00FF0000;
							maskA = 0xFF000000;
							break;
						case TextureFormat.L8_UNORM:
							maskR = 0xFF;
							break;
						case TextureFormat.R8_UNORM:
							maskR = 0xFF;
							break;
						case TextureFormat.R8G8B8:
							maskB = 0xFF0000;
							maskG = 0x00FF00;
							maskR = 0x0000FF;
							break;
						case TextureFormat.B8G8R8:
							maskR = 0xFF0000;
							maskG = 0x00FF00;
							maskB = 0x0000FF;
							break;
						case TextureFormat.R8G8_UNORM:
							maskG = 0xFF00;
							maskR = 0x00FF;
							break;
					}
				}
				headerWriter.Write(bpp);
				headerWriter.Write(maskR);
				headerWriter.Write(maskG);
				headerWriter.Write(maskB);
				headerWriter.Write(maskA);

				int cap2 = DDSCAPS_TEXTURE;
				if (hasMipmap)
					cap2 |= DDSCAPS_MIPMAP;
				if (hasMipmap || hasArray)
					cap2 |= DDSCAPS_COMPLEX;
				headerWriter.Write(cap2);

				headerWriter.Write(hasArray ? (DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_ALLFACES) : 0);

				headerWriter.Write(0);
				headerWriter.Write(0);
				headerWriter.Write(0);
			}
			writeStream.Write(standardHeader, 0, standardHeader.Length);
			standardHeader = null;

			if (mode == 2)
			{
				byte[] dx10Header = new byte[20];
				using (var headerWriter = new BinaryWriter(new MemoryStream(dx10Header, true)))
				{
					headerWriter.Write(format.GetDxgiFormat());
					headerWriter.Write(3); // texture 2d
					headerWriter.Write(hasArray ? 0x4 : 0);
					headerWriter.Write(1); // may incorrect...
					switch (format)
					{
						case TextureFormat.DXT2:
						case TextureFormat.DXT4:
							headerWriter.Write(0x2); // premultiplied alpha
							break;
						default:
							headerWriter.Write(0x1);
							break;
					}
				}
				writeStream.Write(dx10Header, 0, dx10Header.Length);
			}

			var data = Texture.TexturePixels.Data.RawImage;
			for (var a = 0; a < data.Length; a++)
			{
				if (SpecifyArray >= 0 && a != SpecifyArray)
					continue;
				for (int m = 0; m < data[a].Length; m++)
				{
					if (!hasMipmap && m > 0)
						continue;
					writeStream.Write(data[a][m], 0, data[a][m].Length);
				}
			}
		}
	}
}