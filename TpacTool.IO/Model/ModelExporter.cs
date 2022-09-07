using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using JetBrains.Annotations;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public static class ModelExporter
	{
		public static void ExportToFile([NotNull] string path, [CanBeNull] Metamesh model,
			[CanBeNull] Skeleton skeleton = null, ModelExportOption option = 0)
		{
			ExportToFile(path, model, skeleton, null, null, option);
		}

		public static void ExportToFile([NotNull] string path, [CanBeNull] Metamesh model,
			[CanBeNull] Skeleton skeleton = null, 
			[CanBeNull] SkeletalAnimation animation = null, [CanBeNull] MorphAnimation morph = null,
			ModelExportOption option = 0)
		{
			if (path.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
				ExportToFile<WavefrontExporter>(path, model, skeleton, animation, morph, option);
			else if (path.EndsWith(".dae", StringComparison.OrdinalIgnoreCase))
				ExportToFile<ColladaExporter>(path, model, skeleton, animation, morph, option);
			else
				throw new FormatException("Unsupported export format");
		}

		public static void ExportToFile<T>([NotNull] string path, [CanBeNull] Metamesh model,
			[CanBeNull] Skeleton skeleton = null, ModelExportOption option = 0)
			where T : AbstractModelExporter, new()
		{
			ExportToFile<T>(path, model, skeleton, null, null, option);
		}

		public static void ExportToFile<T>([NotNull] string path, [CanBeNull] Metamesh model,
			[CanBeNull] Skeleton skeleton = null, 
			[CanBeNull] SkeletalAnimation animation = null, [CanBeNull] MorphAnimation morph = null,
			ModelExportOption option = 0)
			where T : AbstractModelExporter, new()
		{
			T exporter = new T();
			ExportToFile(exporter, path, model, skeleton, animation, morph, option);
		}

		public static void ExportToFile([NotNull] AbstractModelExporter exporter, [NotNull] string path, [CanBeNull] Metamesh model,
			[CanBeNull] Skeleton skeleton = null, 
			[CanBeNull] SkeletalAnimation animation = null, [CanBeNull] MorphAnimation morph = null,
			ModelExportOption option = 0)
		{
			if (model == null)
				model = Metamesh.EmptyMesh;

			var dirPath = Path.GetDirectoryName(path) + "/";

			var exportedMeshes = model.Meshes;
			var lodMask = int.MaxValue;
			if (!option.HasFlag(ModelExportOption.ExportAllLod))
			{
				lodMask = 1;
				exportedMeshes = exportedMeshes.FindAll(mesh => mesh.Lod == 0);
			}
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
			exporter.Animation = animation;
			exporter.Morph = morph;
			exporter.LodMask = lodMask;
			exporter.FixBoneForBlender = option.HasFlag(ModelExportOption.FixBoneForBlender);
			exporter.IsNegYAxisForward = option.HasFlag(ModelExportOption.NegYAxisForward);
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
			NegYAxisForward = 0x4,
			ExportTextures = 0x1000,
			ExportTexturesSubFolder = 0x2000,
			ExportDiffuseOnly = 0x4000,
			FixBoneForBlender = 0x10000,
			PreferPng = 0x20000,
			PreferDds = 0x40000,
			ExportAllLod = 0x100000
		}
	}
}