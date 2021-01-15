using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class AnimationDefinitionData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("d07d816d-3ced-421c-a6af-b793e75dc2be");

		[NotNull]
		public string Name { set; get; }

		public uint UnknownRootPositionUint1 { set; get; } // always 0

		public uint UnknownRootPositionUint2 { set; get; } // always 16

		public SortedList<float, AnimationFrame<Vector4>> RootPositionFrames { private set; get; }

		public uint UnknownRootScaleUint1 { set; get; } // always 0

		public uint UnknownRootScaleUint2 { set; get; } // always 16

		public SortedList<float, AnimationFrame<Vector3>> RootScaleFrames { private set; get; }

		public List<BoneAnim> BoneAnims { private set; get; }

		public AnimationDefinitionData() : base(TYPE_GUID)
		{
			this.Name = String.Empty;
			RootPositionFrames = new SortedList<float, AnimationFrame<Vector4>>();
			RootScaleFrames = new SortedList<float, AnimationFrame<Vector3>>();
			BoneAnims = new List<BoneAnim>();
			UnknownRootPositionUint1 = 0;
			UnknownRootPositionUint2 = 16;
			UnknownRootScaleUint1 = 0;
			UnknownRootScaleUint2 = 16;
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			this.Name = stream.ReadSizedString();
			var num = stream.ReadInt32();
			uint length;
			float[] tempFloats = null;
			for (int i = 0; i < num; i++) // pre bone
			{
				var boneAnim = new BoneAnim();
				boneAnim.UnknownRotationUint1 = stream.ReadUInt32();
				boneAnim.UnknownRotationUint2 = stream.ReadUInt32();
				length = stream.ReadUInt32();
				if (tempFloats == null || tempFloats.Length < length)
				{
					tempFloats = new float[length + 16];
				}
				for (int j = 0; j < length; j++)
				{
					tempFloats[j] = stream.ReadSingle();
				}
				for (int j = 0; j < length; j++)
				{
					boneAnim.RotationFrames.Add(tempFloats[j], 
						new AnimationFrame<Quaternion>(tempFloats[j], stream.ReadQuat()));
				}
				boneAnim.UnknownRotationUint1 = stream.ReadUInt32();
				boneAnim.UnknownRotationUint2 = stream.ReadUInt32();
				length = stream.ReadUInt32();
				if (tempFloats == null || tempFloats.Length < length)
				{
					tempFloats = new float[length + 16];
				}
				for (int j = 0; j < length; j++)
				{
					tempFloats[j] = stream.ReadSingle();
				}
				for (int j = 0; j < length; j++)
				{
					boneAnim.PositionFrames.Add(tempFloats[j],
						new AnimationFrame<Vector4>(tempFloats[j], stream.ReadVec4()));
				}
				BoneAnims.Add(boneAnim);
			}
			UnknownRootPositionUint1 = stream.ReadUInt32();
			UnknownRootPositionUint2 = stream.ReadUInt32();
			length = stream.ReadUInt32();
			if (tempFloats == null || tempFloats.Length < length)
			{
				tempFloats = new float[length + 16];
			}
			for (int j = 0; j < length; j++)
			{
				tempFloats[j] = stream.ReadSingle();
			}
			for (int j = 0; j < length; j++)
			{
				RootPositionFrames.Add(tempFloats[j],
					new AnimationFrame<Vector4>(tempFloats[j], stream.ReadVec4()));
			}
			UnknownRootScaleUint1 = stream.ReadUInt32();
			UnknownRootScaleUint2 = stream.ReadUInt32();
			length = stream.ReadUInt32();
			if (tempFloats == null || tempFloats.Length < length)
			{
				tempFloats = new float[length];
			}
			for (int j = 0; j < length; j++)
			{
				tempFloats[j] = stream.ReadSingle();
			}
			for (int j = 0; j < length; j++)
			{
				RootScaleFrames.Add(tempFloats[j],
					new AnimationFrame<Vector3>(tempFloats[j], stream.ReadVec4AsVec3()));
			}
		}

		public override void WriteData(BinaryWriter stream, IDictionary<object, object> userdata)
		{
			stream.WriteSizedString(Name);
			stream.Write(BoneAnims.Count);
			for (var i = 0; i < BoneAnims.Count; i++)
			{
				var anim = BoneAnims[i];

				stream.Write(anim.UnknownRotationUint1);
				stream.Write(anim.UnknownRotationUint2);
				stream.Write(anim.RotationFrames.Count);
				foreach (var frame in anim.RotationFrames)
				{
					stream.Write(frame.Value.Time);
				}
				foreach (var frame in anim.RotationFrames)
				{
					stream.Write(frame.Value.Value);
				}

				stream.Write(anim.UnknownPositionUint1);
				stream.Write(anim.UnknownPositionUint2);
				stream.Write(anim.PositionFrames.Count);
				foreach (var frame in anim.PositionFrames)
				{
					stream.Write(frame.Value.Time);
				}
				foreach (var frame in anim.PositionFrames)
				{
					stream.Write(frame.Value.Value);
				}
			}

			stream.Write(UnknownRootPositionUint1);
			stream.Write(UnknownRootPositionUint2);
			stream.Write(RootPositionFrames.Count);
			foreach (var frame in RootPositionFrames)
			{
				stream.Write(frame.Value.Time);
			}
			foreach (var frame in RootPositionFrames)
			{
				stream.Write(frame.Value.Value);
			}

			stream.Write(UnknownRootScaleUint1);
			stream.Write(UnknownRootScaleUint2);
			stream.Write(RootScaleFrames.Count);
			foreach (var frame in RootScaleFrames)
			{
				stream.Write(frame.Value.Time);
			}
			foreach (var frame in RootScaleFrames)
			{
				stream.WriteVec3AsVec4(frame.Value.Value);
			}
		}

		public class BoneAnim
		{
			public uint UnknownPositionUint1 { set; get; }

			public uint UnknownPositionUint2 { set; get; }

			public SortedList<float, AnimationFrame<Vector4>> PositionFrames { private set; get; }

			public uint UnknownRotationUint1 { set; get; }

			public uint UnknownRotationUint2 { set; get; }

			public SortedList<float, AnimationFrame<Quaternion>> RotationFrames { private set; get; }

			public BoneAnim()
			{
				PositionFrames = new SortedList<float, AnimationFrame<Vector4>>();
				RotationFrames = new SortedList<float, AnimationFrame<Quaternion>>();
				UnknownPositionUint1 = 0;
				UnknownPositionUint2 = 16;
				UnknownRotationUint1 = 0;
				UnknownRotationUint2 = 16;
			}
		}
	}
}