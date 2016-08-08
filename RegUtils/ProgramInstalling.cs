using Microsoft.Win32;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace RegUtils
{
    public static class ProgramInstalling
    {
        private const string UNPATH_32 = "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
        private const string UNPATH_64 = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
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
            info.InstallDate = DateTime.ParseExact(key.GetValue("InstallDate", TODAY).ToString(), DATE_KEY_FORMAT, EN_US);
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

        #endregion
    }
}
