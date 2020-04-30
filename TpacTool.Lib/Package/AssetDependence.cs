using System;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public sealed class AssetDependence<T> where T : class, IDependence
	{
		public Guid Guid { set; get; }

		private string Name { set; get; }

		private T Item { set; get; }

		public AssetDependence()
		{
			Guid = Guid.Empty;
		}

		public AssetDependence(Guid guid)
		{
			Guid = guid;
		}

		public AssetDependence(T item)
		{
			Item = item;
			Name = item.Name;
			Guid = item.Guid;
		}

		public static AssetDependence<T> CreateEmpty()
		{
			return new AssetDependence<T>();
		}

		public bool IsEmpty()
		{
			return Guid == Guid.Empty;
		}

		public bool TryGetItem([CanBeNull] out T result)
		{
			if (DefaultDependenceResolver.Instance != null)
				return TryGetItem(DefaultDependenceResolver.Instance, out result);

			if (Item == null || Item.Invalid)
			{
				result = null;
				return false;
			}

			result = Item;
			return true;
		}

		public bool TryGetItem(IDependenceResolver resolver, out T result)
		{
			if (Item != null && !Item.Invalid)
			{
				result = Item;
				return true;
			}

			if (resolver.Resolve(Guid, Name, out T res))
			{
				Name = res.Name;
				result = Item = res;
				return true;
			}

			result = null;
			return false;
		}

		public T GetItem()
		{
			if (DefaultDependenceResolver.Instance != null)
				return GetItem(DefaultDependenceResolver.Instance);
			if (Item == null)
				throw new UnresolvedDependenceException<T>(this);
			if (Item.Invalid)
				throw new ExpiredDependenceException<T>(this);
			return Item;
		}

		public T GetItem(IDependenceResolver resolver)
		{
			if (Item != null && !Item.Invalid)
				return Item;
			if (resolver.Resolve(Guid, Name, out T res))
			{
				Name = res.Name;
				Item = res;
				return res;
			}
			throw new ResolveFailedException<T>(this);
		}
	}
}