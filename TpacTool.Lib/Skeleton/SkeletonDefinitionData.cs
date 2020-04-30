using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class SkeletonDefinitionData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("11d07d37-e720-406b-ab67-c846f96a8771");

		[NotNull]
		public string Name { set; get; }

		public List<BoneNode> Bones { private set; get; }

		public SkeletonDefinitionData() : base(TYPE_GUID)
		{
			this.Name = String.Empty;
			Bones = new List<BoneNode>();
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			this.Name = stream.ReadSizedString();
			var num = stream.ReadInt32();
			Bones.Clear();
			Bones.Capacity = num;
			var boneParents = new int[num];
			for (int i = 0; i < num; i++)
			{
				var bone = new BoneNode();
				bone.Name = stream.ReadSizedString();
				boneParents[i] = stream.ReadInt32();
				bone.RestFrame = stream.ReadMat4();
				Bones.Add(bone);
			}

			for (int i = 0; i < num; i++)
			{
				if (boneParents[i] >= 0)
				{
					Debug.Assert(boneParents[i] < num);
					Bones[i].Parent = Bones[boneParents[i]];
				}
			}
		}
	}
}