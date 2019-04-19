namespace Net7MultiClientUnlocker.Domain
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Net7MultiClientUnlocker.Framework;

    public class PresetGroups : List<PresetGroup>
    {
        public static PresetGroups FromContext(NotifyingDataContext dataContext)
        {
            if (dataContext == null)
            {
                return new PresetGroups();
            }

            var groups = dataContext[DataPath.PresetGroups] as ObservableCollection<NotifyingDataContext>;
            if (groups == null)
            {
                return new PresetGroups();
            }

            var presetGroups = new PresetGroups();
            presetGroups.AddRange(groups.Select(PresetGroup.FromContext));
            return presetGroups;
        }
    }
}
