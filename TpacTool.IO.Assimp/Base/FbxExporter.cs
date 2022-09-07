using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Assimp;
using UkooLabs.FbxSharpie;
using UkooLabs.FbxSharpie.Extensions;
using UkooLabs.FbxSharpie.Tokens;
using UkooLabs.FbxSharpie.Tokens.Value;
using UkooLabs.FbxSharpie.Tokens.ValueArray;

namespace TpacTool.IO.Assimp
{
	public class FbxExporter : AbstractAssimpExporter
	{
		private const long FBX_SECOND = 46186158000;

		public override string AssimpFormatId => "fbx";

		public override string Extension => "fbx";

		public override bool SupportsSecondMaterial => true;

		public override bool SupportsSecondUv => true;

		public override bool SupportsSecondColor => true;

		public override bool SupportsSkeleton => true;

		// Fbx supports morph, of course. However Assimp.NET is buggy and out-of-date.
		// Assimp.NET doesn't support exporting fbx with morph.
		// So we have to manually finish it.
		public override bool SupportsMorph => false;

		public override bool SupportMorphAnimation => false;

		public override bool SupportsSkeletalAnimation => true;

		public override bool SupportTRSInAnimation => false;

		public bool UseAsciiFormat { set; get; } = false;

		private long _lastUsedUid = 1145141919;

		private static FieldInfo _nodesFi;

