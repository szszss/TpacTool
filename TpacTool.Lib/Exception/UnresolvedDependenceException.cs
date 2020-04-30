using System;

namespace TpacTool.Lib
{
	public class UnresolvedDependenceException<T> : Exception where T : class, IDependence
	{
		public UnresolvedDependenceException() : base()
		{

		}

		public UnresolvedDependenceException(AssetDependence<T> dependence) : base()
		{
		}
	}
}