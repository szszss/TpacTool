using System;
using System.IO;
using System.Numerics;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public sealed class BoneNode
	{
		[CanBeNull]
		public BoneNode Parent { set; get; }

		[NotNull]
		public string Name { set; get; }

		public Matrix4x4 RestFrame { set; get; }

		public BoneNode()
		{
			this.Parent = null;
			this.Name = String.Empty;
			this.RestFrame = new Matrix4x4();
		}

		public BoneNode Clone(BoneNode parentsClone)
		{
			return new BoneNode()
			{
				Name = this.Name,
				RestFrame = this.RestFrame,
				Parent = parentsClone
			};
		}
	}
}