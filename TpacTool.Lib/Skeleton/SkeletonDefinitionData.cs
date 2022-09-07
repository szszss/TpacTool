using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
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

		public int GetBoneId(BoneNode bone)
		{
			return bone == null ? -1 : Bones.FindIndex(node => node == bone);
		}

		public int[] CreateParentLookup()
		{
			var map = new int[Bones.Count];
			for (var i = 0; i < Bones.Count; i++)
			{
				map[i] = GetBoneId(Bones[i].Parent);
			}

			return map;
		}

		public Matrix4x4[] CreateBoneMatrices(bool ignoreM44 = false)
		{
			var parents = CreateParentLookup();
			var matrices = new Matrix4x4[Bones.Count];
			for (var i = 0; i < Bones.Count; i++)
			{
				var parent = parents[i];
				var parentMat = Matrix4x4.Identity;
				if (parent >= 0)
				{
					parentMat = matrices[parent];
				}

				var mat = Bones[i].RestFrame;
				if (ignoreM44)
					mat.M44 = 1f;

				matrices[i] = mat * parentMat;
			}

			return matrices;
		}

		/*public int[] CreateUpdatingOrder()
		{
			var bones = new List<BoneNode>(Bones);
			bones.Sort((left, right) =>
			{
				var leftDepsRight = false;
				var parent = left.Parent;
				while (parent != null)
				{
					if (parent == right)
					{
						leftDepsRight = true;
						break;
					}

					parent = parent.Parent;
				}

				return leftDepsRight ? 1 : 0;
			});

			var order = new int[bones.Count];
			for (var i = 0; i < order.Length; i++)
			{
				order[i] = GetBoneId(bones[i]);
			}

			return order;
		}*/

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

		public override void WriteData(BinaryWriter stream, IDictionary<object, object> userdata)
		{
			stream.WriteSizedString(Name);
			stream.Write(Bones.Count);

			for (int i = 0; i < Bones.Count; i++)
			{
				var bone = Bones[i];
				int parentIndex = bone.Parent == null ? -1 : Bones.FindIndex(node => node == bone.Parent);
				stream.WriteSizedString(bone.Name);
				stream.Write(parentIndex);
				stream.Write(bone.RestFrame);
			}
		}

		public override ExternalData Clone()
		{
			var clone = new SkeletonDefinitionData()
			{
				Name = this.Name
			};
			clone.CloneDo(this);
			clone.Bones.Capacity = this.Bones.Count;
			for (var i = 0; i < this.Bones.Count; i++)
			{
				var parentBone = this.Bones[i].Parent;
				if (parentBone != null)
					parentBone = clone.Bones[this.GetBoneId(parentBone)];
				var cloneBone = this.Bones[i].Clone(parentBone);
				clone.Bones.Add(cloneBone);
			}
			return clone;
		}
	}
}