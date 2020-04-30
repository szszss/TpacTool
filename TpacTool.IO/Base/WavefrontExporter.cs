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

		public override void Export(Stream writeStream)
		{
			bool largeSize = IsLargerSize;
			bool swapYz = IsYAxisUp;
#if NET40
			var memStream = new MemoryStream();
			using (var stream = new StreamWriter(writeStream, Encoding.UTF8))
#else
			using (var stream = new StreamWriter(writeStream, Encoding.UTF8, 4096, true))
#endif
			{
				stream.Write("# Exported by TpacTool.IO\n\n");

				stream.Write("s ");
				stream.Write(Model.Name);
				stream.Write('\n');

				var meshes = Model.Meshes.FindAll(mesh => mesh.Lod == 0);
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

				foreach (var mesh in meshes)
				{
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
							float x, y, z;
							switch (largeSize)
							{
								default:
									switch (swapYz)
									{
										default:
											x = position.X;
											y = position.Y;
											z = position.Z;
											break;
										case true:
											x = position.X;
											y = position.Z;
											z = -position.Y;
											break;
									}
									break;
								case true:
									switch (swapYz)
									{
										default:
											x = position.X * ResizeFactor;
											y = position.Y * ResizeFactor;
											z = position.Z * ResizeFactor;
											break;
										case true:
											x = position.X * ResizeFactor;
											y = position.Z * ResizeFactor;
											z = -position.Y * ResizeFactor;
											break;
									}
									break;
							}
							stream.Write("v ");
							stream.Write(x);
							stream.Write(' ');
							stream.Write(y);
							stream.Write(' ');
							stream.Write(z);
							stream.Write('\n');
						}

						for (var i = 0; i < med.Vertices.Length; i++)
						{
							float x, y, z;
							var vertex = med.Vertices[i];
							switch (swapYz)
							{
								default:
									x = vertex.Normal.X;
									y = vertex.Normal.Y;
									z = vertex.Normal.Z;
									break;
								case true:
									x = vertex.Normal.X;
									y = vertex.Normal.Z;
									z = -vertex.Normal.Y;
									break;
							}
							posPointer[i] = (int)vertex.PositionIndex;
							stream.Write("vn ");
							stream.Write(x);
							stream.Write(' ');
							stream.Write(y);
							stream.Write(' ');
							stream.Write(z);
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
			stream.Write("Kd 1.000 1.000 1.000\n");
			stream.Write("Ks 0.000 0.000 0.000\n");
		}
	}
}