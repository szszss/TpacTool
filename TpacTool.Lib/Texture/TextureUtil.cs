using System;
using System.IO;
using System.Runtime.InteropServices;
using SystemHalf;
using JetBrains.Annotations;

#if !NETSTANDARD
using System.Drawing.Imaging;
#endif

namespace TpacTool.Lib
{
	public static class TextureUtil
	{
		public static bool IsSupported(this TextureFormat format)
		{
			switch (format)
			{
				// in fact R16G16B16 can't be exported, too
				case TextureFormat.DF24:
				case TextureFormat.ATOC:
				case TextureFormat.A2M0:
				case TextureFormat.A2M1:
				case TextureFormat.BC6H_UF16:
				case TextureFormat.BC7:
					return false;
				default:
					return true;
			}
		}

		public static bool IsBlockCompression(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.DXT1:
				case TextureFormat.DXT2:
				case TextureFormat.DXT3:
				case TextureFormat.DXT4:
				case TextureFormat.DXT5:
				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.BC6H_UF16:
				case TextureFormat.BC7:
					return true;
				default:
					return false;
			}
		}

		public static int GetFourCC(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.DXT1:
					return ('D') | ('X' << 8) | ('T' << 16) | ('1' << 24);
				case TextureFormat.DXT2:
					return ('D') | ('X' << 8) | ('T' << 16) | ('2' << 24);
				case TextureFormat.DXT3:
					return ('D') | ('X' << 8) | ('T' << 16) | ('3' << 24);
				case TextureFormat.DXT4:
					return ('D') | ('X' << 8) | ('T' << 16) | ('4' << 24);
				case TextureFormat.DXT5:
					return ('D') | ('X' << 8) | ('T' << 16) | ('5' << 24);
				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.BC6H_UF16:
				case TextureFormat.BC7:
					return ('D') | ('X' << 8) | ('1' << 16) | ('0' << 24);
				default:
					throw new FormatException(format + " hasn't FourCC");
			}
		}

		public static bool IsVisual(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.R32G32B32A32_UINT:
				case TextureFormat.R32G32B32_UINT:
				case TextureFormat.R16G16B16A16_UINT:
				case TextureFormat.R16G16_UINT:
				case TextureFormat.R16_UINT:
				case TextureFormat.R11G11B10F:
					return false;
				default:
					return true;
			}
		}

		public static bool IsPremultiplied(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.DXT2:
				case TextureFormat.DXT4:
					return true;
				default:
					return false;
			}
		}

		public static bool HasAlpha(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.B8G8R8X8_UNORM:
				case TextureFormat.R16G16_UNORM:
				case TextureFormat.R16G16F:
				case TextureFormat.R32_UINT:
				case TextureFormat.L8_UNORM:
				case TextureFormat.R8_UNORM:
				case TextureFormat.R16_UNORM:
				case TextureFormat.R16F:
				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.D24_UNORM_S8_UINT:
				case TextureFormat.D24_UNORM_X8_UINT:
				case TextureFormat.D16_UNORM:
				case TextureFormat.D32F:
				case TextureFormat.L16_UNORM:
				case TextureFormat.R32F:
				case TextureFormat.R32G32B32F:
				case TextureFormat.R11G11B10F:
				case TextureFormat.R16G16B16:
				case TextureFormat.R8G8B8:
				case TextureFormat.B8G8R8:
				case TextureFormat.R8_UINT:
				case TextureFormat.R16_UINT:
				case TextureFormat.R24G8_TYPELESS:
				case TextureFormat.R32G32B32_UINT:
				case TextureFormat.D32_S8X24_UINT:
				case TextureFormat.R16G16_UINT:
				case TextureFormat.R8G8_UNORM:
				case TextureFormat.R32G32F:
				case TextureFormat.R32G32_UINT:
					return false;
				default:
					return true;
			}
		}

		public static int GetAlignSize(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.DXT1:
				case TextureFormat.DXT2:
				case TextureFormat.DXT3:
				case TextureFormat.DXT4:
				case TextureFormat.DXT5:
				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.BC6H_UF16:
				case TextureFormat.BC7:
					return 4;
				default:
					return 1;
			}
		}

		public static int GetBitsPerPixel(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.R32G32B32A32F:
				case TextureFormat.R32G32B32A32_UINT:
					return 128;
				case TextureFormat.R32G32B32F:
				case TextureFormat.R32G32B32_UINT:
					return 96;
				case TextureFormat.R16G16B16A16_UNORM:
				case TextureFormat.R16G16B16A16F:
				case TextureFormat.D32_S8X24_UINT:
				case TextureFormat.R32G32F:
				case TextureFormat.R32G32_UINT:
				case TextureFormat.R16G16B16A16_UINT:
					return 64;
				case TextureFormat.R16G16B16:
					return 48;
				case TextureFormat.B8G8R8A8_UNORM:
				case TextureFormat.B8G8R8X8_UNORM:
				case TextureFormat.R16G16_UNORM:
				case TextureFormat.R16G16F:
				case TextureFormat.R32_UINT:
				case TextureFormat.R8G8B8A8_UNORM:
				case TextureFormat.R8G8B8A8_UINT:
				case TextureFormat.D24_UNORM_S8_UINT:
				case TextureFormat.D24_UNORM_X8_UINT:
				case TextureFormat.D32F:
				case TextureFormat.R32F:
				case TextureFormat.R11G11B10F:
				case TextureFormat.R24G8_TYPELESS:
				case TextureFormat.R16G16_UINT:
					return 32;
				case TextureFormat.R8G8B8:
				case TextureFormat.B8G8R8:
					return 24;
				case TextureFormat.R16_UNORM:
				case TextureFormat.R16F:
				case TextureFormat.D16_UNORM:
				case TextureFormat.L16_UNORM:
				case TextureFormat.R16_UINT:
				case TextureFormat.R8G8_UNORM:
					return 16;
				case TextureFormat.A8_UNORM:
				case TextureFormat.L8_UNORM:
				case TextureFormat.R8_UNORM:
				case TextureFormat.DXT2:
				case TextureFormat.DXT3:
				case TextureFormat.DXT4:
				case TextureFormat.DXT5:
				case TextureFormat.BC5:
				case TextureFormat.BC6H_UF16:
				case TextureFormat.BC7:
				case TextureFormat.R8_UINT:
					return 8;
				case TextureFormat.DXT1:
				case TextureFormat.BC4:
					return 4;
				default:
					throw new Exception("Unsupported format:" + format.ToString());
			}
		}

		public static int GetColorChannel(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.R32G32B32A32F:
				case TextureFormat.R32G32B32A32_UINT:
				case TextureFormat.R16G16B16A16_UNORM:
				case TextureFormat.R16G16B16A16F:
				case TextureFormat.R16G16B16A16_UINT:
				case TextureFormat.B8G8R8A8_UNORM:
				case TextureFormat.R8G8B8A8_UNORM:
				case TextureFormat.R8G8B8A8_UINT:
				case TextureFormat.DXT1:
				case TextureFormat.DXT2:
				case TextureFormat.DXT3:
				case TextureFormat.DXT4:
				case TextureFormat.DXT5:
				case TextureFormat.BC6H_UF16:
				case TextureFormat.BC7:
					return 4;
				case TextureFormat.R32G32B32F:
				case TextureFormat.R32G32B32_UINT:
				case TextureFormat.B8G8R8X8_UNORM:
				case TextureFormat.R16G16B16:
				case TextureFormat.R11G11B10F:
				case TextureFormat.R8G8B8:
				case TextureFormat.B8G8R8:
					return 3;
				case TextureFormat.D32_S8X24_UINT:
				case TextureFormat.R32G32F:
				case TextureFormat.R32G32_UINT:
				case TextureFormat.R16G16_UNORM:
				case TextureFormat.R16G16F:
				case TextureFormat.D24_UNORM_S8_UINT:
				case TextureFormat.R24G8_TYPELESS:
				case TextureFormat.R16G16_UINT:
				case TextureFormat.R8G8_UNORM:
				case TextureFormat.BC5:
					return 2;
				case TextureFormat.R32_UINT:
				case TextureFormat.D24_UNORM_X8_UINT:
				case TextureFormat.D32F:
				case TextureFormat.R32F:
				case TextureFormat.R16_UNORM:
				case TextureFormat.R16F:
				case TextureFormat.D16_UNORM:
				case TextureFormat.L16_UNORM:
				case TextureFormat.R16_UINT:
				case TextureFormat.A8_UNORM:
				case TextureFormat.L8_UNORM:
				case TextureFormat.R8_UNORM:
				case TextureFormat.R8_UINT:
				case TextureFormat.BC4:
					return 1;
				default:
					throw new Exception("Unsupported format:" + format.ToString());
			}
		}

		public static int GetDxgiFormat(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.UNKNOWN:
					return 0;
				case TextureFormat.B8G8R8A8_UNORM:
					return 87;
				case TextureFormat.B8G8R8X8_UNORM:
					return 88;
				case TextureFormat.R16G16_UNORM:
					return 35;
				case TextureFormat.R16G16F:
					return 34;
				case TextureFormat.R32_UINT:
					return 42;
				case TextureFormat.A8_UNORM:
					return 65;
				case TextureFormat.R8G8B8A8_UNORM:
					return 28;
				case TextureFormat.R8G8B8A8_UINT:
					return 30;
				case TextureFormat.R16G16B16A16_UNORM:
					return 11;
				case TextureFormat.L8_UNORM:
				case TextureFormat.R8_UNORM:
					return 61;
				case TextureFormat.R16_UNORM:
					return 56;
				case TextureFormat.R16F:
					return 54;
				case TextureFormat.DXT1:
					return 71;
				case TextureFormat.DXT2:
				case TextureFormat.DXT3:
					return 74;
				case TextureFormat.DXT4:
				case TextureFormat.DXT5:
					return 77;
				case TextureFormat.BC4:
					return 80;
				case TextureFormat.BC5:
					return 83;
				case TextureFormat.BC6H_UF16:
					return 95;
				case TextureFormat.BC7:
					return 98;
				case TextureFormat.D24_UNORM_S8_UINT:
					return 45;
				case TextureFormat.D24_UNORM_X8_UINT:
					return 46;
				case TextureFormat.D16_UNORM:
					return 55;
				case TextureFormat.D32F:
					return 40;
				case TextureFormat.L16_UNORM:
					return 56; // R16_UNORM
				//case TextureFormat.INDEX16:
				//case TextureFormat.INDEX32:
				case TextureFormat.R16G16B16A16F:
					return 10;
				case TextureFormat.R32F:
					return 41;
				case TextureFormat.R32G32B32F:
					return 6;
				case TextureFormat.R32G32B32A32F:
					return 2;
				//case TextureFormat.DF24:
				//case TextureFormat.ATOC:
				//case TextureFormat.A2M0:
				//case TextureFormat.A2M1:
				case TextureFormat.R11G11B10F:
					return 26;
				//case TextureFormat.R16G16B16:
				//case TextureFormat.R8G8B8:
				//case TextureFormat.B8G8R8:
				case TextureFormat.R32G32B32A32_UINT:
					return 3;
				case TextureFormat.R8_UINT:
					return 62;
				case TextureFormat.R16_UINT:
					return 57;
				case TextureFormat.R24G8_TYPELESS:
					return 44;
				case TextureFormat.R32G32B32_UINT:
					return 7;
				case TextureFormat.D32_S8X24_UINT:
					return 20;
				case TextureFormat.R16G16_UINT:
					return 36;
				case TextureFormat.R8G8_UNORM:
					return 49;
				case TextureFormat.R32G32F:
					return 16;
				case TextureFormat.R32G32_UINT:
					return 17;
				case TextureFormat.R16G16B16A16_UINT:
					return 12;
				default:
					return -1;
			}
		}

