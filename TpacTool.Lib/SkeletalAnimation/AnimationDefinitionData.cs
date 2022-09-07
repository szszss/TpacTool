using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class AnimationDefinitionData : ExternalData, AnimationDefinitionData.IPositionInterpolable, AnimationDefinitionData.IScaleInterpolable
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
			RootPositionFrames = new SortedList<float, AnimationFrame<Vector4>>(FloatComparer.CreateInstance());
			RootScaleFrames = new SortedList<float, AnimationFrame<Vector3>>(FloatComparer.CreateInstance());
			BoneAnims = new List<BoneAnim>();
			UnknownRootPositionUint1 = 0;
			UnknownRootPositionUint2 = 16;
			UnknownRootScaleUint1 = 0;
			UnknownRootScaleUint2 = 16;
		}

		public AnimationDefinitionData DeepCopy()
		{
			var copy = new AnimationDefinitionData
			{
				Name = this.Name,
				UnknownRootPositionUint1 = this.UnknownRootPositionUint1,
				UnknownRootPositionUint2 = this.UnknownRootPositionUint2,
				UnknownRootScaleUint1 = this.UnknownRootScaleUint1,
				UnknownRootScaleUint2 = this.UnknownRootScaleUint2,
				RootPositionFrames = {Capacity = this.RootPositionFrames.Count},
				RootScaleFrames = {Capacity = this.RootScaleFrames.Count},
				BoneAnims = {Capacity = this.BoneAnims.Count}
			};

			foreach (var pair in this.RootPositionFrames)
			{
				copy.RootPositionFrames[pair.Key] = pair.Value;
			}
			
			foreach (var pair in this.RootScaleFrames)
			{
				copy.RootScaleFrames[pair.Key] = pair.Value;
			}

			for (var i = 0; i < this.BoneAnims.Count; i++)
			{
				copy.BoneAnims.Add(this.BoneAnims[i]?.DeepCopy());
			}

			return copy;
		}

		public bool HasRootPositionTransform()
		{
			foreach (var frame in RootPositionFrames)
			{
				var vec4 = frame.Value.Value;
				if (vec4.X != 0 || vec4.Y != 0 || vec4.Z != 0)
					return true;
			}

			return false;
		}

		public bool HasRootScaleTransform()
		{
			foreach (var frame in RootScaleFrames)
			{
				var vec3 = frame.Value.Value;
				if (Math.Abs(vec3.X - 1) > 0.00001f || Math.Abs(vec3.Y - 1) > 0.00001f || Math.Abs(vec3.Z - 1) > 0.00001f)
					return true;
			}

			return false;
		}

		private static Tuple<AnimationFrame<T>, AnimationFrame<T>> GetNonInterpolatedPropertyDo<T>(
			SortedList<float, AnimationFrame<T>> propertyList, float time, T defaultValue = default(T)) where T : struct
		{
			if (propertyList.Count == 0)
				return Tuple.Create(
					new AnimationFrame<T>(0, defaultValue), 
					new AnimationFrame<T>(0, defaultValue));

			float first = propertyList.First().Key;
			float last = first;

			foreach (var pair in propertyList)
			{
				if (time < pair.Key)
				{
					last = pair.Key;
					break;
				}

				first = pair.Key;
			}

			return Tuple.Create(propertyList[first],
				!float.IsNaN(last) ? propertyList[last] : propertyList[first]);
		}

		public Tuple<Vector4, Vector4> GetNonInterpolatedPosition(float time, out float lastFrame, out float nextFrame)
		{
			var result = GetNonInterpolatedPropertyDo(RootPositionFrames, time, Vector4.UnitW);
			lastFrame = result.Item1.Time;
			nextFrame = result.Item2.Time;
			return Tuple.Create(result.Item1.Value, result.Item2.Value);
		}

		public Vector4 GetInterpolatedPosition(float time, out float lastFrame, out float nextFrame)
		{
			var result = GetNonInterpolatedPosition(time, out lastFrame, out nextFrame);
			if (Math.Abs(lastFrame - nextFrame) > 0.0001f)
			{
				var progress = (time - lastFrame) / (nextFrame - lastFrame);
				return Vector4.Lerp(result.Item1, result.Item2, progress);
			}
			return result.Item1;
		}

		public Tuple<Vector3, Vector3> GetNonInterpolatedScale(float time, out float lastFrame, out float nextFrame)
		{
			var result = GetNonInterpolatedPropertyDo(RootScaleFrames, time, Vector3.One);
			lastFrame = result.Item1.Time;
			nextFrame = result.Item2.Time;
			return Tuple.Create(result.Item1.Value, result.Item2.Value);
		}

		public Vector3 GetInterpolatedScale(float time, out float lastFrame, out float nextFrame)
		{
			var result = GetNonInterpolatedScale(time, out lastFrame, out nextFrame);
			if (Math.Abs(lastFrame - nextFrame) > 0.0001f)
			{
				var progress = (time - lastFrame) / (nextFrame - lastFrame);
				return Vector3.Lerp(result.Item1, result.Item2, progress);
			}
			return result.Item1;
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

		public class BoneAnim : IPositionInterpolable, IRotationInterpolable
		{
			public uint UnknownPositionUint1 { set; get; }

			public uint UnknownPositionUint2 { set; get; }

			public SortedList<float, AnimationFrame<Vector4>> PositionFrames { private set; get; }

			public uint UnknownRotationUint1 { set; get; }

			public uint UnknownRotationUint2 { set; get; }

			public SortedList<float, AnimationFrame<Quaternion>> RotationFrames { private set; get; }

			public BoneAnim()
			{
				PositionFrames = new SortedList<float, AnimationFrame<Vector4>>(FloatComparer.CreateInstance());
				RotationFrames = new SortedList<float, AnimationFrame<Quaternion>>(FloatComparer.CreateInstance());
				UnknownPositionUint1 = 0;
				UnknownPositionUint2 = 16;
				UnknownRotationUint1 = 0;
				UnknownRotationUint2 = 16;
			}

			public BoneAnim DeepCopy()
			{
				var copy = new BoneAnim
				{
					UnknownPositionUint1 = this.UnknownPositionUint1,
					UnknownPositionUint2 = this.UnknownPositionUint2,
					UnknownRotationUint1 = this.UnknownRotationUint1,
					UnknownRotationUint2 = this.UnknownRotationUint2,
					PositionFrames = {Capacity = this.PositionFrames.Count},
					RotationFrames = {Capacity = this.RotationFrames.Count}

				};

				foreach (var pair in this.PositionFrames)
				{
					copy.PositionFrames[pair.Key] = pair.Value;
				}
				
				foreach (var pair in this.RotationFrames)
				{
					copy.RotationFrames[pair.Key] = pair.Value;
				}

				return copy;
			}

			public bool HasPositionTransform()
			{
				foreach (var frame in PositionFrames)
				{
					var vec4 = frame.Value.Value;
					if (vec4.X != 0 || vec4.Y != 0 || vec4.Z != 0)
						return true;
				}

				return false;
			}

			public bool HasRotationTransform()
			{
				foreach (var frame in RotationFrames)
				{
					var quat = frame.Value.Value;
					if (Math.Abs(quat.W - 1) > 0.00001f)
						return true;
				}

				return false;
			}

			public Tuple<Vector4, Vector4> GetNonInterpolatedPosition(float time, out float lastFrame, out float nextFrame)
			{
				var result = GetNonInterpolatedPropertyDo(PositionFrames, time, Vector4.UnitW);
				lastFrame = result.Item1.Time;
				nextFrame = result.Item2.Time;
				return Tuple.Create(result.Item1.Value, result.Item2.Value);
			}

			public Vector4 GetInterpolatedPosition(float time, out float lastFrame, out float nextFrame)
			{
				var result = GetNonInterpolatedPosition(time, out lastFrame, out nextFrame);
				if (Math.Abs(lastFrame - nextFrame) > 0.0001f)
				{
					var progress = (time - lastFrame) / (nextFrame - lastFrame);
					return Vector4.Lerp(result.Item1, result.Item2, progress);
				}
				return result.Item1;
			}

			public Tuple<Quaternion, Quaternion> GetNonInterpolatedRotation(float time, out float lastFrame, out float nextFrame)
			{
				var result = GetNonInterpolatedPropertyDo(RotationFrames, time, Quaternion.Identity);
				lastFrame = result.Item1.Time;
				nextFrame = result.Item2.Time;
				return Tuple.Create(result.Item1.Value, result.Item2.Value);
			}

			public Quaternion GetInterpolatedRotation(float time, out float lastFrame, out float nextFrame)
			{
				var result = GetNonInterpolatedRotation(time, out lastFrame, out nextFrame);
				if (Math.Abs(lastFrame - nextFrame) > 0.0001f)
				{
					var progress = (time - lastFrame) / (nextFrame - lastFrame);
					return Quaternion.Slerp(result.Item1, result.Item2, progress);
				}
				return result.Item1;
			}

			public void ReplenishKeyframes(params float[] ascendingKeyframes)
			{
				if (ascendingKeyframes != null && ascendingKeyframes.Length > 0)
				{
					foreach (var f in ascendingKeyframes)
					{
						if (!PositionFrames.ContainsKey(f))
						{
							PositionFrames.Add(f, new AnimationFrame<Vector4>(f, GetInterpolatedPosition(f, out _, out _)));
						}

						if (!RotationFrames.ContainsKey(f))
						{
							RotationFrames.Add(f, new AnimationFrame<Quaternion>(f, GetInterpolatedRotation(f, out _, out _)));
						}
					}
				}
			}
		}

		public interface IPositionInterpolable
		{
			Vector4 GetInterpolatedPosition(float time, out float lastFrame, out float nextFrame);

			Tuple<Vector4, Vector4> GetNonInterpolatedPosition(float time, out float lastFrame, out float nextFrame);
		}

		public interface IRotationInterpolable
		{
			Quaternion GetInterpolatedRotation(float time, out float lastFrame, out float nextFrame);

			Tuple<Quaternion, Quaternion> GetNonInterpolatedRotation(float time, out float lastFrame, out float nextFrame);
		}

		public interface IScaleInterpolable
		{
			Vector3 GetInterpolatedScale(float time, out float lastFrame, out float nextFrame);

			Tuple<Vector3, Vector3> GetNonInterpolatedScale(float time, out float lastFrame, out float nextFrame);
		}

		internal class FloatComparer : IComparer<float>
		{
			public static FloatComparer CreateInstance() => new FloatComparer();

			public int Compare(float x, float y)
			{
				if (Math.Abs(x - y) < 0.00001)
					return 0;
				if (x < y)
					return -1;
				if (x > y)
					return 1;
				return 0;
			}
		}
	}
}