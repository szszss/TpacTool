using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public static class ModelExporter
	{
		public static void ExportToFile([NotNull] string path, [NotNull] Metamesh model,
			[CanBeNull] Skeleton skeleton, ModelExportOption option = 0)
		{
			if (path.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
				ExportToFile<WavefrontExporter>(path, model, skeleton, option);
			else if (path.EndsWith(".dae", StringComparison.OrdinalIgnoreCase))
				ExportToFile<ColladaExporter>(path, model, skeleton, option);
			else
				throw new FormatException("Unsupported export format");
		}

		public static void ExportToFile<T>([NotNull] string path, [NotNull] Metamesh model,
			[CanBeNull] Skeleton skeleton, ModelExportOption option = 0)
			where T : AbstractModelExporter, new()
		{
			T exporter = new T();
			var dirPath = Path.GetDirectoryName(path) + "/";

			var exportedMeshes = model.Meshes.FindAll(mesh => mesh.Lod == 0);
			HashSet<Texture> textures = new HashSet<Texture>();
			foreach (var mesh in exportedMeshes)
			{
				if (mesh.Material.TryGetItem(out var mat1))
				{
					foreach (var texDep in mat1.Textures.Values)
					{
						if (texDep.TryGetItem(out var tex))
							textures.Add(tex);
					}
				}

				if (exporter.SupportsSecondMaterial && mesh.Material.TryGetItem(out var mat2))
				{
					foreach (var texDep in mat2.Textures.Values)
					{
						if (texDep.TryGetItem(out var tex))
							textures.Add(tex);
					}
				}
			}

			string prefix = option.HasFlag(ModelExportOption.ExportTexturesSubFolder)
				? model.Name + "/"
				: string.Empty;
			foreach (var tex in textures)
			{
				var texRelPath = prefix + tex.Name + "." + GetTextureFormat(tex.Format, option);
				exporter.TexturePathMapping[tex] = texRelPath;
				var texFullPath = dirPath + texRelPath;
				if (tex.HasPixelData && (option.HasFlag(ModelExportOption.ExportTextures) ||
										option.HasFlag(ModelExportOption.ExportTexturesSubFolder)))
					TextureExporter.ExportToFile(texFullPath, tex);
			}

			exporter.Model = model;
			exporter.Skeleton = skeleton;
			exporter.FixBoneForBlender = option.HasFlag(ModelExportOption.FixBoneForBlender);
			exporter.IsYAxisUp = option.HasFlag(ModelExportOption.YAxisUp);
			exporter.IsLargerSize = option.HasFlag(ModelExportOption.LargerSize);
			exporter.IsDiffuseOnly = option.HasFlag(ModelExportOption.ExportDiffuseOnly);
			exporter.Export(path);
		}

		private static string GetTextureFormat(TextureFormat format, ModelExportOption option)
		{
			return MaterialExporter.GetBestTextureFormat(format, (MaterialExporter.MaterialExportOption)option);
		}

		/*public static bool CheckAssimpInited()
		{
			return AssimpLibrary.Instance.IsLibraryLoaded;
		}*/

		[Flags]
		public enum ModelExportOption
		{
			LargerSize = 0x1,
			YAxisUp = 0x2,
			ExportTextures = 0x1000,
			ExportTexturesSubFolder = 0x2000,
			ExportDiffuseOnly = 0x4000,
			FixBoneForBlender = 0x10000,
			PreferPng = 0x100000,
			PreferDds = 0x200000
		}
	}
}