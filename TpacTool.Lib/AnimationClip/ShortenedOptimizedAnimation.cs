using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace TpacTool.Lib
{
	public class ShortenedOptimizedAnimation : OptimizedAnimation
	{
		public new static readonly Guid TYPE_GUID = Guid.Parse("8fc3fc2a-4bc9-46c5-8525-f33a6e257b72");

		public ShortenedOptimizedAnimation()
		{
			this.TypeGuid = TYPE_GUID;
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			base.ReadData(stream, userdata, totalSize);
		}
	}
}