#if !NETSTANDARD
		public static PixelFormat GetBestBitmapFormat(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.R32G32B32A32F:
                case TextureFormat.R32G32B32A32_UINT:
                case TextureFormat.R32G32B32F:
                case TextureFormat.R32G32B32_UINT:
                case TextureFormat.R16G16B16A16_UNORM:
                case TextureFormat.R16G16B16A16F:
                case TextureFormat.R32G32F:
                case TextureFormat.R32G32_UINT:
                case TextureFormat.R16G16B16A16_UINT:
                case TextureFormat.R16G16B16:
                case TextureFormat.R16G16_UNORM:
                case TextureFormat.R16G16F:
                case TextureFormat.R16G16_UINT:
                case TextureFormat.R24G8_TYPELESS:
                case TextureFormat.D32_S8X24_UINT:
                case TextureFormat.D24_UNORM_S8_UINT:
                case TextureFormat.R11G11B10F:
                    return PixelFormat.Format64bppArgb;
                case TextureFormat.D24_UNORM_X8_UINT:
                case TextureFormat.D32F:
                case TextureFormat.R32_UINT:
                case TextureFormat.R32F:
                case TextureFormat.R16_UNORM:
                case TextureFormat.R16F:
                case TextureFormat.D16_UNORM:
                case TextureFormat.L16_UNORM:
                case TextureFormat.R16_UINT:
                    return PixelFormat.Format16bppGrayScale;
                case TextureFormat.A8_UNORM:
                case TextureFormat.L8_UNORM:
                case TextureFormat.R8_UNORM:
                case TextureFormat.R8_UINT:
                    return PixelFormat.Format8bppIndexed;
                case TextureFormat.B8G8R8A8_UNORM:
                case TextureFormat.B8G8R8X8_UNORM:
                case TextureFormat.R8G8B8A8_UNORM:
                case TextureFormat.R8G8B8A8_UINT:
                case TextureFormat.R8G8B8:
                case TextureFormat.B8G8R8:
                case TextureFormat.R8G8_UNORM:
                case TextureFormat.DXT1:
                case TextureFormat.DXT2:
                case TextureFormat.DXT3:
                case TextureFormat.DXT4:
                case TextureFormat.DXT5:
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H_UF16:
                case TextureFormat.BC7:
                    return PixelFormat.Format32bppArgb;
                default:
                    throw new Exception("Unsupported format:" + format.ToString());
            }
        }
