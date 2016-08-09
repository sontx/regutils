using Microsoft.Win32;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace In.Sontx.RegUtils
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

        public static List<RunInformation> GetRunInformations(RegistryKey hroot, string runKeyPath)
        {
            using (var hkey = hroot.OpenSubKey(runKeyPath, false))
            {
                var programNames = hkey.GetValueNames();
                var infos = new List<RunInformation>(programNames.Length);
                foreach (var programName in programNames)
                {
                    string executablePath = hkey.GetValue(programName, "").ToString();
                    RunInformation info = new RunInformation()
                    {
                        DisplayName = programName,
                        ExecutablePath = executablePath
                    };
                    infos.Add(info);
                }
                return infos;
            }
        }

        public static List<RunInformation> GetRunInformations(ProgramScope scope, Architecture architecture)
        {
            switch (scope)
            {
                case ProgramScope.Machine:
                    switch (architecture)
                    {
                        case Architecture.x64:
                            return GetRunInformations(Registry.LocalMachine, RUNPATH_64);
                        case Architecture.x86:
                            return GetRunInformations(Registry.LocalMachine, RUNPATH_32);
                        case Architecture.Undefined:
                            return GetRunInformations(Registry.LocalMachine, RUNPATH_UN);
                    }
                    break;
                case ProgramScope.User:
                    switch (architecture)
                    {
                        case Architecture.x64:
                            return GetRunInformations(Registry.CurrentUser, RUNPATH_64);
                        case Architecture.x86:
                            return GetRunInformations(Registry.CurrentUser, RUNPATH_32);
                        case Architecture.Undefined:
                            return GetRunInformations(Registry.CurrentUser, RUNPATH_UN);
                    }
                    break;
            }
            return null;
        }

        public static Task<List<RunInformation>> GetRunInformationsAsync(ProgramScope scope, Architecture architecture)
        {
            return Task.Run(() => { return GetRunInformations(scope, architecture); });
        }

        public static List<RunInformation> GetRunInformations()
        {
            return GetRunInformations(ProgramScope.User, Architecture.Undefined);
        }

        public static Task<List<RunInformation>> GetRunInformationsAsync()
        {
            return Task.Run(() => { return GetRunInformations(); });
        }
    }
}
