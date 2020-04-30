using System;

namespace TpacTool.Lib
{
	public class ExpiredDependenceException<T> : Exception where T : class, IDependence
	{
		public ExpiredDependenceException() : base()
		{

		}

		public ExpiredDependenceException(AssetDependence<T> dependence) : base()
		{
		}
	}
}