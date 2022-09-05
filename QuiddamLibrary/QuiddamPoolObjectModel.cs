using Newtonsoft.Json;
using System;

namespace QuiddamLibrary
{
    public sealed class QuiddamPoolObjectModel
    {
        private DateTime? m_dtExpiringDate;

        public UInt32 LifetimeInSeconds { get; private set; }
        public DateTime CreationDate { get; private set; }
        public DateTime? LastEditingDate { get; private set; }
        public Object WrappedObject { get; private set; }
        public String Name { get; private set; }

        public QuiddamPoolObjectModel(String sName, Object oObjectToWrap, UInt32 ui32LifetimeInSeconds)
        {
            Name = !String.IsNullOrWhiteSpace(sName) ? sName.ToUpper().Trim() : "";
            CreationDate = DateTime.UtcNow;
            WrappedObject = oObjectToWrap;
        }

        public void Edit(Object oObjectToWrap, UInt32 ui32LifetimeInSeconds)
        {
            WrappedObject = oObjectToWrap;
            LastEditingDate = DateTime.UtcNow;
            if (LifetimeInSeconds > 0)
                m_dtExpiringDate = LastEditingDate.Value.AddSeconds(LifetimeInSeconds);
            else
                m_dtExpiringDate = null;
        }

        public Boolean IsExpired()
        {
            return
                m_dtExpiringDate != null
                    ? (DateTime.UtcNow - m_dtExpiringDate.Value).TotalSeconds > 0
                    : false;
        }
    }
}