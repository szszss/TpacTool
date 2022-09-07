using System;
using System.IO;
using System.Runtime.InteropServices;
using SystemHalf;
using JetBrains.Annotations;

#if NETFRAMEWORK
using System.Drawing.Imaging;
#endif

#if NET5_0_OR_GREATER
using Half = SystemHalf.Half;
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
				case TextureFormat.INDEX16:
				case TextureFormat.INDEX32:
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

#if NETFRAMEWORK
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

#pragma warning disable 612

		public static void DecodeTextureDataToWriter(byte[] data, int width, int height, TextureFormat format,
													PipelineWriter writer, bool silentlyFail = false)
		{
			if (!format.IsSupported())
			{
				if (!silentlyFail)
					throw new FormatException("Unsupported format: " + format.ToString());
				byte[] byteBuffer = new byte[width * 4];
				for (int x = 0; x < width; x++)
				{
					int i = x * 4;
					byteBuffer[i] = 160;
					byteBuffer[i + 1] = 160;
					byteBuffer[i + 2] = 160;
					byteBuffer[i + 3] = 255;
				}
				for (int y = 0; y < height; y++)
				{
					writer.WriteLine(byteBuffer, true);
				}
			}
			else
			{
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
		}

		[Obsolete]
		public static PixelColor[] DecodeTextureData(byte[] data, int width, int height, TextureFormat format,
													bool bottomToTop = false, bool rightToLeft = false)
		{
			var result = new PixelColor[width * height];
			PipelineWriter writer = new RawWriter(result, width, height, bottomToTop, rightToLeft);
			DecodeTextureDataToWriter(data, width, height, format, writer);
			return result;
		}

#if NETFRAMEWORK
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
					switch (format)
					{
						case TextureFormat.A8_UNORM:
						case TextureFormat.L8_UNORM:
						case TextureFormat.R8_UNORM:
						case TextureFormat.R8_UINT:
						case TextureFormat.R8G8_UNORM:
						case TextureFormat.R8G8B8:
						case TextureFormat.B8G8R8:
						case TextureFormat.B8G8R8A8_UNORM:
						case TextureFormat.B8G8R8X8_UNORM:
						case TextureFormat.R8G8B8A8_UNORM:
						case TextureFormat.R8G8B8A8_UINT:
							byte[] byteBuffer = new byte[width * 4];
							for (int y = 0; y < height; y++)
							{
								ReadLine8bpp(stream, output, byteBuffer);
								Array.Clear(byteBuffer, 0, byteBuffer.Length);
							}
							byteBuffer = null;
							break;
						case TextureFormat.R16_UINT:
						case TextureFormat.R16_UNORM:
						case TextureFormat.D16_UNORM:
						case TextureFormat.L16_UNORM:
						case TextureFormat.R16G16_UNORM:
						case TextureFormat.R16G16_UINT:
						case TextureFormat.R16G16B16:
						case TextureFormat.R16G16B16A16_UINT:
							ushort[] ushortBuffer = new ushort[width * 4];
							for (int y = 0; y < height; y++)
							{
								ReadLine16bpp(stream, output, ushortBuffer);
								Array.Clear(ushortBuffer, 0, ushortBuffer.Length);
							}
							ushortBuffer = null;
							break;
						case TextureFormat.R32_UINT:
						case TextureFormat.R32G32_UINT:
						case TextureFormat.R32G32B32_UINT:
						case TextureFormat.R32G32B32A32_UINT:
						case TextureFormat.R24G8_TYPELESS:
							uint[] uintBuffer = new uint[width * 4];
							for (int y = 0; y < height; y++)
							{
								ReadLine32bpp(stream, output, uintBuffer);
								Array.Clear(uintBuffer, 0, uintBuffer.Length);
							}
							uintBuffer = null;
							break;
						case TextureFormat.D24_UNORM_S8_UINT:
						case TextureFormat.D24_UNORM_X8_UINT:
						case TextureFormat.D32_S8X24_UINT:
						case TextureFormat.D32F:
							float[] depthBuffer = new float[width];
							byte[] stencilBuffer = new byte[width];
							for (int y = 0; y < height; y++)
							{
								ReadLineDepth(stream, output, depthBuffer, stencilBuffer);
							}
							depthBuffer = null;
							stencilBuffer = null;
							break;
						case TextureFormat.R11G11B10F:
						case TextureFormat.R16F:
						case TextureFormat.R16G16F:
						case TextureFormat.R16G16B16A16F:
						case TextureFormat.R32F:
						case TextureFormat.R32G32F:
						case TextureFormat.R32G32B32F:
						case TextureFormat.R32G32B32A32F:
							float[] floatBuffer = new float[width * 4];
							for (int y = 0; y < height; y++)
							{
								ReadLineFloat(stream, output, floatBuffer);
								Array.Clear(floatBuffer, 0, floatBuffer.Length);
							}
							floatBuffer = null;
							break;
						default:
							throw new Exception("Unsupported format:" + format.ToString());
					}
				}
			}

			private void ReadLine8bpp(SimpleBinaryStream input, PipelineWriter output, byte[] buffer)
			{
				bool normalized = true;
				switch (format)
				{
					case TextureFormat.A8_UNORM:
						for (int i = 0; i < width; i++)
							buffer[i * 4 + 3] = input.ReadByte();
						break;
					case TextureFormat.L8_UNORM:
					case TextureFormat.R8_UNORM:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadByte();
							buffer[j + 3] = byte.MaxValue;
						}
						break;
					case TextureFormat.R8_UINT:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadByte();
							buffer[j + 3] = byte.MaxValue;
						}
						normalized = false;
						break;
					case TextureFormat.R8G8_UNORM:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadByte();
							buffer[j + 1] = input.ReadByte();
							buffer[j + 3] = byte.MaxValue;
						}
						break;
					case TextureFormat.R8G8B8:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadByte();
							buffer[j + 1] = input.ReadByte();
							buffer[j + 2] = input.ReadByte();
							buffer[j + 3] = byte.MaxValue;
						}
						break;
					case TextureFormat.B8G8R8:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j + 2] = input.ReadByte();
							buffer[j + 1] = input.ReadByte();
							buffer[j] = input.ReadByte();
							buffer[j + 3] = byte.MaxValue;
						}
						break;
					case TextureFormat.B8G8R8A8_UNORM:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j + 2] = input.ReadByte();
							buffer[j + 1] = input.ReadByte();
							buffer[j] = input.ReadByte();
							buffer[j + 3] = input.ReadByte();
						}
						break;
					case TextureFormat.B8G8R8X8_UNORM:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j + 2] = input.ReadByte();
							buffer[j + 1] = input.ReadByte();
							buffer[j] = input.ReadByte();
							input.ReadByte();
							buffer[j + 3] = byte.MaxValue;
						}
						break;
					case TextureFormat.R8G8B8A8_UNORM:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadByte();
							buffer[j + 1] = input.ReadByte();
							buffer[j + 2] = input.ReadByte();
							buffer[j + 3] = input.ReadByte();
						}
						break;
					case TextureFormat.R8G8B8A8_UINT:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadByte();
							buffer[j + 1] = input.ReadByte();
							buffer[j + 2] = input.ReadByte();
							buffer[j + 3] = input.ReadByte();
						}
						normalized = false;
						break;
				}
				output.WriteLine(buffer, normalized);
			}

			private void ReadLine16bpp(SimpleBinaryStream input, PipelineWriter output, ushort[] buffer)
			{
				bool normalized = true;
				switch (format)
				{
					case TextureFormat.R16_UINT:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadUInt16();
							buffer[j + 3] = ushort.MaxValue;
						}
						normalized = false;
						break;
					case TextureFormat.R16_UNORM:
					case TextureFormat.D16_UNORM:
					case TextureFormat.L16_UNORM:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadUInt16();
							buffer[j + 3] = ushort.MaxValue;
						}
						break;
					case TextureFormat.R16G16_UNORM:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadUInt16();
							buffer[j + 1] = input.ReadUInt16();
							buffer[j + 3] = ushort.MaxValue;
						}
						break;
					case TextureFormat.R16G16_UINT:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadUInt16();
							buffer[j + 1] = input.ReadUInt16();
							buffer[j + 3] = ushort.MaxValue;
						}
						normalized = false;
						break;
					case TextureFormat.R16G16B16:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadUInt16();
							buffer[j + 1] = input.ReadUInt16();
							buffer[j + 2] = input.ReadUInt16();
							buffer[j + 3] = ushort.MaxValue;
						}
						break;
					case TextureFormat.R16G16B16A16_UINT:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadUInt16();
							buffer[j + 1] = input.ReadUInt16();
							buffer[j + 2] = input.ReadUInt16();
							buffer[j + 3] = input.ReadUInt16();
						}
						break;
				}
				output.WriteLine(buffer, normalized);
			}

			private void ReadLine32bpp(SimpleBinaryStream input, PipelineWriter output, uint[] buffer)
			{
				bool normalized = false;
				switch (format)
				{
					case TextureFormat.R32_UINT:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadUInt32();
							buffer[j + 3] = uint.MaxValue;
						}
						break;
					case TextureFormat.R32G32_UINT:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadUInt32();
							buffer[j + 1] = input.ReadUInt32();
							buffer[j + 3] = uint.MaxValue;
						}
						break;
					case TextureFormat.R32G32B32_UINT:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadUInt32();
							buffer[j + 1] = input.ReadUInt32();
							buffer[j + 2] = input.ReadUInt32();
							buffer[j + 3] = uint.MaxValue;
						}
						break;
					case TextureFormat.R32G32B32A32_UINT:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadUInt32();
							buffer[j + 1] = input.ReadUInt32();
							buffer[j + 2] = input.ReadUInt32();
							buffer[j + 3] = input.ReadUInt32();
						}
						break;
					case TextureFormat.R24G8_TYPELESS:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadUInt24();
							buffer[j + 1] = input.ReadByte();
							buffer[j + 3] = uint.MaxValue;
						}
						break;
				}
				output.WriteLine(buffer, normalized);
			}

			private void ReadLineDepth(SimpleBinaryStream input, PipelineWriter output, float[] depth, byte[] stencil)
			{
				switch (format)
				{
					case TextureFormat.D24_UNORM_S8_UINT:
						for (int i = 0; i < width; i++)
						{
							depth[i] = Int24ToSingle(input.ReadUInt24());
							stencil[i] = input.ReadByte();
						}
						break;
					case TextureFormat.D24_UNORM_X8_UINT:
						for (int i = 0; i < width; i++)
						{
							depth[i] = Int24ToSingle(input.ReadUInt24());
							input.ReadByte();
						}
						break;
					case TextureFormat.D32_S8X24_UINT:
						for (int i = 0; i < width; i++)
						{
							depth[i] = Int32ToSingle(input.ReadUInt32());
							stencil[i] = input.ReadByte();
							input.ReadUInt24();
						}
						break;
					case TextureFormat.D32F:
						for (int i = 0; i < width; i++)
						{
							depth[i] = input.ReadSingle();
						}
						break;
				}
				output.WriteLine(depth, stencil);
			}

			private void ReadLineFloat(SimpleBinaryStream input, PipelineWriter output, float[] buffer)
			{
				switch (format)
				{
					case TextureFormat.R11G11B10F:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							UnpackR11G11B10F(input.ReadUInt32(), out float r, out float g, out float b);
							buffer[j] = r;
							buffer[j + 1] = g;
							buffer[j + 2] = b;
							buffer[j + 3] = 1f;
						}
						break;
					case TextureFormat.R16F:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = HalfToSingle(input.ReadUInt16());
							buffer[j + 3] = 1f;
						}
						break;
					case TextureFormat.R16G16F:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = HalfToSingle(input.ReadUInt16());
							buffer[j + 1] = HalfToSingle(input.ReadUInt16());
							buffer[j + 3] = 1f;
						}
						break;
					case TextureFormat.R16G16B16A16F:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = HalfToSingle(input.ReadUInt16());
							buffer[j + 1] = HalfToSingle(input.ReadUInt16());
							buffer[j + 2] = HalfToSingle(input.ReadUInt16());
							buffer[j + 3] = HalfToSingle(input.ReadUInt16());
						}
						break;
					case TextureFormat.R32F:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadSingle();
							buffer[j + 3] = 1f;
						}
						break;
					case TextureFormat.R32G32F:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadSingle();
							buffer[j + 1] = input.ReadSingle();
							buffer[j + 3] = 1f;
						}
						break;
					case TextureFormat.R32G32B32F:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadSingle();
							buffer[j + 1] = input.ReadSingle();
							buffer[j + 2] = input.ReadSingle();
							buffer[j + 3] = 1f;
						}
						break;
					case TextureFormat.R32G32B32A32F:
						for (int i = 0, j = 0; i < width; i++, j = i * 4)
						{
							buffer[j] = input.ReadSingle();
							buffer[j + 1] = input.ReadSingle();
							buffer[j + 2] = input.ReadSingle();
							buffer[j + 3] = input.ReadSingle();
						}
						break;
				}
				output.WriteLine(buffer);
			}

			private static float Int24ToSingle(uint value)
			{
				return (float)(value / 16777215d);
			}

			private static float Int32ToSingle(uint value)
			{
				return (float)(value / (double)UInt32.MaxValue);
			}

			private static float HalfToSingle(ushort value)
			{
				return Half.ToHalf(value);
			}

			private static void UnpackR11G11B10F(uint value, out float r, out float g, out float b)
			{
				// https://github.com/anonymousguy198/swiftshader-compiled/blob/a6bc61d61d6fe9551d72f917629bf6bccfeafce0/src/Common/Half.hpp#L73
				uint bits = (value & 0x7FFu) << 4;
				r = HalfToSingle((ushort) bits);
				bits = ((value >> 11) & 0x7FFu) << 4;
				g = HalfToSingle((ushort) bits);
				bits = ((value >> 22) & 0x3FFu) << 5;
				b = HalfToSingle((ushort) bits);
			}
		}

		private struct RGB565
		{
			internal readonly byte R;
			internal readonly byte G;
			internal readonly byte B;
			internal readonly byte A;

			public RGB565(ushort value) : this(value, byte.MaxValue)
			{
			}

			public RGB565(ushort value, byte alpha)
			{
				int r = (value >> 11) & 0x1F;
				int g = (value >> 5) & 0x3F;
				int b = value & 0x1F;
				r = r << 3 | r >> 2;
				g = g << 2 | g >> 3;
				b = b << 3 | b >> 2;
				R = (byte)r;
				G = (byte)g;
				B = (byte)b;
				A = alpha;
			}

			private RGB565(byte r, byte g, byte b, byte a)
			{
				R = r;
				G = g;
				B = b;
				A = a;
			}

			public void WriteToBytes(byte[] rgba8, int offset)
			{
				rgba8[offset] = R;
				rgba8[offset + 1] = G;
				rgba8[offset + 2] = B;
				rgba8[offset + 3] = A;
			}

			// return (left * 2 + right) / 3 (ignore alpha)
			public static RGB565 MAD(RGB565 left, RGB565 right)
			{
				int r = (((left.R << 1) + right.R) / 3) & 0xFF;
				int g = (((left.G << 1) + right.G) / 3) & 0xFF;
				int b = (((left.B << 1) + right.B) / 3) & 0xFF;
				return new RGB565((byte) r, (byte) g, (byte) b, left.A);
			}

			// return (left + right) / 2 (ignore alpha)
			public static RGB565 AVG(RGB565 left, RGB565 right)
			{
				int r = ((left.R + right.R) >> 1) & 0xFF;
				int g = ((left.G + right.G) >> 1) & 0xFF;
				int b = ((left.B + right.B) >> 1) & 0xFF;
				return new RGB565((byte)r, (byte)g, (byte)b, left.A);
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
				byte[][] cache0 = new byte[4][];
				for (int i = 0; i < 4; i++)
					cache0[i] = new byte[blockWidth * 4 * 4];
				byte[][] cache1 = new byte[4][];
				for (int i = 0; i < 4; i++)
					cache1[i] = new byte[blockWidth * 4 * 4];
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
								ReadBlock(stream, cache1, x);
						}
						else
						{
							for (int x = 0; x < blockWidth; x++)
								ReadBlock(stream, cache0, x);
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
										output.WriteLine(cache1[y2], false);
									}
									else
									{
										output.WriteLine(cache0[y2], false);
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

			/*protected static PixelColor GetColorFromRGB565(ushort rgb565)
			{

				int b = rgb565 & 0x1F;
				int g = (rgb565 & 0x7E0) >> 5;
				int r = (rgb565 & 0xF800) >> 11;
				b = b << 3 | b >> 2;
				g = g << 2 | g >> 3;
				r = r << 3 | r >> 2;
				return new PixelColor(r / 255f, g / 255f, b / 255f, 1f);
			}*/

			protected abstract void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX);
		}

		private class DXT1Reader : BlockReader
		{
			public DXT1Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
			{
			}

			protected override void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX)
			{
				int posX = blockX * 4;
				
				RGB565[] baseColor = new RGB565[4];
				ushort color0, color1;
				baseColor[0] = new RGB565(color0 = stream.ReadUInt16());
				baseColor[1] = new RGB565(color1 = stream.ReadUInt16());
				if (color0 > color1)
				{
					// baseColor[2] = (baseColor[0] * 2 + baseColor[1]) / 3;
					baseColor[2] = RGB565.MAD(baseColor[0], baseColor[1]);
					baseColor[3] = RGB565.MAD(baseColor[1], baseColor[0]);
				}
				else
				{
					baseColor[2] = RGB565.AVG(baseColor[0], baseColor[1]);
					baseColor[3] = new RGB565(0, byte.MaxValue);
				}

				for (int y = 0; y < 4; y++)
				{
					byte[] writeLine = cache[y];
					byte index = stream.ReadByte();
					baseColor[(index >> 0) & 0x3].WriteToBytes(writeLine, (posX + 0) * 4);
					baseColor[(index >> 2) & 0x3].WriteToBytes(writeLine, (posX + 1) * 4);
					baseColor[(index >> 4) & 0x3].WriteToBytes(writeLine, (posX + 2) * 4);
					baseColor[(index >> 6) & 0x3].WriteToBytes(writeLine, (posX + 3) * 4);
				}
			}
		}

		private class DXT3Reader : DXT1Reader
		{
			public DXT3Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
			{
			}

			protected override void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX)
			{
				ulong alphaData = stream.ReadUInt64();
				base.ReadBlock(stream, cache, blockX);
				int posX = blockX * 4;
				int i = 0;
				for (int y = 0; y < 4; y++)
				{
					byte[] writeLine = cache[y];
					for (int x = 0; x < 4; x++)
					{
						int alpha = (int)((alphaData >> (i * 4)) & 0xF);
						i++;
						alpha = (alpha << 4 | alpha) & 0xFF;
						writeLine[(posX + x) * 4 + 3] = (byte) alpha;
					}
				}
			}
		}

		private class DXT5Reader : DXT1Reader
		{
			public DXT5Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
			{
			}

			protected override void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX)
			{
				long alphaData = stream.ReadInt64();
				base.ReadBlock(stream, cache, blockX);
				ReadBC3AlphaBlock(alphaData, cache, blockX * 4, (out int maskR, out int maskG, out int maskB, out int maskA,
					out byte addR, out byte addG, out byte addB, out byte addA) =>
				{
					maskR = maskG = maskB = 0;
					maskA = byte.MaxValue;
					addR = addG = addB = addA = 0;
				});
				/*ReadBC3AlphaBlock(alphaData, cache, blockX * 4, (ref PixelColor pixel, float value) =>
				{
					pixel.A = value;
				});*/
			}

			public delegate void GetPixelMask(out int maskR, out int maskG, out int maskB, out int maskA,
											out byte addR, out byte addG, out byte addB, out byte addA);

			public static void ReadBC3AlphaBlock(long block, byte[][] cache, int posX, GetPixelMask writeMask)
			{
				writeMask(out var maskR, out var maskG, out var maskB, out var maskA,
						out var addR, out var addG, out var addB, out var addA);
				int alpha0 = (int)(block & 0xFF);
				int alpha1 = (int)((block >> 8) & 0xFF);
				bool isFirstGreater = alpha0 > alpha1;
				block = block >> 16;
				byte[] alphaLookup = new byte[8];
				for (int j = 0; j < 8; j++)
				{
					alphaLookup[j] = (byte) BC3GradientInterpolate(j, alpha0, alpha1, isFirstGreater);
				}
				int i = 0;
				for (int y = 0; y < 4; y++)
				{
					byte[] writeLine = cache[y];
					for (int x = 0; x < 4; x++)
					{
						int alphaIndex = (int)(block >> (i * 3)) & 0x7;
						byte value = alphaLookup[alphaIndex];
						i++;
						int writePos = (posX + x) * 4;
						writeLine[writePos + 0] = (byte) (((value & maskR) + (writeLine[writePos + 0] & ~maskR)) + addR);
						writeLine[writePos + 1] = (byte) (((value & maskG) + (writeLine[writePos + 1] & ~maskG)) + addG);
						writeLine[writePos + 2] = (byte) (((value & maskB) + (writeLine[writePos + 2] & ~maskB)) + addB);
						writeLine[writePos + 3] = (byte) (((value & maskA) + (writeLine[writePos + 3] & ~maskA)) + addA);
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

			protected override void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX)
			{
				DXT5Reader.ReadBC3AlphaBlock(stream.ReadInt64(), cache, blockX * 4, (out int maskR, out int maskG, out int maskB, out int maskA,
					out byte addR, out byte addG, out byte addB, out byte addA) =>
				{
					maskR = byte.MaxValue;
					maskG = maskB = maskA  = 0;
					addR = addG = addB = 0;
					addA = byte.MaxValue;
				});
				/*DXT5Reader.ReadBC3AlphaBlock(stream.ReadInt64(), ref cache, blockX * 4, (ref PixelColor pixel, float value) =>
				{
					pixel.R = value;
					pixel.A = 1f;
				});*/
			}
		}

		private class BC5Reader : BC4Reader
		{
			public BC5Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
			{
			}

			protected override void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX)
			{
				base.ReadBlock(stream, cache, blockX);
				DXT5Reader.ReadBC3AlphaBlock(stream.ReadInt64(), cache, blockX * 4, (out int maskR, out int maskG, out int maskB, out int maskA,
					out byte addR, out byte addG, out byte addB, out byte addA) =>
				{
					maskG = byte.MaxValue;
					maskR  = maskB = maskA = 0;
					addR = addG = addB = addA = 0;
				});
				/*DXT5Reader.ReadBC3AlphaBlock(stream.ReadInt64(), ref cache, blockX * 4, (ref PixelColor pixel, float value) =>
				{
					pixel.G = value;
				});*/
			}
		}

		public abstract class PipelineWriter
		{
			protected int width, height;

			public abstract void WriteLine(byte[] rgba8, bool normalized);

			public abstract void WriteLine(ushort[] rgba16, bool normalized);

			public abstract void WriteLine(uint[] rgba32, bool normalized);

			public abstract void WriteLine(float[] depth, byte[] stencil);

			public abstract void WriteLine(float[] rgba32f);
		}

		private static byte FloatToByte(float v)
		{
			int i = Math.Min(Math.Max((int)Math.Round(v * 255), 0), 255);
			return (byte)i;
		}

		public abstract class SimplePipelineWriter : PipelineWriter
		{
			private byte[] tempBuffer;

			public override void WriteLine(ushort[] rgba16, bool normalized)
			{
				if (tempBuffer == null || tempBuffer.Length < rgba16.Length)
					tempBuffer = new byte[rgba16.Length];
				for (int i = 0, j = rgba16.Length; i < j; i++)
				{
					tempBuffer[i] = (byte) ((rgba16[i] >> 8) & 0xFF);
				}
				WriteLine(tempBuffer, normalized);
			}

			public override void WriteLine(uint[] rgba32, bool normalized)
			{
				if (tempBuffer == null || tempBuffer.Length < rgba32.Length)
					tempBuffer = new byte[rgba32.Length];
				for (int i = 0, j = rgba32.Length; i < j; i++)
				{
					tempBuffer[i] = (byte)((rgba32[i] >> 24) & 0xFF);
				}
				WriteLine(tempBuffer, normalized);
			}

			public override void WriteLine(float[] depth, byte[] stencil)
			{
				if (tempBuffer == null || tempBuffer.Length < depth.Length * 4)
					tempBuffer = new byte[depth.Length * 4];
				for (int i = 0, j = depth.Length; i < j; i++)
				{
					tempBuffer[i * 4] = FloatToByte(depth[i]);
					tempBuffer[i * 4 + 1] = stencil[i];
					tempBuffer[i * 4 + 3] = byte.MaxValue;
				}
				WriteLine(tempBuffer, true);
			}

			public override void WriteLine(float[] rgba32f)
			{
				if (tempBuffer == null || tempBuffer.Length < rgba32f.Length)
					tempBuffer = new byte[rgba32f.Length];
				for (int i = 0, j = rgba32f.Length; i < j; i++)
				{
					tempBuffer[i] = FloatToByte(rgba32f[i]);
				}
				WriteLine(tempBuffer, true);
			}
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

			public override void WriteLine(byte[] rgba8, bool normalized)
			{
				for (int i = 0; i < lineLimiter; i++)
				{
					int j = i * 4;
					// B G R A
					buffer[i] = rgba8[j + 2] | (rgba8[j + 1] << 8) | (rgba8[j] << 16) | (rgba8[j + 3] << 24);
				}
				Marshal.Copy(buffer, 0, data, lineLimiter);
				data = IntPtr.Add(data, stride);
			}

			public override void WriteLine(ushort[] rgba16, bool normalized)
			{
				for (int i = 0; i < lineLimiter; i++)
				{
					int j = i * 4;
					// B G R A
					buffer[i] = (rgba16[j + 2] >> 8) | 
								((rgba16[j + 1] >> 8) << 8) |
								((rgba16[j] >> 8) << 16) |
								((rgba16[j + 3] >> 8) << 24);
				}
				Marshal.Copy(buffer, 0, data, lineLimiter);
				data = IntPtr.Add(data, stride);
			}

			public override void WriteLine(uint[] rgba32, bool normalized)
			{
				for (int i = 0; i < lineLimiter; i++)
				{
					int j = i * 4;
					// B G R A
					buffer[i] =  (int)(rgba32[j + 2] >> 24) |
								((int)(rgba32[j + 1] >> 24) << 8) |
								((int)(rgba32[j] >> 24) << 16) |
								((int)(rgba32[j + 3] >> 24) << 24);
				}
				Marshal.Copy(buffer, 0, data, lineLimiter);
				data = IntPtr.Add(data, stride);
			}

			public override void WriteLine(float[] depth, byte[] stencil)
			{
				for (int i = 0; i < lineLimiter; i++)
				{
					// B G R A
					buffer[i] = 0 |
								(stencil[i] << 8) |
								(FloatToByte(depth[i]) << 16) |
								byte.MaxValue;
				}
				Marshal.Copy(buffer, 0, data, lineLimiter);
				data = IntPtr.Add(data, stride);
			}

			public override void WriteLine(float[] rgba32f)
			{
				for (int i = 0; i < lineLimiter; i++)
				{
					int j = i * 4;
					// B G R A
					buffer[i] = FloatToByte(rgba32f[j + 2]) |
								(FloatToByte(rgba32f[j + 1]) << 8) |
								(FloatToByte(rgba32f[j]) << 16) |
								(FloatToByte(rgba32f[j + 3]) << 24);
				}
				Marshal.Copy(buffer, 0, data, lineLimiter);
				data = IntPtr.Add(data, stride);
			}
		}

		public class RawWriter : PipelineWriter
		{
			private PixelColor[] buffer;
			private int baseOffset = 0;
			private bool bottomToTop, rightToLeft;


			public RawWriter([NotNull] PixelColor[] target, int width, int height, bool bottomToTop, bool rightToLeft)
			{
				this.buffer = target;
				this.width = width;
				this.height = height;
				this.bottomToTop = bottomToTop;
				this.rightToLeft = rightToLeft;
				if (bottomToTop)
					baseOffset = width * (height - 1);
			}

			public override void WriteLine(byte[] rgba8, bool normalized)
			{
				int i = rightToLeft ? width - 1 : 0;
				int inc = rightToLeft ? -1 : 1;
				int end = rightToLeft ? -1 : width;
				for (; i != end; i += inc)
				{
					int j = i * 4;
					if (normalized)
					{
						buffer[baseOffset + i] = new PixelColor(
							rgba8[j] / (float)byte.MaxValue, 
							rgba8[j + 1] / (float)byte.MaxValue, 
							rgba8[j + 2] / (float)byte.MaxValue, 
							rgba8[j + 3] / (float)byte.MaxValue);
					}
					else
					{
						buffer[baseOffset + i] = new PixelColor(rgba8[j], rgba8[j + 1], rgba8[j + 2], rgba8[j + 3]);
					}
				}
				baseOffset += bottomToTop ? width : -width;
			}

			public override void WriteLine(ushort[] rgba16, bool normalized)
			{
				int i = rightToLeft ? width - 1 : 0;
				int inc = rightToLeft ? -1 : 1;
				int end = rightToLeft ? -1 : width;
				for (; i != end; i += inc)
				{
					int j = i * 4;
					if (normalized)
					{
						buffer[baseOffset + i] = new PixelColor(
							rgba16[j] / (float)ushort.MaxValue, 
							rgba16[j + 1] / (float)ushort.MaxValue, 
							rgba16[j + 2] / (float)ushort.MaxValue, 
							rgba16[j + 3] / (float)ushort.MaxValue);
					}
					else
					{
						buffer[baseOffset + i] = new PixelColor(rgba16[j], rgba16[j + 1], rgba16[j + 2], rgba16[j + 3]);
					}
				}
				baseOffset += bottomToTop ? width : -width;
			}

			public override void WriteLine(uint[] rgba32, bool normalized)
			{
				int i = rightToLeft ? width - 1 : 0;
				int inc = rightToLeft ? -1 : 1;
				int end = rightToLeft ? -1 : width;
				for (; i != end; i += inc)
				{
					int j = i * 4;
					if (normalized)
					{
						buffer[baseOffset + i] = new PixelColor(
							(float)(rgba32[j] / (double)uint.MaxValue), 
							(float)(rgba32[j + 1] / (double)uint.MaxValue), 
							(float)(rgba32[j + 2] / (double)uint.MaxValue),
							(float)(rgba32[j + 3] / (double)uint.MaxValue));
					}
					else
					{
						buffer[baseOffset + i] = new PixelColor(rgba32[j], rgba32[j + 1], rgba32[j + 2], rgba32[j + 3]);
					}
				}
				baseOffset += bottomToTop ? width : -width;
			}

			public override void WriteLine(float[] depth, byte[] stencil)
			{
				int i = rightToLeft ? width - 1 : 0;
				int inc = rightToLeft ? -1 : 1;
				int end = rightToLeft ? -1 : width;
				for (; i != end; i += inc)
				{
					buffer[baseOffset + i] = new PixelColor(
						depth[i],
						stencil[i],
						0,
						1f);
				}
				baseOffset += bottomToTop ? width : -width;
			}

			public override void WriteLine(float[] rgba32f)
			{
				int i = rightToLeft ? width - 1 : 0;
				int inc = rightToLeft ? -1 : 1;
				int end = rightToLeft ? -1 : width;
				for (; i != end; i += inc)
				{
					int j = i * 4;
					buffer[baseOffset + i] = new PixelColor(rgba32f[j], rgba32f[j + 1], rgba32f[j + 2], rgba32f[j + 3]);
				}
				baseOffset += bottomToTop ? width : -width;
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

			public uint ReadUInt24()
			{
				byte b0 = data[pointer];
				byte b1 = data[pointer + 1];
				byte b2 = data[pointer + 2];
				pointer += 3;
				return (uint)(b0 | b1 << 8 | b2 << 16);
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

		[Obsolete]
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

#pragma warning restore 612

		#endregion
	}
}