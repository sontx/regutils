using System;

namespace RegUtils
{
    public class UninstallInformation
    {
        public string DisplayName { get; set; }
        public string DisplayIcon { get; set; }
        public string DisplayVersion { get; set; }
        public string HelpLink { get; set; }
        public string Publisher { get; set; }
        public string UninstallString { get; set; }
        public string URLInfoAbout { get; set; }
        public string Comments { get; set; }
        public string Contact { get; set; }
        public DateTime InstallDate { get; set; }
        public string InstallLocation { get; set; }
        public int EstimateSize { get; set; }
    }
}
