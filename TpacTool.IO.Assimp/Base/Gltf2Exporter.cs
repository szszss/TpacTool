using Assimp;

namespace TpacTool.IO.Assimp
{
	public class Gltf2Exporter : AbstractAssimpExporter
	{
		public override string AssimpFormatId => "gltf2";

		public override string Extension => "gltf";

		public override bool SupportsSecondMaterial => false;

		public override bool SupportsSecondUv => false;

		public override bool SupportsSecondColor => false;

		public override bool SupportsSkeleton => true;

		public override bool SupportsMorph => true;

		public override bool SupportsSkeletalAnimation => true;

		public override bool SupportMorphAnimation => false;

		public override bool SupportTRSInAnimation => false;

		protected override void SetupScene(Scene scene)
		{
			// fix a bug of gltf2 exporter
			foreach (var mesh in scene.Meshes)
			{
				foreach (var bone in mesh.Bones)
				{
					if (!bone.HasVertexWeights && mesh.VertexCount > 0)
					{
						bone.VertexWeights.Add(new VertexWeight(0, 0));
					}
				}
			}
		}
	}
}