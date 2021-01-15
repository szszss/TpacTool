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

		public float BoundingBoxPadding { set; get; }

		public Vector3 BoundingBoxMin { set; get; }

		public Vector3 BoundingBoxMax { set; get; }

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
			BoundingBoxPadding = stream.ReadSingle();
			BoundingBoxMin = stream.ReadVec4AsVec3();
			BoundingBoxMax = stream.ReadVec4AsVec3();
			Usage = stream.ReadSizedString();
			UnknownString = stream.ReadSizedString();
			UnknownGuid = stream.ReadGuid();
			int num = stream.ReadInt32();
			Bodies.Clear();
			Bodies.Capacity = num;
			for (int i = 0; i < num; i++)
			{
				var body = new Body();
				body.BoneName = stream.ReadSizedString();
				body.EnableBlend = stream.ReadBoolean();
				body.Type = stream.ReadSizedString();
				body.BodyType = stream.ReadSizedString();
				body.Mass = stream.ReadSingle();
				body.RagdollPosition1 = stream.ReadVec4AsVec3();
				body.RagdollPosition2 = stream.ReadVec4AsVec3();
				body.RagdollRadius = stream.ReadSingle();
				body.CollisionPosition1 = stream.ReadVec4AsVec3();
				body.CollisionPosition2 = stream.ReadVec4AsVec3();
				body.CollisionRadius = stream.ReadSingle();
				body.CollisionMaxRadius = stream.ReadSingle();
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
				var rot = stream.ReadQuat();
				var pos = stream.ReadVec4AsVec3();
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
						d6.AxisLockX = stream.ReadSizedString();
						d6.AxisLockY = stream.ReadSizedString();
						d6.AxisLockZ = stream.ReadSizedString();
						d6.TwistLock = stream.ReadSizedString();
						d6.Swing1Lock = stream.ReadSizedString();
						d6.Swing2Lock = stream.ReadSizedString();
						d6.AxisLimit = stream.ReadSingle();
						d6.TwistLowerLimit = stream.ReadSingle();
						d6.TwistUpperLimit = stream.ReadSingle();
						d6.Swing1Limit = stream.ReadSingle();
						d6.Swing2Limit = stream.ReadSingle();
						constraint = d6;
						break;
					case IKConstraint.TYPE:
						var ik = new IKConstraint();
						ik.UnknownUint = stream.ReadUInt32();
						ik.Swing1Limit = stream.ReadSingle();
						ik.Swing2Limit = stream.ReadSingle();
						ik.TwistLowerLimit = stream.ReadSingle();
						ik.TwistUpperLimit = stream.ReadSingle();
						constraint = ik;
						break;
					default:
						throw new Exception("Unknown constraint type: " + type);
				}

				constraint.Name = s1;
				constraint.Bone1 = s2;
				constraint.Bone2 = s3;
				constraint.EntitySpaceRotation = rot;
				constraint.Position = pos;
				Constraints.Add(constraint);
			}
		}

		public override void WriteData(BinaryWriter stream, IDictionary<object, object> userdata)
		{
			stream.Write(BoundingBoxPadding);
			stream.WriteVec3AsVec4(BoundingBoxMin);
			stream.WriteVec3AsVec4(BoundingBoxMax);

			stream.WriteSizedString(Usage);
			stream.WriteSizedString(UnknownString);
			stream.Write(UnknownGuid);

			stream.Write(Bodies.Count);
			for (int i = 0; i < Bodies.Count; i++)
			{
				var body = Bodies[i];
				stream.WriteSizedString(body.BoneName);
				stream.Write(body.EnableBlend);
				stream.WriteSizedString(body.Type);
				stream.WriteSizedString(body.BodyType);
				stream.Write(body.Mass);
				stream.WriteVec3AsVec4(body.RagdollPosition1);
				stream.WriteVec3AsVec4(body.RagdollPosition2);
				stream.Write(body.RagdollRadius);
				stream.WriteVec3AsVec4(body.CollisionPosition1);
				stream.WriteVec3AsVec4(body.CollisionPosition2);
				stream.Write(body.CollisionRadius);
				stream.Write(body.CollisionMaxRadius);
			}

			stream.Write(UnknownInt);
			stream.Write(Constraints.Count);
			for (int i = 0; i < Constraints.Count; i++)
			{
				var constraint = Constraints[i];
				stream.Write((int) 0);
				switch (constraint)
				{
					case HingeJointConstraint hinge:
						stream.WriteSizedString(HingeJointConstraint.TYPE);
						break;
					case D6JointConstraint d6:
						stream.WriteSizedString(D6JointConstraint.TYPE);
						break;
					case IKConstraint ik:
						stream.WriteSizedString(IKConstraint.TYPE);
						break;
				}

				stream.WriteSizedString(constraint.Name);
				stream.WriteSizedString(constraint.Bone1);
				stream.WriteSizedString(constraint.Bone2);
				stream.Write(constraint.EntitySpaceRotation);
				stream.WriteVec3AsVec4(constraint.Position);

				switch (constraint)
				{
					case HingeJointConstraint hinge:
						stream.Write(hinge.UnknownFloat1);
						stream.Write(hinge.UnknownFloat2);
						break;
					case D6JointConstraint d6:
						stream.WriteSizedString(d6.AxisLockX);
						stream.WriteSizedString(d6.AxisLockY);
						stream.WriteSizedString(d6.AxisLockZ);
						stream.WriteSizedString(d6.TwistLock);
						stream.WriteSizedString(d6.Swing1Lock);
						stream.WriteSizedString(d6.Swing2Lock);
						stream.Write(d6.AxisLimit);
						stream.Write(d6.TwistLowerLimit);
						stream.Write(d6.TwistUpperLimit);
						stream.Write(d6.Swing1Limit);
						stream.Write(d6.Swing2Limit);
						break;
					case IKConstraint ik:
						stream.Write(ik.UnknownUint);
						stream.Write(ik.Swing1Limit);
						stream.Write(ik.Swing2Limit);
						stream.Write(ik.TwistLowerLimit);
						stream.Write(ik.TwistUpperLimit);
						break;
				}
			}
		}

		public abstract class Constraint
		{
			public string Name { set; get; }

			public string Bone1 { set; get; }

			public string Bone2 { set; get; }

			public Quaternion EntitySpaceRotation { set; get; }

			public Vector3 Position { set; get; }

			protected Constraint()
			{
				Name = String.Empty;
				Bone1 = String.Empty;
				Bone2 = String.Empty;
				EntitySpaceRotation = new Quaternion(1, 0, 0, 0);
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

			public string AxisLockX { set; get; }

			public string AxisLockY { set; get; }

			public string AxisLockZ { set; get; }

			public string TwistLock { set; get; }

			public string Swing1Lock { set; get; }

			public string Swing2Lock { set; get; }

			public float AxisLimit { set; get; }

			public float TwistLowerLimit { set; get; }

			public float TwistUpperLimit { set; get; }

			public float Swing1Limit { set; get; }

			public float Swing2Limit { set; get; }

			public D6JointConstraint()
			{
			}
		}

		public class IKConstraint : Constraint
		{
			public const string TYPE = "ik";

			public uint UnknownUint { set; get; }

			public float Swing1Limit { set; get; }

			public float Swing2Limit { set; get; }

			public float TwistLowerLimit { set; get; }

			public float TwistUpperLimit { set; get; }
		}

		public class Body
		{
			public string BoneName { set; get; }

			// not sure. true for human pelvis and legs. false for others.
			// if set it to false for huamn legs, then the legs will act oddly
			public bool EnableBlend { set; get; }

			public string Type { set; get; }

			public string BodyType { set; get; }

			public float Mass { set; get; }

			public Vector3 RagdollPosition1 { set; get; }

			public Vector3 RagdollPosition2 { set; get; }

			public float RagdollRadius { set; get; }

			public Vector3 CollisionPosition1 { set; get; }

			public Vector3 CollisionPosition2 { set; get; }

			public float CollisionMaxRadius { set; get; }

			public float CollisionRadius { set; get; }
		}
	}
}