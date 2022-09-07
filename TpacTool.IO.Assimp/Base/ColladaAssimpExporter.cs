using System.IO;
using Assimp;

namespace TpacTool.IO.Assimp
{
	public class ColladaAssimpExporter : AbstractAssimpExporter
	{
		public override string AssimpFormatId => "collada";

		public override string Extension => "dae";

		public override bool SupportsSecondMaterial => true;

		public override bool SupportsSecondUv => true;

		public override bool SupportsSecondColor => true;

		public override bool SupportsSkeleton => true;

		public override bool SupportsMorph => true;

		public override bool SupportsSkeletalAnimation => true;

		public override bool SupportMorphAnimation => false;

		public override bool SupportTRSInAnimation => false;

		protected override void SetupScene(Scene scene)
		{
			base.SetupScene(scene);
			var rot = Matrix4x4.Identity;
			if (!IsYAxisUp)
				rot = new Matrix4x4(
						1, 0, 0, 0,
						0, 0, 1, 0,
						0, -1, 0, 0,
						0, 0, 0, 0);
			var scale = Matrix4x4.Identity;
			if (IsLargerSize)
				scale = Matrix4x4.FromScaling(new Vector3D(10, 10, 10));
			scene.RootNode.Transform = scale * rot;
		}
	}
}