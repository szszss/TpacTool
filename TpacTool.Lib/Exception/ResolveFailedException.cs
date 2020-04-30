using System;

namespace TpacTool.Lib
{
	public class ResolveFailedException<T> : Exception where T : class, IDependence
	{
		public ResolveFailedException() : base()
		{

		}

		public ResolveFailedException(AssetDependence<T> dependence) : base()
		{
		}
	}
}