namespace Net7MultiClientUnlocker.Framework
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    using Net7MultiClientUnlocker.Domain;

    public class ValidateBindingExtension : MarkupExtension
    {
        private Binding binding;

        public ValidateBindingExtension()
        {
            this.Initialize();
        }

        public string Path { get; set; }

        public DataPath DataContextPath
        {
            get => (DataPath)Enum.Parse(typeof(DataPath), this.Path);
            set => this.Path = this.GetPath(value.ToString(), true, false);
        }

        public DataPath GlobalContextPath
        {
            get => (DataPath)Enum.Parse(typeof(DataPath), this.Path);
            set => this.Path = this.GetPath(value.ToString(), true, true);
        }

        public TypeConverter Converter { get; set; }

        public string ElementName { get; set; }

        public bool UseRootDataContext { get; set; }

        public UpdateSourceTrigger UpdateSourceTrigger { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.UseRootDataContext)
            {
                // Append DataContext. before the path for easy reference in Xaml.
                this.binding.Path = new PropertyPath("DataContext." + this.Path);

                // Set Mode to look upwards (find the ancestor) and the type to Window (finds the first Window upwards in the Xaml tree).
                this.binding.RelativeSource = new RelativeSource { Mode = RelativeSourceMode.FindAncestor, AncestorType = typeof(Window) };
            }
            else
            {
                this.binding.Path = new PropertyPath(this.Path);

                // Setting ElementName while using RelativeSource will lead into an InvalidOperationException (Binding.RelativeSource cannot be set while using Binding.RelativeSource).
                this.binding.ElementName = this.ElementName;
            }

            this.binding.Converter = ValueConverterFactory.Create(this.Converter);

            this.binding.UpdateSourceTrigger = this.UpdateSourceTrigger;

            return this.binding.ProvideValue(serviceProvider);
        }

        private void Initialize()
        {
            this.UpdateSourceTrigger = UpdateSourceTrigger.Default;
            this.binding = new Binding { Mode = BindingMode.TwoWay, ValidatesOnDataErrors = true, NotifyOnValidationError = true };
        }

        private string GetPath(string pathToConvert, bool dataContextBinding, bool globalContextBinding)
        {
            var result = pathToConvert;

            if (dataContextBinding)
            {
                result = "[" + result + "]";
            }

            if (globalContextBinding)
            {
                result = "GlobalContext." + result;
            }

            return result;
        }
    }
}
