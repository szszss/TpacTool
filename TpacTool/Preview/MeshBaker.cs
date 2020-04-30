using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using TpacTool.Lib;

namespace TpacTool
{
	public static class MeshBaker
	{
		public static GeometryModel3D BakeMesh(Mesh mesh, bool hasMaterial)
		{
			var data = mesh.VertexStream.Data;
			var gm3d = new GeometryModel3D();
			var mesh3d = new MeshGeometry3D();
			var mat = new DiffuseMaterial(new SolidColorBrush(Colors.Gainsboro));

			var positions = new Point3DCollection(mesh.VertexCount);
			foreach (var position in data.Positions)
			{
				positions.Add(new Point3D(position.X, position.Y, position.Z));
			}
			mesh3d.Positions = positions;

			var normals = new Vector3DCollection(mesh.VertexCount);
			foreach (var normal in data.Normals)
			{
				normals.Add(new Vector3D(normal.X, normal.Y, normal.Z));
			}
			mesh3d.Normals = normals;

			var uvs = new PointCollection(mesh.VertexCount);
			foreach (var uv in data.Uv1)
			{
				uvs.Add(new Point(uv.X, uv.Y));
			}
			mesh3d.TextureCoordinates = uvs;

			var indices = new Int32Collection(mesh.FaceCount * 3);
			foreach (var index in data.Indices)
			{
				indices.Add(index);
			}
			mesh3d.TriangleIndices = indices;

			gm3d.Geometry = mesh3d;
			gm3d.Material = mat;
			gm3d.Freeze();
			return gm3d;
		}
	}
}