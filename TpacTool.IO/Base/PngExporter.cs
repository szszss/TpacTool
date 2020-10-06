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
			private int x, y;

			public PngWriter(PngBuilder builder, int width)
			{
				this.builder = builder;
				this.width = width;
			}

			public override void Write(TextureUtil.PixelColor color)
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
			}
		}

		private static byte SingleToByte(float value)
		{
			return (byte)Math.Max(Math.Min((int)(value * 255), 255), 0);
		}
	}
}