#endif

		public static int ReadUInt24(this BinaryReader stream)
		{
			return (stream.ReadByte() | (stream.ReadByte() << 8) | (stream.ReadByte() << 16)) & 0xFFFFFF;
		}

		#region TextureToBitmap

		public static void DecodeTextureDataToWriter(byte[] data, int width, int height, TextureFormat format,
													PipelineWriter writer)
		{
			if (!format.IsSupported())
				throw new FormatException("Unsupported format: " + format.ToString());
			PipelineReader reader = null;
			switch (format)
			{
				case TextureFormat.DXT1:
					reader = new DXT1Reader(data, format, width, height);
					break;
				case TextureFormat.DXT2:
				case TextureFormat.DXT3:
					reader = new DXT3Reader(data, format, width, height);
					break;
				case TextureFormat.DXT4:
				case TextureFormat.DXT5:
					reader = new DXT5Reader(data, format, width, height);
					break;
				case TextureFormat.BC4:
					reader = new BC4Reader(data, format, width, height);
					break;
				case TextureFormat.BC5:
					reader = new BC5Reader(data, format, width, height);
					break;
				default:
					reader = new DefaultReader(data, format, width, height);
					break;
			}
			reader.Read(writer);
		}

		public static PixelColor[] DecodeTextureData(byte[] data, int width, int height, TextureFormat format,
													bool bottomToTop = false, bool rightToLeft = false)
		{
			var result = new PixelColor[width * height];
			PipelineWriter writer = new RawWriter(result, width, height, bottomToTop, rightToLeft);
			DecodeTextureDataToWriter(data, width, height, format, writer);
			return result;
		}

