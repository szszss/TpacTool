using System;

namespace TpacTool.Lib
{
	public class PhysicsShape : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("e8528e0e-64b6-4e61-bae0-7569c0452aea");

		public PhysicsShape() : base(TYPE_GUID)
		{
		}
	}
}