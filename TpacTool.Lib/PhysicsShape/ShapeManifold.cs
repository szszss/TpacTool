using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace TpacTool.Lib
{
	public sealed class ShapeManifold
	{
		public Vector3[] Vertices { get; private set; }

		public Face[] Faces { get; private set; }

		public List<string> UnknownPhysicsMaterials { get; private set; }

		public uint UnknownUint { get; private set; }

		//public string UnknownPhysicsMaterialName { get; set; } // may be dominant material?

		public ShapeManifold(int vertexNum, int faceNum)
		{
			Vertices = new Vector3[vertexNum];
			Faces = new Face[faceNum];
			UnknownPhysicsMaterials = new List<string>();
			//UnknownPhysicsMaterialName = string.Empty;
		}

		public ShapeManifold(BinaryReader stream)
		{
			UnknownUint = stream.ReadUInt32();
			int length = stream.ReadInt32();
			Vertices = new Vector3[length];
			for (int i = 0; i < length; i++)
			{
				Vertices[i] = stream.ReadVec4AsVec3();
			}

			UnknownPhysicsMaterials = stream.ReadStringList();

			length = stream.ReadInt32();
			Faces = new Face[length];
			for (int i = 0; i < length; i++)
			{
				Faces[i] = new Face(stream);
			}
			//UnknownPhysicsMaterialName = stream.ReadBoolean() ? stream.ReadCrfString() : string.Empty;
		}

		public struct Face
		{
			public int PhysicsMaterialIndex;
			public int Index1, Index2, Index3, Index4;
			public List<string> Flags;

			public Face(BinaryReader stream)
			{
				PhysicsMaterialIndex = stream.ReadInt32();
				Flags = stream.ReadStringList();
				Index1 = stream.ReadInt32();
				Index2 = stream.ReadInt32();
				Index3 = stream.ReadInt32();
				Index4 = stream.ReadInt32();
			}
		}
	}
}