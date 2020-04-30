using System;
using System.IO;
using JetBrains.Annotations;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public static class MaterialExporter
	{
		public static void ExportToFolder([NotNull] string path, [NotNull] Material material,
			MaterialExportOption option = 0)
		{

			path = new DirectoryInfo(path).FullName + "\\";
			Directory.CreateDirectory(path);
			foreach (var texDep in material.Textures.Values)
			{
				if (texDep.TryGetItem(out var tex) && tex.HasPixelData)
				{
					var extName = GetBestTextureFormat(tex.Format, option);
					var fileName = string.Format("{0}{1}.{2}", path, tex.Name, extName);
					TextureExporter.ExportToFile(fileName, tex);
				}
			}
		}

		public static string GetBestTextureFormat(TextureFormat format, MaterialExportOption option = 0)
		{
			if (option.HasFlag(MaterialExportOption.PreferDds))
			{
				if (format.CanExportAsDds())
					return "dds";
				return "png";
			}
			else if (option.HasFlag(MaterialExportOption.PreferPng))
			{
				if (format.CanExportAsPng())
					return "png";
				return "dds";
			}
			else
			{
				return format.GetSuggestedFormat().ToLower();
			}
		}

		[Flags]
		public enum MaterialExportOption
		{
			PreferPng = 0x100000,
			PreferDds = 0x200000
		}
	}
}