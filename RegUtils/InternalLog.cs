#if DEBUG
using System;
using System.Diagnostics;

namespace RegUtils
{
    internal static class InternalLog
    {
        public static void Log(Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
#endif