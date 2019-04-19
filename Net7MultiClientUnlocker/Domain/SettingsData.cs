namespace Net7MultiClientUnlocker.Domain
{
    public class SettingsData
    {
        public SettingsData()
        {
            this.PresetGroups = new PresetGroups();
            this.MainWindowInfo = new MainWindowInfo();
        }

        public PresetGroups PresetGroups { get; set; }

        public bool AutoAssignPresets { get; set; }

        public bool RemoveMutexLock { get; set; }

        public bool AutoAcceptTOS { get; set; }

        public bool AutoInterruptSizzle { get; set; }

        public MainWindowInfo MainWindowInfo { get; set; }

        public string CurrentPresetGroup { get; set; }

        public string PathToNet7Launcher { get; set; }
    }
}
