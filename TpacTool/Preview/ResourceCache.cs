using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using TpacTool.Lib;
using BoundingBox = SharpDX.BoundingBox;

namespace TpacTool
{
	public static class ResourceCache
	{
		public const int CHANNEL_MODE_RGBA = 1;
		public const int CHANNEL_MODE_RGB = 2;
		public const int CHANNEL_MODE_RG = 3;
		public const int CHANNEL_MODE_R = 4;
		public const int CHANNEL_MODE_ALPHA = 5;

		private static int imageCacheCleanCounter = 0;
		private static int modelCacheCleanCounter = 0;
		private static Dictionary<Tuple<Texture, int, int>, WeakReference<BitmapSource>> imageCache = 
							new Dictionary<Tuple<Texture, int, int>, WeakReference<BitmapSource>>();
		private static Dictionary<Mesh, WeakReference<MeshBaker.BakedMesh>> modelCache =
							new Dictionary<Mesh, WeakReference<MeshBaker.BakedMesh>>();

		public static MeshBaker.BakedMesh GetModel(Mesh mesh)
		{
			MeshBaker.BakedMesh target = null;
			var key = mesh;
			lock (modelCache)
			{
				if (!modelCache.TryGetValue(key, out var weakRef) || !weakRef.TryGetTarget(out target))
				{
					if (++modelCacheCleanCounter > 100)
					{
						modelCacheCleanCounter = 0;
						var removed = modelCache.Where(pair => !pair.Value.TryGetTarget(out var unused)).ToArray();
						foreach (var item in removed)
							modelCache.Remove(item.Key);
					}

					target = MeshBaker.BakeMesh(mesh, true);
					modelCache[key] = new WeakReference<MeshBaker.BakedMesh>(target);
				}
			}
			return target;
		}

		public static BitmapSource GetImage(Texture texture, int clampSize, int mode)
		{
			BitmapSource target = null;
			int width = (int)texture.Width;
			int height = (int)texture.Height;
			int targetMipmap = 0;
			int maxMipmap = texture.MipmapCount - 1;
			while (clampSize != 0 && width > clampSize && height > clampSize && targetMipmap < maxMipmap)
			{
				width >>= 1;
				height >>= 1;
				targetMipmap++;
			}
			var key = Tuple.Create(texture, targetMipmap, mode);
			lock (imageCache)
			{
				if (!imageCache.TryGetValue(key, out var weakRef) || !weakRef.TryGetTarget(out target))
				{
					if (++imageCacheCleanCounter > 100)
					{
						imageCacheCleanCounter = 0;
						var removed = imageCache.Where(pair => !pair.Value.TryGetTarget(out var unused)).ToArray();
						foreach (var item in removed)
							imageCache.Remove(item.Key);
					}

					PixelFormat pf = PixelFormats.Default;
					byte[] rawData = texture.TexturePixels.Data.RawImage[0][targetMipmap];
					int stride = 0;
					switch (mode)
					{
						case CHANNEL_MODE_RGBA:
							pf = PixelFormats.Bgra32;
							stride = width * 4;
							break;
						case CHANNEL_MODE_RGB:
							pf = PixelFormats.Bgr24;
							stride = width * 3;
							break;
						case CHANNEL_MODE_RG:
							pf = PixelFormats.Bgr24;
							stride = width * 3;
							break;
						case CHANNEL_MODE_R:
							pf = PixelFormats.Gray8;
							stride = width * 1;
							break;
						case CHANNEL_MODE_ALPHA:
							pf = PixelFormats.Gray8;
							stride = width * 1;
							break;
						default:
							throw new ArgumentOutOfRangeException("mode");
					}
					byte[] rawImage = new byte[stride * height];
					var writer = new ImageWriter(rawImage, width, mode);
					TextureUtil.DecodeTextureDataToWriter(rawData, width, height, texture.Format, writer);
					/*int length = width * height;
					for (int i = 0; i < length; i++)
					{
						rawImage[i * 4] = 255; // b
						rawImage[i * 4 + 1] = 127; // g
						rawImage[i * 4 + 2] = 63; // r
						rawImage[i * 4 + 3] = 255; // a
					}*/
					target = BitmapSource.Create(width, height,
						96, 96, pf, null,
						rawImage, stride);
					imageCache[key] = new WeakReference<BitmapSource>(target);
				}
			}
			return target;
		}

		public static void Cleanup()
		{
			imageCache.Clear();
			modelCache.Clear();
		}

		private class ImageWriter : TextureUtil.SimplePipelineWriter
		{
			private byte[] buff;
			private int pointer;
			private int mode;

			public ImageWriter(byte[] buff, int width, int mode)
			{
				this.buff = buff;
				this.width = width;
				this.mode = mode;
			}

			public override void WriteLine(byte[] rgba8, bool normalized)
			{
				for (int i = 0; i < width; i++)
				{
					int j = i * 4;
					switch (mode)
					{
						case CHANNEL_MODE_RGBA:
							buff[pointer++] = rgba8[j + 2];
							buff[pointer++] = rgba8[j + 1];
							buff[pointer++] = rgba8[j + 0];
							buff[pointer++] = rgba8[j + 3];
							break;
						case CHANNEL_MODE_RGB:
							buff[pointer++] = rgba8[j + 2];
							buff[pointer++] = rgba8[j + 1];
							buff[pointer++] = rgba8[j + 0];
							break;
						case CHANNEL_MODE_RG:
							buff[pointer++] = 0;
							buff[pointer++] = rgba8[j + 1];
							buff[pointer++] = rgba8[j + 0];
							break;
						case CHANNEL_MODE_R:
							buff[pointer++] = rgba8[j + 0];
							break;
						case CHANNEL_MODE_ALPHA:
							buff[pointer++] = rgba8[j + 3];
							break;
					}
				}
			}
		}
	}
}