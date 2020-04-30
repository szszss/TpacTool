using System;
using System.IO;

namespace TpacTool.Lib
{
	public class SkeletalAnimation : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("bafab007-7e3f-453f-bac6-e7640043112b");

		public Guid UnknownGuid1 { set; get; }

		// false for almost all animation. true for a few which have strange name (take_001, game_ready...)
		public bool UnknownBool { set; get; }

		public Guid UnknownGuid2 { set; get; } // always empty

		public int BoneNum { set; get; }

		public int Duration { set; get; }

		public int UnknownInt { set; get; } // always 0

		public SkeletalAnimation() : base(TYPE_GUID)
		{
		}

		public override void ReadMetadata(BinaryReader stream, int totalSize)
		{
			var version = stream.ReadUInt32();
			if (version == 0)
				throw new Exception("TpacTool doesn't support version 0 of skeletal animation");
			UnknownGuid1 = stream.ReadGuid();
			UnknownBool = stream.ReadBoolean();
			UnknownGuid2 = stream.ReadGuid();
			BoneNum = stream.ReadInt32();
			Duration = stream.ReadInt32();
			UnknownInt = stream.ReadInt32();
		}
	}
}