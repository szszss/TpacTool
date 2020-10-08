using System;
using System.IO;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public class PngExporter : AbstractTextureExporter
	{
		public override string Extension => "png";

		public override void Export(Stream writeStream)
		{
			CheckStreamAndTexture(writeStream);

			int width = (int) Texture.Width;
			int height = (int) Texture.Height;
			var builder = PngBuilder.Create(width, height, Texture.Format.HasAlpha());
			var writer = new PngWriter(builder, width);
			byte[] data = Texture.TexturePixels.Data.PrimaryRawImage;
			if (SpecifyArray >= 0 && SpecifyArray < Texture.ArrayCount)
			{
				data = Texture.TexturePixels.Data.RawImage[SpecifyArray][0];
			}
			TextureUtil.DecodeTextureDataToWriter(data, width, height, Texture.Format, writer);
			builder.Save(writeStream);
		}

		private class PngWriter : TextureUtil.PipelineWriter
		{
			private PngBuilder builder;
			private int y;

			public PngWriter(PngBuilder builder, int width)
			{
				this.builder = builder;
				this.width = width;
			}

			public override void WriteLine(byte[] rgba8, bool normalized)
			{
				for (int x = 0, readPos = 0; x < width; x++, readPos = x * 4)
				{
					builder.SetPixel(
						new Pixel(rgba8[readPos], 
							rgba8[readPos + 1], 
							rgba8[readPos + 2], 
							rgba8[readPos + 3], false), x, y);
				}
				y++;
			}

			public override void WriteLine(ushort[] rgba16, bool normalized)
			{
				for (int x = 0, readPos = 0; x < width; x++, readPos = x * 4)
				{
					builder.SetPixel(
						new Pixel(ShortToByte(rgba16[readPos]), 
							ShortToByte(rgba16[readPos + 1]), 
							ShortToByte(rgba16[readPos + 2]),
							ShortToByte(rgba16[readPos + 3]), false), x, y);
				}
				y++;
			}

			public override void WriteLine(uint[] rgba32, bool normalized)
			{
				for (int x = 0, readPos = 0; x < width; x++, readPos = x * 4)
				{
					builder.SetPixel(new Pixel(IntToByte(rgba32[readPos]),
						IntToByte(rgba32[readPos + 1]),
						IntToByte(rgba32[readPos + 2]),
						IntToByte(rgba32[readPos + 3]), false), x, y);
				}
				y++;
			}

			public override void WriteLine(float[] depth, byte[] stencil)
			{
				for (int x = 0; x < width; x++)
				{
					builder.SetPixel(SingleToByte(depth[x]), stencil[x], 0, x, y);
				}
				y++;
			}

			public override void WriteLine(float[] rgba32f)
			{
				for (int x = 0, readPos = 0; x < width; x++, readPos = x * 4)
				{
					builder.SetPixel(new Pixel(SingleToByte(rgba32f[readPos]), 
						SingleToByte(rgba32f[readPos + 1]), 
						SingleToByte(rgba32f[readPos + 2]),
						SingleToByte(rgba32f[readPos + 3]), false), x, y);
				}
				y++;
			}

			/*public override void Write(TextureUtil.PixelColor color)
			{
				builder.SetPixel(new Pixel(SingleToByte(color.R),
											SingleToByte(color.G),
											SingleToByte(color.B),
											SingleToByte(color.A), false), x, y);
				if (++x >= width)
				{
					x = 0;
					y++;
				}
			}*/

			private static byte ShortToByte(ushort value)
			{
				return (byte) (value >> 8);
			}

			private static byte IntToByte(uint value)
			{
				return (byte)(value >> 24);
			}

			private static byte SingleToByte(float value)
			{
				return (byte)Math.Max(Math.Min((int)(value * byte.MaxValue), byte.MaxValue), 0);
			}
		}
	}
}