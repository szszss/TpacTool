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
#elif NET48
			return "net48";
#elif NETSTANDARD1_3
			return "netstandard1.3";
#elif NETSTANDARD2_0
			return "netstandard2.0";
#elif NETSTANDARD2_1
			return "netstandard2.1";
#elif NET5_0
			return "net5";
#elif NET6_0
			return "net6";
#else
			return "Unknown";
#endif
		}
	}
}