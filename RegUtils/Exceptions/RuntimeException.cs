using System;

namespace In.Sontx.RegUtils.Exceptions
{
    public abstract class RuntimeException : Exception
    {
        public RuntimeException() { }
        public RuntimeException(string message) : base(message) { }
    }
}
