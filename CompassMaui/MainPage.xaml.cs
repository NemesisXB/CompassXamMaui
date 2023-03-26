namespace CompassMaui;

public partial class MainPage : ContentPage
{

        MainPageViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new MainPageViewModel();
            _viewModel.CompassCanvas = compassCanvasView;
        }

        private void compassCanvasView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
        {
            _viewModel.OnCanvasViewPaintSurface(sender, e);
        }

        private void arrowCanvasView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
        {
            _viewModel.OnArrowCanvasViewPaintSurface(sender, e);
        }
}

