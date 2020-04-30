using System.IO;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	internal static class ExceptionHelper
	{
		public static void CheckFileExist([NotNull] this FileInfo file)
		{
			if (!file.Exists)
				throw new FileNotFoundException("Cannot find file: " + file.FullName);
		}
	}
}