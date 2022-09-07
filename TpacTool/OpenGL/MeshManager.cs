using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using TpacTool.Lib;
using static OpenTK.Graphics.OpenGL4.GL;

namespace TpacTool
{
	public static class MeshManager
	{
		private const int MAX_CACHE = 32;
		private static readonly LinkedList<(Mesh, OglMesh)> cache = new LinkedList<(Mesh, OglMesh)>();

		public static OglMesh Get(Mesh mesh)
		{
			LinkedListNode<(Mesh, OglMesh)> node;
			for (node = cache.First; node != null; node = node.Next)
			{
				if (node.Value.Item1 == mesh)
					break;
			}

			if (node != null)
			{
				if (node.Previous != null)
				{
					cache.Remove(node);
					cache.AddFirst(node);
				}

				return node.Value.Item2;
			}

			var m = new OglMesh(mesh);
			cache.AddFirst((mesh, m));

			while (cache.Count > MAX_CACHE)
			{
				cache.Last.Value.Item2.Release();
				cache.RemoveLast();
			}

			return m;
		}

		public static void Clear()
		{
			foreach (var (_, oglMesh) in cache)
			{
				oglMesh.Release();
			}
			cache.Clear();
		}

		public class OglMesh
		{
			private const int VBO_POSITION = 0;
			private const int VBO_NORMAL = 1;
			private const int VBO_UV = 2;
			private const int VBO_COLOR = 3;
			private const int VBO_COLOR2 = 4;
			private const int VBO_BONEID = 5;
			private const int VBO_BONEWEIGHT = 6;
			private const int VBO_INDEX = 7;
			private const int TOTAL_VBO = 8;

			private int _vaoId = -1;
			private readonly int _vertexCount;
			private int[] _vboIds;

