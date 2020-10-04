using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using SharpDX;
using TpacTool.Lib;
using BoundingBox = SharpDX.BoundingBox;
using Material = System.Windows.Media.Media3D.Material;
using Point = System.Windows.Point;

namespace TpacTool
{
	public static class MeshBaker
	{
		public static BakedMesh BakeMesh(Mesh mesh, bool hasMaterial)
		{
			var bakedMesh = new BakedMesh();
			var data = mesh.VertexStream.Data;
			var gm3d = new GeometryModel3D();
			var mesh3d = new MeshGeometry3D();
			MaterialGroup mat = new MaterialGroup();
			mat.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.Gainsboro)));
			mat.Children.Add(new SpecularMaterial());
			double massX = 0d, massY = 0d, massZ = 0d;
			float geoMinX = float.MaxValue, geoMinY = float.MaxValue, geoMinZ = float.MaxValue;
			float geoMaxX = float.MinValue, geoMaxY = float.MinValue, geoMaxZ = float.MinValue;
			int numVert = data.Positions.Length;

			var positions = new Point3DCollection(mesh.VertexCount);
			foreach (var position in data.Positions)
			{
				massX += position.X;
				massY += position.Y;
				massZ += position.Z;
				geoMinX = Math.Min(geoMinX, position.X);
				geoMinY = Math.Min(geoMinY, position.Y);
				geoMinZ = Math.Min(geoMinZ, position.Z);
				geoMaxX = Math.Max(geoMaxX, position.X);
				geoMaxY = Math.Max(geoMaxY, position.Y);
				geoMaxZ = Math.Max(geoMaxZ, position.Z);
				positions.Add(new Point3D(position.X, position.Y, position.Z));
			}

			if (numVert > 0)
			{
				massX /= numVert;
				massY /= numVert;
				massZ /= numVert;
			}
			else
			{
				geoMinX = geoMinY = geoMinZ = geoMaxX = geoMaxY = geoMaxZ = 0f;
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
			bakedMesh.Mesh = gm3d;
			bakedMesh.CenterOfMass = new Point3D(massX, massY, massZ);
			bakedMesh.BoundingBox = new BoundingBox(
				new Vector3(geoMinX, geoMinY, geoMinZ), 
				new Vector3(geoMaxX, geoMaxY, geoMaxZ));
			return bakedMesh;
		}

		public sealed class BakedMesh
		{
			public GeometryModel3D Mesh { internal set; get; }
			public Point3D CenterOfMass { internal set; get; }
			public BoundingBox BoundingBox { internal set; get; }
		}
	}
}