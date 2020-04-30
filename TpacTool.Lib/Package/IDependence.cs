using System;

namespace TpacTool.Lib
{
	public interface IDependence
	{
		Guid Guid { get; }

		bool Invalid { get; }

		string Name { get; }
	}
}