			public OglMesh(Mesh mesh)
			{
				var vertexStream = mesh.VertexStream?.Data;
				var editData = mesh.EditData?.Data;
				if (vertexStream != null && vertexStream.Positions != null)
				{
					_vaoId = GenVertexArray();
					BindVertexArray(_vaoId);
					_vertexCount = vertexStream.Indices.Length;
					_vboIds = new int[TOTAL_VBO];
					GenBuffers(TOTAL_VBO, _vboIds);

					BindBuffer(BufferTarget.ElementArrayBuffer, _vboIds[VBO_INDEX]);
					BufferData(BufferTarget.ElementArrayBuffer,
						sizeof(uint) * vertexStream.Indices.Length,
						vertexStream.Indices,
						BufferUsageHint.StaticDraw);

					BindBuffer(BufferTarget.ArrayBuffer, _vboIds[VBO_POSITION]);
					BufferData(BufferTarget.ArrayBuffer, 
						3 * sizeof(float) * vertexStream.Positions.Length,
						vertexStream.Positions,
						BufferUsageHint.StaticDraw);
					VertexAttribPointer(VBO_POSITION, 3, VertexAttribPointerType.Float, false, 0, 0);
					EnableVertexAttribArray(VBO_POSITION);

					BindBuffer(BufferTarget.ArrayBuffer, _vboIds[VBO_NORMAL]);
					BufferData(BufferTarget.ArrayBuffer,
						3 * sizeof(float) * vertexStream.Normals.Length,
						vertexStream.Normals,
						BufferUsageHint.StaticDraw);
					VertexAttribPointer(VBO_NORMAL, 3, VertexAttribPointerType.Float, false, 0, 0);
					EnableVertexAttribArray(VBO_NORMAL);

					BindBuffer(BufferTarget.ArrayBuffer, _vboIds[VBO_UV]);
					BufferData(BufferTarget.ArrayBuffer,
						2 * sizeof(float) * vertexStream.Uv1.Length,
						vertexStream.Uv1,
						BufferUsageHint.StaticDraw);
					VertexAttribPointer(VBO_UV, 2, VertexAttribPointerType.Float, false, 0, 0);
					EnableVertexAttribArray(VBO_UV);

					if (vertexStream.Colors1?.Length > 0)
					{
						BindBuffer(BufferTarget.ArrayBuffer, _vboIds[VBO_COLOR]);
						BufferData(BufferTarget.ArrayBuffer,
							4 * sizeof(byte) * vertexStream.Colors1.Length,
							vertexStream.Colors1,
							BufferUsageHint.StaticDraw);
						VertexAttribPointer(VBO_COLOR, 4, VertexAttribPointerType.UnsignedByte, true, 0, 0);
						EnableVertexAttribArray(VBO_COLOR);
					}

					if (vertexStream.Colors2?.Length > 0)
					{
						BindBuffer(BufferTarget.ArrayBuffer, _vboIds[VBO_COLOR2]);
						BufferData(BufferTarget.ArrayBuffer,
							4 * sizeof(byte) * vertexStream.Colors2.Length,
							vertexStream.Colors2,
							BufferUsageHint.StaticDraw);
						VertexAttribPointer(VBO_COLOR2, 4, VertexAttribPointerType.UnsignedByte, true, 0, 0);
						EnableVertexAttribArray(VBO_COLOR2);
					}

					if (vertexStream.BoneIndices?.Length > 0)
					{
						BindBuffer(BufferTarget.ArrayBuffer, _vboIds[VBO_BONEID]);
						BufferData(BufferTarget.ArrayBuffer,
							4 * sizeof(byte) * vertexStream.BoneIndices.Length,
							vertexStream.BoneIndices,
							BufferUsageHint.StaticDraw);
						VertexAttribPointer(VBO_BONEID, 4, VertexAttribPointerType.UnsignedByte, false, 0, 0);
						EnableVertexAttribArray(VBO_BONEID);
					}

					if (vertexStream.BoneWeights?.Length > 0)
					{
						BindBuffer(BufferTarget.ArrayBuffer, _vboIds[VBO_BONEWEIGHT]);
						BufferData(BufferTarget.ArrayBuffer,
							4 * sizeof(byte) * vertexStream.BoneWeights.Length,
							vertexStream.BoneWeights,
							BufferUsageHint.StaticDraw);
						VertexAttribPointer(VBO_BONEWEIGHT, 4, VertexAttribPointerType.UnsignedByte, true, 0, 0);
						EnableVertexAttribArray(VBO_BONEWEIGHT);
					}

					BindVertexArray(0);
				}
				else if (editData != null)
				{
					_vaoId = GenVertexArray();
					BindVertexArray(_vaoId);
					_vertexCount = editData.Faces.Length * 3;
					_vboIds = new int[3];
					GenBuffers(3, _vboIds);

					BindBuffer(BufferTarget.ElementArrayBuffer, _vboIds[0]);
					BufferData(BufferTarget.ElementArrayBuffer,
						3 * sizeof(uint) * editData.Faces.Length,
						editData.Faces,
						BufferUsageHint.StaticDraw);

					var posArray = new Vector4[editData.Vertices.Length];
					for (var i = 0; i < editData.Vertices.Length; i++)
					{
						posArray[i] = editData.Positions[editData.Vertices[i].PositionIndex];
					}

					BindBuffer(BufferTarget.ArrayBuffer, _vboIds[1]);
					BufferData(BufferTarget.ArrayBuffer,
						4 * sizeof(float) * posArray.Length,
						posArray,
						BufferUsageHint.StaticDraw);
					VertexAttribPointer(VBO_POSITION, 3, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
					EnableVertexAttribArray(VBO_POSITION);

					var size = Marshal.SizeOf<MeshEditData.Vertex>();
					BindBuffer(BufferTarget.ArrayBuffer, _vboIds[2]);
					BufferData(BufferTarget.ArrayBuffer,
						size * editData.Vertices.Length,
						editData.Vertices,
						BufferUsageHint.StaticDraw);
					VertexAttribPointer(VBO_NORMAL, 3, VertexAttribPointerType.Float, false, size, 4);
					VertexAttribPointer(VBO_UV, 2, VertexAttribPointerType.Float, false, size, 68);
					VertexAttribPointer(VBO_COLOR, 4, VertexAttribPointerType.UnsignedByte, true, size, 84);
					VertexAttribPointer(VBO_COLOR2, 4, VertexAttribPointerType.UnsignedByte, true, size, 88);
					EnableVertexAttribArray(VBO_NORMAL);
					EnableVertexAttribArray(VBO_UV);
					EnableVertexAttribArray(VBO_COLOR);
					EnableVertexAttribArray(VBO_COLOR2);

					BindVertexArray(0);
				}
			}

			public OglMesh(int[] indices, float[] positions, 
				float[] normals = null, float[] uvs = null)
			{
				_vaoId = GenVertexArray();
				BindVertexArray(_vaoId);

				_vertexCount = indices.Length;
				_vboIds = new int[4];
				GenBuffers(4, _vboIds);

				BindBuffer(BufferTarget.ElementArrayBuffer, _vboIds[0]);
				BufferData(BufferTarget.ElementArrayBuffer,
					sizeof(uint) * indices.Length,
					indices,
					BufferUsageHint.StaticDraw);

				BindBuffer(BufferTarget.ArrayBuffer, _vboIds[1]);
				BufferData(BufferTarget.ArrayBuffer,
					sizeof(float) * positions.Length,
					positions,
					BufferUsageHint.StaticDraw);
				VertexAttribPointer(VBO_POSITION, 3, VertexAttribPointerType.Float, false, 0, 0);
				EnableVertexAttribArray(VBO_POSITION);

				if (normals != null)
				{
					BindBuffer(BufferTarget.ArrayBuffer, _vboIds[2]);
					BufferData(BufferTarget.ArrayBuffer,
						sizeof(float) * normals.Length,
						normals,
						BufferUsageHint.StaticDraw);
					VertexAttribPointer(VBO_NORMAL, 3, VertexAttribPointerType.Float, false, 0, 0);
					EnableVertexAttribArray(VBO_NORMAL);
				}

				if (uvs != null)
				{
					BindBuffer(BufferTarget.ArrayBuffer, _vboIds[3]);
					BufferData(BufferTarget.ArrayBuffer,
						sizeof(float) * uvs.Length,
						uvs,
						BufferUsageHint.StaticDraw);
					VertexAttribPointer(VBO_UV, 2, VertexAttribPointerType.Float, false, 0, 0);
					EnableVertexAttribArray(VBO_UV);
				}
				
				BindVertexArray(0);
			}

			public void Draw()
			{
				if (_vaoId >= 0)
				{
					BindVertexArray(_vaoId);

					DrawElements(BeginMode.Triangles, _vertexCount, DrawElementsType.UnsignedInt, 0);

					BindVertexArray(0);
				}
			}

			public void DrawInstanced(int instance)
			{
				if (_vaoId >= 0)
				{
					BindVertexArray(_vaoId);

					DrawElementsInstanced(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, instance);

					BindVertexArray(0);
				}
			}

			public void Release()
			{
				if (_vaoId >= 0)
				{
					DeleteVertexArray(_vaoId);
					_vaoId = 0;
				}

				if (_vboIds != null)
				{
					DeleteBuffers(_vboIds.Length, _vboIds);
					_vboIds = null;
				}
			}
		}
	}
}