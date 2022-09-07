using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class MorphDefinitionData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("b77ddd3c-afe9-449b-9fe5-99de8b4060c1");

		[NotNull]
		public string Name { set; get; }

		public SortedList<int, List<MorphStatus>> MorphFrame { private set; get; }

		public MorphDefinitionData() : base(TYPE_GUID)
		{
			this.Name = String.Empty;
			MorphFrame = new SortedList<int, List<MorphStatus>>();
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			Name = stream.ReadSizedString();
			var num = stream.ReadInt32();
			MorphFrame.Clear();
			MorphFrame.Capacity = num;
			for (int i = 0; i < num; i++)
			{
				var frame = stream.ReadInt32();
				var vertNum = stream.ReadInt32();
				var list = new List<MorphStatus>(vertNum);
				for (int j = 0; j < vertNum; j++)
				{
					var target = stream.ReadInt32();
					var progress = stream.ReadSingle();
					list.Add(new MorphStatus() { Target = target, Progress = progress });
				}
				MorphFrame.Add(frame, list);
			}
		}

		public Dictionary<int, List<Tuple<int, float>>> OrderByMorphShapes()
		{
			var targets = new Dictionary<int, List<Tuple<int, float>>>();
			foreach (var pair in MorphFrame)
			{
				var frame = pair.Key;
				foreach (var morphStatus in pair.Value)
				{
					var target = morphStatus.Target;
					List<Tuple<int, float>> seq;
					if (!targets.TryGetValue(target, out seq))
					{
						seq = new List<Tuple<int, float>>();
						targets[target] = seq;
					}
					seq.Add(Tuple.Create(frame, morphStatus.Progress));
				}
			}
			return targets;
		}

		public struct MorphStatus
		{
			public int Target;
			public float Progress;
		}
	}
}