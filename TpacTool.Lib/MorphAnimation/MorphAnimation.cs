using System;

namespace TpacTool.Lib
{
	public class MorphAnimation : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("cab5ba7b-818e-4432-8e37-03e291d9b8c0");

		public ExternalLoader<MorphDefinitionData> Definition { set; get; }

		public MorphAnimation() : base(TYPE_GUID)
		{
		}

		public override void ConsumeDataSegments(AbstractExternalLoader[] externalData)
		{
			foreach (var externalLoader in externalData)
			{
				if (externalLoader is ExternalLoader<MorphDefinitionData> defin)
				{
					Definition = defin;
				}
			}

			base.ConsumeDataSegments(externalData);
		}
	}
}