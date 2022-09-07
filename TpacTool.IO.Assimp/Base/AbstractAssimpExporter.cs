using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Assimp;
using Assimp.Unmanaged;
using TpacTool.Lib;
using UkooLabs.FbxSharpie;
using Material = Assimp.Material;
using Matrix4x4 = Assimp.Matrix4x4;
using Mesh = Assimp.Mesh;
using Quaternion = Assimp.Quaternion;

namespace TpacTool.IO.Assimp
{
	public abstract class AbstractAssimpExporter : AbstractModelExporter
	{ 
		public abstract string AssimpFormatId { get; }

		public abstract bool SupportTRSInAnimation { get; }

		public override void Export(string path)
		{
			var parentPath = Directory.GetParent(path);
			if (parentPath != null)
				Directory.CreateDirectory(parentPath.FullName);

			using (AssimpContext assctx = new AssimpContext())
			{
				Scene scene = new Scene();
				string nodeName = Skeleton != null ? Skeleton.Name : Model != null ? Model.Name : "empty_model";

				var meshes = GetAllMeshes();
				CollectUniqueMaterialsAndTextures(meshes, out var materials, out var textures);
				var matMapping = ExportMaterials(materials, scene.Materials);
				var meshMapping = ExportMeshes(meshes, matMapping, scene.Meshes);

				Node rootNode = new Node("Scene");
				scene.RootNode = rootNode;

				Node skeletonNode = Skeleton != null ? new Node(Skeleton.Name) : rootNode;
				if (Skeleton != null)
				{
					rootNode.Children.Add(skeletonNode);
					var boneMapping = new Dictionary<BoneNode, Node>();

					foreach (var boneNode in Skeleton.Definition.Data.Bones)
					{
						Node node = new Node(boneNode.Name);
						var restFrame = boneNode.RestFrame;
						if (IgnoreScale)
							restFrame.M44 = 1f;
						node.Transform = restFrame.ToAssimpMatrix();
						boneMapping[boneNode] = node;
					}

					foreach (var boneNode in Skeleton.Definition.Data.Bones)
					{
						Node curNode = boneMapping[boneNode];
						Node parentNode = boneNode.Parent != null ? boneMapping[boneNode.Parent] : skeletonNode;
						parentNode.Children.Add(curNode);
					}
				}

				Node modelNode = scene.Meshes.Count == 1 ? new Node(Model.Name) :
					new Node(Model != null ? $"{Model.Name}_node" : "empty_model");
				skeletonNode.Children.Add(modelNode);
				for (var i = 0; i < scene.Meshes.Count; i++)
				{
					modelNode.MeshIndices.Add(i);
				}

				if (Animation != null)
				{
					scene.Animations.Add(ExportAnimation(Skeleton != null ? skeletonNode.Name : modelNode.Name));
				}

				SetupScene(scene);

				var blob = assctx.ExportToBlob(scene, AssimpFormatId, PostProcessSteps.None);
				var data = PostProcess(scene, blob.Data);

				using (var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					stream.Write(data, 0, data.Length);
				}
			}
		}

		public sealed override void Export(Stream writeStream)
		{
			throw new NotImplementedException("Assimp Exporter doesn't support streaming export");
		}

		protected virtual void SetupScene(Scene scene)
		{
		}

		protected virtual byte[] PostProcess(Scene scene, byte[] data)
		{
			return data;
		}

		protected virtual List<TpacTool.Lib.Mesh> GetAllMeshes()
		{
			if (Model != null)
			{
				return Model.Meshes.FindAll(mesh => (LodMask & (1 << mesh.Lod)) > 0);
			}
			return new List<TpacTool.Lib.Mesh>(0);
		}

		protected string GetMorphName(int index)
		{
			var isHumanHead = Model.Name.StartsWith("head_male_") || Model.Name.StartsWith("head_female_");
			return isHumanHead ? MorphNameMapping.GetHumanHeadMorphName(index) : $"KeyTime_{index}";
		}

