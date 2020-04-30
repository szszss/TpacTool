using System;
using System.IO;
using TpacTool.Lib;

namespace TpacTool.IO
{
	public abstract class AbstractTextureExporter
	{
		public Texture Texture { set; get; }

		public bool IgnoreMipmap { set; get; } = false;

		public bool IgnoreArray { set; get; } = false;

		public int SpecifyArray { set; get; } = -1;

		public abstract string Extension { get; }

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
			if (Texture == null)
				throw new ArgumentNullException("Texture");
			if (!Texture.HasPixelData)
				throw new ArgumentException("Texture " + Texture.Name + " has no pixel data");
		}
	}
}