#if !NETSTANDARD
		public static System.Drawing.Bitmap DecodeTextureDataToBitmap(byte[] data, int width, int height, TextureFormat format)
		{
			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height,
				format.IsPremultiplied() ? PixelFormat.Format32bppPArgb : PixelFormat.Format32bppArgb);
            BitmapData bitmapData = null;
            try
            {
				bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), 
                    ImageLockMode.WriteOnly, bitmap.PixelFormat);
                PipelineWriter writer = new ARGB32Writer(bitmapData.Scan0, bitmapData.Width,
														bitmapData.Height, bitmapData.Stride);
				DecodeTextureDataToWriter(data, width, height, format, writer);
			}
            finally
            {
                if (bitmapData != null)
                    bitmap.UnlockBits(bitmapData);
            }
            return bitmap;
        }
#endif

		private abstract class PipelineReader
		{
			protected byte[] dataSource;
			protected int width, height;
			protected TextureFormat format;

			protected PipelineReader(byte[] dataSource, TextureFormat format, int width, int height)
			{
				this.dataSource = dataSource;
				this.format = format;
				this.width = width;
				this.height = height;
			}

			public abstract void Read(PipelineWriter output);
		}

		private class DefaultReader : PipelineReader
		{
			private int bytePerPixel;

			public DefaultReader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
			{
				bytePerPixel = format.GetBitsPerPixel() / 8;
			}

			public override void Read(PipelineWriter output)
			{
				using (SimpleBinaryStream stream = new SimpleBinaryStream(dataSource))
				{
					for (int y = 0; y < height; y++)
					{
						for (int x = 0; x < width; x++)
						{
							output.Write(ReadPixel(stream));
						}
					}
				}
			}

			private static float UnormByteToSingle(byte value)
			{
				return value / 255f;
			}

			private static float ByteToSingle(byte value)
			{
				return value;
			}

			private static float UnormUShortToSingle(ushort value)
			{
				return value / 65535f;
			}

			private static float UShortToSingle(ushort value)
			{
				return value;
			}

			private static float IntToSingle(uint value)
			{
				return value;
			}

			private static float UnormInt24ToSingle(int value)
			{
				return value / 16777215f;
			}

			private static float Int24ToSingle(int value)
			{
				return value / 16777215f;
			}

			private static float HalfToSingle(ushort value)
			{
				return Half.ToHalf(value);
			}

			// don't laugh. it's a historical problem :)
			private static float SingleToSingle(float value)
			{
				return value;
			}

			private PixelColor ReadPixel(SimpleBinaryStream stream)
			{
				float r = 0, g = 0, b = 0, a = 1.0f;
				switch (format)
				{
					case TextureFormat.B8G8R8A8_UNORM:
						b = UnormByteToSingle(stream.ReadByte());
						g = UnormByteToSingle(stream.ReadByte());
						r = UnormByteToSingle(stream.ReadByte());
						a = UnormByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.B8G8R8X8_UNORM:
						b = UnormByteToSingle(stream.ReadByte());
						g = UnormByteToSingle(stream.ReadByte());
						r = UnormByteToSingle(stream.ReadByte());
						stream.ReadByte();
						break;
					case TextureFormat.R16G16_UNORM:
						r = UnormUShortToSingle(stream.ReadUInt16());
						g = UnormUShortToSingle(stream.ReadUInt16());
						break;
					case TextureFormat.R16G16F:
						r = HalfToSingle(stream.ReadUInt16());
						g = HalfToSingle(stream.ReadUInt16());
						break;
					case TextureFormat.R32_UINT: // not good for visual
						r = IntToSingle(stream.ReadUInt32());
						break;
					case TextureFormat.A8_UNORM:
						r = UnormByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.R8G8B8A8_UNORM:
						r = UnormByteToSingle(stream.ReadByte());
						g = UnormByteToSingle(stream.ReadByte());
						b = UnormByteToSingle(stream.ReadByte());
						a = UnormByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.R8G8B8A8_UINT:
						r = UnormByteToSingle(stream.ReadByte());
						g = UnormByteToSingle(stream.ReadByte());
						b = UnormByteToSingle(stream.ReadByte());
						a = UnormByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.R16G16B16A16_UNORM:
						r = UnormUShortToSingle(stream.ReadUInt16());
						g = UnormUShortToSingle(stream.ReadUInt16());
						b = UnormUShortToSingle(stream.ReadUInt16());
						a = UnormUShortToSingle(stream.ReadUInt16());
						break;
					case TextureFormat.L8_UNORM:
						r = UnormByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.R8_UNORM:
						r = UnormByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.R16_UNORM:
						r = UnormUShortToSingle(stream.ReadUInt16());
						break;
					case TextureFormat.R16F:
						r = HalfToSingle(stream.ReadUInt16());
						break;
					case TextureFormat.D24_UNORM_S8_UINT:
						r = UnormInt24ToSingle(stream.ReadUInt24());
						g = UnormByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.D24_UNORM_X8_UINT:
						r = UnormInt24ToSingle(stream.ReadUInt24());
						stream.ReadByte();
						break;
					case TextureFormat.D16_UNORM:
						r = UnormUShortToSingle(stream.ReadUInt16());
						break;
					case TextureFormat.D32F:
						r = SingleToSingle(stream.ReadSingle());
						break;
					case TextureFormat.L16_UNORM:
						r = UnormUShortToSingle(stream.ReadUInt16());
						break;
					case TextureFormat.R16G16B16A16F:
						r = HalfToSingle(stream.ReadUInt16());
						g = HalfToSingle(stream.ReadUInt16());
						b = HalfToSingle(stream.ReadUInt16());
						a = HalfToSingle(stream.ReadUInt16());
						break;
					case TextureFormat.R32F:
						r = SingleToSingle(stream.ReadSingle());
						break;
					case TextureFormat.R32G32B32F:
						r = SingleToSingle(stream.ReadSingle());
						g = SingleToSingle(stream.ReadSingle());
						b = SingleToSingle(stream.ReadSingle());
						break;
					case TextureFormat.R32G32B32A32F:
						r = SingleToSingle(stream.ReadSingle());
						g = SingleToSingle(stream.ReadSingle());
						b = SingleToSingle(stream.ReadSingle());
						a = SingleToSingle(stream.ReadSingle());
						break;
					case TextureFormat.R11G11B10F: // unsupported! daze!
						stream.ReadUInt32();
						break;
					case TextureFormat.R16G16B16:
						r = UShortToSingle(stream.ReadUInt16());
						g = UShortToSingle(stream.ReadUInt16());
						b = UShortToSingle(stream.ReadUInt16());
						break;
					case TextureFormat.R8G8B8:
						r = UnormByteToSingle(stream.ReadByte());
						g = UnormByteToSingle(stream.ReadByte());
						b = UnormByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.B8G8R8:
						b = UnormByteToSingle(stream.ReadByte());
						g = UnormByteToSingle(stream.ReadByte());
						r = UnormByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.R32G32B32A32_UINT: // not good for visual
						r = IntToSingle(stream.ReadUInt32());
						g = IntToSingle(stream.ReadUInt32());
						b = IntToSingle(stream.ReadUInt32());
						a = IntToSingle(stream.ReadUInt32());
						break;
					case TextureFormat.R8_UINT:
						r = UnormByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.R16_UINT:
						r = UShortToSingle(stream.ReadUInt16());
						break;
					case TextureFormat.R24G8_TYPELESS: // not sure if it should be normalized
						r = Int24ToSingle(stream.ReadUInt24());
						g = ByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.R32G32B32_UINT: // not good for visual
						r = IntToSingle(stream.ReadUInt32());
						g = IntToSingle(stream.ReadUInt32());
						b = IntToSingle(stream.ReadUInt32());
						break;
					case TextureFormat.D32_S8X24_UINT:
						r = SingleToSingle(stream.ReadSingle());
						g = ByteToSingle((byte)(stream.ReadUInt32() & 0xFF));
						break;
					case TextureFormat.R16G16_UINT: // not good for visual
						r = UShortToSingle(stream.ReadUInt16());
						g = UShortToSingle(stream.ReadUInt16());
						break;
					case TextureFormat.R8G8_UNORM:
						r = UnormByteToSingle(stream.ReadByte());
						g = UnormByteToSingle(stream.ReadByte());
						break;
					case TextureFormat.R32G32F:
						r = SingleToSingle(stream.ReadSingle());
						g = SingleToSingle(stream.ReadSingle());
						break;
					case TextureFormat.R32G32_UINT: // not good for visual
						r = IntToSingle(stream.ReadUInt32());
						g = IntToSingle(stream.ReadUInt32());
						break;
					case TextureFormat.R16G16B16A16_UINT:
						r = UShortToSingle(stream.ReadUInt16());
						g = UShortToSingle(stream.ReadUInt16());
						b = UShortToSingle(stream.ReadUInt16());
						a = UShortToSingle(stream.ReadUInt16());
						break;
					default:
						throw new Exception("Unsupported format:" + format.ToString());
				}

				return new PixelColor(r, g, b, a);
			}
		}

		private abstract class BlockReader : PipelineReader
		{
			private int bytesPerBlock;

			public BlockReader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
			{
				bytesPerBlock = (format.GetBitsPerPixel() * 16) / 8;
			}

			public override void Read(PipelineWriter output)
			{
				int blockWidth = Math.Max((width + 3) / 4, 1);
				int blockHeight = Math.Max((height + 3) / 4, 1);
				PixelColor[,] cache0 = new PixelColor[4, blockWidth * 4];
				PixelColor[,] cache1 = new PixelColor[4, blockWidth * 4];
				bool isPingPong = false;
#if !NET40
				System.Threading.Tasks.Task task = null;
#endif
				using (SimpleBinaryStream stream = new SimpleBinaryStream(dataSource))
				{
					for (int y = 0; y < blockHeight; y++)
					{
						if (isPingPong)
						{
							for (int x = 0; x < blockWidth; x++)
								ReadBlock(stream, ref cache1, x);
						}
						else
						{
							for (int x = 0; x < blockWidth; x++)
								ReadBlock(stream, ref cache0, x);
						}

#if !NET40
						if (task != null)
							task.Wait();
#endif
						bool pingPoing = isPingPong;
						int baseY = y;
#if !NET40
						task = System.Threading.Tasks.Task.Run((() =>
						{
#endif
							for (int y2 = 0; y2 < 4; y2++)
							{
								int currentY = baseY * 4 + y2;
								if (currentY < height)
								{
									if (pingPoing)
									{
										for (int x = 0; x < width; x++)
											output.Write(cache1[y2, x]);
									}
									else
									{
										for (int x = 0; x < width; x++)
											output.Write(cache0[y2, x]);
									}
								}
							}
#if !NET40
						}));
#endif
						isPingPong = !isPingPong;
					}
#if !NET40
					task.Wait();
#endif
				}
			}

			protected static PixelColor GetColorFromRGB565(ushort rgb565)
			{

				int b = rgb565 & 0x1F;
				int g = (rgb565 & 0x7E0) >> 5;
				int r = (rgb565 & 0xF800) >> 11;
				b = b << 3 | b >> 2;
				g = g << 2 | g >> 3;
				r = r << 3 | r >> 2;
				return new PixelColor(r / 255f, g / 255f, b / 255f, 1f);
			}

			protected abstract void ReadBlock(SimpleBinaryStream stream, ref PixelColor[,] cache, int blockX);
		}

		private class DXT1Reader : BlockReader
		{
			public DXT1Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
			{
			}

			protected override void ReadBlock(SimpleBinaryStream stream, ref PixelColor[,] cache, int blockX)
			{
				blockX *= 4;
				PixelColor[] baseColor = new PixelColor[4];
				ushort color0, color1;
				baseColor[0] = GetColorFromRGB565(color0 = stream.ReadUInt16());
				baseColor[1] = GetColorFromRGB565(color1 = stream.ReadUInt16());
				if (color0 > color1)
				{
					// baseColor[2] = (baseColor[0] * 2 + baseColor[1]) / 3;
					baseColor[2] = PixelColor.MAD(baseColor[0], 2, baseColor[1], 3);
					baseColor[3] = PixelColor.MAD(baseColor[1], 2, baseColor[0], 3);
				}
				else
				{
					baseColor[2] = PixelColor.AVG(baseColor[0], baseColor[1]);
					baseColor[3] = new PixelColor(0, 0, 0, 1f);
				}

				for (int y = 0; y < 4; y++)
				{
					byte index = stream.ReadByte();
					cache[y, blockX + 0] = baseColor[(index >> 0) & 0x3];
					cache[y, blockX + 1] = baseColor[(index >> 2) & 0x3];
					cache[y, blockX + 2] = baseColor[(index >> 4) & 0x3];
					cache[y, blockX + 3] = baseColor[(index >> 6) & 0x3];
				}
			}
		}

		private class DXT3Reader : DXT1Reader
		{
			public DXT3Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
			{
			}

			protected override void ReadBlock(SimpleBinaryStream stream, ref PixelColor[,] cache, int blockX)
			{
				ulong alphaData = stream.ReadUInt64();
				base.ReadBlock(stream, ref cache, blockX);
				blockX *= 4;
				int i = 0;
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						int alpha = (int)((alphaData >> (i * 4)) & 0xF);
						i++;
						alpha = alpha << 4 | alpha;
						cache[y, blockX + x].A = alpha / 255f;
					}
				}
			}
		}

		private class DXT5Reader : DXT1Reader
		{
			public DXT5Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
			{
			}

			protected override void ReadBlock(SimpleBinaryStream stream, ref PixelColor[,] cache, int blockX)
			{
				long alphaData = stream.ReadInt64();
				base.ReadBlock(stream, ref cache, blockX);
				ReadBC3AlphaBlock(alphaData, ref cache, blockX * 4, (ref PixelColor pixel, float value) =>
				{
					pixel.A = value;
				});
			}

			public delegate void PixelAction(ref PixelColor pixel, float value);

			public static void ReadBC3AlphaBlock(long block, ref PixelColor[,] cache, int posX, PixelAction writeOut)
			{
				int alpha0 = (int)(block & 0xFF);
				int alpha1 = (int)((block >> 8) & 0xFF);
				bool isFirstGreater = alpha0 > alpha1;
				block = block >> 16;
				float[] alphaLookup = new float[8];
				for (int j = 0; j < 8; j++)
				{
					alphaLookup[j] = BC3GradientInterpolate(j, alpha0, alpha1, isFirstGreater) / 255f;
				}
				int i = 0;
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						int alphaIndex = (int)(block >> (i * 3)) & 0x7;
						float alpha = alphaLookup[alphaIndex];
						i++;
						//cache[y, posX + x].a = alpha;
						writeOut(ref cache[y, posX + x], alpha);
					}
				}
			}

			public static int BC3GradientInterpolate(int index, int alpha0, int alpha1, bool isFirstGreater)
			{
				if (isFirstGreater)
				{
					switch (index)
					{
						case 0:
							return alpha0;
						case 1:
							return alpha1;
						case 2:
							return (alpha0 * 6 + alpha1 * 1) / 7;
						case 3:
							return (alpha0 * 5 + alpha1 * 2) / 7;
						case 4:
							return (alpha0 * 4 + alpha1 * 3) / 7;
						case 5:
							return (alpha0 * 3 + alpha1 * 4) / 7;
						case 6:
							return (alpha0 * 2 + alpha1 * 5) / 7;
						case 7:
							return (alpha0 * 1 + alpha1 * 6) / 7;
					}
				}
				else
				{
					switch (index)
					{
						case 0:
							return alpha0;
						case 1:
							return alpha1;
						case 2:
							return (alpha0 * 4 + alpha1 * 1) / 5;
						case 3:
							return (alpha0 * 3 + alpha1 * 2) / 5;
						case 4:
							return (alpha0 * 2 + alpha1 * 3) / 5;
						case 5:
							return (alpha0 * 1 + alpha1 * 4) / 5;
						case 6:
							return 0;
						case 7:
							return ushort.MaxValue;
					}
				}
				throw new IndexOutOfRangeException("Bad alpha index: " + index);
			}
		}

		private class BC4Reader : BlockReader
		{
			public BC4Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
			{
			}

			protected override void ReadBlock(SimpleBinaryStream stream, ref PixelColor[,] cache, int blockX)
			{
				DXT5Reader.ReadBC3AlphaBlock(stream.ReadInt64(), ref cache, blockX * 4, (ref PixelColor pixel, float value) =>
				{
					pixel.R = value;
					pixel.A = 1f;
				});
			}
		}

		private class BC5Reader : BC4Reader
		{
			public BC5Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
			{
			}

			protected override void ReadBlock(SimpleBinaryStream stream, ref PixelColor[,] cache, int blockX)
			{
				base.ReadBlock(stream, ref cache, blockX);
				DXT5Reader.ReadBC3AlphaBlock(stream.ReadInt64(), ref cache, blockX * 4, (ref PixelColor pixel, float value) =>
				{
					pixel.G = value;
				});
			}
		}

		public abstract class PipelineWriter
		{
			protected int width, height;

			public abstract void Write(PixelColor color);
		}

		public class ARGB32Writer : PipelineWriter
		{
			protected IntPtr data;
			protected int lineLimiter;
			protected int pointer;
			protected int stride;
			private int[] buffer;

			public ARGB32Writer(IntPtr ptr, int width, int height, int stride)
			{
				this.data = ptr;
				this.width = width;
				this.height = height;
				this.stride = stride;
				lineLimiter = width;
				buffer = new int[lineLimiter];
			}

			public override void Write(PixelColor color)
			{
				buffer[pointer++] = color.ToARGB32();
				if (pointer >= lineLimiter)
				{
					pointer = 0;
					Marshal.Copy(buffer, 0, data, lineLimiter);
					data = IntPtr.Add(data, stride);
				}
			}
		}

		public class RawWriter : PipelineWriter
		{
			private PixelColor[] buffer;
			private int baseOffset, lineOffset;
			private bool bottomToTop, rightToLeft;

			public RawWriter([NotNull] PixelColor[] target, int width, int height, bool bottomToTop, bool rightToLeft)
			{
				this.buffer = target;
				this.width = width;
				this.height = height;
				this.bottomToTop = bottomToTop;
				this.rightToLeft = rightToLeft;
				if (rightToLeft)
					lineOffset = width - 1;
				if (bottomToTop)
					baseOffset = width * (height - 1);
			}

			public override void Write(PixelColor color)
			{
				buffer[baseOffset + lineOffset] = color;
				lineOffset += rightToLeft ? -1 : 1;
				if (lineOffset >= width || lineOffset < 0)
				{
					lineOffset = rightToLeft ? width - 1 : 0;
					baseOffset += bottomToTop ? width : -width;
				}
			}
		}

		private sealed class SimpleBinaryStream : IDisposable
		{
			private byte[] data;
			private int pointer;

			public SimpleBinaryStream(byte[] data)
			{
				this.data = data;
			}

			public byte ReadByte()
			{
				return data[pointer++];
			}

			public ushort ReadUInt16()
			{
				byte b0 = data[pointer];
				byte b1 = data[pointer + 1];
				pointer += 2;
				return (ushort)(b0 | b1 << 8);
			}

			public int ReadUInt24()
			{
				byte b0 = data[pointer];
				byte b1 = data[pointer + 1];
				byte b2 = data[pointer + 2];
				pointer += 3;
				return (int)(b0 | b1 << 8 | b2 << 16);
			}

			public uint ReadUInt32()
			{
				byte b0 = data[pointer];
				byte b1 = data[pointer + 1];
				byte b2 = data[pointer + 2];
				byte b3 = data[pointer + 3];
				pointer += 4;
				return (uint)(b0 | b1 << 8 | b2 << 16 | b3 << 24);
			}

			public ulong ReadUInt64()
			{
				uint numLSB = (uint)(data[pointer] | data[pointer + 1] << 8 |
									data[pointer + 2] << 16 | data[pointer + 3] << 24);
				uint numMSB = (uint)(data[pointer + 4] | data[pointer + 5] << 8 |
									data[pointer + 6] << 16 | data[pointer + 7] << 24);
				pointer += 8;
				return (ulong)numMSB << 32 | numLSB;
			}

			public long ReadInt64()
			{
				uint numLSB = (uint)(data[pointer] | data[pointer + 1] << 8 |
									data[pointer + 2] << 16 | data[pointer + 3] << 24);
				uint numMSB = (uint)(data[pointer + 4] | data[pointer + 5] << 8 |
									data[pointer + 6] << 16 | data[pointer + 7] << 24);
				pointer += 8;
				return (long)((ulong)numMSB << 32 | numLSB);
			}

			public float ReadSingle()
			{
				var f = BitConverter.ToSingle(data, pointer);
				pointer += 4;
				return f;
			}

			public void Dispose()
			{
			}
		}

		public struct PixelColor
		{
			public float R, G, B, A;

			public PixelColor(float r, float g, float b, float a)
			{
				this.R = r;
				this.G = g;
				this.B = b;
				this.A = a;
			}

			public PixelColor(PixelColor other)
			{
				this.R = other.R;
				this.G = other.G;
				this.B = other.B;
				this.A = other.A;
			}

			public int ToARGB32()
			{
				return FloatToByte(B) | (FloatToByte(G) << 8) | (FloatToByte(R) << 16) | (FloatToByte(A) << 24);
			}

			private static byte FloatToByte(float v)
			{
				int i = Math.Min(Math.Max((int)Math.Round(v * 255), 0), 255);
				return (byte) i;
			}

			private static float MAD_Do(float left, int mul, float right, int div)
			{
				return (left * mul + right) / div;
			}

			// return (left * mul + right) / div
			public static PixelColor MAD(PixelColor left, int mul, PixelColor right, int div)
			{
				PixelColor result = new PixelColor();
				result.R = MAD_Do(left.R, mul, right.R, div);
				result.G = MAD_Do(left.G, mul, right.G, div);
				result.B = MAD_Do(left.B, mul, right.B, div);
				result.A = MAD_Do(left.A, mul, right.A, div);
				return result;
			}

			private static float AVG_Do(float left, float right)
			{
				return (left + right) / 2;
			}

			// return (left + right) / 2
			public static PixelColor AVG(PixelColor left, PixelColor right)
			{
				PixelColor result = new PixelColor();
				result.R = AVG_Do(left.R, right.R);
				result.G = AVG_Do(left.G, right.G);
				result.B = AVG_Do(left.B, right.B);
				result.A = AVG_Do(left.A, right.A);
				return result;
			}

			/*public static PixelColor operator *(int left, PixelColor right)
            {
                return right * left;
            }

            public static PixelColor operator *(PixelColor left, int right)
            {
                return new PixelColor(left.r * right, left.g * right, left.b * right, left.a * right);
            }

            public static PixelColor operator +(PixelColor left, PixelColor right)
            {
                return new PixelColor(left.r + right.r, left.g + right.g, left.b + right.b, left.a + right.a);
            }

            public static PixelColor operator /(PixelColor left, int right)
            {
                return new PixelColor(left.r / right, left.g / right, left.b / right, left.a / right);
            }*/
		}
		#endregion
	}
}