using System;
using System.IO;
using System.Threading;
using Assimp;
using Assimp.Unmanaged;
using JetBrains.Annotations;
using TpacTool.Lib;

namespace TpacTool.IO.Assimp
{
	public static class AssimpModelExporter
	{
		private static bool Inited = false;

		public static bool InitAssimp()
		{
			if (Inited)
				return IsAssimpAvailable();
			Inited = true;
			var loaded = false;
			try
			{
				loaded = AssimpLibrary.Instance.LoadLibrary();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				loaded = false;
			}

			return loaded;
		}

		public static bool IsAssimpAvailable()
		{
			if (!Inited)
				return InitAssimp();
			return AssimpLibrary.Instance.IsLibraryLoaded;
		}

		public static void ExportToFile([NotNull] string path, [CanBeNull] Metamesh model,
			[CanBeNull] Skeleton skeleton = null,
			ModelExporter.ModelExportOption option = 0, AssimpModelExportOption assimpOption = 0,
			float animationFrameRate = 24f)
		{
			ExportToFile(path, model, skeleton, null, null, option, assimpOption, animationFrameRate);
		}

		public static void ExportToFile([NotNull] string path, [CanBeNull] Metamesh model,
			[CanBeNull] Skeleton skeleton = null, 
			[CanBeNull] SkeletalAnimation animation = null, [CanBeNull] MorphAnimation morph = null,
			ModelExporter.ModelExportOption option = 0, AssimpModelExportOption assimpOption = 0,
			float animationFrameRate = 24f)
		{
			if (path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
			{
				var exporter = new FbxExporter
				{
					UseAsciiFormat = (assimpOption & AssimpModelExportOption.UseAscii) != 0,
					AnimationFrameRate = animationFrameRate
				};
				ModelExporter.ExportToFile(exporter, path, model, skeleton, animation, morph, option);
			}
			else
				ModelExporter.ExportToFile(path, model, skeleton, animation, morph, option);
		}

		[Flags]
		public enum AssimpModelExportOption
		{
			UseAscii = 0x10
		}
	}
}