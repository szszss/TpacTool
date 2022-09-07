using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace TpacTool.Lib
{
	public class TexturePixelData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("70ee4e2c-79e4-4b2d-8d54-d53ecd2a559c");

		public const string KEY_WIDTH = "width";

		public const string KEY_HEIGHT = "height";

		public const string KEY_ARRAY = "array";

		public const string KEY_MIPMAP = "mipmap";

		public const string KEY_FORMAT = "format";

		/*public const string KEY_PIXELSIZE = "pixelSize";

		public const string KEY_ALIGN = "align";*/

		public byte[][][] RawImage { set; get; }

		public byte[] PrimaryRawImage { set; get; }

		public TexturePixelData() : base(TYPE_GUID)
		{
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			int array = 1, mipmap = 1;
			if (userdata.TryGetValue(KEY_ARRAY, out var arrayObj))
			{
				array = (int) arrayObj;
			}
			if (userdata.TryGetValue(KEY_MIPMAP, out var mipmapObj))
			{
				mipmap = (int)mipmapObj;
			}
			if (!userdata.TryGetValue(KEY_WIDTH, out var widthObj) ||
				!userdata.TryGetValue(KEY_HEIGHT, out var heightObj) ||
				!userdata.TryGetValue(KEY_FORMAT, out var formatObj))
			{
				throw new ArgumentException("No enough user data for width, height and format");
			}

			int width = (int) widthObj;
			int height = (int) heightObj;
			TextureFormat format = (TextureFormat)formatObj;
			int aligh = format.GetAlignSize();
			int pixelSize = format.GetBitsPerPixel();

			//var raw = stream.ReadBytes(totalSize);

			var raw = new byte[array][][];
			for (int a = 0; a < array; a++)
			{
				raw[a] = new byte[mipmap][];
				int imageWidth = width;
				int imageHeight = height;
				for (int m = 0; m < mipmap; m++)
				{
					int alignedWidth = imageWidth;
					int alignedHeight = imageHeight;
					if (alignedWidth % aligh != 0)
					{
						alignedWidth += aligh - (alignedWidth % aligh);
					}
					if (alignedHeight % aligh != 0)
					{
						alignedHeight += aligh - (alignedHeight % aligh);
					}
					// prevent from overflow for mega textures or underflow for very small mipmap
					// should be alignedWidth * alignedHeight * pixelSize / 8
					int readSize = alignedWidth * alignedHeight;
					// if readSize & 7 != 0, then it cannot be divided exactly
					if (readSize >= 8 && (readSize & 7) == 0)
						readSize = readSize / 8 * pixelSize;
					else
						readSize = readSize * pixelSize / 8;
					raw[a][m] = stream.ReadBytes(readSize);
					imageWidth = Math.Max(imageWidth >> 1, 1);
					imageHeight = Math.Max(imageHeight >> 1, 1);
				}
			}

			RawImage = raw;
			PrimaryRawImage = raw[0][0];
		}

		public override void WriteData(BinaryWriter stream, IDictionary<object, object> userdata)
		{
			int array = 1, mipmap = 1;
			if (userdata.TryGetValue(KEY_ARRAY, out var arrayObj))
			{
				array = (int)arrayObj;
			}
			if (userdata.TryGetValue(KEY_MIPMAP, out var mipmapObj))
			{
				mipmap = (int)mipmapObj;
			}

			for (int a = 0; a < array; a++)
			{
				var raw = RawImage[a];

				for (int m = 0; m < mipmap; m++)
				{
					stream.Write(raw[m]);
				}
			}
		}
	}
}