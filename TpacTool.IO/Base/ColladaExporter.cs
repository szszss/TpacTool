using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml;
using Collada141;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public class ColladaExporter : AbstractModelExporter
	{
		public override string Extension => "dae";

		public override bool SupportsSecondMaterial => false;

		public override bool SupportsSecondUv => false;

		public override bool SupportsSecondColor => false;

		public override bool SupportsSkeleton => true;

		public override bool SupportsMorph => true;

		public override bool SupportsSkeletalAnimation => false;

		public override bool SupportMorphAnimation => false;

		public override void Export(Stream writeStream)
		{
			CheckStreamAndTexture(writeStream);

			COLLADA collada = new COLLADA();
			collada.version = VersionType.Item141;
			var asset = collada.asset = new asset();
			asset.contributor = new[] { new assetContributor() { author = "Taleworlds", authoring_tool = "TpacTool" } };
			asset.created = asset.modified = DateTime.UtcNow;
			asset.unit = new assetUnit() { name = "meter", meter = IsLargerSize ? 10d : 1d };
			//asset.up_axis = IsYAxisUp ? UpAxisType.Y_UP : UpAxisType.Z_UP;
			asset.up_axis = UpAxisType.Z_UP;

			var meshes = Model.Meshes.FindAll(mesh => (LodMask & (1 << mesh.Lod)) > 0);
			CollectUniqueMaterialsAndTextures(meshes, out var materialSet, out var textureSet);
			var images = new library_images();
			var imageList = new List<image>();
			foreach (var texture in textureSet)
			{
				var image = new image() { id = texture.Name, name = texture.Name };
				if (TexturePathMapping.TryGetValue(texture, out var path))
					image.Item = path;
				else
					image.Item = string.Empty;
				imageList.Add(image);
			}
			images.image = imageList.ToArray();
			imageList = null;

			var effects = new library_effects();
			var effectList = new List<effect>();
			foreach (var material in materialSet)
			{
				effectList.Add(CreateEffect(material));
			}
			effects.effect = effectList.ToArray();

			var materials = new library_materials();
			List<material> materialList = new List<material>();
			foreach (var material in materialSet)
			{
				var matNode = new material() { id = material.Name + "-material", name = material.Name };
				matNode.instance_effect = new instance_effect() {url = "#" + material.Name + "-effect"};
				materialList.Add(matNode);
			}
			materials.material = materialList.ToArray();

			var geometries = new library_geometries();
			List<geometry> geos = new List<geometry>();
			List<geometry> geosExcludeMorph = new List<geometry>();
			Dictionary<Mesh, geometry> meshMapping = new Dictionary<Mesh, geometry>();
			Dictionary<geometry, int> morphCounts = new Dictionary<geometry, int>();
			int geoId = 0;
			HashSet<Mesh> unprocessedMeshes = new HashSet<Mesh>(meshes);
			while (unprocessedMeshes.Count > 0)
			{
				var mesh = unprocessedMeshes.First();
				//processingMeshes.Add(mesh);
				List<Mesh> processingMeshes = unprocessedMeshes.Where(
											meshB => HasSameMorphFrames(mesh, meshB) && mesh.Lod == meshB.Lod).ToList();
				unprocessedMeshes.RemoveWhere(meshB => processingMeshes.Contains(meshB));

				var id = GetGeoId(geoId++);
				// if this is the only one geometry, then its name is the name of model
				var name = processingMeshes.Count == meshes.Count ? Model.Name : mesh.Name;
				var geo = CreateMesh(processingMeshes, id, name, IsNegYAxisForward);
				geos.Add(geo);
				geosExcludeMorph.Add(geo);
				foreach (var processingMesh in processingMeshes)
				{
					meshMapping[processingMesh] = geo;
				}
				var morphs = mesh.EditData.Data.MorphFrames;
				if (morphs.Count > 0)
				{
					int morphIndex = 0;
					morphCounts[geo] = morphs.Count;
					var isHumanHead = Model.Name == "head_male_a" || Model.Name == "head_female_a";
					for (var i = 0; i < morphs.Count; i++)
					{
						var label = isHumanHead ? MorphNameMapping.GetHumanHeadMorphName(i) :
									"KeyTime_" + i;
						var morphGeo = CreateMorphMesh(processingMeshes, id, label, i, IsNegYAxisForward);
						morphIndex++;
						geos.Add(morphGeo);
					}
				}
			}
			unprocessedMeshes = null;
			geometries.geometry = geos.ToArray();

			var controllers = new library_controllers();
			List<controller> controls = new List<controller>();
			foreach (var pair in morphCounts)
			{
				var geo = pair.Key;
				var num = pair.Value;
				var sb = new StringBuilder();
				for (int i = 0; i < num; i++)
				{
					if (i != 0)
						sb.Append(' ');
					sb.Append(geo.id).Append("_morph").Append(i);
				}
				var controller = new controller()
				{
					id = geo.id + "_morph",
					name = geo.id + "_morph"
				};
				var morph = new morph()
				{
					source1 = "#" + geo.id,
					method = MorphMethodType.NORMALIZED,
					source = new[]
					{
						new source()
						{
							id = geo.id + "-targets",
							Item = new IDREF_array()
							{
								id = geo.id + "-targets-array",
								count = (ulong) num,
								Value = sb.ToString()
							},
							technique_common = new sourceTechnique_common()
							{
								accessor = new accessor()
								{
									source = "#" + geo.id + "-targets-array",
									count = (ulong) num,
									stride = 1,
									// should the name be "MORPH_TARGET"?
									param = new []{ new param() { name = "IDREF", type = "IDREF" }}
								}
							}
						},
						new source()
						{
							id = geo.id + "-weights",
							Item = new float_array()
							{
								id = geo.id + "-weights-array",
								count = (ulong) num,
								Values = new double[num]
							},
							technique_common = new sourceTechnique_common()
							{
								accessor = new accessor()
								{
									source = "#" + geo.id + "-weights-array",
									count = (ulong) num,
									stride = 1,
									param = new []{ new param() { name = "MORPH_WEIGHT", type = "float" }}
								}
							}
						}
					},
					targets = new morphTargets()
					{
						input = new[]
						{
							new InputLocal()
							{
								semantic = "MORPH_TARGET", source = "#" + geo.id + "-targets"
							},
							new InputLocal()
							{
								semantic = "MORPH_WEIGHT", source = "#" + geo.id + "-weights"
							}
						}
					}
				};
				controller.Item = morph;
				controls.Add(controller);
			}

			Matrix4x4[] invBindMatrices = null;
			if (Skeleton != null)
			{
				var skelData = Skeleton.Definition.Data;
				bool ignoreScale = Skeleton.UserData != null &&
									(Skeleton.UserData.Data.Usage == SkeletonUserData.USAGE_HUMAN ||
									Skeleton.UserData.Data.Usage == SkeletonUserData.USAGE_HORSE);
				invBindMatrices = new Matrix4x4[skelData.Bones.Count];
				for (var i = 0; i < invBindMatrices.Length; i++)
				{
					var bone = skelData.Bones[i];
					Matrix4x4 matrix = GetBoneRestFrame(bone, ignoreScale);
					Matrix4x4.Invert(matrix, out matrix);
					BoneNode parentBone = null;
					while ((parentBone = bone.Parent) != null)
					{
						var leftMatrix = GetBoneRestFrame(parentBone, ignoreScale);
						Matrix4x4.Invert(leftMatrix, out leftMatrix);
						matrix = Matrix4x4.Multiply(leftMatrix, matrix);
						bone = parentBone;
					}

					if (IsNegYAxisForward)
						matrix = Matrix4x4.Multiply(NegYMatrix, matrix);
					invBindMatrices[i] = matrix;
				}

				foreach (var geometry in geosExcludeMorph)
				{
					var controller = new controller()
					{
						id = Skeleton.Name + "_" + geometry.id + "-skin",
						name = Skeleton.Name
					};
					var skinNode = new skin() {source1 = "#" + geometry.id};
					controller.Item = skinNode;
					skinNode.bind_shape_matrix = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1";

					var sourceName = new source() {id = controller.id + "-joints"};
					var nameArray = new Name_array() {id = sourceName.id + "-array"};
					nameArray.count = (ulong) skelData.Bones.Count;
					nameArray.Values = skelData.Bones.Select(bone => bone.Name.Replace(' ', '_')).ToArray();
					sourceName.Item = nameArray;
					sourceName.technique_common = new sourceTechnique_common()
					{
						accessor = new accessor()
						{
							count = (ulong)skelData.Bones.Count,
							source = "#" + nameArray.id,
							stride = 1,
							param = new[]
							{
								new param() {name = "JOINT", type = "name"}
							}
						}
					};

					var sourcePose = new source() { id = controller.id + "-bind_poses" };
					var posArray = new float_array() { id = sourcePose.id + "-array" };
					posArray.count = (ulong) skelData.Bones.Count * 16;
					posArray.Values = invBindMatrices
						.SelectMany(
							matrix => EnumerateMartix(matrix),
							(matrix, f) => (double)f)
						.ToArray();

					sourcePose.Item = posArray;
					sourcePose.technique_common = new sourceTechnique_common()
					{
						accessor = new accessor()
						{
							count = (ulong)skelData.Bones.Count,
							source = "#" + posArray.id,
							stride = 16,
							param = new[]
							{
								new param() {name = "TRANSFORM", type = "float4x4"}
							}
						}
					};

					var sourceWeight = new source() { id = controller.id + "-weights" };
					var weightArray = new float_array() { id = sourceWeight.id + "-array" };
					List<double> weightList = new List<double>();
					List<int> boneList = new List<int>();
					List<int> boneCountPerVertexList = new List<int>();
					int vertexCount = 0;
					int tempCount = 0;
					foreach (var pair in meshMapping)
					{
						if (pair.Value == geometry)
						{
							vertexCount += pair.Key.VertexCount;
							var vs = pair.Key.VertexStream.Data;
							for (var i = 0; i < vs.BoneWeights.Length; i++)
							{
								int k = 0;
								var boneIndex = vs.BoneIndices[i];
								var boneWeight = vs.BoneWeights[i];
								if (boneWeight.W1 > 0)
								{
									weightList.Add(boneWeight.W1 / 255d);
									boneList.Add(boneIndex.B1);
									boneList.Add(tempCount++);
									k++;
								}
								if (boneWeight.W2 > 0)
								{
									weightList.Add(boneWeight.W2 / 255d);
									boneList.Add(boneIndex.B2);
									boneList.Add(tempCount++);
									k++;
								}
								if (boneWeight.W3 > 0)
								{
									weightList.Add(boneWeight.W3 / 255d);
									boneList.Add(boneIndex.B3);
									boneList.Add(tempCount++);
									k++;
								}
								if (boneWeight.W4 > 0)
								{
									weightList.Add(boneWeight.W4 / 255d);
									boneList.Add(boneIndex.B4);
									boneList.Add(tempCount++);
									k++;
								}
								boneCountPerVertexList.Add(k);
							}
							/*foreach (var bone in editData.Bones)
							{
								int i = 0;
								if (bone.W0 > 0)
								{
									weightList.Add(bone.W0);
									boneList.Add(bone.B0);
									boneList.Add(tempCount++);
									i++;
								}
								if (bone.W1 > 0)
								{
									weightList.Add(bone.W1);
									boneList.Add(bone.B1);
									boneList.Add(tempCount++);
									i++;
								}
								if (bone.W2 > 0)
								{
									weightList.Add(bone.W2);
									boneList.Add(bone.B2);
									boneList.Add(tempCount++);
									i++;
								}
								if (bone.W3 > 0)
								{
									weightList.Add(bone.W3);
									boneList.Add(bone.B3);
									boneList.Add(tempCount++);
									i++;
								}
								boneCountPerVertexList.Add(i);
							}*/
						}
					}

					weightArray.count = (ulong)weightList.Count;
					weightArray.Values = weightList.ToArray();
					sourceWeight.Item = weightArray;
					sourceWeight.technique_common = new sourceTechnique_common()
					{
						accessor = new accessor()
						{
							count = (ulong)weightList.Count,
							source = "#" + weightArray.id,
							stride = 1,
							param = new[]
							{
								new param() {name = "WEIGHT", type = "float"}
							}
						}
					};
					weightList = null;

					skinNode.source = new[] {sourceName, sourcePose, sourceWeight };

					var jointsNode = new skinJoints();
					jointsNode.input = new[]
					{
						new InputLocal() { semantic = "JOINT", source = "#" + sourceName.id },
						new InputLocal() { semantic = "INV_BIND_MATRIX", source = "#" + sourcePose.id }
					};
					skinNode.joints = jointsNode;

					var vertexWeightNode = new skinVertex_weights() { count = (ulong)vertexCount };
					vertexWeightNode.input = new[]
					{
						new InputLocalOffset() {semantic = "JOINT", source = "#" + sourceName.id, offset = 0},
						new InputLocalOffset() {semantic = "WEIGHT", source = "#" + sourceWeight.id, offset = 1},
					};
					vertexWeightNode.vcount = COLLADA.ConvertFromArray(boneCountPerVertexList);
					vertexWeightNode.v = COLLADA.ConvertFromArray(boneList);
					skinNode.vertex_weights = vertexWeightNode;

					controls.Add(controller);
				}
			}
			controllers.controller = controls.ToArray();

			var visualScenes = new library_visual_scenes();
			var visScene = new visual_scene() { id = "Scene", name = "Scene" };
			List<node> nodes = new List<node>();
			node modelRoot = null;

			if (Skeleton != null)
			{
				modelRoot = CreateSkeleton(Skeleton, invBindMatrices);
				nodes.Add(modelRoot);
			}
			//else if (geosExcludeMorph.Count > 1)
			else
			{
				modelRoot = new node() { type = NodeType.NODE };
				if (string.IsNullOrEmpty(Model.Name))
				{
					modelRoot.id = "node_" + Model.Name;
					modelRoot.name = Model.Name;
				}
				nodes.Add(modelRoot);
			}
			modelRoot.Items = new object[]
			{
				new TargetableFloat3() { sid = "scale", Values = new double[]{ 1, 1, 1 } },
				new rotate() { sid = "rotationZ", Values = new double[]{ 0, 0, 1, 0 } },
				new rotate() { sid = "rotationY", Values = new double[]{ 0, 1, 0, 0 } },
				new rotate() { sid = "rotationX", Values = new double[]{ 1, 0, 0, 0 } },
				new TargetableFloat3() { sid = "location", Values = new double[]{ 0, 0, 0 } }
			};
			modelRoot.ItemsElementName = new ItemsChoiceType2[]
			{
				ItemsChoiceType2.scale,
				ItemsChoiceType2.rotate,
				ItemsChoiceType2.rotate,
				ItemsChoiceType2.rotate,
				ItemsChoiceType2.translate
			};

			var animations = new library_animations();
			if (Animation != null)
			{
				var data = Animation.Definition.Data;
				var animContainerName = "action_container-" + (Skeleton != null ? Skeleton.Name : Model.Name);
				var anim = new animation() { id = animContainerName, name = Animation.Name };
				List<animation> animChannel = new List<animation>();

				CreateAnimationRootTransform(modelRoot.id, data, animChannel);
				if (Skeleton != null)
				{
					// XXX: redundant ignoreScale
					bool ignoreScale = Skeleton.UserData != null &&
					                   (Skeleton.UserData.Data.Usage == SkeletonUserData.USAGE_HUMAN ||
					                    Skeleton.UserData.Data.Usage == SkeletonUserData.USAGE_HORSE);
					var skelData = Skeleton.Definition.Data;
					var j = Math.Min(data.BoneAnims.Count, skelData.Bones.Count);
					for (var i = 0; i < j; i++)
					{
						var boneAnim = data.BoneAnims[i];
						CreateAnimationBoneTransform(Skeleton.Name + "_" + skelData.Bones[i].Name, boneAnim,
							GetBoneRestFrame(skelData.Bones[i], ignoreScale), animChannel);
					}
				}

				anim.Items = animChannel.Cast<object>().ToArray();
				animations.animation = new[] { anim };
			}

			foreach (var geometry in geosExcludeMorph)
			{
				var matList = new List<instance_material>();
				string geoName = geosExcludeMorph.Count == 1 ? Model.Name : string.Empty;
				foreach (var pair in meshMapping)
				{
					if (pair.Value == geometry)
					{
						if (geoName == string.Empty)
							geoName = pair.Key.Name;
						if (pair.Key.Material.TryGetItem(out var mat))
						{
							var matBind = new instance_material();
							matBind.symbol = mat.Name + "-material";
							matBind.target = "#" + matBind.symbol;
							// fix blender 2.8x importing
							matBind.bind_vertex_input = new instance_materialBind_vertex_input[]
							{
								new instance_materialBind_vertex_input()
								{
									semantic = "TEXCOORD0",
									input_semantic = "TEXCOORD",
									input_set = 0
								}
							};
							matList.Add(matBind);
						}
					}
				}
				var node = new node() { id = "node_" + geometry.id, name = geoName };

				if (Skeleton != null)
				{
					var insCon = new instance_controller()
					{
						url = "#" + Skeleton.Name + "_" + geometry.id + "-skin"
					};
					var skelRoot = GetSkeletonRootNode(modelRoot);
					insCon.skeleton = new[] {"#" + skelRoot.id};
					var bindMat = new bind_material();

					bindMat.technique_common = matList.ToArray();
					insCon.bind_material = bindMat;

					node.instance_controller = new[] { insCon };
				}
				else
				{
					var insGeo = new instance_geometry() {url = "#" + geometry.id};
					var bindMat = new bind_material();

					bindMat.technique_common = matList.ToArray();
					insGeo.bind_material = bindMat;

					node.instance_geometry = new[] {insGeo};
				}

				var matrix = new matrix();
				matrix.Values = new[]
				{
					1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1d
				};
				node.Items = new[] { (object)matrix };
				node.ItemsElementName = new[] { ItemsChoiceType2.matrix };
				if (modelRoot != null)
					AppendNode(modelRoot, node);
				else
					nodes.Add(node);
			}

			visScene.node = nodes.ToArray();
			visualScenes.visual_scene = new[] { visScene };

			collada.Items = new object[] { images, effects, materials, geometries, controllers, animations, visualScenes };

			var scene = collada.scene = new COLLADAScene();
			scene.instance_visual_scene = new InstanceWithExtra() { url = "#Scene" };

			collada.Save(writeStream);
		}

		private effect CreateEffect(Material material)
		{
			var effect = new effect() { id = material.Name + "-effect" };
			var profile = new effectFx_profile_abstractProfile_COMMON();
			effect.Items = new[] { profile };
			Texture[] textures = new Texture[(int)TextureUsage.Unknown];
			List<common_newparam_type> newparams = new List<common_newparam_type>();
			foreach (var texDep in material.Textures.Values)
			{
				if (texDep.TryGetItem(out var tex))
				{
					TextureUsage usage = GuessTextureUsage(tex);
					if (usage == TextureUsage.Unknown || textures[(int)usage] != null)
						continue;
					if (usage != TextureUsage.Diffuse && IsDiffuseOnly)
						continue;
					var surface = new common_newparam_type()
					{
						sid = tex.Name + "-surface", ItemElementName = ItemChoiceType.surface
					};
					var surfaceItem = new fx_surface_common() { type = fx_surface_type_enum.Item2D };
					surfaceItem.init_from = new[] { new fx_surface_init_from_common() { Value = tex.Name } };
					surface.Item = surfaceItem;

					var sampler = new common_newparam_type()
					{
						sid = tex.Name + "-sampler", ItemElementName = ItemChoiceType.sampler2D
					};
					var samplerItem = new fx_sampler2D_common();
					samplerItem.source = tex.Name + "-surface";
					sampler.Item = samplerItem;

					newparams.Add(surface);
					newparams.Add(sampler);
					textures[(int)usage] = tex;
				}
			}
			profile.Items = newparams.ToArray();

			var tech = new effectFx_profile_abstractProfile_COMMONTechnique() { sid = "common" };
			var phong = new effectFx_profile_abstractProfile_COMMONTechniquePhong();
			profile.technique = tech;
			tech.Item = phong;

			phong.emission = new common_color_or_texture_type()
			{
				Item = new common_color_or_texture_typeColor { sid = "emission", Values = new double[] { 0, 0, 0, 1 } }
			};

			phong.ambient = new common_color_or_texture_type()
			{
				Item = new common_color_or_texture_typeColor { sid = "ambient", Values = new double[] { 0, 0, 0, 1 } }
			};

			if (textures[(int)TextureUsage.Diffuse] != null)
			{
				phong.diffuse = new common_color_or_texture_type()
				{
					Item = new common_color_or_texture_typeTexture()
					{
						texture = textures[(int)TextureUsage.Diffuse].Name + "-sampler",
						texcoord = "TEXCOORD0" // fix blender 2.8x importing
					}
				};
			}
			else
			{
				phong.diffuse = new common_color_or_texture_type()
				{
					Item = new common_color_or_texture_typeColor { sid = "diffuse", Values = new double[] { 1, 1, 1, 1 } }
				};
			}

			if (textures[(int)TextureUsage.Specular] != null)
			{
				phong.specular = new common_color_or_texture_type()
				{
					Item = new common_color_or_texture_typeTexture()
					{
						texture = textures[(int)TextureUsage.Specular].Name + "-sampler",
						texcoord = "TEXCOORD0" // fix blender 2.8x importing
					}
				};
			}
			else
			{
				phong.specular = new common_color_or_texture_type()
				{
					Item = new common_color_or_texture_typeColor
					{
						sid = "specular",
						Values = new double[] { 0.25, 0.25, 0.25, 1 }
					}
				};
			}

			phong.shininess = new common_float_or_param_type()
			{
				Item = new common_float_or_param_typeFloat() { sid = "shininess", Value = 50 }
			};

			phong.index_of_refraction = new common_float_or_param_type()
			{
				Item = new common_float_or_param_typeFloat() { sid = "index_of_refraction", Value = 1 }
			};

			if (textures[(int)TextureUsage.Normal] != null)
			{
				tech.extra = new[]{ new extra() { technique = new []
					{
						new technique()
						{
							profile = "FCOLLADA", bump = new bump()
							{
								bumptype = "NORMALMAP",
								Item = new common_color_or_texture_typeTexture()
								{
									texture = textures[(int)TextureUsage.Normal].Name + "-sampler",
									texcoord = "TEXCOORD0" // fix blender 2.8x importing
								}
							}
						}
					}}};
			}

			return effect;
		}

		private static bool HasSameMorphFrames(Mesh meshA, Mesh meshB)
		{
			if (meshA == meshB)
				return true;

			bool meshAHasMorph = meshA.EditData != null && meshA.EditData.Data.MorphFrames.Count > 0;
			bool meshBHasMorph = meshB.EditData != null && meshB.EditData.Data.MorphFrames.Count > 0;
			if (!meshAHasMorph && !meshBHasMorph) // if both meshes have no morph, return true
				return true;

			var morphFramesA = meshA.EditData.Data.MorphFrames;
			var morphFramesB = meshB.EditData.Data.MorphFrames;
			if (morphFramesA.Count != morphFramesB.Count)
				return false;

			for (var i = 0; i < morphFramesA.Count; i++)
			{
				var morphA = morphFramesA[i];
				var morphB = morphFramesB[i];
				if (morphA.Time != morphB.Time)
					return false;
			}

			return true;
		}

		private static geometry CreateMesh(List<Mesh> meshes, string id, string name, bool isNegYAxisForward)
		{
			var geo = new geometry();
			geo.id = id;
			geo.name = name;
			var cMesh = new mesh();
			var srcPos = GetPositionSource(geo.id + "-positions", meshes, isNegYAxisForward);
			var srcNormal = GetNormalSource(geo.id + "-normals", meshes, isNegYAxisForward);
			var srcTex0 = GetUvSource(geo.id + "-tex0", meshes);
			var srcColor0 = GetColorSource(geo.id + "-color0", meshes);
			cMesh.source = new[] { srcPos, srcNormal, srcTex0, srcColor0 };
			var vert = cMesh.vertices = new vertices() { id = geo.id + "-vertices" };
			vert.input = new[]
			{
				new InputLocal() {semantic = "POSITION", source = "#" + srcPos.id},
				new InputLocal() {semantic = "NORMAL", source = "#" + srcNormal.id},
				new InputLocal() {semantic = "TEXCOORD", source = "#" + srcTex0.id},
				new InputLocal() {semantic = "COLOR", source = "#" + srcColor0.id}
			};

			int offset = 0;
			List<triangles> trianglesList = new List<triangles>();
			foreach (var mesh in meshes)
			{
				var indices = new triangles();
				if (mesh.Material.TryGetItem(out var mat))
					indices.material = mat.Name + "-material";
				else
					indices.material = String.Empty;
				var input = new InputLocalOffset();
				input.offset = 0;
				input.semantic = "VERTEX";
				input.source = "#" + vert.id;
				indices.input = new[] { input };
				var offsetIndices = mesh.VertexStream.Data.Indices.Select(index => index + offset).ToArray();
				indices.p = COLLADA.ConvertFromArray(offsetIndices);
				indices.count = (ulong)(mesh.VertexStream.Data.Indices.Length / 3);
				offset += mesh.VertexCount;
				trianglesList.Add(indices);
			}
			
			cMesh.Items = trianglesList.ToArray();
			geo.Item = cMesh;
			return geo;
		}

		private static geometry CreateMorphMesh(List<Mesh> meshes, string id, string name, int morphIndex, 
												bool isNegYAxisForward)
		{
			var morphGeo = CreateMesh(meshes, id + "_morph" + morphIndex, name, isNegYAxisForward);
			var m = morphGeo.Item as mesh;
			var posSrc = m.source[0];
			var normalSrc = m.source[1];
			var posArray = (posSrc.Item as float_array).Values;
			var normalArray = (normalSrc.Item as float_array).Values;
			var negYMatrix = NegYMatrix;

			int offset = 0;
			foreach (var mesh in meshes)
			{
				var sharedVertexData = mesh.EditData.Data.Vertices;
				var vertexFrame = mesh.EditData.Data.MorphFrames[morphIndex];
				for (var j = 0; j < sharedVertexData.Length; j++)
				{
					int k = (offset + j) * 3;
					var vertex = sharedVertexData[j];
					var pos = vertexFrame.Positions[vertex.PositionIndex];
					if (isNegYAxisForward)
						pos = Vector4.Transform(pos, negYMatrix);
					posArray[k] = pos.X;
					posArray[k + 1] = pos.Y;
					posArray[k + 2] = pos.Z;
				}

				offset += sharedVertexData.Length;
			}

			offset = 0;
			foreach (var mesh in meshes)
			{
				var sharedVertexData = mesh.EditData.Data.Vertices;
				var vertexFrame = mesh.EditData.Data.MorphFrames[morphIndex];
				for (var j = 0; j < vertexFrame.Normals.Length; j++)
				{
					int k = (offset + j) * 3;
					var normal = vertexFrame.Normals[j];
					if (isNegYAxisForward)
						normal = Vector4.Transform(normal, negYMatrix);
					normalArray[k] = normal.X;
					normalArray[k + 1] = normal.Y;
					normalArray[k + 2] = normal.Z;
				}

				offset += sharedVertexData.Length;
			}

			return morphGeo;
		}

		private static source GetColorSource(string id, List<Mesh> meshes)
		{
			var src = ToSource(id);
			src.name = "Col";
			var array = src.Item as float_array;
			var totalLength = meshes.Sum(mesh => mesh.VertexStream.Data.Colors1.Length);
			var vals = new double[totalLength * 4];
			int offset = 0;
			foreach (var mesh in meshes)
			{
				var colors = mesh.VertexStream.Data.Colors1;
				for (var i = 0; i < colors.Length; i++)
				{
					int j = (offset + i) * 4;
					vals[j] = colors[i].R / 255f;
					vals[j + 1] = colors[i].G / 255f;
					vals[j + 2] = colors[i].B / 255f;
					vals[j + 3] = colors[i].A / 255f;
				}

				offset += colors.Length;
			}

			array.Values = vals;
			array.count = (ulong)vals.Length;
			src.technique_common = new sourceTechnique_common()
			{
				accessor = new accessor()
				{
					count = (ulong)totalLength,
					offset = 0,
					source = "#" + array.id,
					stride = 4,
					param = new[]
					{
						new param() {name = "R", type = "float"},
						new param() {name = "G", type = "float"},
						new param() {name = "B", type = "float"},
						new param() {name = "A", type = "float"}
					}
				}
			};
			return src;
		}

		private static source GetUvSource(string id, List<Mesh> meshes)
		{
			var src = ToSource(id);
			src.name = "UVMap";
			var array = src.Item as float_array;
			var totalLength = meshes.Sum(mesh => mesh.VertexStream.Data.Uv1.Length);
			var vals = new double[totalLength * 2];
			int offset = 0;
			foreach (var mesh in meshes)
			{
				var vec2 = mesh.VertexStream.Data.Uv1;
				for (var i = 0; i < vec2.Length; i++)
				{
					int j = (offset + i) * 2;
					vals[j] = vec2[i].X;
					vals[j + 1] = 1f - vec2[i].Y;
				}

				offset += vec2.Length;
			}

			array.Values = vals;
			array.count = (ulong)vals.Length;
			src.technique_common = new sourceTechnique_common()
			{
				accessor = new accessor()
				{
					count = (ulong)totalLength,
					offset = 0,
					source = "#" + array.id,
					stride = 2,
					param = new[]
					{
						new param() {name = "S", type = "float"},
						new param() {name = "T", type = "float"}
					}
				}
			};
			return src;
		}

		private static source GetPositionSource(string id, List<Mesh> meshes, bool isNegYAxisForward)
		{
			var negYMatrix = NegYMatrix;
			var src = ToSource(id);
			var array = src.Item as float_array;
			var totalLength = meshes.Sum(mesh => mesh.VertexStream.Data.Positions.Length);
			var vals = new double[totalLength * 3];
			int offset = 0;
			foreach (var mesh in meshes)
			{
				var vec3 = mesh.VertexStream.Data.Positions;
				for (var i = 0; i < vec3.Length; i++)
				{
					var vec = vec3[i];
					if (isNegYAxisForward)
						vec = Vector3.Transform(vec, negYMatrix);
					int j = (offset + i) * 3;
					vals[j] = vec.X;
					vals[j + 1] = vec.Y;
					vals[j + 2] = vec.Z;
				}

				offset += vec3.Length;
			}

			array.Values = vals;
			array.count = (ulong)vals.Length;
			src.technique_common = new sourceTechnique_common()
			{
				accessor = new accessor()
				{
					count = (ulong)totalLength,
					offset = 0,
					source = "#" + array.id,
					stride = 3,
					param = new[]
					{
						new param() {name = "X", type = "float"},
						new param() {name = "Y", type = "float"},
						new param() {name = "Z", type = "float"}
					}
				}
			};
			return src;
		}

		private static source GetNormalSource(string id, List<Mesh> meshes, bool isNegYAxisForward)
		{
			var negYMatrix = NegYMatrix;
			var src = ToSource(id);
			var array = src.Item as float_array;
			var totalLength = meshes.Sum(mesh => mesh.VertexStream.Data.Normals.Length);
			var vals = new double[totalLength * 3];
			int offset = 0;
			foreach (var mesh in meshes)
			{
				var vec3 = mesh.VertexStream.Data.Normals;
				for (var i = 0; i < vec3.Length; i++)
				{
					var vec = vec3[i];
					if (isNegYAxisForward)
						vec = Vector3.Transform(vec, negYMatrix);
					int j = (offset + i) * 3;
					vals[j] = vec.X;
					vals[j + 1] = vec.Y;
					vals[j + 2] = vec.Z;
				}

				offset += vec3.Length;
			}

			array.Values = vals;
			array.count = (ulong)vals.Length;
			src.technique_common = new sourceTechnique_common()
			{
				accessor = new accessor()
				{
					count = (ulong)totalLength,
					offset = 0,
					source = "#" + array.id,
					stride = 3,
					param = new[]
					{
						new param() {name = "X", type = "float"},
						new param() {name = "Y", type = "float"},
						new param() {name = "Z", type = "float"}
					}
				}
			};
			return src;
		}

		private static source ToSource(string id)
		{
			var src = new source();
			src.id = id;
			src.name = id;
			var array = new float_array();
			array.id = id + "-array";
			src.Item = array;
			return src;
		}

		private void CreateAnimationRootTransform(string animTarget, AnimationDefinitionData data, List<animation> outList)
		{
			if (data.HasRootPositionTransform())
			{
				outList.Add(CreateAnimationData(animTarget + "_location", animTarget, "location", Matrix4x4.Identity,  data.RootPositionFrames));
			}

			if (data.HasRootScaleTransform())
			{
				outList.Add(CreateAnimationData(animTarget + "_scale", animTarget, "scale", Matrix4x4.Identity,  data.RootScaleFrames));
			}
		}

		private void CreateAnimationBoneTransform(string animTarget,
			AnimationDefinitionData.BoneAnim data, Matrix4x4 boneRestMatrix, List<animation> outList)
		{
			if (data.HasPositionTransform())
			{
				outList.Add(CreateAnimationData(animTarget + "_location", animTarget, "location", boneRestMatrix, data.PositionFrames));
			}

			if (data.HasRotationTransform())
			{
				outList.Add(CreateAnimationData(animTarget + "_rotationX", animTarget, "rotationX", boneRestMatrix, data.RotationFrames, 0));
				outList.Add(CreateAnimationData(animTarget + "_rotationY", animTarget, "rotationY", boneRestMatrix, data.RotationFrames, 1));
				outList.Add(CreateAnimationData(animTarget + "_rotationZ", animTarget, "rotationZ", boneRestMatrix, data.RotationFrames, 2));
			}
		}

		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		private animation CreateAnimationData<T>(
			string animId, string animTarget, string animType, Matrix4x4 boneRestMatrix,
			SortedList<float, AnimationFrame<T>> position, int rotationComponent = 0) where T : struct
		{
			if (typeof(T) != typeof(Vector3) && typeof(T) != typeof(Vector4) && typeof(T) != typeof(Quaternion))
				throw new ArgumentException("Generic type of data must be one of Vector3, Vector4 or Quaternion");
			if (rotationComponent < 0 || rotationComponent > 2)
				throw new ArgumentException("rotationComponent must be in [0, 2]", nameof(rotationComponent));
			source sourceTime = null;
			{
				var timeArray = new float_array()
				{
					id = animId + "-input-array",
					count = (ulong) position.Count,
					Values = position.Keys.Select(key => (double) (key / AnimationFrameRate)).ToArray()
				};
				var timeTech = new sourceTechnique_common()
				{
					accessor = new accessor()
					{
						source = "#" + timeArray.id,
						count = timeArray.count,
						stride = 1,
						param = new param[]
						{
							new param() { name = "TIME", type = "float" }
						}
					}
				};
				sourceTime = new source() { id = animId + "-input", Item = timeArray, technique_common = timeTech };
			}

			source sourceOutput = null;
			{
				var outputArray = new float_array()
				{
					id = animId + "-output-array",
					count = (ulong) position.Count * (typeof(T) == typeof(Quaternion) ? 1ul : 3ul)
				};
				var outputTech = new sourceTechnique_common()
				{
					accessor = new accessor()
					{
						source = "#" + outputArray.id,
						count = (ulong)position.Count,
						stride = typeof(T) == typeof(Quaternion) ? 1ul : 3ul
					}
				};

				if (position is SortedList<float, AnimationFrame<Vector3>> vec3List)
				{
					outputArray.Values = vec3List.Values.SelectMany(value =>
					{
						var vec3 = value.Value;
						return new double[] { vec3.X, vec3.Y, vec3.Z };
					}).ToArray();
					outputTech.accessor.param = new param[]
					{
						new param() { name = "X", type = "float" },
						new param() { name = "Y", type = "float" },
						new param() { name = "Z", type = "float" }
					};
				}
				else if (position is SortedList<float, AnimationFrame<Vector4>> vec4List)
				{
					outputArray.Values = vec4List.Values.SelectMany(value =>
					{
						var vec4 = value.Value;
						return new double[] { vec4.X, vec4.Y, vec4.Z }; // ignore W
					}).ToArray();
					outputTech.accessor.param = new param[]
					{
						new param() { name = "X", type = "float" },
						new param() { name = "Y", type = "float" },
						new param() { name = "Z", type = "float" }
					};
				}
				else if (position is SortedList<float, AnimationFrame<Quaternion>> quatList)
				{
					var restQuat = Quaternion.Identity;
					if (Matrix4x4.Decompose(boneRestMatrix, out var unusedScale, out var boneQuat, out var unusedTrans))
					{
						restQuat = ConvertHand(Quaternion.Inverse(boneQuat));
					}

					outputArray.Values = quatList.Values.Select(q =>
					{
						//var quat = Quaternion.Multiply(Quaternion.CreateFromAxisAngle(Vector3.UnitX, 1.570795f), 
						//	Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1.570795f));
						var quat = Quaternion.Multiply(restQuat, ConvertHand(q.Value));
						//var quat = ConvertHand(q.Value);
						// y z is right, x is neg
						//quat = Quaternion.Multiply(new Quaternion(0, 0, -0.707f, 0.707f), Quaternion.Multiply(quat, new Quaternion(0, 0, 0.707f, 0.707f)));
						quat = Quaternion.Multiply(new Quaternion(-0.707f, 0.707f, 0, 0), Quaternion.Multiply(quat, new Quaternion(0.707f, -0.707f, 0, 0)));
						quat = Quaternion.Normalize(quat);

						//quat = Quaternion.Inverse(quat);
						//quat = new Quaternion(-quat.X, quat.Y, -quat.Z, quat.W);
						//var quat = Quaternion.Identity;
						var mat = Matrix4x4.CreateFromQuaternion(quat);
						GetEularFromMatrix(mat, out var rotX, out var rotY, out var rotZ);

						switch (rotationComponent)
						{
							case 0:
								return rotX * 180d / Math.PI;
							case 1:
								return rotY * 180d / Math.PI;
							case 2:
								return -rotZ * 180d / Math.PI;
							default:
								throw new ArgumentException();
						}
					}).ToArray();
					outputTech.accessor.param = new param[]
					{
						new param() { name = "ANGLE", type = "float" }
					};
				}

				sourceOutput = new source() {id = animId + "-output", Item = outputArray, technique_common = outputTech};
			}

			source sourceInterpolation = null;
			{
				var interpolationArray = new Name_array()
				{
					id = animId + "-interpolation-array",
					count = (ulong) position.Count,
					Values = Enumerable.Repeat("LINEAR", position.Count).ToArray()
				};
				var interpolationTech = new sourceTechnique_common()
				{
					accessor = new accessor()
					{
						source = "#" + interpolationArray.id,
						count = interpolationArray.count,
						stride = 1,
						param = new param[]
						{
							new param() { name = "INTERPOLATION", type = "name" }
						}
					}
				};
				sourceInterpolation = new source() 
					{ id = animId + "-interpolation", Item = interpolationArray, technique_common = interpolationTech };
			}

			sampler samp = new sampler()
			{
				id = animId + "-sampler", input = new InputLocal[]
				{
					new InputLocal() { semantic = "INPUT", source = "#" + sourceTime.id },
					new InputLocal() { semantic = "OUTPUT", source = "#" + sourceOutput.id },
					new InputLocal() { semantic = "INTERPOLATION", source = "#" + sourceInterpolation.id }
				}
			};

			channel chan = new channel() { source = "#" + samp.id, target = animTarget + "/" + animType };

			return new animation() 
			{
				id = animId,
				name = animTarget,
				Items = new object[]
				{
					sourceTime,
					sourceOutput,
					sourceInterpolation,
					samp,
					chan
				}
			};
		}

		private static void QuaternionToEulerAngles(Quaternion q, out double value, int component = 0)
		{
			var rotMat = Matrix4x4.CreateFromQuaternion(q);
			rotMat = Matrix4x4.Transpose(rotMat);

			switch (component)
			{
				case 1:
					value = Math.Asin(Math.Min(Math.Max(rotMat.M13, -1), 1));
					break;
				case 0:
					if (Math.Abs(rotMat.M23) < 0.99999f)
					{
						value = Math.Atan2(-rotMat.M23, rotMat.M33);
					}
					else
					{
						value = Math.Atan2(rotMat.M32, rotMat.M22);
					}
					break;
				case 2:
					if (Math.Abs(rotMat.M23) < 0.99999f)
					{
						value = Math.Atan2(-rotMat.M12, rotMat.M11);
					}
					else
					{
						value = 0;
					}
					break;
				default:
					throw new ArgumentException("Unknown component: " + component, nameof(component));
			}

			value = value * 180 / Math.PI;
		}

		private node CreateSkeleton(Skeleton skeleton, Matrix4x4[] invBindMatrices)
		{
			var root = new node();
			var data = skeleton.Definition.Data;
			bool ignoreScale = Skeleton.UserData != null &&
								(Skeleton.UserData.Data.Usage == SkeletonUserData.USAGE_HUMAN ||
								Skeleton.UserData.Data.Usage == SkeletonUserData.USAGE_HORSE);
			root.id = skeleton.Name;
			root.name = skeleton.Name;
			//root.Items = new object[] { CreateMatrix(Matrix4x4.Identity) };
			//root.ItemsElementName = new [] { ItemsChoiceType2.matrix };
			//root.type = NodeType.NODE;
			var mapping = new Dictionary<BoneNode, node>();
			var childrenNum = new ConcurrentDictionary<BoneNode, int>();
			foreach (var bone in data.Bones)
			{
				var boneNode = new node();
				boneNode.id = skeleton.Name + "_" + bone.Name;
				boneNode.name = bone.Name;
				boneNode.sid = bone.Name;
				boneNode.type = NodeType.JOINT;
				var restMatrix = GetBoneRestFrame(bone, ignoreScale);
				if (Animation != null &&
				    Matrix4x4.Decompose(restMatrix, out var scale, out var _, out var translation))
				{
					/*rotation = ConvertHand(rotation);
					QuaternionToEulerAngles(rotation, out var rotX, 0);
					QuaternionToEulerAngles(rotation, out var rotY, 1);
					QuaternionToEulerAngles(rotation, out var rotZ, 2);*/
					GetEularFromMatrix(restMatrix, out var rotX, out var rotY, out var rotZ);
					boneNode.Items = new object[]
					{
						new TargetableFloat3() { sid = "scale", Values = new double[]{ scale.X, scale.Y, scale.Z } },
						new rotate() { sid = "rotationX", Values = new double[]{ 1, 0, 0, rotX } },
						new rotate() { sid = "rotationY", Values = new double[]{ 0, 1, 0, rotY } },
						new rotate() { sid = "rotationZ", Values = new double[]{ 0, 0, 1, rotZ } },
						new TargetableFloat3() { sid = "location", Values = new double[]{ translation.X, translation.Y, translation.Z } }
					};
					boneNode.ItemsElementName = new ItemsChoiceType2[]
					{
						ItemsChoiceType2.scale,
						ItemsChoiceType2.rotate,
						ItemsChoiceType2.rotate,
						ItemsChoiceType2.rotate,
						ItemsChoiceType2.translate
					};
				}
				else
				{
					boneNode.Items = new object[] { CreateMatrix(restMatrix) };
					boneNode.ItemsElementName = new[] { ItemsChoiceType2.matrix };
				}
				mapping[bone] = boneNode;
			}

			foreach (var bone in data.Bones)
			{
				var parentBone = bone.Parent == null ? root : mapping[bone.Parent];
				AppendNode(parentBone, mapping[bone]);
				if (bone.Parent != null)
				{
					int num = childrenNum.GetOrAdd(bone.Parent, 0);
					childrenNum[bone.Parent] = ++num;
				}
			}

			for (var i = 0; i < data.Bones.Count; i++)
			{
				var bone = data.Bones[i];
				var child = GetBestConnectTarget(skeleton, bone, ignoreScale);

				technique tech = new technique();
				tech.profile = "blender";

				if (child != null)
				{
					var restMatrix = AccumulateBoneMatrix(bone, ignoreScale);
					var tip = GetBoneRestFrame(child, ignoreScale).Translation;
					tip.Y = tip.Z = 0f;
					tip = Vector3.Transform(tip, restMatrix);
					tip = Vector3.Subtract(tip, restMatrix.Translation);
					if (IsNegYAxisForward)
						tip = Vector3.Transform(tip, NegYMatrix);
					//Quaternion rot = Quaternion.CreateFromRotationMatrix(restMatrix);
					//float f = ExtractRollFromQuaternion(rot);
					tech.tip_x = new tip_x() { sid = "tip_x", type = "float", Text = tip.X.ToString() };
					tech.tip_y = new tip_y() { sid = "tip_y", type = "float", Text = tip.Y.ToString() };
					tech.tip_z = new tip_z() { sid = "tip_z", type = "float", Text = tip.Z.ToString() };
					//tech.roll = new roll() { sid = "roll", type = "float", Text = f.ToString() };
					//tech.roll = new roll() { sid = "roll", type = "float", Text = "0" };
				}
				else
				{
					var restMatrix = AccumulateBoneMatrix(bone, ignoreScale);
					var tip = new Vector3(0.1f, 0, 0);
					tip = Vector3.Transform(tip, restMatrix);
					tip = Vector3.Subtract(tip, restMatrix.Translation);
					if (IsNegYAxisForward)
						tip = Vector3.Transform(tip, NegYMatrix);
					//Quaternion rot = Quaternion.CreateFromRotationMatrix(restMatrix);
					//float f = ExtractRollFromQuaternion(rot);
					tech.tip_x = new tip_x() { sid = "tip_x", type = "float", Text = tip.X.ToString() };
					tech.tip_y = new tip_y() { sid = "tip_y", type = "float", Text = tip.Y.ToString() };
					tech.tip_z = new tip_z() { sid = "tip_z", type = "float", Text = tip.Z.ToString() };
					//tech.roll = new roll() { sid = "roll", type = "float", Text = f.ToString() };
					//tech.roll = new roll() { sid = "roll", type = "float", Text = "0" };
				}
				
				mapping[bone].extra = new[] { new extra() { technique = new[] { tech } } };
			}
			
			return root;
		}

		private static Matrix4x4 GetBoneRestFrame(BoneNode bone, bool ignoreScale)
		{
			Matrix4x4 matrix = bone.RestFrame;
			if (ignoreScale)
				matrix.M44 = 1f;
			return matrix;
		}

		private static BoneNode GetBestConnectTarget(Skeleton skeleton, BoneNode bone, bool ignoreScale)
		{
			var data = skeleton.Definition.Data;
			BoneNode target = null;
			var closestDis = 0.0001f;

			foreach (var child in data.Bones)
			{
				if (child != bone && child.Parent == bone)
				{
					var trans = child.RestFrame.Translation;
					if (trans.X < 0) // don't connect to the child on its back (see the tail of cat_skeleton)
						continue;
					trans.X = 0;
					var dis = trans.LengthSquared();
					if (dis < closestDis)
					{
						closestDis = dis;
						target = child;
					}
				}
			}

			return target;
		}

		private static Matrix4x4 AccumulateBoneMatrix(BoneNode bone, bool ignoreScale)
		{
			var mat = GetBoneRestFrame(bone, ignoreScale);
			BoneNode parent = null;

			while ((parent = bone.Parent) != null)
			{
				mat = Matrix4x4.Multiply(mat, GetBoneRestFrame(parent, ignoreScale));
				bone = parent;
			}

			return mat;
		}

		private static float ExtractRollFromQuaternion(Quaternion quat)
		{
			/*double pitch = Math.Asin(2.0 * (quat.Z * quat.X - quat.Z * quat.Y));
			if (pitch < -2 * Math.PI)
				pitch += 2 * Math.PI;
			else if (pitch > 2 * Math.PI)
				pitch -= 2 * Math.PI;*/
			var quat2 = Quaternion.Multiply(new Quaternion(0, 0, 0.707f, 0.707f),
				Quaternion.Multiply(quat, new Quaternion(0, 0, -0.707f, 0.707f)));
			//var quat2 = quat;
			QuaternionToEulerAngles(quat2, out var roll, 0);
			QuaternionToEulerAngles(quat2, out var pitch, 1);
			QuaternionToEulerAngles(quat2, out var yaw, 2);
			roll = -roll / 180 * Math.PI;
			return (float) roll;
		}

		private static void AppendNode(node parent, node child)
		{
			if (parent.node1 == null)
			{
				parent.node1 = new[] {child};
			}
			else
			{
				var objs = new node[parent.node1.Length + 1];
				Array.Copy(parent.node1, objs, parent.node1.Length);
				objs[objs.Length - 1] = child;
				parent.node1 = objs;
			}
		}

		private static matrix CreateMatrix(Matrix4x4 mat)
		{
			var matNode = new matrix();
			matNode.sid = "transform";
			matNode.Values = new double[]
			{
				mat.M11, mat.M21, mat.M31, mat.M41,
				mat.M12, mat.M22, mat.M32, mat.M42,
				mat.M13, mat.M23, mat.M33, mat.M43,
				mat.M14, mat.M24, mat.M34, mat.M44
			};
			return matNode;
		}

		private static node GetSkeletonRootNode(node searchBegin)
		{
			if (searchBegin.type == NodeType.JOINT)
				return searchBegin;
			if (searchBegin.node1 == null)
				return null;
			foreach (var child in searchBegin.node1)
			{
				var result = GetSkeletonRootNode(child);
				if (result != null)
					return result;
			}

			return null;
		}

		private static string GetGeoId(int id)
		{
			return "meshId" + id;
		}

		private static TextureUsage GuessTextureUsage(Texture texture)
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
				return TextureUsage.Diffuse;
			}
			if (name.EndsWith("_n") ||
				name.EndsWith("_normal") ||
				name.EndsWith("_n_4k"))
			{
				return TextureUsage.Normal;
			}
			if (name.EndsWith("_s") ||
				name.EndsWith("_specular") ||
				name.EndsWith("_s_4k") ||
				name == "nospec" ||
				name == "default_specular")
			{
				return TextureUsage.Specular;
			}
			return TextureUsage.Unknown;
		}

		private static IEnumerable<float> EnumerateMartix(Matrix4x4 matrix)
		{
			/*yield return matrix.M11;
			yield return matrix.M12;
			yield return matrix.M13;
			yield return matrix.M14;
			yield return matrix.M21;
			yield return matrix.M22;
			yield return matrix.M23;
			yield return matrix.M24;
			yield return matrix.M31;
			yield return matrix.M32;
			yield return matrix.M33;
			yield return matrix.M34;
			yield return matrix.M41;
			yield return matrix.M42;
			yield return matrix.M43;
			yield return matrix.M44;*/
			yield return matrix.M11;
			yield return matrix.M21;
			yield return matrix.M31;
			yield return matrix.M41;
			yield return matrix.M12;
			yield return matrix.M22;
			yield return matrix.M32;
			yield return matrix.M42;
			yield return matrix.M13;
			yield return matrix.M23;
			yield return matrix.M33;
			yield return matrix.M43;
			yield return matrix.M14;
			yield return matrix.M24;
			yield return matrix.M34;
			yield return matrix.M44;
		}

		private static Quaternion ConvertHand(Quaternion quat)
		{
			//  x  y  z
			// -y -x -z
			return new Quaternion(quat.X, quat.Y, quat.Z, quat.W);
		}

		private static void GetEularFromMatrix(Matrix4x4 matrix, out float x, out float y, out float z)
		{
			y = (float) Math.Asin(matrix.M13);

			if (Math.Abs(matrix.M13) < 0.9999999f)
			{
				x = (float)Math.Atan2(-matrix.M23, matrix.M33);
				z = (float)Math.Atan2(-matrix.M12, matrix.M11);
			}
			else
			{
				x = (float)Math.Atan2(matrix.M32, matrix.M22);
				z = 0;
			}
		}

		private enum TextureUsage : int
		{
			Diffuse = 0,
			Specular = 1,
			Normal = 2,
			Unknown = 3
		}
	}
}