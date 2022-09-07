using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
#pragma warning disable CS0612 // no obsolete warning for FaceSmoothingGroupMasks
	public class MeshEditData : AbstractMeshData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("3d41985f-24d2-4fc1-82e4-6a6e0da3f4e2");

		private static int sizeOfStruct = GetStructSize<Vertex>();

		[NotNull]
		public Vector4[] Positions { set; get; } // should be vec3, the last component is a padding and will be ignored by the game

		[NotNull]
		public Vector4[] UnusedVec3 { set; get; } // never be used by any model of bannerlord

		[NotNull]
		public Vertex[] Vertices { set; get; }

		[NotNull]
		public Face[] Faces { set; get; }

		public List<VertexFrame> MorphFrames { private set; get; }

		[NotNull]
		[Obsolete] // seems bug, don't use it for now
		public Bone[] Bones { set; get; }

		[NotNull]
		[Obsolete]
		public int[] FaceSmoothingGroupMasks { set; get; } // deprecated after multiplayer beta

		public MeshEditData() : base(TYPE_GUID)
		{
			Positions = CreateEmptyArray<Vector4>();
			UnusedVec3 = CreateEmptyArray<Vector4>();
			Vertices = CreateEmptyArray<Vertex>();
			Faces = CreateEmptyArray<Face>();
			MorphFrames = new List<VertexFrame>();
			Bones = CreateEmptyArray<Bone>();
			FaceSmoothingGroupMasks = CreateEmptyArray<int>();
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			Positions = ReadStructArray<Vector4>(stream);
			UnusedVec3 = ReadStructArray<Vector4>(stream);
			Vertices = ReadStructArray<Vertex>(stream);
			Faces = ReadStructArray<Face>(stream);
			int num = stream.ReadInt32();
			MorphFrames.Clear();
			MorphFrames.Capacity = num;
			for (int i = 0; i < num; i++)
			{
				var frame = new VertexFrame();
				frame.Time = stream.ReadInt32();
				MorphFrames.Add(frame);
			}

			for (int i = 0; i < num; i++)
			{
				var frame = MorphFrames[i];
				frame.Positions = ReadStructArray<Vector4>(stream);
				frame.Normals = ReadStructArray<Vector4>(stream);
			}
			Bones = ReadStructArray<Bone>(stream);
			FaceSmoothingGroupMasks = ReadStructArray<int>(stream);
		}

		public override void WriteData(BinaryWriter stream, IDictionary<object, object> userdata)
		{
			WriteStructArray(stream, Positions);
			WriteStructArray(stream, UnusedVec3);
			WriteStructArray(stream, Vertices);
			WriteStructArray(stream, Faces);

			stream.Write(MorphFrames.Count);
			for (var i = 0; i < MorphFrames.Count; i++)
			{
				stream.Write(MorphFrames[i].Time);
			}
			for (var i = 0; i < MorphFrames.Count; i++)
			{
				WriteStructArray(stream, MorphFrames[i].Positions);
				WriteStructArray(stream, MorphFrames[i].Normals);
			}

			WriteStructArray(stream, Bones);
			WriteStructArray(stream, FaceSmoothingGroupMasks);
		}

		public bool HasSecondUv()
		{
			foreach (var vertex in Vertices)
			{
				if (Math.Abs(vertex.SecondUv.X) > 0.001f || Math.Abs(vertex.SecondUv.Y) > 0.001f)
					return true;
			}

			return false;
		}

		public bool HasSecondColor()
		{
			foreach (var vertex in Vertices)
			{
				if ((vertex.SecondColor.R & vertex.SecondColor.G & vertex.SecondColor.B & vertex.SecondColor.A) != byte.MaxValue)
					return true;
			}

			return false;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Vertex
		{
			public uint PositionIndex; // Offset (in byte): 0
			public Vector4 Normal; // Offset: 4
			//-----------------
			// this is actully a 4x3 TBN matrix
			public Vector4 Tangent; // Offset: 20
			public Vector4 Binormal; // Offset: 36
			/// <summary>
			/// Values equal to Normal.
			/// </summary>
			public Vector4 Padding; // Offset: 52
			//-----------------
			public Vector2 Uv; // Offset: 68
			public Vector2 SecondUv; // Offset: 76
			public Color Color; // Offset: 84
			public Color SecondColor; // Offset: 88
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Face
		{
			public int V0;
			public int V1;
			public int V2;

			public int this[int i]
			{
				get
				{
					switch (i)
					{
						case 0:
							return V0;
						case 1:
							return V1;
						case 2:
							return V2;
						default:
							throw new ArgumentOutOfRangeException("The index of a face must be in [0, 3)");
					}
				}
				set
				{
					switch (i)
					{
						case 0:
							V0 = value;
							break;
						case 1:
							V1 = value;
							break;
						case 2:
							V2 = value;
							break;
						default:
							throw new ArgumentOutOfRangeException("The index of a face must be in [0, 3)");
					}
				}
			}
		}

		public sealed class VertexFrame
		{
			public int Time { get; set; }
			public Vector4[] Positions { get; set; }
			public Vector4[] Normals { get; set; }
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Bone
		{
			public float W0, W1, W2, W3;

			public byte B0, B1, B2, B3;
		}
	}
#pragma warning restore CS0612
}