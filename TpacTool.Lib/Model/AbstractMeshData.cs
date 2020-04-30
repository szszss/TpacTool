using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

#if NATIVE_INTEROP_SHIT
using Buffer = NativeInterop.Buffer;
#endif

namespace TpacTool.Lib
{
	public abstract class AbstractMeshData : ExternalData
	{
		public AbstractMeshData()
		{
		}

		public AbstractMeshData(Guid typeGuid) : base(typeGuid)
		{
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Color : IFormattable
		{
			public byte R;
			public byte G;
			public byte B;
			public byte A;

			public override string ToString()
			{
				return ToString("G", CultureInfo.CurrentCulture);
			}

			public string ToString(string format)
			{
				return ToString(format, CultureInfo.CurrentCulture);
			}

			public string ToString(string format, IFormatProvider formatProvider)
			{
				StringBuilder stringBuilder = new StringBuilder();
				string numberGroupSeparator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
				stringBuilder.Append('<');
				stringBuilder.Append(((IFormattable)R).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)G).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)B).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)A).ToString(format, formatProvider));
				stringBuilder.Append('>');
				return stringBuilder.ToString();
			}
		}

		protected static T[] ReadStructArray<T>(BinaryReader stream) where T : struct
		{
			int count = stream.ReadInt32();
			return ReadStructArray<T>(stream, count);
		}

		protected static T[] ReadStructArray<T>(BinaryReader stream, int count) where T : struct
		{
			int unitSize = GetStructSize<T>();
			int size = count * unitSize;
			var rawData = stream.ReadBytes(size);
			var array = new T[size / unitSize];
			if (size > 0)
			{
#if NATIVE_INTEROP_SHIT
				Buffer.Copy(rawData, array);
#else
				var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
				IntPtr ptr = handle.AddrOfPinnedObject();
				Marshal.Copy(rawData, 0, ptr, rawData.Length);
				handle.Free();
#endif
			}

			return array;
		}

		protected static int GetStructSize<T>()
		{
#if NET40 || NET45
			return Marshal.SizeOf(typeof(T));
#else
			return Marshal.SizeOf<T>();
#endif
		}

		protected static T[] CreateEmptyArray<T>()
		{
			return new T[0];
		}
	}
}