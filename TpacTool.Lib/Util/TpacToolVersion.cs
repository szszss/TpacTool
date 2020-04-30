namespace TpacTool.Lib
{
	public static class TpacToolVersion
	{
		public static string GetLibraryTargetPlatform()
		{
#if NET40
			return "net40";
#elif NET45
			return "net45";
#elif NET451
			return "net451";
#elif NET462
			return "net462";
#elif NET472
			return "net472";
#elif NETSTANDARD1_3
			return "netstandard1.3";
#elif NETSTANDARD2_0
			return "netstandard2.0";
#else
			return "Unknown";
#endif
		}
	}
}