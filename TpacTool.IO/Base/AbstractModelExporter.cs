using System;
using System.Collections.Generic;
using System.IO;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public abstract class AbstractModelExporter
	{
		protected float ResizeFactor = 10f;

		public Skeleton Skeleton { set; get; }

		public Metamesh Model { set; get; }

		public bool IsLargerSize { set; get; } = false;

		public bool IsYAxisUp { set; get; } = false;

		public bool IsDiffuseOnly { set; get; } = false;

		public int SelectedLod { set; get; } = 0;

		//public bool IgnoreMaterial { set; get; } = false;

		public Dictionary<Texture, string> TexturePathMapping { private set; get; }

		public abstract string Extension { get; }

		public abstract bool SupportsSecondMaterial { get; }

		public abstract bool SupportsSecondUv { get; }

		public abstract bool SupportsSecondColor { get; }

		public abstract bool SupportsSkeleton { get; }

		public abstract bool SupportsMorph { get; }

		protected AbstractModelExporter()
		{
			TexturePathMapping = new Dictionary<Texture, string>();
		}

		public virtual void Export(string path)
		{
			Directory.CreateDirectory(Directory.GetParent(path).FullName);
			using (var stream = File.Create(path, 4096))
			{
				Export(stream);
				stream.Flush();
			}
		}

		public abstract void Export(Stream writeStream);

		public void CheckStreamAndTexture(Stream stream)
		{
			if (!stream.CanWrite)
				throw new IOException("The output stream must support write");
			if (Model == null)
				throw new ArgumentNullException("Model");
		}

		public void CollectUniqueMaterialsAndTextures(List<Mesh> meshes, 
			out ISet<Material> materials, out ISet<Texture> textures, int lod = 0)
		{
			materials = new HashSet<Material>();
			textures = new HashSet<Texture>();
			foreach (var mesh in meshes)
			{
				if (mesh.Material.TryGetItem(out var mat1))
				{
					materials.Add(mat1);
					foreach (var texDep in mat1.Textures.Values)
					{
						if (texDep.TryGetItem(out var tex))
							textures.Add(tex);
					}
				}
				if (SupportsSecondMaterial && mesh.Material.TryGetItem(out var mat2))
				{
					materials.Add(mat2);
					foreach (var texDep in mat2.Textures.Values)
					{
						if (texDep.TryGetItem(out var tex))
							textures.Add(tex);
					}
				}
			}
		}
	}
}