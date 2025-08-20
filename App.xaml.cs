using System.Runtime.InteropServices;

namespace perfopt
{
    public partial class App : Application
    {
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        public Window window;

        public App()
        {
            this.InitializeComponent();

            AllocConsole();

            this.UnhandledException += App_UnhandledException;

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Console.WriteLine($"AppDomain Unhandled Exception: {e.ExceptionObject}");
                Console.WriteLine("Press Enter to close...");
                Console.ReadLine();
            };
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            window = new PopupWindow();
            window.Activate();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"UI Thread Exception: {e.Exception}");
            Console.WriteLine("Press Enter to close...");
            Console.ReadLine();
            e.Handled = true;
        }
    }
}
