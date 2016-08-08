using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace RegUtils
{
    public static class ProgramInstalling
    {
        private const string UNPATH_32 = "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
        private const string UNPATH_64 = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";

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
    }
}
