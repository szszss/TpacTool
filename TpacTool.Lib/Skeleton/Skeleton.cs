using System;
using System.IO;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class Skeleton : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("c635a3d5-eabb-45dd-883e-aa57e4196113");

		public bool UnknownBool { set; get; }

		public Guid UnknownGuid { set; get; }

		[CanBeNull]
		public ExternalLoader<SkeletonDefinitionData> Definition { set; get; }

		[CanBeNull]
		public ExternalLoader<SkeletonUserData> UserData { set; get; }

		public Skeleton() : base(TYPE_GUID)
		{
		}

		public override void ReadMetadata(BinaryReader stream, int totalSize)
		{
			var version = stream.ReadUInt32();
			UnknownBool = stream.ReadBoolean();
			UnknownGuid = stream.ReadGuid();
		}

		public override void ConsumeDataSegments(AbstractExternalLoader[] externalData)
		{
			foreach (var externalLoader in externalData)
			{
				var defin = externalLoader as ExternalLoader<SkeletonDefinitionData>;
				if (defin != null)
				{
					Definition = defin;
				}

				var user = externalLoader as ExternalLoader<SkeletonUserData>;
				if (user != null)
				{
					UserData = user;
				}
			}

			base.ConsumeDataSegments(externalData);
		}
	}
}