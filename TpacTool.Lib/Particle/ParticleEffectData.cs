using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class ParticleEffectData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("326587ce-bb0c-4c22-8782-97e20cf03c5e");

		[NotNull]
		public string SoundCode { set; get; }

		[NotNull]
		public List<float> UnknownFloats { set; get; }

		public ParticleEffectData() : base(TYPE_GUID)
		{
			SoundCode = String.Empty;
			UnknownFloats = new List<float>();
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			SoundCode = stream.ReadSizedString();
			int length = stream.ReadInt32();
			// should a version check here
			UnknownFloats.Clear();
			UnknownFloats.Capacity = length;
			for (int i = 0; i < length; i++)
			{
				UnknownFloats.Add(stream.ReadSingle());
			}

			var i1 = stream.ReadInt32(); // emmiter number
			for (int i = 0; i < i1; i++)
			{
				var emitter = new Emitter(stream);
			}
		}

		public class Emitter
		{
			public Emitter()
			{
			}

			public Emitter(BinaryReader stream)
			{
				var subVersion = stream.ReadUInt32();

				var g1 = stream.ReadGuid();
				var g2 = stream.ReadGuid();
				var g3 = stream.ReadGuid();
				var g4 = stream.ReadGuid();
				var name = stream.ReadSizedString();
				var u2 = stream.ReadUInt32();
				var flags = stream.ReadStringList();

				var particle_size_curve_op = stream.ReadSizedString();
				var s4 = stream.ReadSizedString();

				var f1 = stream.ReadSingle();
				var u3 = stream.ReadUInt32();
				var f2 = stream.ReadSingle();
				var f3 = stream.ReadSingle();
				var i1 = stream.ReadInt32();
				var i2 = stream.ReadInt32();
				var i3 = stream.ReadInt32();
				var i4 = stream.ReadInt32();
				var i5 = stream.ReadInt32();
				var f4 = stream.ReadSingle();
				var f5 = stream.ReadSingle();
				var f6 = stream.ReadSingle();
				var f7 = stream.ReadSingle();
				var f8 = stream.ReadSingle();
				var f9 = stream.ReadSingle();
				var f10 = stream.ReadSingle();
				var f11 = stream.ReadSingle();
				var backlight_multiplier = stream.ReadSingle();
				var diffuse_multiplier = stream.ReadSingle();
				var emissive_multiplier = stream.ReadSingle();
				var heatmap_multiplier = stream.ReadSingle();
				var f16 = stream.ReadSingle();
				var f17 = stream.ReadSingle();
				var cone_emit_angle = stream.ReadSingle();
				var f19 = stream.ReadSingle();
				var f20 = stream.ReadSingle();
				var f21 = stream.ReadSingle();
				var v1 = stream.ReadVec4AsVec3();
				var v2 = stream.ReadVec4AsVec3();

				var curve1 = new EmitterParameter(stream);

				var g5 = stream.ReadGuid();
				var collision_behaviour = stream.ReadSizedString();
				var emission_velocity_model = stream.ReadSizedString();
				string s3 = null;
				if (subVersion >= 2)
				{
					s3 = stream.ReadSizedString();
				}

				var curve2 = new EmitterParameter(stream);
				var curve3 = new EmitterParameter(stream);
				var curve4 = new EmitterParameter(stream);
				var curve5 = new EmitterParameter(stream);
				var curve6 = new EmitterParameter(stream);
				var u4 = stream.ReadUInt32();
				var u5 = stream.ReadUInt32();
				var particle_size_base = stream.ReadSingle();
				var particle_size_bias = stream.ReadSingle();
				var curve7 = new Curve(stream);
				var curve8 = new Curve(stream);
				var curve9 = new EmitterParameter(stream);
				var curve10 = new EmitterParameter(stream);
				var curve11 = new EmitterParameter(stream);
				var curve12 = new EmitterParameter(stream);
				var curve13 = new EmitterParameter(stream);
				var curve14 = new EmitterParameter(stream);
				EmitterParameter curve15 = null;
				if (subVersion >= 1)
				{
					curve15 = new EmitterParameter(stream);
				}
				var u6 = stream.ReadUInt32();
				var f22 = stream.ReadSingle();
				var f23 = stream.ReadSingle();
				var u7 = stream.ReadUInt32();
				var f24 = stream.ReadSingle();
				var f25 = stream.ReadSingle();
				var u8 = stream.ReadUInt32();
				var f26 = stream.ReadSingle();
				var f27 = stream.ReadSingle();
				var billboard_type = stream.ReadSizedString();
				var emit_volume_type = stream.ReadSizedString();

				var color = new ParticleColorParameter(stream);
				var v3 = stream.ReadVec4AsVec3();
				var f28 = stream.ReadSingle();
				var f29 = stream.ReadSingle();
				var f30 = stream.ReadSingle();
				var f31 = stream.ReadSingle();
				var v4 = stream.ReadVec4AsVec3();
				var v5 = stream.ReadVec4AsVec3();
				var gravity = stream.ReadVec4AsVec3();
				var v7 = stream.ReadVec4AsVec3();
				var fixed_billboard_direction = stream.ReadVec4AsVec3();
				var f32 = stream.ReadSingle();
				var texture_sprite_count = Tuple.Create(stream.ReadInt32(), stream.ReadInt32());
				var texture_sprite_frame_count = stream.ReadUInt32();
				var texture_sprite_frame_rate = stream.ReadSingle();

				int length = stream.ReadInt32();
				Guid[] guids = new Guid[length];
				for (int i = 0; i < length; i++)
				{
					guids[i] = stream.ReadGuid();
				}
				var decal_min_scale = stream.ReadVec2();
				var decal_max_scale = stream.ReadVec2();
				var quad_scale = stream.ReadVec2();
				var quad_bias = stream.ReadVec2();
				var skinned_decal_start_index = stream.ReadInt32();
				var skinned_decal_end_index = stream.ReadInt32();
				var max_alive_particle_count = stream.ReadUInt32(); // not sure
				var emitterSoundCode = stream.ReadSizedString();
				var s11 = stream.ReadSizedString(); // unknown. always empty
				var f34 = stream.ReadSingle(); // unknown, always 1f
			}
		}

		public class EmitterParameter
		{
			public uint UnknownUInt1 { set; get; }

			public uint UnknownUInt2 { set; get; }

			public float UnknownFloat1 { set; get; }

			public float UnknownFloat2 { set; get; }

			[NotNull]
			public Curve Curve { set; get; }

			public EmitterParameter()
			{
				Curve = new Curve();
			}

			public EmitterParameter(BinaryReader stream)
			{
				UnknownUInt1 = stream.ReadUInt32();
				UnknownUInt2 = stream.ReadUInt32();
				UnknownFloat1 = stream.ReadSingle();
				UnknownFloat2 = stream.ReadSingle();

				Curve = new Curve(stream);
			}
		}

		public class Curve
		{
			public uint Version { set; get; }

			public float Default { set; get; }

			public float CurveMultiplier { set; get; }

			[NotNull]
			public List<Vector4> Keys { private set; get; }

			public Curve()
			{
				Keys = new List<Vector4>();
			}

			public Curve(BinaryReader stream)
			{
				Version = stream.ReadUInt32();
				Default = stream.ReadSingle();
				CurveMultiplier = stream.ReadSingle();
				var length = stream.ReadInt32();
				Keys = new List<Vector4>(length * 2);
				for (int i = 0; i < length; i++)
				{
					Keys.Add(stream.ReadVec4());
					Keys.Add(stream.ReadVec4());
				}
			}
		}

		public class ParticleColorParameter
		{
			public uint UnknownUInt { set; get; }

			public SortedList<float, Vector3> Colors { private set; get; }

			public SortedList<float, float> Alphas { private set; get; }

			public ParticleColorParameter()
			{
				Colors = new SortedList<float, Vector3>();
				Alphas = new SortedList<float, float>();
			}

			public ParticleColorParameter(BinaryReader stream)
			{
				UnknownUInt = stream.ReadUInt32();

				int length = stream.ReadInt32();
				Colors = new SortedList<float, Vector3>(length);
				for (int i = 0; i < length; i++)
				{
					var key = stream.ReadSingle();
					Colors[key] = stream.ReadVec4AsVec3();
				}

				length = stream.ReadInt32();
				Alphas = new SortedList<float, float>(length);
				for (int i = 0; i < length; i++)
				{
					var key = stream.ReadSingle();
					Alphas[key] = stream.ReadSingle();
				}
			}
		}
	}
}