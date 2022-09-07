using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using SystemHalf;
using JetBrains.Annotations;

#if NET5_0_OR_GREATER
using Half = SystemHalf.Half;
#endif

namespace TpacTool.Lib
{
	public class VertexStreamData : AbstractMeshData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("bb1df897-584f-4770-abf2-663fe449f247");

		public const string KEY_IS_32BIT_INDEX = "is32bit";

		public const string KEY_HAS_QTANGENT = "hasqtangent";

		public int[] Indices { set; get; }

		public Color[] Colors1 { set; get; }

		public Color[] Colors2 { set; get; }

		public Vector2[] Uv1 { set; get; }

		public Vector2[] Uv2 { set; get; }

		public Vector3[] Positions { set; get; }

		public Vector3[] UnknownAnotherPositions { set; get; }

		public Vector3[] Normals { set; get; }

		public Vector4[] Tangents { set; get; }

		public BoneWeight[] BoneWeights { set; get; }

		public BoneIndex[] BoneIndices { set; get; }

		public X11Y11Z10[] CompressedNormals { set; get; }

		public Half4[] CompressedPositions { set; get; }

		public X10Y11Z10W1[] CompressedTangents { set; get; }

		[CanBeNull]
		public SnormShort4[] TangentTransform { set; get; }

		public VertexStreamData() : base(TYPE_GUID)
		{
			Indices = CreateEmptyArray<int>();
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			bool use32bit = HasUserdataTag(userdata, KEY_IS_32BIT_INDEX);
			bool hasQTangent = HasUserdataTag(userdata, KEY_HAS_QTANGENT);
			int indexNum = stream.ReadInt32();
			if (use32bit)
			{
				Indices = ReadStructArray<int>(stream, indexNum);
			}
			else
			{
				int[] temp = new int[indexNum];
				for (int i = 0; i < indexNum; i++)
				{
					temp[i] = stream.ReadUInt16();
				}
				Indices = temp;
			}

			int arrayLength = 26;
			if (hasQTangent)
				arrayLength += 2;
			var sizes = ReadStructArray<ulong>(stream, arrayLength);
			Colors1 = ReadStructArray<Color>(stream, (int)(sizes[1] / 4));
			Colors2 = ReadStructArray<Color>(stream, (int)(sizes[3] / 4));
			Uv1 = ReadStructArray<Vector2>(stream, (int) (sizes[5] / 8));
			Uv2 = ReadStructArray<Vector2>(stream, (int)(sizes[7] / 8));
			Positions = ReadStructArray<Vector3>(stream, (int)(sizes[9] / 12));
			UnknownAnotherPositions = ReadStructArray<Vector3>(stream, (int)(sizes[11] / 12));
			Normals = ReadStructArray<Vector3>(stream, (int)(sizes[13] / 12));
			Tangents = ReadStructArray<Vector4>(stream, (int)(sizes[15] / 16));
			BoneWeights = ReadStructArray<BoneWeight>(stream, (int)(sizes[19] / 4));
			BoneIndices = ReadStructArray<BoneIndex>(stream, (int)(sizes[17] / 4));
			CompressedNormals = ReadStructArray<X11Y11Z10>(stream, (int) (sizes[21] / 4));
			CompressedPositions = ReadStructArray<Half4>(stream, (int) (sizes[23] / 8));
			CompressedTangents = ReadStructArray<X10Y11Z10W1>(stream, (int) (sizes[25] / 4));
			if (hasQTangent)
				TangentTransform = ReadStructArray<SnormShort4>(stream, (int)(sizes[27] / 8));
		}

