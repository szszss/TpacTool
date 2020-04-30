using System;

namespace TpacTool.Lib
{
	// never appear in tpac
	public class Geometry : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("3eba3679-debd-4c7a-8634-f121f6325e33");

		public Geometry() : base(TYPE_GUID)
		{
		}
	}
}