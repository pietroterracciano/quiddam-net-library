using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using static QuiddamLibrary.Quiddam;

namespace QuiddamLibrary
{
    public sealed class QuiddamPool
    {
        private readonly ReaderWriterLockSlim _oRWSlimLock;
        private Dictionary<String, QuiddamPoolObjectModel> _dKeysNames2QPObjects;

        public DateTime CreationDate { get; private set; }
        public DateTime? LastFlushingDate { get; private set; }
        public String Name { get; private set; }
        public QuiddamPoolConfigModel Config { get; private set; }

        public QuiddamPool(String sName)
        {
            Config = new QuiddamPoolConfigModel();
            LastFlushingDate = null;
            CreationDate = DateTime.UtcNow;
            Name = SanitizeKeyName(sName);
            if (String.IsNullOrWhiteSpace(Name)) Name = "";
            _oRWSlimLock = new ReaderWriterLockSlim();
            _dKeysNames2QPObjects = new Dictionary<String, QuiddamPoolObjectModel>();
        }

        public Boolean Flush()
        {
            Boolean bIsThreadSafe = Config.IsThreadSafe;
            Int32 i32ThreadSafeLockTimeout = Config.ThreadSafeLockTimeout;
            Boolean bIsFlushed = false;

            if (bIsThreadSafe)
                try { if (!_oRWSlimLock.TryEnterWriteLock(i32ThreadSafeLockTimeout)) goto END_METHOD; }
                catch { goto END_METHOD; }

            _dKeysNames2QPObjects = new Dictionary<String, QuiddamPoolObjectModel>();

            if (_dKeysNames2QPObjects.Count < 1)
            {
                bIsFlushed = true;
                LastFlushingDate = DateTime.UtcNow;
            }

            if (bIsThreadSafe)
                try { _oRWSlimLock.ExitWriteLock(); } catch { }

            END_METHOD:

            return bIsFlushed;
        }

        public Boolean AddEditObject(String sKeyName, Object oObjectToCache)
        {
            return AddEditObject(sKeyName, oObjectToCache, 0);
        }

        public Boolean AddEditObject(String sKeyName, Object oObjectToCache, UInt32 ui32LifetimeInSeconds)
        {
            Boolean bIsQuiddamPoolObjectAddEdit = false;
            Boolean bIsThreadSafe = Config.IsThreadSafe;
            Int32 i32ThreadSafeLockTimeout = Config.ThreadSafeLockTimeout;

            if (String.IsNullOrWhiteSpace(sKeyName = SanitizeKeyName(sKeyName))) goto END_METHOD;

            if(bIsThreadSafe)
                try { if (!_oRWSlimLock.TryEnterWriteLock(i32ThreadSafeLockTimeout)) goto END_METHOD; }
                catch { goto END_METHOD; }

            QuiddamPoolObjectModel oQPObject;
            _dKeysNames2QPObjects.TryGetValue(sKeyName, out oQPObject);

            if (oQPObject != null)
                _dKeysNames2QPObjects[sKeyName].Edit(oObjectToCache, ui32LifetimeInSeconds);
            else
                _dKeysNames2QPObjects[sKeyName] = new QuiddamPoolObjectModel(sKeyName, oObjectToCache, ui32LifetimeInSeconds);

            if (bIsThreadSafe)
                try { _oRWSlimLock.ExitWriteLock(); } catch { }

            END_METHOD:

            return bIsQuiddamPoolObjectAddEdit;
        }

        public Boolean GetObject<CachedObjectType>(String sKeyName, out CachedObjectType oCachedObject)
        {
            oCachedObject = default(CachedObjectType);
            Boolean bIsQPObjectExistent;
            try { bIsQPObjectExistent = GetObject(sKeyName, out oCachedObject); }
            catch { bIsQPObjectExistent = false; }
            return bIsQPObjectExistent;
        }

        public Boolean GetObject(String sKeyName, out Object oCachedObject)
        {
            oCachedObject = null;
            Boolean bIsThreadSafe = Config.IsThreadSafe, bIsQPObjectExistent = false;
            Int32 i32ThreadSafeLockTimeout = Config.ThreadSafeLockTimeout;

            if (String.IsNullOrWhiteSpace(sKeyName = SanitizeKeyName(sKeyName))) goto END_METHOD;

            if (bIsThreadSafe)
                try { if (!_oRWSlimLock.TryEnterReadLock(i32ThreadSafeLockTimeout)) goto END_METHOD; }
                catch { goto END_METHOD; }

            QuiddamPoolObjectModel oQPObject;
            Boolean bIsQPObjectIntoDictionary = _dKeysNames2QPObjects.TryGetValue(sKeyName, out oQPObject);

            if (!bIsQPObjectIntoDictionary)
                goto END_READER_WRITER_SLIM_LOCK;
            else if (oQPObject == null || oQPObject.IsExpired())
            {
                _dKeysNames2QPObjects.Remove(sKeyName);
                goto END_READER_WRITER_SLIM_LOCK;
            }

            bIsQPObjectExistent = true;
            oCachedObject = oQPObject.WrappedObject;

            END_READER_WRITER_SLIM_LOCK:

            if(bIsThreadSafe)
                try { _oRWSlimLock.ExitReadLock(); } catch { }

            END_METHOD:

            return bIsQPObjectExistent;
        }
        
        public Boolean RemoveObject(String sKeyName)
        {
            Boolean bIsThreadSafe = Config.IsThreadSafe;
            Int32 i32ThreadSafeLockTimeout = Config.ThreadSafeLockTimeout;
            Boolean bIsQPObjectRemoved = false;

            if (String.IsNullOrWhiteSpace(sKeyName = SanitizeKeyName(sKeyName))) goto END_METHOD;

            if (bIsThreadSafe)
                try { if (!_oRWSlimLock.TryEnterWriteLock(i32ThreadSafeLockTimeout)) goto END_METHOD; }
                catch { goto END_METHOD; }       

            bIsQPObjectRemoved = _dKeysNames2QPObjects.Remove(sKeyName);

            if (bIsThreadSafe)
                try { _oRWSlimLock.ExitWriteLock(); }
                catch { }

            END_METHOD:

            return bIsQPObjectRemoved;
        }
        
        public TimeSpan GetUpTime()
        {
            return DateTime.UtcNow - CreationDate;
        }

        private static String SanitizeKeyName(String sKeyName)
        {
            return !String.IsNullOrWhiteSpace(sKeyName) ? sKeyName.ToUpper().Trim() : null;
        }
        
    }
}