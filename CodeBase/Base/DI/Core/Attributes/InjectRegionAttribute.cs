using System;

namespace destructive_code.DI
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