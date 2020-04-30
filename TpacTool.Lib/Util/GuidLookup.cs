using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace TpacTool.Lib
{
	public static class GuidLookup
	{
		private static ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		private static Dictionary<string, Guid> strToGuid = new Dictionary<string, Guid>();
		private static Dictionary<Guid, string> guidToStr = new Dictionary<Guid, string>();

		public static bool GetGuid(string name, out Guid result)
		{
			readWriteLock.EnterReadLock();
			try
			{
				return strToGuid.TryGetValue(name, out result);
			}
			finally
			{
				readWriteLock.ExitReadLock();
			}
		}

		public static bool GetName(Guid guid, out string result)
		{
			result = null;
			readWriteLock.EnterReadLock();
			try
			{
				return guidToStr.TryGetValue(guid, out result);
			}
			finally
			{
				readWriteLock.ExitReadLock();
			}
		}

		public static Guid Add(string name, Guid guid)
		{
			readWriteLock.EnterWriteLock();
			try
			{
				if (strToGuid.TryGetValue(name, out var result))
					return result;
				strToGuid[name] = guid;
				guidToStr[guid] = name;
				return guid;
			}
			finally
			{
				readWriteLock.ExitWriteLock();
			}
		}
	}
}