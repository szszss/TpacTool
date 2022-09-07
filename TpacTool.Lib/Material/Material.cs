using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class Material : AssetItem
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("1db01393-6902-4f19-83ba-b37a39830717");

		public Guid BillboardGuid { set; get; }

		public uint UnknownUint1 { set; get; } // always 0

		public List<string> Flags { private set; get; }

		public uint UnknownUint2 { set; get; } // always 0

		public List<string> VertexLayoutFlags { private set; get; }

		[NotNull]
		public string BlendMode { private set; get; }
		/*
		 no_alpha_blend
		 modulate
		 add_alpha
		 multiply
		 add
		 max
		 factor
		 add_modulate_combined
		 no_alpha_blend_no_write
		 modulate_no_write
		 gbuffer_alpha_blend
		 gbuffer_alpha_blend_with_vt_resolve
		 */

		[NotNull]
		public AssetDependence<Shader> Shader { set; get; }

		public SortedDictionary<int, AssetDependence<Texture>> Textures { private set; get; }

		public float AlphaTest { set; get; }

		public List<string> ShaderMaterialFlags { private set; get; }

		[NotNull]
		public ExtraMaterialSetting ExtraMaterialSettings { set; get; }

		public Material() : base(TYPE_GUID)
		{
			Flags = new List<string>();
			VertexLayoutFlags = new List<string>();
			BlendMode = "no_alpha_blend";
			Shader = AssetDependence<Shader>.CreateEmpty();
			Textures = new SortedDictionary<int, AssetDependence<Texture>>();
			ShaderMaterialFlags = new List<string>();
			ExtraMaterialSettings = new ExtraMaterialSetting();
		}

		public override void ReadMetadata(BinaryReader stream, int totalSize)
		{
			var version = stream.ReadUInt32();
			BillboardGuid = stream.ReadGuid();
			var subVersion = stream.ReadUInt32();
			UnknownUint1 = stream.ReadUInt32();
			Flags = stream.ReadStringList();
			UnknownUint2 = stream.ReadUInt32();
			VertexLayoutFlags = stream.ReadStringList();
			BlendMode = stream.ReadSizedString();
			Shader = new AssetDependence<Shader>(stream.ReadGuid());
			var texCount = stream.ReadInt32();
			Textures.Clear();
			for (int i = 0; i < texCount; i++)
			{
				var index = stream.ReadInt32();
				var texGuid = stream.ReadGuid();
				Textures[index] = new AssetDependence<Texture>(texGuid);
			}

			AlphaTest = stream.ReadSingle();
			ShaderMaterialFlags = stream.ReadStringList();
			ExtraMaterialSettings.Load(stream, subVersion);
		}

		public class ExtraMaterialSetting
		{
			public float AreamapScale { get; set; }
			public float AreamapAmount { get; set; }
			public float DetailnormalScale { get; set; }
			public float NormalmapPower { get; set; }
			public Vector4 MeshVectorArgument { get; set; }
			public Vector4 MeshVectorArgument2 { get; set; }
			public Vector4 MeshFactorColorMultiplier { get; set; }
			public Vector4 MeshFactor2ColorMultiplier { get; set; }
			public int RenderOrder { get; set; }
			public float MipmapBias { get; set; }
			public float SpecularCoef { get; set; }
			public float GlossCoef { get; set; }
			public float ParallaxAmount { get; set; }
			public float ParallaxOffset { get; set; }
			public float AmbientOcclusionCoef { get; set; }
			public float ExposureCompensation { get; set; }

			public ExtraMaterialSetting()
			{
				AreamapScale = 1f;
				AreamapAmount = 0.65f;
				DetailnormalScale = 1f;
				NormalmapPower = 1f;
				MeshVectorArgument = Vector4.Zero;
				MeshVectorArgument2 = Vector4.Zero;
				MeshFactorColorMultiplier = Vector4.Zero;
				MeshFactor2ColorMultiplier = Vector4.Zero;
				ParallaxAmount = 0f;
				ParallaxOffset = 0.5f;
				AmbientOcclusionCoef = 1f;
				SpecularCoef = 1f;
				GlossCoef = 1f;
				ExposureCompensation = 1f;
			}

			internal void Load(BinaryReader stream, uint subVersion = 2)
			{
				AreamapScale = stream.ReadSingle();
				AreamapAmount = stream.ReadSingle();
				DetailnormalScale = stream.ReadSingle();
				NormalmapPower = stream.ReadSingle();
				MeshVectorArgument = stream.ReadVec4();
				MeshVectorArgument2 = stream.ReadVec4();
				MeshFactorColorMultiplier = stream.ReadVec4();
				MeshFactor2ColorMultiplier = stream.ReadVec4();
				RenderOrder = stream.ReadInt32();
				MipmapBias = stream.ReadSingle();
				SpecularCoef = stream.ReadSingle();
				GlossCoef = stream.ReadSingle();
				ParallaxAmount = stream.ReadSingle();
				if (subVersion >= 1)
					ParallaxOffset = stream.ReadSingle();
				AmbientOcclusionCoef = stream.ReadSingle();
				if (subVersion >= 2) // since 1.8.0
					ExposureCompensation = stream.ReadSingle();
			}
		}
	}
}