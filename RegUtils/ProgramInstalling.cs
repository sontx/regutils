using Microsoft.Win32;
using RegUtils.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace RegUtils
{
    public static class ProgramInstalling
    {
        private const string UNPATH_32 = "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
        private const string UNPATH_64 = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
        private const string UNPATH_UN = UNPATH_64;
        private const string DATE_KEY_FORMAT = "yyyyMMdd";
        private static readonly string TODAY = DateTime.Now.ToString(DATE_KEY_FORMAT);
        private static readonly CultureInfo EN_US = new CultureInfo("en-US");

        #region Check Program Installed Yet

        private static bool CheckProgramInstalled(RegistryKey hkey, string displayNamePattern)
        {
            displayNamePattern = displayNamePattern.ToUpper();
            var subkeys = hkey.GetSubKeyNames();
            foreach (var key in subkeys)
            {
                if (key.ToUpper().Contains(displayNamePattern))
                    return true;
                using (RegistryKey subHKey = hkey.OpenSubKey(key, false))
                {
                    string displayName = subHKey.GetValue("DisplayName", "").ToString().ToUpper();
                    if (displayName.Contains(displayNamePattern))
                        return true;
                }
            }
            return false;
        }

        private static bool CheckProgramInstalled(string uninstallKeyPath, string displayNamePattern)
        {
            try
            {
                using (RegistryKey hkey = Registry.LocalMachine.OpenSubKey(uninstallKeyPath, false))
                {
                    if (CheckProgramInstalled(hkey, displayNamePattern))
                        return true;
                }
                using (RegistryKey hkey = Registry.CurrentUser.OpenSubKey(uninstallKeyPath, false))
                {
                    return CheckProgramInstalled(hkey, displayNamePattern);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                InternalLog.Log(ex);
#endif
            }
            return false;
        }

        public static bool CheckProgramInstalled(string displayNamePattern)
        {
            if (string.IsNullOrEmpty(displayNamePattern))
                throw new ArgumentNullException("displayNamePattern is null or empty.");
            return CheckProgramInstalled(UNPATH_32, displayNamePattern) ||
                   CheckProgramInstalled(UNPATH_64, displayNamePattern);
        }

        public static Task<bool> CheckProgramInstalledAsync(string displayNamePattern)
        {
            return Task.Run(() => { return CheckProgramInstalled(displayNamePattern); });
        }

        #endregion

        #region Get Uninstall Information

        private static UninstallInformation GetUninstallInformation(RegistryKey key)
        {
            var info = new UninstallInformation();
            info.DisplayName = key.GetValue("DisplayName", "Unknown").ToString();
            info.DisplayIcon = key.GetValue("DisplayIcon", "").ToString();
            info.DisplayVersion = key.GetValue("DisplayVersion", "Unknown").ToString();
            info.HelpLink = key.GetValue("HelpLink", "").ToString();
            info.Publisher = key.GetValue("Publisher", "Unknown").ToString();
            info.UninstallString = key.GetValue("UninstallString", "").ToString();
            info.URLInfoAbout = key.GetValue("URLInfoAbout", "").ToString();
            info.Comments = key.GetValue("Comments", "").ToString();
            info.Contact = key.GetValue("Contact", "").ToString();
            info.InstallLocation = key.GetValue("InstallLocation", "").ToString();
            info.EstimateSize = int.Parse(key.GetValue("EstimateSize", "0").ToString());

            string installDateString = key.GetValue("InstallDate", "").ToString();

            if (installDateString != "")
            {
                if (key.GetValueKind("InstallDate") == RegistryValueKind.String)
                {
                    DateTime installDate;
                    if (DateTime.TryParseExact(installDateString, DATE_KEY_FORMAT, EN_US, DateTimeStyles.NoCurrentDateDefault, out installDate))
                        info.InstallDate = installDate;
                    else
                        info.InstallDate = DateTime.Now;
                }
                else
                {
                    DateTime startDate = new DateTime(1970, 1, 1, 0, 0, 0);
                    Int64 regVal = Convert.ToInt64(installDateString);
                    info.InstallDate = startDate.AddSeconds(regVal);
                }
            }
            
            return info;
        }

        private static UninstallInformation GetUninstallInformation(RegistryKey hkey, string displayNamePattern)
        {
            displayNamePattern = displayNamePattern.ToUpper();
            var subkeys = hkey.GetSubKeyNames();
            string programKeyName = null;
            foreach (var key in subkeys)
            {
                if (key.ToUpper().Contains(displayNamePattern))
                {
                    programKeyName = key;
                    break;
                }
                using (RegistryKey subHKey = hkey.OpenSubKey(key, false))
                {
                    string displayName = subHKey.GetValue("DisplayName", "").ToString().ToUpper();
                    if (displayName.Contains(displayNamePattern))
                    {
                        programKeyName = key;
                        break;
                    }
                }
            }

            if (programKeyName == null)
                return null;
            using (RegistryKey key = hkey.OpenSubKey(programKeyName, false))
            {
                return GetUninstallInformation(key);
            }
        }

        private static UninstallInformation GetUninstallInformation(string uninstallKeyPath, string displayNamePattern)
        {
            try
            {
                using (RegistryKey hkey = Registry.LocalMachine.OpenSubKey(uninstallKeyPath, false))
                {
                    var info = GetUninstallInformation(hkey, displayNamePattern);
                    if (info != null)
                        return info;
                }
                using (RegistryKey hkey = Registry.CurrentUser.OpenSubKey(uninstallKeyPath, false))
                {
                    return GetUninstallInformation(hkey, displayNamePattern);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                InternalLog.Log(ex);
#endif
            }
            return null;
        }

        public static UninstallInformation GetUninstallInformation(string displayNamePattern)
        {
            var info = GetUninstallInformation(UNPATH_32, displayNamePattern);
            if (info != null)
                return info;
            return GetUninstallInformation(UNPATH_64, displayNamePattern);
        }

        public static Task<UninstallInformation> GetUninstallInformationAsync(string displayNamePattern)
        {
            return Task.Run(() => { return GetUninstallInformation(displayNamePattern); });
        }

        private static List<UninstallInformation> GetUninstallInformations(RegistryKey hroot, string keyPath)
        {
            using (var hkey = hroot.OpenSubKey(keyPath, false))
            {
                var programKeys = hkey.GetSubKeyNames();
                var infos = new List<UninstallInformation>(programKeys.Length);

                foreach (var programkey in programKeys)
                {
                    using (var key = hkey.OpenSubKey(programkey, false))
                    {
                        var info = GetUninstallInformation(key);
                        if (!string.IsNullOrEmpty(info.DisplayName))
                            infos.Add(info);
                    }
                }

                return infos;
            }
        }

        public static List<UninstallInformation> GetUninstallInformations(ProgramScope scope, Architecture architecture)
        {
            switch (scope)
            {
                case ProgramScope.Machine:
                    switch (architecture)
                    {
                        case Architecture.x64:
                            return GetUninstallInformations(Registry.LocalMachine, UNPATH_64);
                        case Architecture.x86:
                            return GetUninstallInformations(Registry.LocalMachine, UNPATH_32);
                        case Architecture.Undefined:
                            return GetUninstallInformations(Registry.LocalMachine, UNPATH_UN);
                    }
                    break;
                case ProgramScope.User:
                    switch (architecture)
                    {
                        case Architecture.x64:
                            return GetUninstallInformations(Registry.CurrentUser, UNPATH_64);
                        case Architecture.x86:
                            return GetUninstallInformations(Registry.CurrentUser, UNPATH_32);
                        case Architecture.Undefined:
                            return GetUninstallInformations(Registry.CurrentUser, UNPATH_UN);
                    }
                    break;
            }
            return null;
        }

        public static Task<List<UninstallInformation>> GetUninstallInformationsAsync(ProgramScope scope, Architecture architecture)
        {
            return Task.Run(() => { return GetUninstallInformations(scope, architecture); });
        }

        #endregion

        #region Uninstall Program

        public static void UninstallAndWait(UninstallInformation info)
        {
            if (info == null)
                throw new ArgumentNullException("info parameter is null.");
            if (string.IsNullOrEmpty(info.DisplayName) || string.IsNullOrEmpty(info.UninstallString))
                throw new ArgumentException("Display name or uninstall string is null or empty.");
            using(Process process = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(info.UninstallString);
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
        }

        public static void UninstallAndWait(string displayNamePattern)
        {
            var info = GetUninstallInformation(displayNamePattern);
            if (info == null)
                throw new ProgramNotFoundException(string.Format("Program '{0}' not found.", displayNamePattern));
            UninstallAndWait(info);
        }

        public static Task UninstallAsync(UninstallInformation info)
        {
            return Task.Run(() => { UninstallAndWait(info); });
        }

        public static Task UninstallAsync(string displayNamePattern)
        {
            return Task.Run(() => { UninstallAndWait(displayNamePattern); });
        }

        #endregion

    }
}
