using System;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public interface IDependenceResolver
	{
		bool Resolve<T>(Guid guid, [CanBeNull] string name, out T result) where T : class, IDependence;
	}
}