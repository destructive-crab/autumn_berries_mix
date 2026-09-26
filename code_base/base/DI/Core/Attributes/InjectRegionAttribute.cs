using System;

namespace destruct.DI
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InjectRegionAttribute : Attribute
    {
        public InjectRegion Region;

        public InjectRegionAttribute(InjectRegion region)
        {
            Region = region;
        }
    }
}