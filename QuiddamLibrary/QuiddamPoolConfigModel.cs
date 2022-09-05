using System;

namespace QuiddamLibrary
{
    public class QuiddamPoolConfigModel
    {
        public Boolean IsThreadSafe;
        public Int32 ThreadSafeLockTimeout;
        public Int64 MaxMemoryConsumption;

        public QuiddamPoolConfigModel()
        {
            IsThreadSafe = true;
            MaxMemoryConsumption = 0;
            ThreadSafeLockTimeout = 10000;
        }
    }
}