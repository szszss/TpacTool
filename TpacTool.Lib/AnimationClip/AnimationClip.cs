using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class AnimationClip : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("506509c8-e563-4ca4-b166-a53b92e913a7");

		[Obsolete]
		public uint UnknownUInt1 { set; get; }

		public float Duration { set; get; }

		public float Source1 { set; get; }

		public float Source2 { set; get; }

		public float Param1 { set; get; }

		public float Param2 { set; get; }

		public float Param3 { set; get; }

		public int Priority { set; get; }

		public Guid Animation { set; get; } = Guid.Empty;

		public Vector4 StepPoints { set; get; }

		[NotNull]
		public string SoundCode { set; get; } = string.Empty;

		[NotNull]
		public string VoiceCode { set; get; } = string.Empty;

		[NotNull]
		public string FacialAnimationId { set; get; } = string.Empty;

		[NotNull]
		public string BlendsWithAction { set; get; } = string.Empty;

		[NotNull]
		public string ContinueWithAction { set; get; } = string.Empty;

		public int LeftHandPose { set; get; }

		public int RightHandPose { set; get; }

		[NotNull]
		public string CombatParameterId { set; get; } = string.Empty;

		public float BlendInPeriod { set; get; }

		public float BlendOutPeriod { set; get; }

		public bool DoNotInterpolate { set; get; }

		public int UnknownInt { set; get; }

		/// <summary>
		/// Used by generated balanced animation (which has a numeric name like 10235460348483436133_3).
		/// The value equals the postfix of name (3 for the above example).
		/// -1 for non-generated animation clip.
		/// </summary>
		public sbyte GeneratedIndex { set; get; } = -1;

		public uint UnknownUInt2 { set; get; }

		public ushort UnknownUShort { set; get; }

		[NotNull]
		public string UnknownClipName { set; get; } = string.Empty;

		[NotNull]
		public string ClipSource1Name { set; get; } = string.Empty;

		[NotNull]
		public string ClipSource2Name { set; get; } = string.Empty;

		[NotNull]
		public List<string> Flags { private set; get; } = new List<string>(4);

		[NotNull]
		public List<ClipUsage> ClipUsages { private set; get; } = new List<ClipUsage>(0);

		public AnimationClip() : base(TYPE_GUID)
		{
		}

		public override void ReadMetadata(BinaryReader stream, int totalSize)
		{
			var version = stream.ReadUInt32();
			Duration = stream.ReadSingle();
			Source1 = stream.ReadSingle();
			Source2 = stream.ReadSingle();
			Param1 = stream.ReadSingle();
			Param2 = stream.ReadSingle();
			Param3 = stream.ReadSingle();
			Priority = stream.ReadInt32();
			Animation = stream.ReadGuid();
			StepPoints = stream.ReadVec4();
			SoundCode = stream.ReadSizedString();
			VoiceCode = stream.ReadSizedString();
			FacialAnimationId = stream.ReadSizedString();
			BlendsWithAction = stream.ReadSizedString();
			ContinueWithAction = stream.ReadSizedString();
			LeftHandPose = stream.ReadInt32();
			RightHandPose = stream.ReadInt32();
			CombatParameterId = stream.ReadSizedString();
			BlendInPeriod = stream.ReadSingle();
			BlendOutPeriod = stream.ReadSingle();
			DoNotInterpolate = stream.ReadBoolean();
			UnknownInt = stream.ReadInt32();
			if (version >= 4)
			{
				UnknownClipName = stream.ReadSizedString();
				ClipSource1Name = stream.ReadSizedString();
				ClipSource2Name = stream.ReadSizedString();
				if (version >= 5)
				{
					GeneratedIndex = stream.ReadSByte();
				}
				UnknownUInt2 = stream.ReadUInt32();
				UnknownUShort = stream.ReadUInt16();
			}
			else
			{
				UnknownUInt2 = stream.ReadUInt32();
			}

			Flags = stream.ReadStringList();

			var count = stream.ReadInt32();
			ClipUsages.Clear();
			ClipUsages.Capacity = count;
			for (int i = 0; i < count; i++)
			{
				var type = stream.ReadSizedString();
				switch (type)
				{
					case "displacement":
						var displacement = new DisplacementUsage();
						displacement.UnknownUInt = stream.ReadUInt32();
						displacement.DisplacementVector = stream.ReadVec4AsVec3();
						displacement.DisplacementEndProgress = stream.ReadSingle();
						ClipUsages.Add(displacement);
						break;
					case "quad_movement":
						var quad = new QuadMovementUsage();
						quad.UnknownUInt = stream.ReadUInt32();
						quad.LoopDisplacement = stream.ReadSingle();
						quad.PaceSwitchLimitMin = stream.ReadSingle();
						quad.PaceSwitchLimitMax = stream.ReadSingle();
						ClipUsages.Add(quad);
						break;
					case "bip_mov_ik":
						var bip = new BipMovIkUsage();
						bip.UnknownUInt = stream.ReadUInt32();
						bip.LoopDisplacement = stream.ReadSingle();
						bip.AdjustingStart0 = stream.ReadSingle();
						bip.AdjustingStart1 = stream.ReadSingle();
						bip.SnappingDuration0 = stream.ReadSingle();
						bip.SnappingDuration1 = stream.ReadSingle();
						bip.SnappingStart0 = stream.ReadSingle();
						bip.SnappingStart1 = stream.ReadSingle();
						ClipUsages.Add(bip);
						break;
					case "blend":
						var blend = new BlendUsage();
						blend.UnknownUInt = stream.ReadUInt32();
						blend.BlendStartProgress = stream.ReadSingle();
						blend.BlendEndProgress = stream.ReadSingle();
						ClipUsages.Add(blend);
						break;
					case "mount_change":
						var mount = new MountChangeUsage();
						mount.UnknownUInt = stream.ReadUInt32();
						mount.ScaleBlendStartProgress = stream.ReadSingle();
						mount.ScaleBlendEndProgress = stream.ReadSingle();
						ClipUsages.Add(mount);
						break;
					default:
						throw new Exception("Unknown clip usage: " + type);
				}
			}
		}

		public override void WriteMetadata(BinaryWriter stream)
		{
			stream.Write(5);
			stream.Write(Duration);
			stream.Write(Source1);
			stream.Write(Source2);
			stream.Write(Param1);
			stream.Write(Param2);
			stream.Write(Param3);
			stream.Write(Priority);
			stream.Write(Animation);
			stream.Write(StepPoints);
			stream.WriteSizedString(SoundCode);
			stream.WriteSizedString(VoiceCode);
			stream.WriteSizedString(FacialAnimationId);
			stream.WriteSizedString(BlendsWithAction);
			stream.WriteSizedString(ContinueWithAction);
			stream.Write(LeftHandPose);
			stream.Write(RightHandPose);
			stream.WriteSizedString(CombatParameterId);
			stream.Write(BlendInPeriod);
			stream.Write(BlendOutPeriod);
			stream.Write(DoNotInterpolate);
			stream.Write(UnknownInt);
			stream.WriteSizedString(UnknownClipName);
			stream.WriteSizedString(ClipSource1Name);
			stream.WriteSizedString(ClipSource2Name);
			stream.Write(GeneratedIndex);
			stream.Write(UnknownUInt2);
			stream.Write(UnknownUShort);
			stream.WriteStringList(Flags);

			stream.Write(ClipUsages.Count);
			foreach (var usage in ClipUsages)
			{
				stream.WriteSizedString(usage.Type);
				switch (usage.Type)
				{
					case "displacement":
						var displacement = usage as DisplacementUsage;
						stream.Write(displacement.UnknownUInt);
						stream.WriteVec3AsVec4(displacement.DisplacementVector);
						stream.Write(displacement.DisplacementEndProgress);
						break;
					case "quad_movement":
						var quad = usage as QuadMovementUsage;
						stream.Write(quad.UnknownUInt);
						stream.Write(quad.LoopDisplacement);
						stream.Write(quad.PaceSwitchLimitMin);
						stream.Write(quad.PaceSwitchLimitMax);
						break;
					case "bip_mov_ik":
						var bip = usage as BipMovIkUsage;
						stream.Write(bip.UnknownUInt);
						stream.Write(bip.LoopDisplacement);
						stream.Write(bip.AdjustingStart0);
						stream.Write(bip.AdjustingStart1);
						stream.Write(bip.SnappingDuration0);
						stream.Write(bip.SnappingDuration1);
						stream.Write(bip.SnappingStart0);
						stream.Write(bip.SnappingStart1);
						break;
					case "blend":
						var blend = usage as BlendUsage;
						stream.Write(blend.UnknownUInt);
						stream.Write(blend.BlendStartProgress);
						stream.Write(blend.BlendEndProgress);
						break;
					case "mount_change":
						var mount = usage as MountChangeUsage;
						stream.Write(mount.UnknownUInt);
						stream.Write(mount.ScaleBlendStartProgress);
						stream.Write(mount.ScaleBlendEndProgress);
						break;
					default:
						throw new Exception("Unknown clip usage: " + usage.Type);
				}
			}
		}

		public abstract class ClipUsage
		{
			[NotNull]
			public string Type { private set; get; }

			public uint UnknownUInt { set; get; }

			public ClipUsage(string type)
			{
				this.Type = type;
			}
		}

		public sealed class DisplacementUsage : ClipUsage
		{
			public const string TYPE_NAME = "displacement";

			public Vector3 DisplacementVector { set; get; } = Vector3.Zero;

			public float DisplacementEndProgress { set; get; }

			public DisplacementUsage() : base(TYPE_NAME)
			{
			}
		}

		public sealed class QuadMovementUsage : ClipUsage
		{
			public const string TYPE_NAME = "quad_movement";

			public float LoopDisplacement { set; get; }

			public float PaceSwitchLimitMin { set; get; }

			public float PaceSwitchLimitMax { set; get; }

			public QuadMovementUsage() : base(TYPE_NAME)
			{
			}
		}

		public sealed class BipMovIkUsage : ClipUsage
		{
			public const string TYPE_NAME = "bip_mov_ik";

			public float LoopDisplacement { set; get; }

			public float AdjustingStart0 { set; get; }

			public float AdjustingStart1 { set; get; }

			public float SnappingDuration0 { set; get; }

			public float SnappingDuration1 { set; get; }

			public float SnappingStart0 { set; get; }

			public float SnappingStart1 { set; get; }

			public BipMovIkUsage() : base(TYPE_NAME)
			{
			}
		}

		public sealed class BlendUsage : ClipUsage
		{
			public const string TYPE_NAME = "blend";

			public float BlendStartProgress { set; get; }

			public float BlendEndProgress { set; get; }

			public BlendUsage() : base(TYPE_NAME)
			{
			}
		}

		public sealed class MountChangeUsage : ClipUsage
		{
			public const string TYPE_NAME = "mount_change";

			public float ScaleBlendStartProgress { set; get; }

			public float ScaleBlendEndProgress { set; get; }

			public MountChangeUsage() : base(TYPE_NAME)
			{
			}
		}
	}
}