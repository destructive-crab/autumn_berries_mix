using System;

namespace destruct.DI
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ProvideAttribute : Attribute
    {
        public bool IsSingleton;

        public ProvideAttribute(bool isSingleton)
        {
            IsSingleton = isSingleton;
        }
    }
}