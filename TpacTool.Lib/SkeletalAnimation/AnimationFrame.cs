using System.Collections.Generic;
using System.Numerics;

namespace TpacTool.Lib
{
	public struct AnimationFrame<T> : IComparer<AnimationFrame<T>> where T : struct 
	{
		public readonly float Time;

		public T Value;

		public AnimationFrame(float time) : this()
		{
			Time = time;
			Value = default(T);
		}

		public AnimationFrame(float time, T value)
		{
			Time = time;
			Value = value;
		}

		public int Compare(AnimationFrame<T> x, AnimationFrame<T> y)
		{
			float left = x.Time, right = y.Time;
			return left > right ? 1 : left < right ? -1 : 0;
		}

		public bool Equals(AnimationFrame<T> other)
		{
			return Time.Equals(other.Time) && Value.Equals(other.Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is AnimationFrame<T> other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Time.GetHashCode() * 397) ^ Value.GetHashCode();
			}
		}
	}
}