		protected virtual Dictionary<TpacTool.Lib.Material, int> ExportMaterials(
																ICollection<TpacTool.Lib.Material> materials,
																ICollection<Material> outList)
		{
			var dic = new Dictionary<TpacTool.Lib.Material, int>();
			int index = outList.Count;
			foreach (var material in materials)
			{
				var assMat = new Material();
				assMat.Name = material.Name;
				assMat.ShadingMode = ShadingMode.Phong;
				assMat.ColorAmbient = new Color4D(0, 0, 0, 1);
				assMat.ColorDiffuse = new Color4D(1, 1, 1, 1);
				assMat.ColorEmissive = new Color4D(0, 0, 0, 1);
				assMat.ColorReflective = new Color4D(0, 0, 0, 0);
				assMat.ColorSpecular = new Color4D(1, 1, 1, 1);
				assMat.ColorTransparent = new Color4D(0, 0, 0, 1);
				// TODO: two-side flag
				foreach (var pair in material.Textures)
				{
					if (pair.Value.TryGetItem(out var tex) && TexturePathMapping.TryGetValue(tex, out var path))
					{
						var usage = GuessTextureUsage(tex);
						if (usage == TextureType.Unknown)
							continue;
						var texSlot = new TextureSlot(path, usage, 0, TextureMapping.FromUV, 0,
							1, TextureOperation.Multiply, TextureWrapMode.Wrap, TextureWrapMode.Wrap, 0);
						assMat.AddMaterialTexture(texSlot);
					}
				}

				outList.Add(assMat);
				dic[material] = index;
				index++;
			}

			if (outList.Count == 0)
			{
				// fix a bug of fbx exporting
				var dummyMaterial = new Material();
				dummyMaterial.Name = "empty_material";
				dummyMaterial.ShadingMode = ShadingMode.Phong;
				dummyMaterial.ColorAmbient = new Color4D(0, 0, 0, 1);
				dummyMaterial.ColorDiffuse = new Color4D(1, 1, 1, 1);
				outList.Add(dummyMaterial);
			}

			return dic;
		}

