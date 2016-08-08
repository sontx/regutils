using Microsoft.Win32;

namespace RegUtils
{
    public static class AutoBoot
    {
        private const string RUNPATH_32 = "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string RUNPATH_64 = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string RUNPATH_UN = RUNPATH_64;

        private static void EnableAutoBoot(RegistryKey hkey, string displayName, string executableFilePath)
        {
            hkey.SetValue(displayName, executableFilePath, RegistryValueKind.String);
        }

        private static void DisableAutoBoot(RegistryKey hkey, string displayName)
        {
            hkey.DeleteValue(displayName, false);
        }

        private static void EnableAutoBoot(RegistryKey hroot, string runKeyPath, string displayName, string executableFilePath)
        {
            using (var key = hroot.OpenSubKey(runKeyPath, true))
            {
                EnableAutoBoot(key, displayName, executableFilePath);
            }
        }

        private static void DisableAutoBoot(RegistryKey hroot, string runKeyPath, string displayName)
        {
            using (var key = hroot.OpenSubKey(runKeyPath, true))
            {
                DisableAutoBoot(key, displayName);
            }
        }

        public static void EnableAutoBoot(ProgramScope scope, Architecture architecture, string displayName, string executableFilePath)
        {
            switch (scope)
            {
                case ProgramScope.Machine:
                    switch (architecture)
                    {
                        case Architecture.x64:
                            EnableAutoBoot(Registry.LocalMachine, RUNPATH_64, displayName, executableFilePath);
                            break;
                        case Architecture.x86:
                            EnableAutoBoot(Registry.LocalMachine, RUNPATH_32, displayName, executableFilePath);
                            break;
                        case Architecture.Undefined:
                            EnableAutoBoot(Registry.LocalMachine, RUNPATH_UN, displayName, executableFilePath);
                            break;
                    }
                    break;
                case ProgramScope.User:
                    switch (architecture)
                    {
                        case Architecture.x64:
                            EnableAutoBoot(Registry.CurrentUser, RUNPATH_64, displayName, executableFilePath);
                            break;
                        case Architecture.x86:
                            EnableAutoBoot(Registry.CurrentUser, RUNPATH_32, displayName, executableFilePath);
                            break;
                        case Architecture.Undefined:
                            EnableAutoBoot(Registry.CurrentUser, RUNPATH_UN, displayName, executableFilePath);
                            break;
                    }
                    break;
            }
        }

        public static void DisableAutoBoot(ProgramScope scope, Architecture architecture, string displayName)
        {
            switch (scope)
            {
                case ProgramScope.Machine:
                    switch (architecture)
                    {
                        case Architecture.x64:
                            DisableAutoBoot(Registry.LocalMachine, RUNPATH_64, displayName);
                            break;
                        case Architecture.x86:
                            DisableAutoBoot(Registry.LocalMachine, RUNPATH_32, displayName);
                            break;
                        case Architecture.Undefined:
                            DisableAutoBoot(Registry.LocalMachine, RUNPATH_UN, displayName);
                            break;
                    }
                    break;
                case ProgramScope.User:
                    switch (architecture)
                    {
                        case Architecture.x64:
                            DisableAutoBoot(Registry.CurrentUser, RUNPATH_64, displayName);
                            break;
                        case Architecture.x86:
                            DisableAutoBoot(Registry.CurrentUser, RUNPATH_32, displayName);
                            break;
                        case Architecture.Undefined:
                            DisableAutoBoot(Registry.CurrentUser, RUNPATH_UN, displayName);
                            break;
                    }
                    break;
            }
        }

        public static void EnableAutoBoot(ProgramScope scope, string displayName, string executableFilePath)
        {
            EnableAutoBoot(scope, Architecture.x86, displayName, executableFilePath);
        }

        public static void DisableAutoBoot(ProgramScope scope, string displayName)
        {
            DisableAutoBoot(scope, Architecture.x86, displayName);
        }

        public static void EnableAutoBoot(string displayName, string executableFilePath)
        {
            EnableAutoBoot(ProgramScope.User, Architecture.Undefined, displayName, executableFilePath);
        }

        public static void DisableAutoBoot(string displayName)
        {
            DisableAutoBoot(ProgramScope.User, Architecture.Undefined, displayName);
        }
    }
}
