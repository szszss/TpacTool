using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public abstract class AbstractModelExporter
	{
		protected float ResizeFactor = 10f;

		protected static readonly Matrix4x4 NegYMatrix = Matrix4x4.CreateRotationZ((float) Math.PI);

		private static Regex _meshNameRegex = new Regex("\\.lod(\\d+)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public Skeleton Skeleton { set; get; }

		public Metamesh Model { set; get; }

		public SkeletalAnimation Animation { set; get; }

		public MorphAnimation Morph { set; get; }

		public bool FixBoneForBlender { set; get; } = true;

		public bool IsLargerSize { set; get; } = false;

		public bool IsNegYAxisForward { set; get; } = false;

		public bool IsYAxisUp { set; get; } = false;

		public bool IsDiffuseOnly { set; get; } = false;

		public int LodMask { set; get; } = 1;

		//public bool IgnoreMaterial { set; get; } = false;

		public Dictionary<Texture, string> TexturePathMapping { private set; get; }

		public abstract string Extension { get; }

		public abstract bool SupportsSecondMaterial { get; }

		public abstract bool SupportsSecondUv { get; }

		public abstract bool SupportsSecondColor { get; }

		public abstract bool SupportsSkeleton { get; }

		public abstract bool SupportsMorph { get; }

		public abstract bool SupportsSkeletalAnimation { get; }

		public abstract bool SupportMorphAnimation { get; }

		public virtual float AnimationFrameRate { set; get; } = 24f;

		public bool ForceExportWeight { set; get; } = false;

		protected bool IgnoreScale { get => Skeleton?.UserData != null && 
		                                    (Skeleton.UserData.Data.Usage == SkeletonUserData.USAGE_HUMAN ||
		                                     Skeleton.UserData.Data.Usage == SkeletonUserData.USAGE_HORSE);
		}

		protected AbstractModelExporter()
		{
			TexturePathMapping = new Dictionary<Texture, string>();
		}

		public virtual void Export(string path)
		{
			var parentPath = Directory.GetParent(path);
			if (parentPath != null)
				Directory.CreateDirectory(parentPath.FullName);
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
			out ISet<Material> materials, out ISet<Texture> textures)
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

		protected SortedDictionary<int, List<Mesh>> SortMeshesByLOD(IEnumerable<Mesh> meshes)
		{
			var result = new SortedDictionary<int, List<Mesh>>();

			foreach (var mesh in meshes)
			{
				var lod = 0;
				var matches = _meshNameRegex.Matches(mesh.Name);
				foreach (Match match in matches)
				{
					// matched 
					if (match.Success)
					{
						int.TryParse(match.Groups[1].Value, out lod);
					}
				}

				if (!result.TryGetValue(lod, out var list))
				{
					list = new List<Mesh>();
					result[lod] = list;
				}

				list.Add(mesh);
			}

			return result;
		}
	}
}