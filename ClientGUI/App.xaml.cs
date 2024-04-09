namespace ClientGUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        //Re write the window
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            const int newWidth = 755;
            const int newHeight = 930;
            window.Width = newWidth;
            window.Height = newHeight;
            return window;
        }
    }
}