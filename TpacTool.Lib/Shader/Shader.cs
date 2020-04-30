using System;

namespace TpacTool.Lib
{
	public class Shader : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("b46528df-0344-4ff9-80f2-e30cd6c4fcc3");

		public Shader() : base(TYPE_GUID)
		{
		}
	}
}