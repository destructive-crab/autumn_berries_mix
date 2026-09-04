using System;

namespace destructive_code.DI
{
    [Flags]
    public enum InjectRegion
    {
        Fields = 1,
        Properties = 2,
        Methods = 4,
        All = Fields | Properties | Methods
    }
}