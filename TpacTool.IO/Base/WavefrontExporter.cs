using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public class WavefrontExporter : AbstractModelExporter
	{
		public override string Extension => "obj";

		public override bool SupportsSecondMaterial => false;

		public override bool SupportsSecondUv => false;

		public override bool SupportsSecondColor => false;

		public override bool SupportsSkeleton => false;

		public override bool SupportsMorph => false;

		public override bool SupportsSkeletalAnimation => false;

		public override bool SupportMorphAnimation => false;

		public override void Export(Stream writeStream)
		{
			bool largeSize = IsLargerSize;
			bool isNegYForward = IsNegYAxisForward;
			bool isYUp = IsYAxisUp;
			var negYMatrix = NegYMatrix;
			var yUpMatrix = Matrix4x4.CreateRotationX((float) (Math.PI / -2));
#if NET40
			var memStream = new MemoryStream();
			using (var stream = new StreamWriter(writeStream, Encoding.UTF8))
#else
			using (var stream = new StreamWriter(writeStream, Encoding.UTF8, 4096, true))
#endif
			{
				stream.Write("# Exported by TpacTool.IO\n\n");

				var meshes = Model.Meshes.FindAll(mesh => (LodMask & (1 << mesh.Lod)) > 0);
				var definedMats = new HashSet<Material>();
				foreach (var mesh in meshes)
				{
					if (mesh.Material.TryGetItem(out var mat))
					{
						if (!definedMats.Contains(mat))
						{
							WriteMaterial(stream, mat);
							definedMats.Add(mat);
						}
					}
				}

				int offsetPos = 1, offsetNormal = 1;
				var meshesByLod = SortMeshesByLOD(meshes);

				foreach (var pair in meshesByLod)
				{
					var lod = pair.Key;
					var meshesInCurLod = pair.Value;

					stream.Write("o ");
					stream.Write(Model.Name);
					if (lod > 0)
						stream.Write($".lod{lod}");
					stream.Write('\n');

					foreach (var mesh in meshesInCurLod)
					{
						stream.Write("s 1\n");

						if (mesh.Material.TryGetItem(out var mat))
						{
							stream.Write("usemtl ");
							stream.Write(mat.Name);
							stream.Write('\n');
						}

						MeshEditData med = mesh.EditData != null ? mesh.EditData.Data : null;

						if (med != null)
						{
							int[] posPointer = new int[mesh.VertexCount];

							foreach (var position in med.Positions)
							{
								var vec = position;
								if (isNegYForward)
									vec = Vector4.Transform(vec, negYMatrix);
								if (isYUp)
									vec = Vector4.Transform(vec, yUpMatrix);
								if (largeSize)
									vec = Vector4.Multiply(vec, ResizeFactor);
								stream.Write("v ");
								stream.Write(vec.X);
								stream.Write(' ');
								stream.Write(vec.Y);
								stream.Write(' ');
								stream.Write(vec.Z);
								stream.Write('\n');
							}

							for (var i = 0; i < med.Vertices.Length; i++)
							{
								var vertex = med.Vertices[i];
								var vec = vertex.Normal;
								if (isNegYForward)
									vec = Vector4.Transform(vec, negYMatrix);
								if (isYUp)
									vec = Vector4.Transform(vec, yUpMatrix);
								posPointer[i] = (int)vertex.PositionIndex;
								stream.Write("vn ");
								stream.Write(vec.X);
								stream.Write(' ');
								stream.Write(vec.Y);
								stream.Write(' ');
								stream.Write(vec.Z);
								stream.Write('\n');
								stream.Write("vt ");
								stream.Write(vertex.Uv.X);
								stream.Write(' ');
								stream.Write(vertex.Uv.Y);
								stream.Write('\n');
							}

							foreach (var face in med.Faces)
							{
								stream.Write("f");
								for (int i = 0; i < 3; i++)
								{
									int index = face[i];
									int posIndex = posPointer[index] + offsetPos;
									int normalIndex = index + offsetNormal;
									stream.Write(' ');
									stream.Write(posIndex);
									stream.Write('/');
									stream.Write(normalIndex);
									stream.Write('/');
									stream.Write(normalIndex);

								}
								stream.Write('\n');
							}
						}
						else
						{
							// TODO: use vertex data stream
							throw new Exception("Cannot find the edit data of " + mesh.Name);
						}

						offsetPos += mesh.PositionCount;
						offsetNormal += mesh.VertexCount;
					}
				}
#if NET40
				memStream.WriteTo(writeStream);
#endif
			}
		}

		protected void WriteMaterial(StreamWriter stream, Material material)
		{
			stream.Write("newmtl ");
			stream.Write(material.Name);
			stream.Write('\n');
			stream.Write("Ka 1.000 1.000 1.000\n");
			stream.Write("Ks 0.000 0.000 0.000\n");
			stream.Write("Kd 1.000 1.000 1.000\n");
			if (material.Textures.TryGetValue(0, out var texDep) && texDep.TryGetItem(out var texture))
				stream.Write($"map_Kd {texture.Name}.png\n");
		}
	}
}