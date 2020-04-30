using System;
using System.Collections.Generic;
using System.IO;

namespace TpacTool.Lib
{
	// not finished
	public class VectorFieldData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("326587ce-bb0c-4c22-8782-97e20cf03c5e");

		public VectorFieldData() : base(TYPE_GUID)
		{
		}
	}
}