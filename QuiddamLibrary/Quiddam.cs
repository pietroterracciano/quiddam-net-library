using System;
using System.Collections.Generic;
using System.Threading;

namespace QuiddamLibrary
{
    public static class Quiddam
    {
        private static readonly Mutex _oMutex = new Mutex(false, nameof(Quiddam));
        private static readonly ReaderWriterLockSlim _oRWSlimLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<String, QuiddamPool> _dPoolsNames2QuiddamPools = new Dictionary<String, QuiddamPool>();

        public static class Config
        {
            public static Boolean IsProcessSafe = false;
            public static Boolean IsThreadSafe = false;
            public static Int32 ThreadSafeLockTimeout = 10000;
        }

        public static QuiddamPool CreatePool(String sQPoolName)
        {
            Boolean bIsThreadSafe = Config.IsThreadSafe;
            Int32 i32ThreadSafeLockTimeout = Config.ThreadSafeLockTimeout;

            QuiddamPool oQuiddamPool = null;

            if (String.IsNullOrWhiteSpace(sQPoolName = SanitizePoolName(sQPoolName))) goto END_METHOD;

            if(bIsThreadSafe)
                try { if (!_oRWSlimLock.TryEnterWriteLock(i32ThreadSafeLockTimeout)) goto END_METHOD; }
                catch { goto END_METHOD; }

            if (!_dPoolsNames2QuiddamPools.ContainsKey(sQPoolName))
                _dPoolsNames2QuiddamPools[sQPoolName] = oQuiddamPool = new QuiddamPool(sQPoolName);

            if (bIsThreadSafe)
                try { _oRWSlimLock.ExitWriteLock(); } catch { }

            END_METHOD:

            return oQuiddamPool;
        }

        public static QuiddamPool GetPool(String sPoolName)
        {
            Boolean bIsThreadSafe = Config.IsThreadSafe;
            Int32 i32ThreadSafeLockTimeout = Config.ThreadSafeLockTimeout;

            QuiddamPool oQuiddamPool = null;

            if (String.IsNullOrWhiteSpace(sPoolName = SanitizePoolName(sPoolName))) goto END_METHOD;

            if (bIsThreadSafe)
                try { if (!_oRWSlimLock.TryEnterWriteLock(i32ThreadSafeLockTimeout)) goto END_METHOD; }
                catch { goto END_METHOD; }

            _dPoolsNames2QuiddamPools.TryGetValue(sPoolName, out oQuiddamPool);

            if (bIsThreadSafe)
                try { _oRWSlimLock.ExitReadLock(); } catch { }

            END_METHOD:

            return oQuiddamPool;
        }

        public static Boolean DeletePool(String sPoolName)
        {
            Boolean bIsQuiddamPoolDeleted = false;
            Boolean bIsThreadSafe = Config.IsThreadSafe;
            Int32 i32ThreadSafeLockTimeout = Config.ThreadSafeLockTimeout;

            if (String.IsNullOrWhiteSpace(sPoolName = SanitizePoolName(sPoolName))) goto END_METHOD;

            if (bIsThreadSafe)
                try { if (!_oRWSlimLock.TryEnterWriteLock(i32ThreadSafeLockTimeout)) goto END_METHOD; }
                catch { goto END_METHOD; }

            bIsQuiddamPoolDeleted = _dPoolsNames2QuiddamPools.Remove(sPoolName);

            if (bIsThreadSafe)
                try { _oRWSlimLock.ExitWriteLock(); } catch { }

            END_METHOD:
            
            return bIsQuiddamPoolDeleted;
        }

        private static String SanitizePoolName(String sPoolName)
        {
            return !String.IsNullOrWhiteSpace(sPoolName) ? sPoolName.ToUpper().Trim() : null;
        }
    }
}