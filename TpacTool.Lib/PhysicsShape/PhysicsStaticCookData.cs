using System;

namespace TpacTool.Lib
{
	public class PhysicsStaticCookData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("8283cbbb-3a79-40ea-aead-7f5a2ceb747f");

		public PhysicsStaticCookData() : base(TYPE_GUID)
		{
		}
	}
}