		protected virtual Dictionary<TpacTool.Lib.Mesh, int> ExportMeshes(
													ICollection<TpacTool.Lib.Mesh> meshes,
													IDictionary<TpacTool.Lib.Material, int> matMapping,
													ICollection<Mesh> outList)
		{
			var dic = new Dictionary<TpacTool.Lib.Mesh, int>();
			int index = outList.Count;
			foreach (var mesh in meshes)
			{
				var assMesh = new Mesh(mesh.Name, PrimitiveType.Triangle);
				var data = mesh.EditData.Data; // TODO: if there is not edit?


				assMesh.SetIndices(data.Faces.SelectMany(face =>
				{
					return new int[] {face.V0, face.V1, face.V2};
				}).ToArray(), 3);

				var positions = data.Positions;
				var vertCount = data.Vertices.Length;
				assMesh.Vertices.Capacity = vertCount;
				assMesh.Normals.Capacity = vertCount;
				assMesh.Tangents.Capacity = vertCount;
				assMesh.BiTangents.Capacity = vertCount;
				var hasSecondUv = SupportsSecondUv && assMesh.TextureCoordinateChannels.Length > 1 && data.HasSecondUv();
				var hasSecondColor = SupportsSecondColor && assMesh.VertexColorChannels.Length > 1 && data.HasSecondColor();
				assMesh.TextureCoordinateChannels[0] = new List<Vector3D>(vertCount);
				assMesh.TextureCoordinateChannels[1] = hasSecondUv ? new List<Vector3D>(vertCount) : null;
				for (int i = 2; i < assMesh.TextureCoordinateChannels.Length; i++)
					assMesh.TextureCoordinateChannels[i] = null;
				assMesh.UVComponentCount[0] = 2;
				if (hasSecondUv)
					assMesh.UVComponentCount[1] = 2;
				assMesh.VertexColorChannels[0] = new List<Color4D>(vertCount);
				assMesh.VertexColorChannels[1] = hasSecondColor ? new List<Color4D>(vertCount) : null;
				for (int i = 2; i < assMesh.VertexColorChannels.Length; i++)
					assMesh.VertexColorChannels[i] = null;

				foreach (var vertex in data.Vertices)
				{
					assMesh.Vertices.Add(positions[vertex.PositionIndex].ToAssimpVec());
					assMesh.Normals.Add(vertex.Normal.ToAssimpVec());
					assMesh.Tangents.Add(vertex.Tangent.ToAssimpVec());
					assMesh.BiTangents.Add(vertex.Binormal.ToAssimpVec());
					assMesh.TextureCoordinateChannels[0].Add(vertex.Uv.ToAssimpVec3());
					if (hasSecondUv)
						assMesh.TextureCoordinateChannels[1].Add(vertex.SecondUv.ToAssimpVec3());
					assMesh.VertexColorChannels[0].Add(vertex.Color.ToAssimpColor());
					if (hasSecondColor)
						assMesh.VertexColorChannels[1].Add(vertex.SecondColor.ToAssimpColor());
				}

#pragma warning disable 612
				bool hasSkeleton = false;
				int boneCount = 0;
				SkeletonDefinitionData skelDate = null;
				if (SupportsSkeleton && Skeleton != null)
				{
					hasSkeleton = true;
					skelDate = Skeleton.Definition.Data;
					//boneCount = Math.Max(skelDate.Bones.Count, data.Bones.Length);
					boneCount = skelDate.Bones.Count;
				}
				else if (SupportsSkeleton && mesh.SkinDataSize > 0 && ForceExportWeight)
				{
					hasSkeleton = true;
					boneCount = data.Bones.Length;
				}

				if (hasSkeleton)
				{
					var boneList = assMesh.Bones;

					var invBindMatrices = new System.Numerics.Matrix4x4[skelDate.Bones.Count];
					for (var i = 0; i < invBindMatrices.Length; i++)
					{
						var bone = skelDate.Bones[i];
						System.Numerics.Matrix4x4 matrix = GetBoneRestFrame(bone);
						System.Numerics.Matrix4x4.Invert(matrix, out matrix);
						BoneNode parentBone = null;
						while ((parentBone = bone.Parent) != null)
						{
							var leftMatrix = GetBoneRestFrame(parentBone);
							System.Numerics.Matrix4x4.Invert(leftMatrix, out leftMatrix);
							matrix = System.Numerics.Matrix4x4.Multiply(leftMatrix, matrix);
							bone = parentBone;
						}

						if (IsNegYAxisForward)
							matrix = System.Numerics.Matrix4x4.Multiply(NegYMatrix, matrix);
						invBindMatrices[i] = matrix;
					}

					for (int i = 0; i < boneCount; i++)
					{
						Bone assBone = new Bone();
						if (Skeleton != null && i < skelDate.Bones.Count)
						{
							assBone.Name = skelDate.Bones[i].Name;

							var restMatrix = invBindMatrices[i].ToAssimpMatrix();
							assBone.OffsetMatrix = restMatrix;
						}
						else
						{
							assBone.Name = "UnknownBone_" + i;
							assBone.OffsetMatrix = Matrix4x4.Identity;
						}
						boneList.Add(assBone);
					}

					for (var i = 0; i < data.Vertices.Length; i++)
					{
						var vertIndex = data.Vertices[i].PositionIndex;
						var bone = data.Bones[vertIndex];
						if (bone.W0 > 0)
							boneList[bone.B0].VertexWeights.Add(new VertexWeight(i, bone.W0));
						if (bone.W1 > 0)
							boneList[bone.B1].VertexWeights.Add(new VertexWeight(i, bone.W1));
						if (bone.W2 > 0)
							boneList[bone.B2].VertexWeights.Add(new VertexWeight(i, bone.W2));
						if (bone.W3 > 0)
							boneList[bone.B3].VertexWeights.Add(new VertexWeight(i, bone.W3));
					}
				}
#pragma warning restore 612

				bool hasMorph = SupportsMorph && data.MorphFrames.Count > 0;

				if (hasMorph)
				{
					assMesh.MorphMethod = MeshMorphingMethod.None;
					assMesh.MeshAnimationAttachments.Capacity = data.MorphFrames.Count;
					
					for (var i = 0; i < data.MorphFrames.Count; i++)
					{
						var morphFrame = data.MorphFrames[i];
						var assMorph = new MeshAnimationAttachment();
						assMorph.Vertices.Capacity = vertCount;
						assMorph.Normals.Capacity = vertCount;
						for (int j = 0; j < vertCount; j++)
						{
							var vert = data.Vertices[j];
							assMorph.Vertices.Add(morphFrame.Positions[vert.PositionIndex].ToAssimpVec());
							// It took me 8 months to find this :)
							//assMorph.Vertices.Add(morphFrame.Normals[j].ToAssimpVec());
							assMorph.Normals.Add(morphFrame.Normals[j].ToAssimpVec());
						}
						/*if (assMesh.VertexColorChannelCount > 0)
							assMorph.VertexColorChannels[0].AddRange(assMesh.VertexColorChannels[0]);
						if (assMesh.VertexColorChannelCount > 1)
							assMorph.VertexColorChannels[1].AddRange(assMesh.VertexColorChannels[1]);
						if (assMesh.TextureCoordinateChannelCount > 0)
							assMorph.TextureCoordinateChannels[0].AddRange(assMesh.TextureCoordinateChannels[0]);
						if (assMesh.TextureCoordinateChannelCount > 1)
							assMorph.TextureCoordinateChannels[1].AddRange(assMesh.TextureCoordinateChannels[1]);*/
						assMorph.Weight = 1;
						assMorph.Name = GetMorphName(i);

						assMesh.MeshAnimationAttachments.Add(assMorph);
					}
				}

				assMesh.MaterialIndex = 0;
				if (!mesh.Material.IsEmpty() && matMapping.TryGetValue(mesh.Material.GetItem(), out var matIndex))
					assMesh.MaterialIndex = matIndex;

				outList.Add(assMesh);
				dic[mesh] = index;
				index++;
			}

			return dic;
		}

