using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class SkeletonUserData : ExternalData
	{
		public static readonly Guid TYPE_GUID = Guid.Parse("9b6ac06d-a546-40af-a555-40d301ab4b2f");

		public const string USAGE_OTHER = "other";

		public const string USAGE_HUMAN = "human";

		public const string USAGE_HORSE = "horse";

		public float UnknownFloat1 { set; get; }

		public Vector3 UnknownVector1 { set; get; }

		public Vector3 UnknownVector2 { set; get; }

		[NotNull]
		public string Usage { set; get; }

		[NotNull]
		public string UnknownString { set; get; }

		public Guid UnknownGuid { set; get; }

		public int UnknownInt { set; get; }

		[NotNull]
		public List<Body> Bodies { private set; get; }

		[NotNull]
		public List<Constraint> Constraints { private set; get; }

		public SkeletonUserData() : base(TYPE_GUID)
		{
			Usage = USAGE_OTHER;
			UnknownString = String.Empty;
			Bodies = new List<Body>();
			Constraints = new List<Constraint>();
		}

		public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
		{
			UnknownFloat1 = stream.ReadSingle();
			UnknownVector1 = stream.ReadVec4AsVec3();
			UnknownVector2 = stream.ReadVec4AsVec3();
			Usage = stream.ReadSizedString();
			UnknownString = stream.ReadSizedString();
			UnknownGuid = stream.ReadGuid();
			int num = stream.ReadInt32();
			Bodies.Clear();
			Bodies.Capacity = num;
			for (int i = 0; i < num; i++)
			{
				var body = new Body();
				body.UnknownString1 = stream.ReadSizedString();
				body.UnknownBool = stream.ReadBoolean();
				body.UnknownString2 = stream.ReadSizedString();
				body.UnknownString3 = stream.ReadSizedString();
				body.UnknownFloat1 = stream.ReadSingle();
				body.UnknownVector1 = stream.ReadVec4AsVec3();
				body.UnknownVector2 = stream.ReadVec4AsVec3();
				body.UnknownFloat2 = stream.ReadSingle();
				body.UnknownVector3 = stream.ReadVec4AsVec3();
				body.UnknownVector4 = stream.ReadVec4AsVec3();
				body.UnknownFloat3 = stream.ReadSingle();
				body.UnknownFloat4 = stream.ReadSingle();
				Bodies.Add(body);
			}

			UnknownInt = stream.ReadInt32();
			num = stream.ReadInt32();
			Constraints.Clear();
			Constraints.Capacity = num;
			for (int i = 0; i < num; i++)
			{
				var version = stream.ReadUInt32();
				var type = stream.ReadSizedString();
				var s1 = stream.ReadSizedString();
				var s2 = stream.ReadSizedString();
				var s3 = stream.ReadSizedString();
				var tf = stream.ReadTransform();
				Constraint constraint = null;
				switch (type)
				{
					case HingeJointConstraint.TYPE:
						var hinge = new HingeJointConstraint();
						hinge.UnknownFloat1 = stream.ReadSingle();
						hinge.UnknownFloat2 = stream.ReadSingle();
						constraint = hinge;
						break;
					case D6JointConstraint.TYPE:
						var d6 = new D6JointConstraint();
						d6.Constraint1 = stream.ReadSizedString();
						d6.Constraint2 = stream.ReadSizedString();
						d6.Constraint3 = stream.ReadSizedString();
						d6.Constraint4 = stream.ReadSizedString();
						d6.Constraint5 = stream.ReadSizedString();
						d6.Constraint6 = stream.ReadSizedString();
						d6.UnknownFloat1 = stream.ReadSingle();
						d6.UnknownFloat2 = stream.ReadSingle();
						d6.UnknownFloat3 = stream.ReadSingle();
						d6.UnknownFloat4 = stream.ReadSingle();
						d6.UnknownFloat5 = stream.ReadSingle();
						constraint = d6;
						break;
					case IKConstraint.TYPE:
						var ik = new IKConstraint();
						ik.UnknownUint = stream.ReadUInt32();
						ik.UnknownFloat1 = stream.ReadSingle();
						ik.UnknownFloat2 = stream.ReadSingle();
						ik.UnknownFloat3 = stream.ReadSingle();
						ik.UnknownFloat4 = stream.ReadSingle();
						constraint = ik;
						break;
					default:
						throw new Exception("Unknown constraint type: " + type);
				}

				constraint.UnknownString1 = s1;
				constraint.UnknownString2 = s2;
				constraint.UnknownString3 = s3;
				constraint.UnknownTransform = tf;
				Constraints.Add(constraint);
			}
		}

		public abstract class Constraint
		{
			public string UnknownString1 { set; get; }

			public string UnknownString2 { set; get; }

			public string UnknownString3 { set; get; }

			public Transform UnknownTransform { set; get; }

			protected Constraint()
			{
				UnknownString1 = String.Empty;
				UnknownString2 = String.Empty;
				UnknownString3 = String.Empty;
			}
		}

		public class HingeJointConstraint : Constraint
		{
			public const string TYPE = "hinge";

			public float UnknownFloat1 { set; get; }

			public float UnknownFloat2 { set; get; }
		}

		public class D6JointConstraint : Constraint
		{
			public const string TYPE = "d6";

			public string Constraint1 { set; get; }

			public string Constraint2 { set; get; }

			public string Constraint3 { set; get; }

			public string Constraint4 { set; get; }

			public string Constraint5 { set; get; }

			public string Constraint6 { set; get; }

			public float UnknownFloat1 { set; get; }

			public float UnknownFloat2 { set; get; }

			public float UnknownFloat3 { set; get; }

			public float UnknownFloat4 { set; get; }

			public float UnknownFloat5 { set; get; }

			public D6JointConstraint()
			{
			}
		}

		public class IKConstraint : Constraint
		{
			public const string TYPE = "ik";

			public uint UnknownUint { set; get; }

			public float UnknownFloat1 { set; get; }

			public float UnknownFloat2 { set; get; }

			public float UnknownFloat3 { set; get; }

			public float UnknownFloat4 { set; get; }
		}

		public class Body
		{
			public string UnknownString1 { set; get; }

			public bool UnknownBool { set; get; }

			public string UnknownString2 { set; get; }

			public string UnknownString3 { set; get; }

			public float UnknownFloat1 { set; get; }

			public Vector3 UnknownVector1 { set; get; }

			public Vector3 UnknownVector2 { set; get; }

			public float UnknownFloat2 { set; get; }

			public Vector3 UnknownVector3 { set; get; }

			public Vector3 UnknownVector4 { set; get; }

			public float UnknownFloat3 { set; get; }

			public float UnknownFloat4 { set; get; }
		}
	}
}