namespace Net7MultiClientUnlocker.Domain
{
    using System.Collections.Concurrent;
    using Net7MultiClientUnlocker.Framework;

    public class ProcessInfo
    {
        public int Id { get; set; }

        public NotifyingDataContext ToContext()
        {
            var data = new ConcurrentDictionary<string, object> { ["Id"] = Id };
            return new NotifyingDataContext(data);
        }
    }
}
