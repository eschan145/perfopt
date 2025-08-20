using Microsoft.UI.Windowing;
using Windows.Foundation;
using Windows.Graphics;

namespace perfopt
{
    public partial class PopupWindow : Window
    {
        public PopupWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(MainFrame);

            if (AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
                presenter.SetBorderAndTitleBar(false, false);
            }

            this.MainFrame.Navigate(typeof(MainPage));
            var content = this.MainFrame.Content as FrameworkElement;

            if (content != null)
            {
                content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                content.Arrange(new Rect(0, 0, content.DesiredSize.Width, content.DesiredSize.Height));

                int width = (int)(content.ActualWidth + 10);
                int height = (int)(content.ActualHeight + 10);

                AppWindow.Resize(new SizeInt32(width, height));
            }

            this.Activated += (s, e) =>
            {
                this.Content.KeyDown += (sender, args) =>
                {
                    if (args.Key == Windows.System.VirtualKey.Escape)
                    {
                        var window = new MainWindow();
                        window.Activate();
                        this.Close();
                    }
                };
            };

            AppWindow.Changed += (s, e) =>
            {
                var presenter = (OverlappedPresenter)AppWindow.Presenter;
                if (presenter.State == OverlappedPresenterState.Maximized)
                    presenter.Restore();
            };
        }
    }
}