		static FbxExporter()
		{
			_nodesFi = typeof(FbxNodeList).GetField("_nodes", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		private long GenerateUid()
		{
			return _lastUsedUid++;
		}

		protected override byte[] PostProcess(Scene scene, byte[] data)
		{
			data = base.PostProcess(scene, data);

			var reader = new FbxBinaryReader(new MemoryStream(data));
			var doc = reader.Read();

			var globalSettings = doc.GetRelative("GlobalSettings/Properties70");
			foreach (FbxNode node in globalSettings.Nodes)
			{
				if (node != null &&
				    node.Properties.Length >= 5 &&
				    node.Properties[0] is StringToken strToken)
				{
					switch (strToken.Value)
					{
						case "UnitScaleFactor":
							((DoubleToken) node.Properties[4]).Value = IsLargerSize ? 1000d : 100d;
							break;
						case "UpAxis":
						case "OriginalUpAxis":
							((IntegerToken)node.Properties[4]).Value = 2; // z as up axis
							break;
						case "FrontAxis":
							((IntegerToken)node.Properties[4]).Value = 1; // y as front axis
							break;
						case "FrontAxisSign":
							var fasToken = (IntegerToken) node.Properties[4];
							fasToken.Value = -1;
							if (IsNegYAxisForward)
								fasToken.Value = -fasToken.Value;
							break;
						case "CoordAxisSign":
							if (IsNegYAxisForward)
								((IntegerToken)node.Properties[4]).Value = -1;
							break;
					}
				}
			}

			var hasMorph = false;
			var hasMorphAnimation = Morph != null && Morph.Definition != null && Morph.Definition.Data.MorphFrame.Count > 0;
			int morphGeometryCount = 0;
			int morphDeformerCount = 0;
			int maxMorphCount = 0;
			var meshes = GetAllMeshes();
			foreach (var mesh in meshes)
			{
				if (mesh.EditData != null && mesh.EditData.Data.MorphFrames.Count > 0)
				{
					hasMorph = true;
					maxMorphCount = Math.Max(maxMorphCount, mesh.EditData.Data.MorphFrames.Count);
					morphGeometryCount += mesh.EditData.Data.MorphFrames.Count;
					morphDeformerCount += mesh.EditData.Data.MorphFrames.Count + 1;
				}
			}

			Dictionary<int, List<Tuple<int, float>>> morphData = null;
			var morphCurveCount = 0;
			if (hasMorphAnimation)
			{
				morphData = Morph.Definition.Data.OrderByMorphShapes();
				foreach (var shapeIndex in morphData.Keys.ToArray())
				{
					var seq = morphData[shapeIndex];
					var hasContent = false;
					foreach (var tuple in seq)
					{
						if (tuple.Item2 > 1e-5f)
						{
							hasContent = true;
							break;
						}
					}

					// TODO: base shape?
					if (!hasContent || shapeIndex >= maxMorphCount)
						morphData.Remove(shapeIndex);
					else
						morphCurveCount++;
				}
			}

			if (hasMorph)
			{
				#region TypeDefinitions

				var definitions = doc["Definitions"][0];
				var hasDeformer = false;
				var hasAnimationStack = false;
				var hasAnimationLayer = false;
				var hasAnimationCurveNode = false;
				var hasAnimationCurve = false;
				var needsNullEnd = false;
				foreach (var node in definitions["ObjectType"])
				{
					if (node.Value is StringToken token1 && token1.Value == "Geometry")
					{
						var countNode = node["Count"][0];
						var count = ((IntegerToken) countNode.Value).Value;
						count += morphGeometryCount;
						countNode.Value = new IntegerToken(count);
					}
					else if (node.Value is StringToken token2 && token2.Value == "Deformer")
					{
						hasDeformer = true;
						var countNode = node["Count"][0];
						var count = ((IntegerToken)countNode.Value).Value;
						count += morphDeformerCount;
						countNode.Value = new IntegerToken(count);
					}
					else if (node.Value is StringToken token3 && token3.Value == "AnimationStack")
					{
						hasAnimationStack = true;
					}
					else if (node.Value is StringToken token4 && token4.Value == "AnimationLayer")
					{
						hasAnimationLayer = true;
					}
					else if (node.Value is StringToken token5 && token5.Value == "AnimationCurveNode")
					{
						hasAnimationCurveNode = true;
						var countNode = node["Count"][0];
						var count = ((IntegerToken)countNode.Value).Value;
						count += morphCurveCount;
						countNode.Value = new IntegerToken(count);
					}
					else if (node.Value is StringToken token6 && token6.Value == "AnimationCurve")
					{
						hasAnimationCurve = true;
						var countNode = node["Count"][0];
						var count = ((IntegerToken)countNode.Value).Value;
						count += morphCurveCount;
						countNode.Value = new IntegerToken(count);
					}
				}

				if (!hasDeformer || hasMorphAnimation)
				{
					var nodes = _nodesFi.GetValue(definitions) as List<FbxNode>;
					if (nodes.Count > 0 && nodes.Last() == null)
					{
						needsNullEnd = true;
						nodes.RemoveAt(nodes.Count - 1);
					}

					if (!hasDeformer)
					{
						var definitionNode = new FbxNode(new IdentifierToken("ObjectType"));
						definitionNode.Value = new StringToken("Deformer");
						var countNode = new FbxNode(new IdentifierToken("Count"));
						countNode.Value = new IntegerToken(morphDeformerCount);
						definitionNode.AddNode(countNode);
						definitionNode.AddNode(null);
						nodes.Add(definitionNode);
					}

					if (!hasAnimationStack)
					{
						var definitionNode = new FbxNode(new IdentifierToken("ObjectType"));
						definitionNode.Value = new StringToken("AnimationStack");
						var countNode = new FbxNode(new IdentifierToken("Count"));
						countNode.Value = new IntegerToken(1);
						definitionNode.AddNode(countNode);
						var propertyTemplate = new FbxNode(new IdentifierToken("PropertyTemplate"));
						propertyTemplate.Value = new StringToken("FbxAnimStack");
						var propertyNode = new FbxNode(new IdentifierToken("Properties70"));
						propertyNode.AddNode(CreatePNode("Description", "KString", "", "", ""));
						propertyNode.AddNode(CreatePNode("LocalStart", "KTime", "KTime", "", 0));
						propertyNode.AddNode(CreatePNode("LocalStop", "KTime", "KTime", "", 0));
						propertyNode.AddNode(CreatePNode("ReferenceStart", "KTime", "KTime", "", 0));
						propertyNode.AddNode(CreatePNode("ReferenceStop", "KTime", "KTime", "", 0));
						propertyNode.AddNode(null);
						propertyTemplate.AddNode(propertyNode);
						propertyTemplate.AddNode(null);
						definitionNode.AddNode(propertyTemplate);
						definitionNode.AddNode(null);
						nodes.Add(definitionNode);
					}

					if (!hasAnimationLayer)
					{
						var definitionNode = new FbxNode(new IdentifierToken("ObjectType"));
						definitionNode.Value = new StringToken("AnimationLayer");
						var countNode = new FbxNode(new IdentifierToken("Count"));
						countNode.Value = new IntegerToken(1);
						definitionNode.AddNode(countNode);
						var propertyTemplate = new FbxNode(new IdentifierToken("PropertyTemplate"));
						propertyTemplate.Value = new StringToken("FbxAnimLayer");
						var propertyNode = new FbxNode(new IdentifierToken("Properties70"));
						propertyNode.AddNode(CreatePNode("Weight", "Number", "", "A", 100));
						propertyNode.AddNode(CreatePNode("Mute", "bool", "", "", 0));
						propertyNode.AddNode(CreatePNode("Solo", "bool", "", "", 0));
						propertyNode.AddNode(CreatePNode("Lock", "bool", "", "", 0));
						propertyNode.AddNode(CreatePNode("Color", "ColorRGB", "Color", "", 0.8, 0.8, 0.8));
						propertyNode.AddNode(CreatePNode("BlendMode", "enum", "", "", 0));
						propertyNode.AddNode(CreatePNode("RotationAccumulationMode", "enum", "", "", 0));
						propertyNode.AddNode(CreatePNode("ScaleAccumulationMode", "enum", "", "", 0));
						propertyNode.AddNode(CreatePNode("BlendModeBypass", "ULongLong", "", "", 0));
						propertyNode.AddNode(null);
						propertyTemplate.AddNode(propertyNode);
						propertyTemplate.AddNode(null);
						definitionNode.AddNode(propertyTemplate);
						definitionNode.AddNode(null);
						nodes.Add(definitionNode);
					}

					if (!hasAnimationCurveNode)
					{
						var definitionNode = new FbxNode(new IdentifierToken("ObjectType"));
						definitionNode.Value = new StringToken("AnimationCurveNode");
						var countNode = new FbxNode(new IdentifierToken("Count"));
						countNode.Value = new IntegerToken(morphCurveCount);
						definitionNode.AddNode(countNode);
						var propertyTemplate = new FbxNode(new IdentifierToken("PropertyTemplate"));
						propertyTemplate.Value = new StringToken("FbxAnimCurveNode");
						var propertyNode = new FbxNode(new IdentifierToken("Properties70"));
						propertyNode.AddNode(CreatePNode("d", "Compound", "", ""));
						propertyNode.AddNode(null);
						propertyTemplate.AddNode(propertyNode);
						propertyTemplate.AddNode(null);
						definitionNode.AddNode(propertyTemplate);
						definitionNode.AddNode(null);
						nodes.Add(definitionNode);
					}

					if (!hasAnimationCurve)
					{
						var definitionNode = new FbxNode(new IdentifierToken("ObjectType"));
						definitionNode.Value = new StringToken("AnimationCurve");
						var countNode = new FbxNode(new IdentifierToken("Count"));
						countNode.Value = new IntegerToken(morphCurveCount);
						definitionNode.AddNode(countNode);
						definitionNode.AddNode(null);
						nodes.Add(definitionNode);
					}

					if (needsNullEnd)
						nodes.Add(null);
				}

				#endregion

				var objects = doc["Objects"][0];
				var connections = doc["Connections"][0];
				var geoSet = new HashSet<long>();
				FbxNode animationStackNode = null;
				FbxNode animationLayerNode = null;
				var animationStackUid = 0L;
				var animationLayerUid = 0L;
				var originMeshLookup = new Dictionary<long, string>();
				var geoUidLookup = new Dictionary<string, long>();

				foreach (var child in objects.Nodes)
				{
					if (child == null)
						continue;

					if (child.Identifier.Value == "Model" &&
					    child.Properties.Length >= 3 &&
					    child.Properties[2] is StringToken typeToken &&
					    typeToken.Value == "Mesh" &&
					    child.Properties[0] is LongToken uidToken &&
					    child.Properties[1] is StringToken nameToken)
					{
						var uid = uidToken.Value;
						var meshName = nameToken.Value.Substring("Model::".Length);
						originMeshLookup[uid] = meshName;
					}
					else if (child.Identifier.Value == "Geometry" &&
					         child.Value is LongToken uidToken2)
					{
						geoSet.Add(uidToken2.Value);
					}
					else if (child.Identifier.Value == "AnimationStack")
					{
						animationStackNode = child;
						animationStackUid = child.Value.GetAsLong();
					}
					else if (child.Identifier.Value == "AnimationLayer")
					{
						animationLayerNode = child;
						animationLayerUid = child.Value.GetAsLong();
					}
				}

				foreach (var child in connections.Nodes)
				{
					if (child != null &&
					    child.Properties.Length >= 3 &&
					    child.Properties[2] is LongToken uidToken &&
					    originMeshLookup.ContainsKey(uidToken.Value) &&
					    child.Properties[1] is LongToken geoUid &&
					    geoSet.Contains(geoUid.Value))
					{
						geoUidLookup[originMeshLookup[uidToken.Value]] = geoUid.Value;
					}
				}

				var objectsNeedNullEnd = false;
				var objNodes = _nodesFi.GetValue(objects) as List<FbxNode>;
				if (objNodes.Count > 0 && objNodes.Last() == null)
				{
					objectsNeedNullEnd = true;
					objNodes.RemoveAt(objNodes.Count - 1);
				}

				var connectionsNeedNullEnd = false;
				var connectionsNodes = _nodesFi.GetValue(connections) as List<FbxNode>;
				if (connectionsNodes.Count > 0 && connectionsNodes.Last() == null)
				{
					connectionsNeedNullEnd = true;
					connectionsNodes.RemoveAt(connectionsNodes.Count - 1);
				}

				var subDeformUids = new List<Tuple<int, long>>(); //shape key index & subdeform uid
				foreach (var mesh in meshes)
				{
					if (mesh.EditData == null)
						continue;
					var editData = mesh.EditData.Data;
					var morphFrames = editData.MorphFrames;
					if (morphFrames.Count > 0)
					{
						var deformerUid = GenerateUid();
						var deformerNode = new FbxNode(new IdentifierToken("Deformer"));
						deformerNode.AddProperty(new LongToken(deformerUid));
						deformerNode.AddProperty(new StringToken($"Deformer::{mesh.Name}"));
						deformerNode.AddProperty(new StringToken("BlendShape"));
						var versionNode = new FbxNode(new IdentifierToken("Version"));
						versionNode.Value = new IntegerToken(100);
						deformerNode.AddNode(versionNode);
						objNodes.Add(deformerNode);

						var connect1 = new FbxNode(new IdentifierToken("C"));
						connect1.AddProperty(new StringToken("OO"));
						connect1.AddProperty(new LongToken(deformerUid));
						connect1.AddProperty(new LongToken(geoUidLookup[mesh.Name]));
						connectionsNodes.Add(connect1);

						var count = mesh.VertexCount;
						var indiceArray = new List<int>(count);
						var weightArray = new List<float>(count);
						var posArray = new List<float>(count * 3);
						var normalArray = new List<float>(count * 3);

						for (var i = 0; i < morphFrames.Count; i++)
						{
							var morph = morphFrames[i];
							var morphName = GetMorphName(i);

							var shapeUid = GenerateUid();
							var shapeNode = new FbxNode(new IdentifierToken("Geometry"));
							shapeNode.AddProperty(new LongToken(shapeUid));
							shapeNode.AddProperty(new StringToken($"Geometry::{morphName}"));
							shapeNode.AddProperty(new StringToken("Shape"));
							var shapePropertyNode = new FbxNode(new IdentifierToken("Properties70"));
							shapePropertyNode.AddNode(null);
							shapeNode.AddNode(shapePropertyNode);
							var shapeVersionNode = new FbxNode(new IdentifierToken("Version"));
							shapeVersionNode.Value = new IntegerToken(100);
							shapeNode.AddNode(shapeVersionNode);

							indiceArray.Clear();
							posArray.Clear();
							normalArray.Clear();
							weightArray.Clear();

							for (int j = 0; j < count; j++)
							{
								var vertex = editData.Vertices[j];
								var animPos = morph.Positions[vertex.PositionIndex].ToAssimpVec();
								var meshPos = editData.Positions[vertex.PositionIndex].ToAssimpVec();
								var delta = animPos - meshPos;
								if (delta.LengthSquared() > 1e-8f)
								{
									indiceArray.Add(j);
									posArray.Add(delta.X);
									posArray.Add(delta.Y);
									posArray.Add(delta.Z);

									var animNormal = morph.Normals[j].ToAssimpVec();
									var meshNormal = vertex.Normal.ToAssimpVec();
									delta = animNormal - meshNormal;
									normalArray.Add(delta.X);
									normalArray.Add(delta.Y);
									normalArray.Add(delta.Z);

									weightArray.Add(100f);
								}
							}

							shapeNode.AddNode(new FbxNode(new IdentifierToken("Indexes"))
							{
								Value = new IntegerArrayToken(indiceArray.ToArray())
							});
							shapeNode.AddNode(new FbxNode(new IdentifierToken("Vertices"))
							{
								Value = new FloatArrayToken(posArray.ToArray())
							});
							shapeNode.AddNode(new FbxNode(new IdentifierToken("Normals"))
							{
								Value = new FloatArrayToken(normalArray.ToArray())
							});
							shapeNode.AddNode(null);
							objNodes.Add(shapeNode);

							var channelUid = GenerateUid();
							subDeformUids.Add(Tuple.Create(i, channelUid));
							var channelNode = new FbxNode(new IdentifierToken("Deformer"));
							channelNode.AddProperty(new LongToken(channelUid));
							channelNode.AddProperty(new StringToken($"SubDeformer::{morphName}"));
							channelNode.AddProperty(new StringToken("BlendShapeChannel"));

							var channelPropertyNode = new FbxNode(new IdentifierToken("Properties70"));
							channelPropertyNode.AddNode(new FbxNode(new IdentifierToken("DeformPercent"))
							{
								Value = new FloatToken(0f)
							});
							channelPropertyNode.AddNode(null);
							channelNode.AddNode(channelPropertyNode);

							var channelVersionNode = new FbxNode(new IdentifierToken("Version"));
							channelVersionNode.Value = new IntegerToken(100);
							channelNode.AddNode(channelVersionNode);

							var channelDeformPercentNode = new FbxNode(new IdentifierToken("DeformPercent"));
							channelDeformPercentNode.Value = new FloatToken(0f);
							channelNode.AddNode(channelDeformPercentNode);

							channelNode.AddNode(new FbxNode(new IdentifierToken("FullWeights"))
							{
								Value = new FloatArrayToken(weightArray.ToArray())
							});

							channelNode.AddNode(null);

							objNodes.Add(channelNode);

							var connect2 = new FbxNode(new IdentifierToken("C"));
							connect2.AddProperty(new StringToken("OO"));
							connect2.AddProperty(new LongToken(channelUid));
							connect2.AddProperty(new LongToken(deformerUid));
							connectionsNodes.Add(connect2);

							var connect3 = new FbxNode(new IdentifierToken("C"));
							connect3.AddProperty(new StringToken("OO"));
							connect3.AddProperty(new LongToken(shapeUid));
							connect3.AddProperty(new LongToken(channelUid));
							connectionsNodes.Add(connect3);
						}
					}
				}

				if (hasMorphAnimation)
				{
					var morphAnimTime = Morph.Definition.Data.MorphFrame.Last().Key / AnimationFrameRate;
					var startTime = 0f;
					var endTime = startTime + morphAnimTime;
					if (animationStackNode == null)
					{
						animationStackUid = GenerateUid();
						animationStackNode = new FbxNode(new IdentifierToken("AnimationStack"));
						animationStackNode.AddProperty(new LongToken(animationStackUid));
						animationStackNode.AddProperty(new StringToken("AnimStack::Scene"));
						animationStackNode.AddProperty(new StringToken(""));

						var propertyNode = new FbxNode(new IdentifierToken("Properties70"));
						propertyNode.AddNode(CreatePNode("LocalStart", "KTime", "Time", "", ToFbxTime(startTime)));
						propertyNode.AddNode(CreatePNode("LocalStop", "KTime", "Time", "", ToFbxTime(morphAnimTime)));
						propertyNode.AddNode(CreatePNode("ReferenceStart", "KTime", "Time", "", ToFbxTime(startTime)));
						propertyNode.AddNode(CreatePNode("ReferenceStop", "KTime", "Time", "", ToFbxTime(morphAnimTime)));
						propertyNode.AddNode(null);

						animationStackNode.AddNode(propertyNode);
						animationStackNode.AddNode(null);
						objNodes.Add(animationStackNode);
					}
					else
					{
						foreach (var child in animationStackNode.Nodes)
						{
							if (child != null && child.Identifier.Value == "Properties70")
							{
								foreach (var child2 in child.Nodes)
								{
									if (child2 != null && child2.Value is StringToken str1 && str1.Value == "LocalStart")
									{
										startTime = ToRealTime(child2.Properties[4].GetAsLong());
										endTime = startTime + morphAnimTime;
									}
									else if (child2 != null && child2.Value is StringToken str2 && 
									         (str2.Value == "LocalStop" || str2.Value == "ReferenceStop"))
									{
										var time = ToFbxTime(endTime);
										if (time > ToRealTime(child2.Properties[4].GetAsLong()))
										{
											child2.Properties[4] = new LongToken(time);
										}
									}
								}
							}
						}
					}

					if (animationLayerNode == null)
					{
						animationLayerUid = GenerateUid();
						animationLayerNode = new FbxNode(new IdentifierToken("AnimationLayer"));
						animationLayerNode.AddProperty(new LongToken(animationLayerUid));
						animationLayerNode.AddProperty(new StringToken("AnimLayer::Scene"));
						animationLayerNode.AddProperty(new StringToken(""));
						animationLayerNode.AddNode(null);
						objNodes.Add(animationLayerNode);

						var connect = new FbxNode(new IdentifierToken("C"));
						connect.AddProperty(new StringToken("OO"));
						connect.AddProperty(new LongToken(animationLayerUid));
						connect.AddProperty(new LongToken(animationStackUid));
						connectionsNodes.Add(connect);
					}
					
					foreach (var pair in morphData)
					{
						var shapeId = pair.Key;
						var animationCurveUid = GenerateUid();
						var animationCurveNode = new FbxNode(new IdentifierToken("AnimationCurve"));
						animationCurveNode.AddProperty(new LongToken(animationCurveUid));
						animationCurveNode.AddProperty(new StringToken("AnimCurve::"));
						animationCurveNode.AddProperty(new StringToken(""));

						animationCurveNode.AddNode(new FbxNode(new IdentifierToken("Default"))
						{
							Value = new IntegerToken(0)
						});
						animationCurveNode.AddNode(new FbxNode(new IdentifierToken("KeyVer"))
						{
							Value = new IntegerToken(4009)
						});
						animationCurveNode.AddNode(new FbxNode(new IdentifierToken("KeyTime"))
						{
							Value = new LongArrayToken(pair.Value
								.Select(tuple => ToFbxTime((tuple.Item1 / AnimationFrameRate) + startTime))
								.ToArray())
						});
						animationCurveNode.AddNode(new FbxNode(new IdentifierToken("KeyValueFloat"))
						{
							Value = new FloatArrayToken(pair.Value
								.Select(tuple => tuple.Item2 * 100)
								.ToArray())
						});
						animationCurveNode.AddNode(new FbxNode(new IdentifierToken("KeyAttrFlags"))
						{
							Value = new IntegerArrayToken(new [] { 24836 }) // Linear
						});
						animationCurveNode.AddNode(new FbxNode(new IdentifierToken("KeyAttrDataFloat"))
						{
							Value = new LongArrayToken(new[] { 0, 0, 255790911L, 0 })
						});
						animationCurveNode.AddNode(new FbxNode(new IdentifierToken("KeyAttrRefCount"))
						{
							Value = new IntegerArrayToken(new[] { pair.Value.Count })
						});
						animationCurveNode.AddNode(null);
						objNodes.Add(animationCurveNode);

						var animationCurveNodeUid = GenerateUid();
						animationCurveNode = new FbxNode(new IdentifierToken("AnimationCurveNode"));
						animationCurveNode.AddProperty(new LongToken(animationCurveNodeUid));
						animationCurveNode.AddProperty(new StringToken("AnimCurveNode::DeformPercent"));
						animationCurveNode.AddProperty(new StringToken(""));

						var propertyNode = new FbxNode(new IdentifierToken("Properties70"));
						propertyNode.AddNode(CreatePNode("d|DeformPercent", "Number", "", "A", 0));
						propertyNode.AddNode(null);

						animationCurveNode.AddNode(propertyNode);
						animationCurveNode.AddNode(null);
						objNodes.Add(animationCurveNode);

						var connect = new FbxNode(new IdentifierToken("C"));
						connect.AddProperty(new StringToken("OP"));
						connect.AddProperty(new LongToken(animationCurveUid));
						connect.AddProperty(new LongToken(animationCurveNodeUid));
						connect.AddProperty(new StringToken("d|DeformPercent"));
						connectionsNodes.Add(connect);

						connect = new FbxNode(new IdentifierToken("C"));
						connect.AddProperty(new StringToken("OO"));
						connect.AddProperty(new LongToken(animationCurveNodeUid));
						connect.AddProperty(new LongToken(animationLayerUid));
						connectionsNodes.Add(connect);

						// link AnimCurveNode to SubDeformer
						foreach (var tuple in subDeformUids)
						{
							if (tuple.Item1 == shapeId)
							{
								connect = new FbxNode(new IdentifierToken("C"));
								connect.AddProperty(new StringToken("OP"));
								connect.AddProperty(new LongToken(animationCurveNodeUid));
								connect.AddProperty(new LongToken(tuple.Item2));
								connect.AddProperty(new StringToken("DeformPercent"));
								connectionsNodes.Add(connect);
							}
						}
					}
				}

				if (objectsNeedNullEnd)
					objNodes.Add(null);

				if (connectionsNeedNullEnd)
					connectionsNodes.Add(null);
			}

			var memoryStream = new MemoryStream(data.Length);
			if (UseAsciiFormat)
			{
				new FbxAsciiWriter(memoryStream).Write(doc);
			}
			else
			{
				new FbxBinaryWriter(memoryStream).Write(doc);
			}
			return memoryStream.ToArray();
		}

		private static FbxNode CreatePNode(params object[] args)
		{
			var node = new FbxNode(new IdentifierToken("P"));
			foreach (var o in args)
			{
				if (o is string str)
					node.AddProperty(new StringToken(str));
				else if (o is int i)
					node.AddProperty(new IntegerToken(i));
				else if (o is long l)
					node.AddProperty(new LongToken(l));
				else if (o is float f)
					node.AddProperty(new FloatToken(f));
				else if (o is double d)
					node.AddProperty(new DoubleToken(d));
				else
					throw new ArgumentException($"Wrong property type {o.GetType()}");
			}
			return node;
		}

		private static long ToFbxTime(float timeInSecond)
		{
			return (long) ((double) timeInSecond * FBX_SECOND);
		}

		private static float ToRealTime(long fbxTime)
		{
			return (float) (fbxTime / (double)FBX_SECOND);
		}
	}
}