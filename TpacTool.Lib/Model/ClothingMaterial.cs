using System.IO;

namespace TpacTool.Lib
{
	public sealed class ClothingMaterial
	{
		public string Name { set; get; }

		public float BendingStiffness { set; get; }

		public float ShearingStiffness { set; get; }

		public float StretchingStiffness { set; get; }

		public float AnchorStiffness { set; get; }

		public float Damping { set; get; }

		public float Gravity { set; get; }

		public float LinearInertia { set; get; }

		public float MaxLinearVelocity { set; get; }

		public float LinearVelocityMultiplier { set; get; }

		public float AirDragMultiplier { set; get; }

		public float Wind { set; get; }

		public ClothingMaterial()
		{
			Name = string.Empty;
			MaxLinearVelocity = -1f;
			LinearVelocityMultiplier = 1f;
		}

		public ClothingMaterial(BinaryReader stream)
		{
			Name = stream.ReadSizedString();
			BendingStiffness = stream.ReadSingle();
			ShearingStiffness = stream.ReadSingle();
			StretchingStiffness = stream.ReadSingle();
			AnchorStiffness = stream.ReadSingle();
			Damping = stream.ReadSingle();
			Gravity = stream.ReadSingle();
			LinearInertia = stream.ReadSingle();
			AirDragMultiplier = stream.ReadSingle();
			Wind = stream.ReadSingle();
		}

		public void ReadExtraData(BinaryReader stream)
		{
			MaxLinearVelocity = stream.ReadSingle();
			LinearVelocityMultiplier = stream.ReadSingle();
		}

		public void WritePrimaryData(BinaryWriter stream)
		{
			stream.WriteSizedString(Name);
			stream.Write(BendingStiffness);
			stream.Write(ShearingStiffness);
			stream.Write(StretchingStiffness);
			stream.Write(AnchorStiffness);
			stream.Write(Damping);
			stream.Write(Gravity);
			stream.Write(LinearInertia);
			stream.Write(AirDragMultiplier);
			stream.Write(Wind);
		}

		public void WriteExtraData(BinaryWriter stream)
		{
			stream.Write(MaxLinearVelocity);
			stream.Write(LinearVelocityMultiplier);
		}
	}
}