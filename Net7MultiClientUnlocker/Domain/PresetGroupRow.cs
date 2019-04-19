namespace Net7MultiClientUnlocker.Domain
{
    using System;
    using System.Collections.Generic;
    using Net7MultiClientUnlocker.Framework;

    public class PresetGroupRow
    {
        public ProcessInfo Process { get; set; }

        public ProcessInfo PreviousProcess { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public string AccountName { get; set; }

        public string Password { get; set; }

        public static PresetGroupRow FromContext(NotifyingDataContext dataContext)
        {
            var process = dataContext["Process"] as NotifyingDataContext;
            var previousProcess = dataContext["PreviousProcess"] as NotifyingDataContext;

            var presetGroupRow = new PresetGroupRow
            {
                PreviousProcess = new ProcessInfo { Id = 0 },
                Process = new ProcessInfo { Id = 0 }
            };

            if (process != null)
            {
                presetGroupRow.Process.Id = Convert.ToInt32(process["Id"]);
            }

            if (previousProcess != null)
            {
                presetGroupRow.PreviousProcess.Id = Convert.ToInt32(previousProcess["Id"]);
            }

            presetGroupRow.Left = Convert.ToInt32(dataContext["Left"]);
            presetGroupRow.Top = Convert.ToInt32(dataContext["Top"]);
            presetGroupRow.AccountName = Convert.ToString(dataContext["AccountName"]);
            presetGroupRow.Password = Convert.ToString(dataContext["Password"]);

            return presetGroupRow;
        }

        public NotifyingDataContext ToContext()
        {
            var data = new Dictionary<string, object>
            {
                ["PreviousProcess"] = PreviousProcess.ToContext(),
                ["Process"] = Process.ToContext(),
                ["Left"] = Left,
                ["Top"] = Top,
                ["AccountName"] = AccountName,
                ["Password"] = Password
            };

            return new NotifyingDataContext(data);
        }
    }
}
