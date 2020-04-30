namespace TpacTool.Lib
{
	public static class DefaultDependenceResolver
	{
		public static IDependenceResolver Instance { set; get; }

		public static void Clear()
		{
			Instance = null;
		}
	}
}