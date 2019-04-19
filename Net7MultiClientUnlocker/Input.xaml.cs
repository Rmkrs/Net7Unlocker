namespace Net7MultiClientUnlocker
{
    using System.Windows;

    public partial class Input
    {
        public Input(Window parent, string title, string input)
        {
            this.Owner = parent;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.Loaded += this.OnWindowLoaded;
            InitializeComponent();
            this.TitleText = title;
            this.InputText = input;
        }

        public string TitleText
        {
            get => TitleTextBox.Text;
            set => TitleTextBox.Text = value;
        }

        public string InputText
        {
            get => InputTextBox.Text;
            set => InputTextBox.Text = value;
        }

        public bool Canceled { get; set; }

        private void ConfirmClick(object sender, RoutedEventArgs e)
        {
            this.Canceled = false;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Canceled = true;
            Close();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
        }
    }
}
