using System;
using System.IO;
using JetBrains.Annotations;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public static class TextureExporter
	{
		public static void ExportToFolder<T>([NotNull] string path, [NotNull] Texture texture,
			TextureExportOption option = 0)
			where T : AbstractTextureExporter, new()
		{
			if (!texture.HasPixelData)
				throw new ArgumentException("Texture " + texture.Name + " has no pixel data");

			path = new DirectoryInfo(path).FullName + "\\";
			Directory.CreateDirectory(path);
			string baseName = path + texture.Name;
			if (texture.ArrayCount > 1 && !option.HasFlag(TextureExportOption.NoArray))
			{
				
				for (int i = 0; i < texture.ArrayCount; i++)
				{
					T exporter = new T();
					exporter.Texture = texture;
					exporter.IgnoreMipmap = option.HasFlag(TextureExportOption.NoMipmap);
					exporter.SpecifyArray = i;
					string fileName;
					if (i == 0)
					{
						fileName = baseName + "." + exporter.Extension;
					}
					else
					{
						fileName = baseName + "_" + i + "." + exporter.Extension;
					}
					exporter.Export(fileName);
				}
			}
			else
			{
				T exporter = new T();
				exporter.Texture = texture;
				exporter.IgnoreMipmap = option.HasFlag(TextureExportOption.NoMipmap);
				exporter.IgnoreArray = option.HasFlag(TextureExportOption.NoArray);
				exporter.Export(baseName + "." + exporter.Extension);
			}
		}

		public static void ExportToFile([NotNull] string path, [NotNull] Texture texture,
			TextureExportOption option = 0)
		{
			if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				ExportToFile<PngExporter>(path, texture, option);
			else if (path.EndsWith(".dds", StringComparison.OrdinalIgnoreCase))
				ExportToFile<DdsExporter>(path, texture, option);
			else
				throw new FormatException("Unsupported export format");
		}

		public static void ExportToFile<T>([NotNull] string path, [NotNull] Texture texture, 
			TextureExportOption option = 0) 
			where T : AbstractTextureExporter, new()
		{
			if (!texture.HasPixelData)
				throw new ArgumentException("Texture " + texture.Name + " has no pixel data");
			if (texture.ArrayCount > 1 && !option.HasFlag(TextureExportOption.NoArray))
			{
				int pos = path.LastIndexOf('.');
				string baseName = path.Substring(0, pos);
				string extName = path.Substring(pos);
				for (int i = 0; i < texture.ArrayCount; i++)
				{
					T exporter = new T();
					exporter.Texture = texture;
					exporter.IgnoreMipmap = option.HasFlag(TextureExportOption.NoMipmap);
					exporter.SpecifyArray = i;
					string fileName = path;
					if (i > 0)
					{
						fileName = baseName + "_" + i + extName;
					}
					exporter.Export(fileName);
				}
			}
			else
			{
				T exporter = new T();
				exporter.Texture = texture;
				exporter.IgnoreMipmap = option.HasFlag(TextureExportOption.NoMipmap);
				exporter.IgnoreArray = option.HasFlag(TextureExportOption.NoArray);
				exporter.Export(path);
			}
		}

		[Flags]
		public enum TextureExportOption
		{
			NoMipmap = 0x1,
			NoArray = 0x2,
			SplitCube = 0x1000,
		}
	}
}