namespace Net7MultiClientUnlocker.Domain
{
    using System.Collections.ObjectModel;

    public class PresetGroupRows
    {
        public PresetGroupRows()
        {
            this.Rows = new ObservableCollection<PresetGroupRow>();
        }

        public ObservableCollection<PresetGroupRow> Rows { get; set; }
    }
}
