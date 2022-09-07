using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TpacTool.Lib
{
	public class OptimizedAnimation : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("6c1e136f-d0b7-41ff-bb7a-275f3630db25");

		/// <summary>
		/// always 1
		/// </summary>
		public int UnknownInt1 { set; get; } = 1;

		/// <summary>
		/// always 1
		/// </summary>
		public int UnknownInt2 { set; get; } = 1;

		public int FrameCount { set; get; }

		public string Skeleton { set; get; } = "";

		/// <summary>
		/// always 0
		/// </summary>
		public int UnknownInt3 { set; get; } = 0;

		/// <summary>
		/// always 1
		/// </summary>
		public byte UnknownByte { set; get; } = 1;

		/// <summary>
		/// always 0
		/// </summary>
		public int UnknownInt4 { set; get; } = 0;

		public int UnknownOffset { set; get; } = 0;

		private const int MysteriousNumber = 17;

		public SortedList<float, AnimationFrame<Vector4>> RootPositionFrames { private set; get; }

		public List<OptimizedBoneAnim> BoneAnims { private set; get; }

		public BoneActivityMap BoneActivities { private set; get; }

		public OptimizedAnimation() : base(TYPE_GUID)
		{
			RootPositionFrames = new SortedList<float, AnimationFrame<Vector4>>(
				AnimationDefinitionData.FloatComparer.CreateInstance());
			BoneAnims = new List<OptimizedBoneAnim>();
			BoneActivities = new BoneActivityMap();
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			var cur = stream.BaseStream.Position;
			UnknownInt1 = stream.ReadInt32(); // always 1
			UnknownInt2 = stream.ReadInt32(); // always 1
			FrameCount = stream.ReadInt32();
			Skeleton = stream.ReadSizedString();
			int frameNum2 = stream.ReadInt32();
			if (frameNum2 != FrameCount)
			{
				throw new Exception($"Frames not equal: {FrameCount} - {frameNum2}");
			}
			int boneNum = stream.ReadInt32();
			BoneAnims.Clear();
			BoneAnims.Capacity = boneNum;
			for (int i = 0; i < boneNum; i++)
			{
				var boneAnim = new OptimizedBoneAnim();

				boneAnim.UnknownInt1 = stream.ReadInt32();
				var boneKeyframeNum = stream.ReadInt32();

				var boneTimes = new float[boneKeyframeNum];
				for (int j = 0; j < boneKeyframeNum; j++)
				{
					boneTimes[j] = stream.ReadSingle();
				}

				var quats = new Quaternion[boneKeyframeNum];
				for (int j = 0; j < boneKeyframeNum; j++)
				{
					quats[j] = stream.ReadQuat();
				}

				boneAnim.RotationFrames.Capacity = boneKeyframeNum;
				for (int j = 0; j < boneKeyframeNum; j++)
				{
					boneAnim.RotationFrames.Add(boneTimes[j], new AnimationFrame<Quaternion>(
						boneTimes[j], quats[j]));
				}

				boneAnim.UnknownInt2 = stream.ReadInt32();
				boneAnim.UnknownInt3 = stream.ReadInt32();
				BoneAnims.Add(boneAnim);
			}

			int rootKeyframeNum = stream.ReadInt32();
			var rootTimes = new float[rootKeyframeNum];
			for (int i = 0; i < rootKeyframeNum; i++)
			{
				rootTimes[i] = stream.ReadSingle();
			}
			var positions = new Vector4[rootKeyframeNum];
			for (int i = 0; i < rootKeyframeNum; i++)
			{
				positions[i] = stream.ReadVec4();
			}

			RootPositionFrames.Clear();
			RootPositionFrames.Capacity = rootKeyframeNum;
			for (int i = 0; i < rootKeyframeNum; i++)
			{
				RootPositionFrames.Add(rootTimes[i], new AnimationFrame<Vector4>(
					rootTimes[i], positions[i]));
			}

			UnknownInt3 = stream.ReadInt32(); // always 0
			UnknownByte = stream.ReadByte(); // always 1
			byte boneNum2 = stream.ReadByte();
			int activityFrameNum = stream.ReadInt32();
			UnknownInt4 = stream.ReadInt32(); // always 0
			BoneActivities.FrameCount = activityFrameNum;
			BoneActivities.BoneCount = boneNum2;

			byte[] lastBytes = null;
			for (int i = 0; i < activityFrameNum; i++)
			{
				var bytes = stream.ReadBytes(boneNum2);

				if (lastBytes == null)
				{
					BoneActivities.SetBoneActivityRange(i,
						bytes.Select(b => b != 0).ToArray());
				}
				else
				{
					BoneActivities.SetBoneActivityRange(i,
						bytes.Select((b, index) => b > lastBytes[index]).ToArray());
				}

				lastBytes = bytes;
			}

			UnknownOffset = stream.ReadInt32(); // always equals length - boneNum2 * 17
			var length = stream.BaseStream.Position - cur;
			int delta = (int) length - UnknownOffset;

			// not finished yet
		}

		public override void WriteData(BinaryWriter stream, IDictionary<object, object> userdata)
		{
			var writePos = stream.BaseStream.Position;

			stream.Write(UnknownInt1);
			stream.Write(UnknownInt2);
			stream.Write(FrameCount);
			stream.WriteSizedString(Skeleton);

			stream.Write(FrameCount);
			stream.Write(BoneAnims.Count);
			foreach (var boneAnim in BoneAnims)
			{
				stream.Write(boneAnim.UnknownInt1);
				stream.Write(boneAnim.RotationFrames.Count);

				foreach (var pair in boneAnim.RotationFrames)
				{
					stream.Write(pair.Value.Time);
				}

				foreach (var pair in boneAnim.RotationFrames)
				{
					stream.Write(pair.Value.Value);
				}

				stream.Write(boneAnim.UnknownInt2);
				stream.Write(boneAnim.UnknownInt3);
			}

			stream.Write(RootPositionFrames.Count);
			foreach (var pair in RootPositionFrames)
			{
				stream.Write(pair.Value.Time);
			}
			foreach (var pair in RootPositionFrames)
			{
				stream.Write(pair.Value.Value);
			}

			stream.Write(UnknownInt3);
			stream.Write(UnknownByte);
			stream.Write((byte) BoneActivities.BoneCount);
			stream.Write(BoneActivities.FrameCount);
			stream.Write(UnknownInt4);

			int[][] boneActSeq = new int[BoneActivities.BoneCount][];
			for (var i = 0; i < boneActSeq.Length; i++)
			{
				boneActSeq[i] = BoneActivities.GetBoneActTimesSequence(i);
			}

			byte[] bytes = new byte[BoneActivities.BoneCount];
			for (int i = 0, j = BoneActivities.FrameCount; i < j; i++)
			{
				for (int k = 0, l = BoneActivities.BoneCount; k < l; k++)
				{
					bytes[k] = (byte) boneActSeq[k][i];
				}
				stream.Write(bytes);
			}

			var length = (int) (stream.BaseStream.Length - writePos);
			stream.Write(length - BoneActivities.BoneCount * MysteriousNumber + 4);
		}

		public sealed class OptimizedBoneAnim
		{
			public int UnknownInt1 { set; get; }

			public int UnknownInt2 { set; get; }

			public int UnknownInt3 { set; get; }

			public SortedList<float, AnimationFrame<Quaternion>> RotationFrames { private set; get; }

			public OptimizedBoneAnim()
			{
				RotationFrames = new SortedList<float, AnimationFrame<Quaternion>>(
					AnimationDefinitionData.FloatComparer.CreateInstance());
			}
		}

		public sealed class BoneActivityMap
		{
			private int _frameCount;

			private List<BitArray> _boneActivityList;

			public int BoneCount
			{
				set
				{
					if (value < 0)
						throw new ArgumentException(nameof(BoneCount));

					while (_boneActivityList.Count < value)
					{
						_boneActivityList.Add(new BitArray(_frameCount));
					}

					while (_boneActivityList.Count > value)
					{
						_boneActivityList.RemoveAt(_boneActivityList.Count - 1);
					}
				}
				get => _boneActivityList.Count;
			}

			public int FrameCount
			{
				set
				{
					if (value < 0)
						throw new ArgumentException(nameof(FrameCount));

					_frameCount = value;

					for (var i = 0; i < _boneActivityList.Count; i++)
					{
						_boneActivityList[i].Length = value;
					}
				}
				get => _frameCount;
			}

			public BoneActivityMap()
			{
				_boneActivityList = new List<BitArray>();
			}

			public void SetBoneActivity(int boneIndex, int frame, bool active = true)
			{
				if (boneIndex < 0 || boneIndex >= _boneActivityList.Count)
					throw new ArgumentOutOfRangeException(nameof(boneIndex));

				if (frame < 0 || frame >= FrameCount)
					throw new ArgumentOutOfRangeException(nameof(frame));

				_boneActivityList[boneIndex].Set(frame, active);
			}

			public void SetBoneActivityRange(int frame, bool[] active)
			{
				if (active == null)
					throw new ArgumentNullException(nameof(active));

				if (active.Length != _boneActivityList.Count)
					throw new ArgumentException(nameof(active));

				if (frame < 0 || frame >= FrameCount)
					throw new ArgumentOutOfRangeException(nameof(frame));

				for (var i = 0; i < active.Length; i++)
				{
					_boneActivityList[i].Set(frame, active[i]);
				}
			}

			public bool IsBoneActive(int boneIndex, int frame)
			{
				if (boneIndex < 0 || boneIndex >= _boneActivityList.Count)
					throw new ArgumentOutOfRangeException(nameof(boneIndex));

				if (frame < 0 || frame >= FrameCount)
					throw new ArgumentOutOfRangeException(nameof(frame));

				return _boneActivityList[boneIndex].Get(frame);
			}

			public int GetBoneActTimes(int boneIndex)
			{
				if (boneIndex < 0 || boneIndex >= _boneActivityList.Count)
					throw new ArgumentOutOfRangeException(nameof(boneIndex));

				int times = 0;
				for (int i = 0, j = FrameCount; i < j; i++)
				{
					if (_boneActivityList[boneIndex][i])
						times++;
				}

				return times;
			}

			public int[] GetBoneActTimesSequence(int boneIndex)
			{
				if (boneIndex < 0 || boneIndex >= _boneActivityList.Count)
					throw new ArgumentOutOfRangeException(nameof(boneIndex));

				var seq = new int[FrameCount];
				int times = 0;
				for (int i = 0, j = FrameCount; i < j; i++)
				{
					if (_boneActivityList[boneIndex][i])
						times++;
					seq[i] = times;
				}

				return seq;
			}
		}
	}
}