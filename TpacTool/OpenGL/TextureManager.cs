using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using TpacTool.Lib;
using static OpenTK.Graphics.OpenGL4.GL;

namespace TpacTool
{
	public static class TextureManager
	{
		private const int MAX_CACHE = 16;
		private static readonly LinkedList<(Texture, OglTexture)> cache = new LinkedList<(Texture, OglTexture)>();
		private static OglTexture _fallbackTexture;

		public static OglTexture FALLBACK_TEXTURE
		{
			get
			{
				if (_fallbackTexture == null)
					_fallbackTexture = new OglTexture(null);
				return _fallbackTexture;
			}
		}

		public static OglTexture Get(Texture texture, int maxTextureSize = 0)
		{
			LinkedListNode<(Texture, OglTexture)> node;
			for (node = cache.First; node != null; node = node.Next)
			{
				if (node.Value.Item1 == texture)
					break;
			}

			if (node != null)
			{
				if (node.Previous != null)
				{
					cache.Remove(node);
					cache.AddFirst(node);
				}

				return node.Value.Item2;
			}

			var tex = new OglTexture(texture, maxTextureSize);
			cache.AddFirst((texture, tex));

			while (cache.Count > MAX_CACHE)
			{
				cache.Last.Value.Item2.Release();
				cache.RemoveLast();
			}

			return tex;
		}

		public static void Clear()
		{
			foreach (var (_, oglTexture) in cache)
			{
				oglTexture.Release();
			}
			cache.Clear();
		}

		public class OglTexture
		{
			private int _texId = -1;

			private static readonly byte[] FALLBACK_TEXTURE =
			{
				0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF,
				0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF,
				0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF,
				0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF, 0xC0, 0xC0, 0xC0, 0xFF
			};

