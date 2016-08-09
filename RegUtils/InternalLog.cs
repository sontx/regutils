#if DEBUG
using System;
using System.Diagnostics;

namespace In.Sontx.RegUtils
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