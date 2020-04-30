using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;

namespace TpacTool.Lib
{
	public class AssetManager : IDependenceResolver
	{
		[CanBeNull]
		public DirectoryInfo WorkDir { set; get; }

		private List<AssetPackage> _loadedPackages;

		private List<AssetItem> _loadedAssets;

		private Dictionary<Guid, AssetPackage> _packageLookup;

		private Dictionary<Guid, AssetItem> _assetLookup;

#if NET40
		public List<AssetPackage> LoadedPackages { private set; get; }

		public List<AssetItem> LoadedAssets { private set; get; }
#else
		public IReadOnlyList<AssetPackage> LoadedPackages { private set; get; }

		public IReadOnlyList<AssetItem> LoadedAssets { private set; get; }
#endif

		//public Dictionary<Guid, AbstractExternalLoader> FixedExternalData { private set; get; }

		public delegate void ProgressCallback(int package, int packageCount, string fileName, bool completed);

		public AssetManager()
		{
			_loadedPackages = new List<AssetPackage>();
			_loadedAssets = new List<AssetItem>();
			_packageLookup = new Dictionary<Guid, AssetPackage>();
			_assetLookup = new Dictionary<Guid, AssetItem>();
#if NET40
			LoadedPackages = _loadedPackages;
			LoadedAssets = _loadedAssets;
#else
			LoadedPackages = _loadedPackages.AsReadOnly();
			LoadedAssets = _loadedAssets.AsReadOnly();
#endif
			//FixedExternalData = new Dictionary<Guid, AbstractExternalLoader>();
		}

	public virtual void Load(DirectoryInfo assetDir)
		{
			if (!assetDir.Exists)
				throw new FileNotFoundException("Asser folder not exists: " + assetDir.FullName);
			WorkDir = assetDir;
			LoadDo();
		}

		public virtual void Load(DirectoryInfo assetDir, ProgressCallback callback)
		{
			if (!assetDir.Exists)
				throw new FileNotFoundException("Asser folder not exists: " + assetDir.FullName);
			WorkDir = assetDir;
#if NETSTANDARD1_3
			LoadDo(callback);
#else
			Thread thread = new Thread(() =>
			{
				LoadDo(callback);
			});
			thread.Name = "Asset Loader";
			thread.IsBackground = true;
			thread.Start();
#endif
		}

		protected virtual void LoadDo(ProgressCallback callback = null)
		{
			bool reportProgress = callback != null;
			var files = WorkDir.EnumerateFiles("*.tpac", SearchOption.TopDirectoryOnly).ToList();
			var packageCount = files.Count;
			int i = 0;
			foreach (var file in files)
			{
				if (reportProgress)
					callback(i++, packageCount, file.Name, false);
				var package = new AssetPackage(file.FullName);
				package.IsGuidLocked = true;
				_loadedPackages.Add(package);
				_packageLookup[package.Guid] = package;
				foreach (var assetItem in package.Items)
				{
					_assetLookup[assetItem.Guid] = assetItem;
					_loadedAssets.Add(assetItem);
				}
			}
			if (reportProgress)
				callback(packageCount, packageCount, String.Empty, true);
		}

		public object this[Guid guid]
		{
			get
			{
				if (_packageLookup.TryGetValue(guid, out var result1))
					return result1;
				if (_assetLookup.TryGetValue(guid, out var result2))
					return result2;
				//if (FixedExternalData.TryGetValue(guid, out var result3))
				//	return result3;
				return null;
			}
		}

		public AssetPackage GetPackage(Guid guid)
		{
			_packageLookup.TryGetValue(guid, out var result);
			return result;
		}

		[Obsolete] // WIP. adding assert lookup is not finished yet
		public void AddPackage(AssetPackage package)
		{
			if (package.IsGuidLocked)
				throw new ArgumentException("The guid of adding package is locked. That is unexpected. " +
									"It can be caused by adding an existed package to its owner manager", "package");
			if (_packageLookup.ContainsKey(package.Guid))
				throw new ArgumentException("The guid is already occupied: " + package.Guid, "package");
			if (package.Guid == Guid.Empty)
				throw new ArgumentException("The guid cannot be empty", "package");
			_loadedPackages.Add(package);
			_packageLookup[package.Guid] = package;
			package.IsGuidLocked = true;
		}

		[Obsolete] // WIP. removing assert lookup is not finished yet
		public bool RemovePackage(Guid guid, out AssetPackage removedPackage)
		{
			if (_packageLookup.TryGetValue(guid, out removedPackage))
			{
				_packageLookup.Remove(guid);
				_loadedPackages.Remove(removedPackage);
				return true;
			}

			return false;
		}

		public AssetItem GetAsset(Guid guid)
		{
			_assetLookup.TryGetValue(guid, out var result);
			return result;
		}

		public T GetAsset<T>(Guid guid) where T : AssetItem
		{
			_assetLookup.TryGetValue(guid, out var result);
			var result2 = result as T;
			return result2;
		}

		bool IDependenceResolver.Resolve<T>(Guid guid, string name, out T result)
		{
			if (guid == Guid.Empty)
			{
				result = null;
				return false;
			}

			if (_assetLookup.TryGetValue(guid, out var res))
			{
				result = res as T;
				return result != null;
			}

			result = null;
			return false;
		}

		public void SetAsDefaultGlobalResolver()
		{
			DefaultDependenceResolver.Instance = this;
		}

		/*public AbstractExternalLoader GetData(Guid guid)
		{
			FixedExternalData.TryGetValue(guid, out var result);
			return result;
		}

		public T GetData<T>(Guid guid) where T : AbstractExternalLoader
		{
			FixedExternalData.TryGetValue(guid, out var result);
			var result2 = result as T;
			return result2;
		}*/

		public sealed class VolatileWork
		{
			public AssetPackage Package { set; get; }

			public List<AssetItem> Items { private set; get; }

			public VolatileWork()
			{
				Items = new List<AssetItem>();
			}
		}
	}
}