			public OglTexture(Texture texture, int maxTextureSize = 0)
			{
				_texId = GenTexture();

				BindTexture(TextureTarget.Texture2D, _texId);

				var internalFormat = InternalFormat.Rgba8;
				var pixelInternalFormat = PixelInternalFormat.Rgba8;
				var pixelFormat = PixelFormat.Rgba;
				var pixelType = PixelType.UnsignedByte;
				var isCompressed = false;
				var available = texture != null;
				var format = texture?.Format ?? TextureFormat.UNKNOWN;

				switch (format)
				{
					case TextureFormat.UNKNOWN:
						available = false;
						break;
					case TextureFormat.B8G8R8A8_UNORM:
						pixelInternalFormat = PixelInternalFormat.Rgba;
						pixelFormat = PixelFormat.Bgra;
						break;
					case TextureFormat.B8G8R8X8_UNORM:
						pixelInternalFormat = PixelInternalFormat.Rgb;
						pixelFormat = PixelFormat.Bgra;
						break;
					case TextureFormat.R16G16_UNORM:
						pixelInternalFormat = PixelInternalFormat.Rg16;
						pixelFormat = PixelFormat.Rg;
						pixelType = PixelType.UnsignedShort;
						break;
					case TextureFormat.R16G16F:
						pixelInternalFormat = PixelInternalFormat.Rg16;
						pixelFormat = PixelFormat.Rg;
						pixelType = PixelType.Float;
						break;
					case TextureFormat.R32_UINT:
						pixelInternalFormat = PixelInternalFormat.R32ui;
						pixelFormat = PixelFormat.RedInteger;
						pixelType = PixelType.UnsignedInt;
						break;
					case TextureFormat.A8_UNORM:
						pixelInternalFormat = PixelInternalFormat.R8;
						pixelFormat = PixelFormat.Alpha;
						break;
					case TextureFormat.R8G8B8A8_UNORM:
						pixelInternalFormat = PixelInternalFormat.Rgba;
						pixelFormat = PixelFormat.Rgba;
						break;
					case TextureFormat.R8G8B8A8_UINT:
						pixelInternalFormat = PixelInternalFormat.Rgba8ui;
						pixelFormat = PixelFormat.RgbaInteger;
						break;
					case TextureFormat.R16G16B16A16_UNORM:
						pixelInternalFormat = PixelInternalFormat.Rgba16;
						pixelFormat = PixelFormat.Rgba;
						pixelType = PixelType.UnsignedShort;
						break;
					case TextureFormat.L8_UNORM:
						pixelInternalFormat = PixelInternalFormat.R8;
						pixelFormat = PixelFormat.Luminance;
						break;
					case TextureFormat.R8_UNORM:
						pixelInternalFormat = PixelInternalFormat.R8;
						pixelFormat = PixelFormat.Red;
						break;
					case TextureFormat.R16_UNORM:
						pixelInternalFormat = PixelInternalFormat.R16;
						pixelFormat = PixelFormat.Red;
						pixelType = PixelType.UnsignedShort;
						break;
					case TextureFormat.R16F:
						pixelInternalFormat = PixelInternalFormat.R16f;
						pixelFormat = PixelFormat.Red;
						pixelType = PixelType.HalfFloat;
						break;
					case TextureFormat.DXT1:
						internalFormat = InternalFormat.CompressedRgbaS3tcDxt1Ext;
						isCompressed = true;
						break;
					case TextureFormat.DXT2:
					case TextureFormat.DXT3:
						internalFormat = InternalFormat.CompressedRgbaS3tcDxt3Ext;
						isCompressed = true;
						break;
					case TextureFormat.DXT4:
					case TextureFormat.DXT5:
						internalFormat = InternalFormat.CompressedRgbaS3tcDxt5Ext;
						isCompressed = true;
						break;
					case TextureFormat.BC4:
						internalFormat = InternalFormat.CompressedRedRgtc1;
						isCompressed = true;
						break;
					case TextureFormat.BC5:
						internalFormat = InternalFormat.CompressedRgRgtc2;
						isCompressed = true;
						break;
					case TextureFormat.BC6H_UF16:
						internalFormat = InternalFormat.CompressedRgbBptcUnsignedFloat;
						isCompressed = true;
						break;
					case TextureFormat.BC7:
						internalFormat = InternalFormat.CompressedRgbaBptcUnorm;
						isCompressed = true;
						break;
					case TextureFormat.D24_UNORM_S8_UINT:
						pixelInternalFormat = PixelInternalFormat.Depth24Stencil8;
						pixelFormat = PixelFormat.DepthStencil;
						pixelType = PixelType.UnsignedInt248;
						break;
					case TextureFormat.D24_UNORM_X8_UINT:
						pixelInternalFormat = PixelInternalFormat.DepthComponent;
						pixelFormat = PixelFormat.DepthComponent;
						pixelType = PixelType.UnsignedInt248;
						break;
					case TextureFormat.D16_UNORM:
						pixelInternalFormat = PixelInternalFormat.DepthComponent16;
						pixelFormat = PixelFormat.DepthComponent;
						pixelType = PixelType.UnsignedShort;
						break;
					case TextureFormat.D32F:
						pixelInternalFormat = PixelInternalFormat.DepthComponent32f;
						pixelFormat = PixelFormat.DepthComponent;
						pixelType = PixelType.Float;
						break;
					case TextureFormat.L16_UNORM:
						pixelInternalFormat = PixelInternalFormat.R16;
						pixelFormat = PixelFormat.Luminance;
						pixelType = PixelType.UnsignedShort;
						break;
					case TextureFormat.INDEX16:
					case TextureFormat.INDEX32:
						available = false;
						break;
					case TextureFormat.R16G16B16A16F:
						pixelInternalFormat = PixelInternalFormat.Rgba16f;
						pixelFormat = PixelFormat.Rgba;
						pixelType = PixelType.HalfFloat;
						break;
					case TextureFormat.R32F:
						pixelInternalFormat = PixelInternalFormat.R32f;
						pixelFormat = PixelFormat.Red;
						pixelType = PixelType.Float;
						break;
					case TextureFormat.R32G32B32F:
						// OpenTK lacks Rgb32f (GL_RGB32F 0x8815)
						pixelInternalFormat = PixelInternalFormat.Rgba32f;
						pixelFormat = PixelFormat.Rgb;
						pixelType = PixelType.Float;
						break;
					case TextureFormat.R32G32B32A32F:
						pixelInternalFormat = PixelInternalFormat.Rgba32f;
						pixelFormat = PixelFormat.Rgba;
						pixelType = PixelType.Float;
						break;
					case TextureFormat.DF24: // is it depth float 24?
					case TextureFormat.ATOC:
					case TextureFormat.A2M0:
					case TextureFormat.A2M1:
						available = false;
						break;
					case TextureFormat.R11G11B10F:
						pixelInternalFormat = PixelInternalFormat.R11fG11fB10f;
						pixelFormat = PixelFormat.Rgb;
						pixelType = PixelType.UnsignedInt10F11F11FRev;
						break;
					case TextureFormat.R16G16B16:
						pixelInternalFormat = PixelInternalFormat.Rgb16;
						pixelFormat = PixelFormat.Rgb;
						pixelType = PixelType.UnsignedShort;
						break;
					case TextureFormat.R8G8B8:
						pixelInternalFormat = PixelInternalFormat.Rgb8;
						pixelFormat = PixelFormat.Rgb;
						break;
					case TextureFormat.B8G8R8:
						pixelInternalFormat = PixelInternalFormat.Rgb8;
						pixelFormat = PixelFormat.Bgr;
						break;
					case TextureFormat.R32G32B32A32_UINT:
						pixelInternalFormat = PixelInternalFormat.Rgba32ui;
						pixelFormat = PixelFormat.RgbaInteger;
						pixelType = PixelType.UnsignedInt;
						break;
					case TextureFormat.R8_UINT:
						pixelInternalFormat = PixelInternalFormat.R8ui;
						pixelFormat = PixelFormat.RedInteger;
						break;
					case TextureFormat.R16_UINT:
						pixelInternalFormat = PixelInternalFormat.R16ui;
						pixelFormat = PixelFormat.RedInteger;
						pixelType = PixelType.UnsignedShort;
						break;
					case TextureFormat.R24G8_TYPELESS:
						pixelInternalFormat = PixelInternalFormat.Depth24Stencil8;
						pixelFormat = PixelFormat.DepthStencil;
						pixelType = PixelType.UnsignedInt248;
						break;
					case TextureFormat.R32G32B32_UINT:
						pixelInternalFormat = PixelInternalFormat.Rgb32ui;
						pixelFormat = PixelFormat.RgbInteger;
						pixelType = PixelType.UnsignedInt;
						break;
					case TextureFormat.D32_S8X24_UINT:
						pixelInternalFormat = PixelInternalFormat.Depth32fStencil8;
						pixelFormat = PixelFormat.DepthStencil;
						pixelType = PixelType.Float32UnsignedInt248Rev;
						break;
					case TextureFormat.R16G16_UINT:
						pixelInternalFormat = PixelInternalFormat.Rg16ui;
						pixelFormat = PixelFormat.RgInteger;
						pixelType = PixelType.UnsignedShort;
						break;
					case TextureFormat.R8G8_UNORM:
						pixelInternalFormat = PixelInternalFormat.Rg8;
						pixelFormat = PixelFormat.Rg;
						break;
					case TextureFormat.R32G32F:
						pixelInternalFormat = PixelInternalFormat.Rg32f;
						pixelFormat = PixelFormat.Rg;
						pixelType = PixelType.Float;
						break;
					case TextureFormat.R32G32_UINT:
						pixelInternalFormat = PixelInternalFormat.Rg32ui;
						pixelFormat = PixelFormat.RgInteger;
						pixelType = PixelType.UnsignedInt;
						break;
					case TextureFormat.R16G16B16A16_UINT:
						pixelInternalFormat = PixelInternalFormat.Rgba16ui;
						pixelFormat = PixelFormat.RgbaInteger;
						pixelType = PixelType.UnsignedShort;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if (available && texture.HasPixelData && texture.TexturePixels != null)
				{
					int width = (int)texture.Width;
					int height = (int)texture.Height;
					int targetMipmap = 0;
					int maxMipmap = texture.MipmapCount - 1;
					while (maxTextureSize > 0 && width > maxTextureSize && height > maxTextureSize && targetMipmap < maxMipmap)
					{
						width >>= 1;
						height >>= 1;
						targetMipmap++;
					}
					var data = texture.TexturePixels.Data.RawImage[0][targetMipmap];

					if (isCompressed)
						CompressedTexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, data.Length, data);
					else
						TexImage2D(TextureTarget.Texture2D, 0, pixelInternalFormat, width, height, 0, pixelFormat, pixelType, data);

					GenerateMipmap(GenerateMipmapTarget.Texture2D);
					TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 0x2703); // GL_LINEAR_MIPMAP_LINEAR
					TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 0x2601); // GL_LINEAR
				}
				else
				{
					TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 4, 4, 0, PixelFormat.Rgba, PixelType.UnsignedByte, FALLBACK_TEXTURE);
					TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 0x2600); // GL_NEAREST
					TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 0x2600); // GL_NEAREST
				}

				BindTexture(TextureTarget.Texture2D, 0);
			}

			public void Bind()
			{
				if (_texId >= 0)
					BindTexture(TextureTarget.Texture2D, _texId);
			}

			public void Release()
			{
				if (_texId >= 0)
				{
					DeleteTexture(_texId);
					_texId = -1;
				}
			}
		}

		public static bool IsFormatSupported(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.UNKNOWN:
				case TextureFormat.INDEX16:
				case TextureFormat.INDEX32:
				case TextureFormat.DF24:
				case TextureFormat.ATOC:
				case TextureFormat.A2M0:
				case TextureFormat.A2M1:
					return false;
			}
			return true;
		}
	}
}