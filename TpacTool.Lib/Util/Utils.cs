using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;

namespace TpacTool.Lib
{
	internal static class Utils
	{
		public static string ReadSizedString(this BinaryReader stream)
		{
			int length = stream.ReadInt32();
			if (length == 0)
				return String.Empty;
			var bytes = stream.ReadBytes(length);
			return Encoding.UTF8.GetString(bytes);
		}

		public static Vector4 ReadVec4(this BinaryReader stream)
		{
			return new Vector4(stream.ReadSingle(), stream.ReadSingle(),
								stream.ReadSingle(), stream.ReadSingle());
		}

		public static Vector3 ReadVec4AsVec3(this BinaryReader stream)
		{
			float f1 = stream.ReadSingle(), f2 = stream.ReadSingle(), f3 = stream.ReadSingle();
			stream.ReadSingle();
			return new Vector3(f1, f2, f3);
		}

		public static Vector2 ReadVec2(this BinaryReader stream)
		{
			return new Vector2(stream.ReadSingle(), stream.ReadSingle());
		}

		public static Quaternion ReadQuat(this BinaryReader stream)
		{
			return new Quaternion(stream.ReadSingle(), stream.ReadSingle(),
				stream.ReadSingle(), stream.ReadSingle());
		}

		public static Matrix4x4 ReadMat4(this BinaryReader stream)
		{
			return new Matrix4x4(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(),
				stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(),
				stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(),
				stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
		}

		public static Transform ReadTransform(this BinaryReader stream)
		{
			return new Transform(stream.ReadVec4(), stream.ReadVec4());
		}

		public static T[] Fill<T>(this T[] array, T obj)
		{
			for (int i = 0, j = array.Length; i < j; i++)
			{
				array[i] = obj;
			}

			return array;
		}

		public static List<string> ReadStringList(this BinaryReader stream)
		{
			int length = stream.ReadInt32();
			var list = new List<string>(length);
			for (int i = 0; i < length; i++)
			{
				list.Add(stream.ReadSizedString());
			}

			return list;
		}

		public static string[] ReadStringArray(this BinaryReader stream, int length)
		{
			var array = new string[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = stream.ReadSizedString();
			}

			return array;
		}

		public static Guid ReadGuid(this BinaryReader stream)
		{
			return new Guid(stream.ReadBytes(16));
		}

		public static BinaryReader OpenBinaryReader(this FileInfo file)
		{
			//if (!file.Exists)
			//	throw new FileNotFoundException("Cannot find file: " + file.FullName);
			return new BinaryReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read));
		}

		public static BinaryReader CreateBinaryReader(this byte[] data)
		{
			//if (data == null)
			//	throw new ArgumentNullException("data");
			return new BinaryReader(new MemoryStream(data, false));
		}

		public static float Clamp(this float value)
		{
			return Clamp(value, -1f, 1f);
		}

		public static float Clamp(this float value, float min, float max)
		{
			return Math.Max(min, Math.Min(max, value));
		}

		private static ThreadLocal<long> DEBUG_POSITION = new ThreadLocal<long>();

		[Conditional("DEBUG")]
		public static void RecordPosition(this BinaryReader reader)
		{
			DEBUG_POSITION.Value = reader.BaseStream.Position;
		}

		[Conditional("DEBUG")]
		public static void AssertLength(this BinaryReader reader, long readLength)
		{
			var length = reader.BaseStream.Position - DEBUG_POSITION.Value;
			Debug.Assert(length == readLength);
		}
	}
}