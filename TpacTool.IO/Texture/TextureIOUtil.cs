using System;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public static class TextureIOUtil
	{
		public static bool CanExportAsPng(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.UNKNOWN:
				case TextureFormat.B8G8R8A8_UNORM:
				case TextureFormat.B8G8R8X8_UNORM:
				case TextureFormat.A8_UNORM:
				case TextureFormat.R8G8B8A8_UNORM:
				case TextureFormat.L8_UNORM:
				case TextureFormat.R8_UNORM:
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
					return true;
				default:
					return false;
			}
		}

		public static bool CanExportAsDds(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.UNKNOWN:
				case TextureFormat.B8G8R8A8_UNORM:
				case TextureFormat.B8G8R8X8_UNORM:
				case TextureFormat.R8G8B8A8_UNORM:
				case TextureFormat.R16G16_UNORM:
				case TextureFormat.R16G16F:
				case TextureFormat.R8_UNORM:
				case TextureFormat.R8G8B8:
				case TextureFormat.B8G8R8:
				case TextureFormat.R8G8_UNORM:
				case TextureFormat.R32_UINT:
				case TextureFormat.R8G8B8A8_UINT:
				case TextureFormat.R16G16B16A16_UNORM:
				case TextureFormat.R16_UNORM:
				case TextureFormat.R16F:
				case TextureFormat.DXT1:
				case TextureFormat.DXT2:
				case TextureFormat.DXT3:
				case TextureFormat.DXT4:
				case TextureFormat.DXT5:
				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.BC6H_UF16:
				case TextureFormat.BC7:
				case TextureFormat.D24_UNORM_S8_UINT:
				case TextureFormat.D24_UNORM_X8_UINT:
				case TextureFormat.D16_UNORM:
				case TextureFormat.D32F:
				case TextureFormat.L16_UNORM:
				case TextureFormat.INDEX16:
				case TextureFormat.INDEX32:
				case TextureFormat.R16G16B16A16F:
				case TextureFormat.R32F:
				case TextureFormat.R32G32B32F:
				case TextureFormat.R32G32B32A32F:
				case TextureFormat.DF24:
				case TextureFormat.ATOC:
				case TextureFormat.A2M0:
				case TextureFormat.A2M1:
				case TextureFormat.R11G11B10F:
				case TextureFormat.R16G16B16:
				case TextureFormat.R32G32B32A32_UINT:
				case TextureFormat.R8_UINT:
				case TextureFormat.R16_UINT:
				case TextureFormat.R24G8_TYPELESS:
				case TextureFormat.R32G32B32_UINT:
				case TextureFormat.D32_S8X24_UINT:
				case TextureFormat.R16G16_UINT:
				case TextureFormat.R32G32F:
				case TextureFormat.R32G32_UINT:
				case TextureFormat.R16G16B16A16_UINT:
					return true;
				default:
					return false;
			}
		}

		public static string GetSuggestedFormat(this TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.UNKNOWN:
				case TextureFormat.B8G8R8A8_UNORM:
				case TextureFormat.B8G8R8X8_UNORM:
				case TextureFormat.A8_UNORM:
				case TextureFormat.R8G8B8A8_UNORM:
				case TextureFormat.L8_UNORM:
				case TextureFormat.R8_UNORM:
				case TextureFormat.R8G8B8:
				case TextureFormat.B8G8R8:
				case TextureFormat.R8G8_UNORM:
					return "PNG";
				case TextureFormat.R16G16_UNORM:
				case TextureFormat.R16G16F:
				case TextureFormat.R32_UINT:
				case TextureFormat.R8G8B8A8_UINT:
				case TextureFormat.R16G16B16A16_UNORM:
				case TextureFormat.R16_UNORM:
				case TextureFormat.R16F:
				case TextureFormat.DXT1:
				case TextureFormat.DXT2:
				case TextureFormat.DXT3:
				case TextureFormat.DXT4:
				case TextureFormat.DXT5:
				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.BC6H_UF16:
				case TextureFormat.BC7:
				case TextureFormat.D24_UNORM_S8_UINT:
				case TextureFormat.D24_UNORM_X8_UINT:
				case TextureFormat.D16_UNORM:
				case TextureFormat.D32F:
				case TextureFormat.L16_UNORM:
				case TextureFormat.INDEX16:
				case TextureFormat.INDEX32:
				case TextureFormat.R16G16B16A16F:
				case TextureFormat.R32F:
				case TextureFormat.R32G32B32F:
				case TextureFormat.R32G32B32A32F:
				case TextureFormat.DF24:
				case TextureFormat.ATOC:
				case TextureFormat.A2M0:
				case TextureFormat.A2M1:
				case TextureFormat.R11G11B10F:
				case TextureFormat.R16G16B16:
				case TextureFormat.R32G32B32A32_UINT:
				case TextureFormat.R8_UINT:
				case TextureFormat.R16_UINT:
				case TextureFormat.R24G8_TYPELESS:
				case TextureFormat.R32G32B32_UINT:
				case TextureFormat.D32_S8X24_UINT:
				case TextureFormat.R16G16_UINT:
				case TextureFormat.R32G32F:
				case TextureFormat.R32G32_UINT:
				case TextureFormat.R16G16B16A16_UINT:
					return "DDS";
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}
	}
}