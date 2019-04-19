namespace Net7MultiClientUnlocker.Domain
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Net7MultiClientUnlocker.Framework;

    public class PresetGroup
    {
        public PresetGroup()
        {
            this.Rows = new List<PresetGroupRow>();
        }

        public string Name { get; set; }

        public List<PresetGroupRow> Rows { get; set; }

        public static PresetGroup FromContext(NotifyingDataContext dataContext)
        {
            if (dataContext == null)
            {
                return new PresetGroup { Rows = new List<PresetGroupRow>() };
            }

            var presetGroup = new PresetGroup
            {
                Name = dataContext["Name"] as string,
                Rows = new List<PresetGroupRow>()
            };

            var rows = dataContext["Rows"] as ObservableCollection<NotifyingDataContext>;
            if (rows == null)
            {
                return presetGroup;
            }

            foreach (var rowContext in rows)
            {
                presetGroup.Rows.Add(PresetGroupRow.FromContext(rowContext));
            }

            return presetGroup;
        }

        public NotifyingDataContext ToContext()
        {
            var dictionary = new Dictionary<string, object> { ["Name"] = Name };
            var rows = new ObservableCollection<NotifyingDataContext>();

            foreach (var presetGroupRow in this.Rows)
            {
                rows.Add(presetGroupRow.ToContext());
            }

            dictionary["Rows"] = rows;
            return new NotifyingDataContext(dictionary);
        }
    }
}
