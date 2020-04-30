using System;

namespace TpacTool.Lib
{
	public class PhysicsDynamicCookData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("df27d12f-d074-45b8-a4ed-8068c0f4988d");

		public PhysicsDynamicCookData() : base(TYPE_GUID)
		{
		}
	}
}