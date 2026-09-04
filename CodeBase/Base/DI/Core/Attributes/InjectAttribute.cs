using System;

namespace destructive_code.DI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class InjectAttribute : Attribute
    {
    }
}