		public override void WriteData(BinaryWriter stream, IDictionary<object, object> userdata)
		{
			stream.Write(Indices.Length);
			if (Indices.Length >= UInt16.MaxValue)
			{
				WriteStructArray(stream, Indices);
				userdata[KEY_IS_32BIT_INDEX] = true;
			}
			else
			{
				for (int i = 0; i < Indices.Length; i++)
				{
					stream.Write((ushort) Indices[i]);
				}
				userdata[KEY_IS_32BIT_INDEX] = false;
			}
			bool hasQTangent = HasUserdataTag(userdata, KEY_HAS_QTANGENT);
			ulong length = 0;
			for (int i = 0, j = hasQTangent ? 14 : 13; i < j; i++)
			{
				int size = 0;
				switch (i)
				{
					case 0: size = Colors1.Length * 4; break;
					case 1: size = Colors2.Length * 4; break;
					case 2: size = Uv1.Length * 8; break;
					case 3: size = Uv2.Length * 8; break;
					case 4: size = Positions.Length * 12; break;
					case 5: size = UnknownAnotherPositions.Length * 12; break;
					case 6: size = Normals.Length * 12; break;
					case 7: size = Tangents.Length * 16; break;
					case 8: size = BoneWeights.Length * 4; break;
					case 9: size = BoneIndices.Length * 4; break;
					case 10: size = CompressedNormals.Length * 4; break;
					case 11: size = CompressedPositions.Length * 8; break;
					case 12: size = CompressedTangents.Length * 4; break;
					case 13: size = TangentTransform.Length * 8; break;
				}

				stream.Write(length);
				stream.Write((ulong) size);
				length += (ulong) size;
			}

			WriteStructArray(stream, Colors1);
			WriteStructArray(stream, Colors2);
			WriteStructArray(stream, Uv1);
			WriteStructArray(stream, Uv2);
			WriteStructArray(stream, Positions);
			WriteStructArray(stream, UnknownAnotherPositions);
			WriteStructArray(stream, Normals);
			WriteStructArray(stream, Tangents);
			WriteStructArray(stream, BoneWeights);
			WriteStructArray(stream, BoneIndices);
			WriteStructArray(stream, CompressedNormals);
			WriteStructArray(stream, CompressedPositions);
			WriteStructArray(stream, CompressedTangents);
			if (hasQTangent)
				WriteStructArray(stream, TangentTransform);
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BoneIndex : IFormattable
		{
			public byte B1;
			public byte B2;
			public byte B3;
			public byte B4;

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
				stringBuilder.Append(((IFormattable)B1).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)B2).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)B3).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)B4).ToString(format, formatProvider));
				stringBuilder.Append('>');
				return stringBuilder.ToString();
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BoneWeight : IFormattable
		{
			public byte W1;
			public byte W2;
			public byte W3;
			public byte W4;

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
				stringBuilder.Append(((IFormattable)(W1 / 255f)).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)(W2 / 255f)).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)(W3 / 255f)).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)(W4 / 255f)).ToString(format, formatProvider));
				stringBuilder.Append('>');
				return stringBuilder.ToString();
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Half4 : IFormattable
		{
			public Half X;
			public Half Y;
			public Half Z;
			public Half W;

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
				stringBuilder.Append(((IFormattable)X).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)Y).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)Z).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)W).ToString(format, formatProvider));
				stringBuilder.Append('>');
				return stringBuilder.ToString();
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct X11Y11Z10 : IFormattable
		{
			public uint RawData;

			public int RawZ
			{
				get { return (int)(RawData & 0x3FF); }
				set { RawData = (RawData & (~0x3FFu)) | ((uint)value & 0x3FFu); }
			}

			public int RawY
			{
				get { return (int)((RawData & 0x1FFC00) >> 10); }
				set { RawData = (RawData & (~0x1FFC00u)) | (((uint)value & 0x7FFu) << 10); }
			}

			public int RawX
			{
				get { return (int)((RawData & 0xFFE00000u) >> 21); }
				set { RawData = (RawData & (~0xFFE00000u)) | (((uint)value & 0x7FFu) << 21); }
			}

			public float Z
			{
				get { return (RawZ / 1023f) * 2f - 1; }
				set { RawZ = Math.Min((int)((value.Clamp() + 1) * 1023) / 2, 0x3FF); }
			}

			public float Y
			{
				get { return (RawY / 2047f) * 2f - 1; }
				set { RawY = Math.Min((int)((value.Clamp() + 1) * 2047) / 2, 0x7FF); }
			}

			public float X
			{
				get { return (RawX / 2047f) * 2f - 1; }
				set { RawX = Math.Min((int)((value.Clamp() + 1) * 2047) / 2, 0x7FF); }
			}

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
				stringBuilder.Append(((IFormattable)X).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)Y).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)Z).ToString(format, formatProvider));
				stringBuilder.Append('>');
				return stringBuilder.ToString();
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct X10Y11Z10W1 : IFormattable
		{
			public uint RawData;

			public int RawZ
			{
				get { return (int)(RawData & 0x3FF); }
				set { RawData = (RawData & (~0x3FFu)) | ((uint)value & 0x3FFu); }
			}

			public int RawY
			{
				get { return (int)((RawData & 0x1FFC00) >> 10); }
				set { RawData = (RawData & (~0x1FFC00u)) | (((uint)value & 0x7FFu) << 10); }
			}

			public int RawX
			{
				get { return (int)((RawData & 0x7FE00000) >> 21); }
				set { RawData = (RawData & (~0x7FE00000u)) | (((uint)value & 0x3FFu) << 21); }
			}

			public int RawW
			{
				get { return (int)((RawData & 0x80000000u) >> 31); }
				set { RawData = (RawData & (~0x80000000u)) | (value > 0 ? 0x80000000u : 0); }
			}

			public float Z
			{
				get { return (RawZ / 1023f) * 2f - 1; }
				set { RawZ = Math.Min((int)((value.Clamp() + 1) * 1023) / 2, 0x3FF); }
			}

			public float Y
			{
				get { return (RawY / 2047f) * 2f - 1; }
				set { RawY = Math.Min((int)((value.Clamp() + 1) * 2047) / 2, 0x7FF); }
			}

			public float X
			{
				get { return (RawX / 1023f) * 2f - 1; }
				set { RawX = Math.Min((int)((value.Clamp() + 1) * 1023) / 2, 0x3FF); }
			}

			public bool IsNeg
			{
				get { return RawW == 1; }
				set { RawW = IsNeg ? 1 : 0; }
			}

			public int Sign
			{
				get { return IsNeg ? -1 : 1; }
				set { IsNeg = value < 0; }
			}


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
				stringBuilder.Append(((IFormattable)X).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)Y).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)Z).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(Sign.ToString(format, formatProvider));
				stringBuilder.Append('>');
				return stringBuilder.ToString();
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SnormShort4 : IFormattable
		{			
			public short RawX;
			public short RawY;
			public short RawZ;
			public short RawW;

			public float X
			{
				get { return RawX < 0 ? -((float)RawX / short.MinValue) : (float)RawX / short.MaxValue; }
				set { RawX = (short) Math.Min(Math.Max((int) (value * short.MaxValue), short.MinValue), short.MaxValue); }
				// there is a very little error when set a negative value
				//(should be "-value * short.MinValue" for the negative values)
			}

			public float Y
			{
				get { return RawY < 0 ? -((float)RawY / short.MinValue) : (float)RawY / short.MaxValue; }
				set { RawY = (short)Math.Min(Math.Max((int)(value * short.MaxValue), short.MinValue), short.MaxValue); }
			}

			public float Z
			{
				get { return RawZ < 0 ? -((float)RawZ / short.MinValue) : (float)RawZ / short.MaxValue; }
				set { RawZ = (short)Math.Min(Math.Max((int)(value * short.MaxValue), short.MinValue), short.MaxValue); }
			}

			public float W
			{
				get { return RawW < 0 ? -((float)RawW / short.MinValue) : (float)RawW / short.MaxValue; }
				set { RawW = (short)Math.Min(Math.Max((int)(value * short.MaxValue), short.MinValue), short.MaxValue); }
			}

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
				stringBuilder.Append(((IFormattable)X).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)Y).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)Z).ToString(format, formatProvider));
				stringBuilder.Append(numberGroupSeparator);
				stringBuilder.Append(' ');
				stringBuilder.Append(((IFormattable)W).ToString(format, formatProvider));
				stringBuilder.Append('>');
				return stringBuilder.ToString();
			}

			public static implicit operator Quaternion(SnormShort4 q)
			{
				return new Quaternion(q.X, q.Y, q.Z, q.W);
			}

			public static implicit operator SnormShort4(Quaternion v)
			{
				return new SnormShort4() { W = v.W, X = v.X, Y = v.Y, Z = v.Z };
			}
		}
	}
}