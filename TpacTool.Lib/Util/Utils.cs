using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	internal static class Utils
	{
		public static string ReadSizedString(this BinaryReader stream)
		{
			int length = stream.ReadInt32();
			if (length == 0)
				return String.Empty;
#if DEBUG
			Debug.Assert(length < 0xFFFF);
#endif
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
			// wtf math
			var w = stream.ReadSingle();
			return new Quaternion(stream.ReadSingle(), stream.ReadSingle(),
				stream.ReadSingle(), w);
		}

		public static Matrix4x4 ReadMat4(this BinaryReader stream)
		{
			return new Matrix4x4(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(),
				stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(),
				stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(),
				stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
		}

		[Obsolete]
		public static Transform ReadTransform(this BinaryReader stream)
		{
			return new Transform(stream.ReadQuat(), stream.ReadVec4());
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

		public static void WriteSizedString(this BinaryWriter stream, [CanBeNull] string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				stream.Write((int) 0);
			}
			else
			{
				var bytes = Encoding.UTF8.GetBytes(value);
				stream.Write(bytes.Length);
				stream.Write(bytes);
			}
		}

		public static uint GetStringSize([CanBeNull] string str, bool incudeLength = false)
		{
			uint size = string.IsNullOrEmpty(str) ? 0 : (uint) Encoding.UTF8.GetBytes(str).Length;
			return incudeLength ? size + 4 : size;
		}

		public static void Write(this BinaryWriter stream, Vector4 value)
		{
			stream.Write(value.X);
			stream.Write(value.Y);
			stream.Write(value.Z);
			stream.Write(value.W);
		}

		public static void WriteVec3AsVec4(this BinaryWriter stream, Vector3 value, float padding = 0)
		{
			stream.Write(value.X);
			stream.Write(value.Y);
			stream.Write(value.Z);
			stream.Write(padding);
		}

		public static void Write(this BinaryWriter stream, Vector2 value)
		{
			stream.Write(value.X);
			stream.Write(value.Y);
		}

		public static void Write(this BinaryWriter stream, Quaternion value)
		{
			stream.Write(value.W);
			stream.Write(value.X);
			stream.Write(value.Y);
			stream.Write(value.Z);
		}

		public static void Write(this BinaryWriter stream, Matrix4x4 value)
		{
			stream.Write(value.M11);
			stream.Write(value.M12);
			stream.Write(value.M13);
			stream.Write(value.M14);
			stream.Write(value.M21);
			stream.Write(value.M22);
			stream.Write(value.M23);
			stream.Write(value.M24);
			stream.Write(value.M31);
			stream.Write(value.M32);
			stream.Write(value.M33);
			stream.Write(value.M34);
			stream.Write(value.M41);
			stream.Write(value.M42);
			stream.Write(value.M43);
			stream.Write(value.M44);
		}

		public static void WriteStringList(this BinaryWriter stream, [CanBeNull] IList<string> values)
		{
			if (values == null || values.Count == 0)
			{
				stream.Write((int) 0);
			}
			else
			{
				stream.Write(values.Count);
				for (var i = 0; i < values.Count; i++)
				{
					stream.WriteSizedString(values[i]);
				}
			}
		}

		public static void WriteStringArray(this BinaryWriter stream, [CanBeNull] string[] values)
		{
			if (values != null && values.Length > 0)
			{
				for (var i = 0; i < values.Length; i++)
				{
					stream.WriteSizedString(values[i]);
				}
			}
		}

		public static void Write(this BinaryWriter stream, Guid value)
		{
			stream.Write(value.ToByteArray());
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