		public Animation ExportAnimation(string rootNodeName)
		{
			var assAnim = new Animation();

			assAnim.Name = Animation.Name;
			assAnim.DurationInTicks = Animation.Duration;
			assAnim.TicksPerSecond = AnimationFrameRate;

			var data = Animation.Definition.Data;
			var rootHasPos = data.HasRootPositionTransform();
			var rootHasScale = data.HasRootScaleTransform();

			if (rootHasPos || rootHasScale)
			{
				var channel = new NodeAnimationChannel();
				channel.NodeName = rootNodeName;

				if (SupportTRSInAnimation)
				{
					if (rootHasPos)
					{
						foreach (var frame in data.RootPositionFrames)
						{
							channel.PositionKeys.Add(new VectorKey(frame.Value.Time, frame.Value.Value.ToAssimpVec()));
						}
					}

					if (rootHasScale)
					{
						foreach (var frame in data.RootScaleFrames)
						{
							channel.ScalingKeys.Add(new VectorKey(frame.Value.Time, frame.Value.Value.ToAssimpVec()));
						}
					}
				}
				else
				{
					var timeSet = new SortedSet<float>();

					if (rootHasPos)
					{
						foreach (var frame in data.RootPositionFrames)
						{
							timeSet.Add(frame.Value.Time);
						}
					}
					if (rootHasScale)
					{
						foreach (var frame in data.RootScaleFrames)
						{
							timeSet.Add(frame.Value.Time);
						}
					}

					foreach (var time in timeSet)
					{
						if (data.RootPositionFrames.TryGetValue(time, out var valuePos))
						{
							channel.PositionKeys.Add(new VectorKey(time / AnimationFrameRate, valuePos.Value.ToAssimpVec()));
						}
						else
						{
							Vector4 cur;
							var next = data.RootPositionFrames.FirstOrDefault(pair => pair.Value.Time > time);
							var prev = data.RootPositionFrames.LastOrDefault(pair => pair.Value.Time < time);
							if (next.Key == 0f)
							{
								cur = data.RootPositionFrames.LastOrDefault().Value.Value;
							}
							else if (prev.Key == 0f)
							{
								throw new Exception(); // todo: ? ? ?
							}
							else
							{
								cur = Vector4.Lerp(prev.Value.Value, next.Value.Value,
									(time - prev.Value.Time) / (next.Value.Time - prev.Value.Time));
							}

							channel.PositionKeys.Add(new VectorKey(time / AnimationFrameRate, cur.ToAssimpVec()));
						}

						if (data.RootScaleFrames.TryGetValue(time, out var valueScale))
						{
							channel.ScalingKeys.Add(new VectorKey(time / AnimationFrameRate, valueScale.Value.ToAssimpVec()));
						}
						else
						{
							if (data.RootScaleFrames.Count > 0)
							{
								Vector3 cur;
								var next = data.RootScaleFrames.FirstOrDefault(pair => pair.Value.Time > time);
								var prev = data.RootScaleFrames.LastOrDefault(pair => pair.Value.Time < time);
								if (next.Key == 0f)
								{
									cur = data.RootScaleFrames.LastOrDefault().Value.Value;
								}
								else if (prev.Key == 0f)
								{
									throw new Exception(); // todo: ? ? ?
								}
								else
								{
									cur = Vector3.Lerp(prev.Value.Value, next.Value.Value,
										(time - prev.Value.Time) / (next.Value.Time - prev.Value.Time));
								}

								channel.ScalingKeys.Add(new VectorKey(time / AnimationFrameRate, cur.ToAssimpVec()));
							}
							else
							{
								channel.ScalingKeys.Add(new VectorKey(time / AnimationFrameRate, new Vector3D(1, 1, 1)));
							}
						}

						channel.RotationKeys.Add(new QuaternionKey(time / AnimationFrameRate, new Quaternion(1, 0, 0, 0)));
					}
				}

				assAnim.NodeAnimationChannels.Add(channel);
			}

			int boneCount = Math.Min(data.BoneAnims.Count, Skeleton != null ? Skeleton.Definition.Data.Bones.Count : 0);
			for (int i = 0; i < boneCount; i++)
			{
				var boneAnim = data.BoneAnims[i];
				var hasPos = boneAnim.HasPositionTransform();
				var hasRot = boneAnim.HasRotationTransform();
				if (hasPos || hasRot)
				{
					var channel = new NodeAnimationChannel();
					channel.NodeName = Skeleton.Definition.Data.Bones[i].Name;

					if (SupportTRSInAnimation)
					{
						if (hasPos)
						{
							foreach (var frame in boneAnim.PositionFrames)
							{
								channel.PositionKeys.Add(new VectorKey(frame.Value.Time, frame.Value.Value.ToAssimpVec()));
							}
						}
						if (hasRot)
						{
							foreach (var frame in boneAnim.RotationFrames)
							{
								channel.RotationKeys.Add(new QuaternionKey(frame.Value.Time, frame.Value.Value.ToAssimpQuaternion()));
							}
						}
					}
					else
					{
						var timeSet = new SortedSet<float>();

						if (hasPos)
						{
							foreach (var frame in boneAnim.PositionFrames)
							{
								timeSet.Add(frame.Value.Time);
							}
						}
						if (hasRot)
						{
							foreach (var frame in boneAnim.RotationFrames)
							{
								timeSet.Add(frame.Value.Time);
							}
						}

						foreach (var time in timeSet)
						{
							if (boneAnim.PositionFrames.TryGetValue(time, out var valuePos))
							{
								channel.PositionKeys.Add(new VectorKey(time / AnimationFrameRate, valuePos.Value.ToAssimpVec()));
							}
							else
							{
								if (boneAnim.PositionFrames.Count > 0)
								{
									Vector4 cur;
									var next = boneAnim.PositionFrames.FirstOrDefault(pair => pair.Value.Time > time);
									var prev = boneAnim.PositionFrames.LastOrDefault(pair => pair.Value.Time < time);
									if (next.Key == 0f)
									{
										cur = boneAnim.PositionFrames.LastOrDefault().Value.Value;
									}
									else if (prev.Key == 0f)
									{
										throw new Exception(); // todo: ? ? ?
									}
									else
									{
										cur = Vector4.Lerp(prev.Value.Value, next.Value.Value,
											(time - prev.Value.Time) / (next.Value.Time - prev.Value.Time));
									}

									channel.PositionKeys.Add(new VectorKey(time / AnimationFrameRate, cur.ToAssimpVec()));
								}
								else
								{
									channel.PositionKeys.Add(new VectorKey(time / AnimationFrameRate, new Vector3D(0, 0, 0)));
								}
							}

							if (boneAnim.RotationFrames.TryGetValue(time, out var valueScale))
							{
								channel.RotationKeys.Add(new QuaternionKey(time / AnimationFrameRate, valueScale.Value.ToAssimpQuaternion()));
							}
							else
							{
								if (boneAnim.RotationFrames.Count > 0)
								{
									System.Numerics.Quaternion cur;
									var next = boneAnim.RotationFrames.FirstOrDefault(pair => pair.Value.Time > time);
									var prev = boneAnim.RotationFrames.LastOrDefault(pair => pair.Value.Time < time);
									if (next.Key == 0f)
									{
										cur = boneAnim.RotationFrames.LastOrDefault().Value.Value;
									}
									else if (prev.Key == 0f)
									{
										throw new Exception(); // todo: ? ? ?
									}
									else
									{
										cur = System.Numerics.Quaternion.Slerp(prev.Value.Value, next.Value.Value,
											(time - prev.Value.Time) / (next.Value.Time - prev.Value.Time));
									}

									channel.RotationKeys.Add(new QuaternionKey(time / AnimationFrameRate, cur.ToAssimpQuaternion()));
								}
								else
								{
									channel.RotationKeys.Add(new QuaternionKey(time / AnimationFrameRate, new Quaternion(1, 0, 0, 0)));
								}
							}

							channel.ScalingKeys.Add(new VectorKey(time / AnimationFrameRate, new Vector3D(1, 1, 1)));
						}
					}

					var restMatrix = Skeleton.Definition.Data.Bones[i].RestFrame;
					if (IgnoreScale)
						restMatrix.M44 = 1;

					System.Numerics.Matrix4x4.Decompose(restMatrix, out _,
						out _,
						out var trans);

					var posList = channel.PositionKeys;
					for (var j = 0; j < posList.Count; j++)
					{
						var vec = posList[j].Value;
						vec.X += trans.X;
						vec.Y += trans.Y;
						vec.Z += trans.Z;
						posList[j] = new VectorKey(posList[j].Time, vec);
					}

					var rotList = channel.RotationKeys;
					for (var j = 0; j < rotList.Count; j++)
					{
						var quat = rotList[j].Value;
						rotList[j] = new QuaternionKey(rotList[j].Time, quat);
					}

					assAnim.NodeAnimationChannels.Add(channel);
				}
			}

			return assAnim;
		}

		private System.Numerics.Matrix4x4 GetBoneRestFrame(BoneNode bone)
		{
			System.Numerics.Matrix4x4 matrix = bone.RestFrame;
			if (IgnoreScale)
				matrix.M44 = 1f;
			return matrix;
		}

		private static TextureType GuessTextureUsage(Texture texture)
		{
			var name = texture.Name.ToLower();
			if (name.EndsWith("_d") ||
			    name.EndsWith("_diffuse") ||
			    name.EndsWith("_d_4k") ||
			    name.EndsWith("_c") ||
			    name.EndsWith("_color") ||
			    name.EndsWith("_base_color") ||
			    name.EndsWith("_c_4k"))
			{
				return TextureType.Diffuse;
			}
			if (name.EndsWith("_n") ||
			    name.EndsWith("_normal") ||
			    name.EndsWith("_n_4k"))
			{
				return TextureType.Normals;
			}
			if (name.EndsWith("_s") ||
			    name.EndsWith("_specular") ||
			    name.EndsWith("_s_4k") ||
			    name == "nospec" ||
			    name == "default_specular")
			{
				return TextureType.Specular;
			}
			return TextureType.Unknown;
		}
	}
}