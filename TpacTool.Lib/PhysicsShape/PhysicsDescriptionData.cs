using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class PhysicsDescriptionData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("86a5d575-5dc1-4542-a52f-3865aa8fb4fa");

		[NotNull]
		public string Name { get; set; }

		[NotNull]
		public List<ShapeCapsule> Capsules { get; private set; }

		[NotNull]
		public List<ShapeSphere> Spheres { get; private set; }

		[NotNull]
		public List<ShapeManifold> Manifolds { get; private set; }

		public PhysicsDescriptionData() : base(TYPE_GUID)
		{
			Name = String.Empty;
			Capsules = new List<ShapeCapsule>();
			Spheres = new List<ShapeSphere>();
			Manifolds = new List<ShapeManifold>();
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			Name = stream.ReadSizedString();

			int length = stream.ReadInt32();
			Capsules.Capacity = length;
			for (int i = 0; i < length; i++)
			{
				Capsules.Add(new ShapeCapsule(stream));
			}

			length = stream.ReadInt32();
			Spheres.Capacity = length;
			for (int i = 0; i < length; i++)
			{
				Spheres.Add(new ShapeSphere(stream));
			}

			length = stream.ReadInt32();
			Manifolds.Capacity = length;
			for (int i = 0; i < length; i++)
			{
				Manifolds.Add(new ShapeManifold(stream));
			}
		}
	}
}