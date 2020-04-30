using System;
using System.IO;

namespace TpacTool.Lib
{
	public class Particle : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("6de14d67-dd9a-45be-9463-0281c3d8dd51");

		public Particle() : base(TYPE_GUID)
		{
		}

		// Particle has no metadata
	}
}