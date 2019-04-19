namespace Net7MultiClientUnlocker.Domain
{
    using System;
    using System.IO;

    using Net7MultiClientUnlocker.Framework;

    public class Settings
    {
        private static readonly string LocalDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Net7Unlocker");

        private readonly string settingsPath;

        public Settings()
        {
            this.settingsPath = GetPathFor("settingsv2.xml");
        }

        public SettingsData Data { get; set; }

        public static string GetPathFor(string filename)
        {
            return Path.Combine(LocalDataFolder, filename);
        }

        public static void EnsurePath(string localFileName)
        {
            string directoryName = Path.GetDirectoryName(localFileName);

            if (directoryName == null)
            {
                return;
            }

            Directory.CreateDirectory(directoryName);
        }

        public void LoadSettings()
        {
            this.Data = Serializer.Deserialize<SettingsData>(this.settingsPath) ?? new SettingsData
            {
                AutoInterruptSizzle = true,
                AutoAssignPresets = true,
                AutoAcceptTOS = true,
                RemoveMutexLock = true
            };
        }

        public void SaveSettings()
        {
            Serializer.Serialize(this.Data, this.settingsPath);
        }
    }
}
