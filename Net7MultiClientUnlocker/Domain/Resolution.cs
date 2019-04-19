namespace Net7MultiClientUnlocker.Domain
{
    public class Resolution
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public string DisplayText => this.Width + " x " + this.Height;
    }
}
