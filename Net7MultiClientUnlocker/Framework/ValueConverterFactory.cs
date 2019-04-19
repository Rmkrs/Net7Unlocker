namespace Net7MultiClientUnlocker.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Data;

    using Net7MultiClientUnlocker.Domain;

    public static class ValueConverterFactory
    {
        private static readonly Dictionary<TypeConverter, IValueConverter> Converters = new Dictionary<TypeConverter, IValueConverter>();

        public static IValueConverter Create(TypeConverter typeConverter)
        {
            if (!Converters.ContainsKey(typeConverter))
            {
                Converters.Add(typeConverter, GetValueConverter(typeConverter));
            }

            return Converters[typeConverter];
        }

        private static IValueConverter GetValueConverter(TypeConverter typeConverter)
        {
            switch (typeConverter)
            {
                case TypeConverter.None:
                    return null;
                case TypeConverter.Integer:
                    return new IntValueConverter();
                case TypeConverter.BooleanToVisibility:
                    return new BooleanToVisibilityConverter();
                case TypeConverter.ObjectNullToBool:
                    return new ObjectNullToBoolConverter();
                case TypeConverter.ProcessSelected:
                    return new ProcessSelectedConverter();
                case TypeConverter.ContainsAny:
                    return new ContainsAnyConverter();
                case TypeConverter.ContainsAnyToVisibility:
                    return new ContainsAnyToVisibilityConverter();
                case TypeConverter.ObjectToObject:
                    return new ObjectToObjectConverter();
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeConverter));
            }
        }
    }
}
