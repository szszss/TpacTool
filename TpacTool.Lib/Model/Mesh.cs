using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public sealed class Mesh
	{
		[NotNull]
		public string Name { set; get; }

		public Guid Guid { set; get; }

		public uint UnknownUInt2 { set; get; } // always 0

		public List<string> Flags { private set; get; }
		/* these flags have been used
			force_enable_cloth
			uses_cloth_simulation
		*/

		public int Lod { set; get; }

		public bool IsCompleteMesh { set; get; }

		public uint UnknownUint1 { set; get; } // 2 for most meshes. 3 for generated meshes.

		[NotNull]
		public AssetDependence<Material> Material { set; get; }

		[NotNull]
		public AssetDependence<Material> SecondMaterial { set; get; }

		public Vector4 FactorColor { set; get; }

		public Vector4 Factor2Color { set; get; }

		public Vector4 VectorArgument { set; get; }

		public Vector4 VectorArgument2 { set; get; }

		public int VertexKeyCount { private set; get; }

		public int PositionCount { private set; get; }

		public int FaceCount { private set; get; }

		public int VertexCount { private set; get; }

		public int SkinDataSize { private set; get; }

		[NotNull]
		public BoundingBox BoundingBox { set; get; }

		public int UnknownInt2 { set; get; } // used bones?

		[NotNull]
		public List<string> MaterialFlags { private set; get; }
		/* these flags have been used
			no_team_color
			use_team_color
			dont_use_tableau
			banner_tableau
			horse_body
			horse_tail
			no_tattoo
		*/

		public float UnknownFloat1 { set; get; } // 1 for most meshes. other positive numbers for many clothes and flags

		[NotNull]
		public ClothingMaterial ClothingMaterial { set; get; }

		public int UnknownInt3 { set; get; } // 120 for most meshes. 240 for a few armors and clothes

		public bool UnknownBool1 { set; get; } // false for most meshs. true for a few clothes

		public bool UnknownBool2 { set; get; } // false for almost all meshes. only true for clo_aserai_robe_c

		public bool UnknownBool3 { set; get; } // false for almost all meshes. only true for campaign_flag.2

		[CanBeNull]
		public ExternalLoader<MeshEditData> EditData { set; get; }

		[CanBeNull]
		public ExternalLoader<VertexStreamData> VertexStream { set; get; }

		public Mesh()
		{
			Name = String.Empty;
			Lod = 0;
			IsCompleteMesh = true;
			UnknownUint1 = 2;
			Material = new AssetDependence<Material>();
			SecondMaterial = new AssetDependence<Material>();
			BoundingBox = new BoundingBox();
			MaterialFlags = new List<string>();
			ClothingMaterial = new ClothingMaterial();
		}

		public Mesh(BinaryReader stream)
		{
			IsCompleteMesh = stream.ReadBoolean();
			Lod = stream.ReadInt32();
			UnknownUint1 = stream.ReadUInt32();
			SecondMaterial = new AssetDependence<Material>(stream.ReadGuid());
			var subVersion = stream.ReadUInt32();
			Guid = stream.ReadGuid();
			Name = stream.ReadSizedString();
			UnknownUInt2 = stream.ReadUInt32(); // always 0
			Flags = stream.ReadStringList();
			Material = new AssetDependence<Material>(stream.ReadGuid());
			FactorColor = stream.ReadVec4();
			Factor2Color = stream.ReadVec4();
			VectorArgument = stream.ReadVec4();
			VectorArgument2 = stream.ReadVec4();
			VertexKeyCount = stream.ReadInt32();
			PositionCount = stream.ReadInt32();
			FaceCount = stream.ReadInt32();
			VertexCount = stream.ReadInt32();
			SkinDataSize = stream.ReadInt32();
			int boundingBoxType = stream.ReadInt32();
			Debug.Assert(boundingBoxType == 0); // should always 0
			BoundingBox = new BoundingBox(stream);
			UnknownInt2 = stream.ReadInt32();
			MaterialFlags = stream.ReadStringList();
			UnknownFloat1 = stream.ReadSingle();
			ClothingMaterial = new ClothingMaterial(stream);
			UnknownInt3 = stream.ReadInt32();
			UnknownBool1 = stream.ReadBoolean();
			UnknownBool2 = stream.ReadBoolean();
			if (subVersion >= 1)
			{
				ClothingMaterial.ReadExtraData(stream);
				//UnknownFloat2 = stream.ReadSingle();
				//UnknownFloat3 = stream.ReadSingle();
				UnknownBool3 = stream.ReadBoolean();
			}
		}

		public void Write(BinaryWriter stream)
		{
			stream.Write(IsCompleteMesh);
			stream.Write(Lod);
			stream.Write(UnknownUint1);
			stream.Write(SecondMaterial.Guid);
			stream.Write((int) 1);
			stream.Write(Guid);
			stream.WriteSizedString(Name);
			stream.Write(UnknownUInt2);
			stream.WriteStringList(Flags);
			stream.Write(Material.Guid);
			stream.Write(FactorColor);
			stream.Write(Factor2Color);
			stream.Write(VectorArgument);
			stream.Write(VectorArgument2);
			stream.Write(VertexKeyCount);
			stream.Write(PositionCount);
			stream.Write(FaceCount);
			stream.Write(VertexCount);
			stream.Write(SkinDataSize);
			stream.Write(0);
			BoundingBox.Write(stream);
			stream.Write(UnknownInt2);
			stream.WriteStringList(MaterialFlags);
			stream.Write(UnknownFloat1);
			ClothingMaterial.WritePrimaryData(stream);
			stream.Write(UnknownInt3);
			stream.Write(UnknownBool1);
			stream.Write(UnknownBool2);
			ClothingMaterial.WriteExtraData(stream);
			stream.Write(UnknownBool3);
		}
	}
}