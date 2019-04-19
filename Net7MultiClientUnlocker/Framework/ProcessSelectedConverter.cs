namespace Net7MultiClientUnlocker.Framework
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class ProcessSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var processContext = value as NotifyingDataContext;
            if (processContext == null)
            {
                return false;
            }

            return System.Convert.ToInt32(processContext["Id"]) > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
