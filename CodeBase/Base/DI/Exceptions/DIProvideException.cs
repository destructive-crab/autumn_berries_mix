using System;

namespace destructive_code.DI
{
    public sealed class DIProvideException : Exception
    {
        public DIProvideException() : base()
        {
        }

        public DIProvideException(string message) : base(message)
        {
        }

        public DIProvideException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}