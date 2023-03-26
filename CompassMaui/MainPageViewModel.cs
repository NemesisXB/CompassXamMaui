
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using Svg.Skia;

namespace CompassMaui
{
    public class MainPageViewModel : BaseViewModel
    {
        static SKSvg arrowSvg;
        static SKSvg compassSvg;
        static SKCanvasView compassCanvas;
        static double rawCompassReading = 0;
        static double compassReadingRounded = 0;

        public AsyncCommand AppearCommand { get; protected set; }
        public Command DisappearCommand { get; protected set; }

        public MainPageViewModel()
        {
            if (Compass.IsSupported)
            {
                Compass.ReadingChanged += Compass_ReadingChanged;
                CompassDetected = true;

                if (!Compass.IsMonitoring)
                {
                    Compass.Start(SensorSpeed.UI);
                }
            }

            AppearCommand = new AsyncCommand(async () =>
            {
                // Load compassSvg
                compassSvg = new();
                using (var stream = await GetImageStream("compass.svg"))
                {
                    compassSvg.Load(stream);
                }
                // Load arrowSvg
                arrowSvg = new();
                using (var stream = await GetImageStream("compass_arrow.svg"))
                {
                    arrowSvg.Load(stream);
                }

                _ = RotateCompass();
            });

            DisappearCommand = new Command(() =>
            {
                if (CompassDetected)
                {
                    try
                    {
                        Compass.Stop();
                    }
                    catch (Exception ex)
                    {
                    }
                }


                Compass.ReadingChanged -= Compass_ReadingChanged;
            });
        }

        private bool useCoriolis = false;
        public bool UseCoriolis
        {
            get => useCoriolis;
            set => SetProperty(ref useCoriolis, value);
        }

        private int azimuthUnitSelectedIndex = -1;
        public int AzimuthUnitSelectedIndex
        {
            get => azimuthUnitSelectedIndex;
            set => SetProperty(ref azimuthUnitSelectedIndex, value);
        }

        private static string azimuthEntry;
        public string AzimuthEntry
        {
            get => azimuthEntry;
            set => SetProperty(ref azimuthEntry, value);
        }

        private int latitudeUnitSelectedIndex = -1;
        public int LatitudeUnitSelectedIndex
        {
            get => latitudeUnitSelectedIndex;
            set => SetProperty(ref latitudeUnitSelectedIndex, value);
        }

        private string latitude;
        public string Latitude
        {
            get => latitude;
            set => SetProperty(ref latitude, value);
        }

        private bool gpsIsActivated = false;
        public bool GpsIsActivated
        {
            get => gpsIsActivated;
            set => SetProperty(ref gpsIsActivated, value);
        }

        private static bool compassDetected;
        public bool CompassDetected
        {
            get => compassDetected;
            set => SetProperty(ref compassDetected, value);
        }

        private string compassInstruction = "Point Arrow Towards Target";
        public string CompassInstruction
        {
            get => compassInstruction;
            set => SetProperty(ref compassInstruction, value);
        }

        public double CompassReading
        {
            get => compassReadingRounded;
            set
            {
                CompassReadingString = value.ToString();
                SetProperty(ref compassReadingRounded, value);
            }
        }

        private string compassReadingString;
        public string CompassReadingString
        {
            get => compassReadingString;
            set => SetProperty(ref compassReadingString, value);
        }

        public SKCanvasView CompassCanvas
        {
            get => compassCanvas;
            set { compassCanvas = value; }
        }

        private string okButtonText;
        public string OkButtonText
        {
            get => okButtonText;
            set => SetProperty(ref okButtonText, value);
        }

        private int compassSize;
        public int CompassSize
        {
            get => compassSize;
            set => SetProperty(ref compassSize, value);
        }

        private async void OnCancelClicked()
        {
            
            string coriolisData = string.Empty;
            MessagingCenter.Send(this, "Coriolis", coriolisData);

            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }
        
        static async Task RotateCompass()
        {
            double oldReading = 0;

            if (compassDetected)
            {
                while (true)
                {
                    // Rotate compass if new reading
                    if (oldReading != rawCompassReading)
                    {
                        compassCanvas.InvalidateSurface();
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1.0 / 1000));
                }
            }
            else
            {
                while (true)
                {
                    uint azimuth;
                    UInt32.TryParse(azimuthEntry, out azimuth);
                    rawCompassReading = azimuth % 360;
                    if (oldReading != rawCompassReading)
                    {
                        compassCanvas.InvalidateSurface();
                        oldReading = rawCompassReading;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1.0 / 10));
                }
            }
        }

        // Get file .svg to folder Images
        private static Task<Stream> GetImageStream(string svgName)
        {
            //var type = typeof(CoriolisViewModel).GetTypeInfo();
            //var assembly = type.Assembly;

            return FileSystem.OpenAppPackageFileAsync(svgName);
            //var svgStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.Images.{svgName}");
            //return svgStream;
        }

        public void OnCanvasViewPaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface;
            var canvas = surface.Canvas;
            int width = e.Info.Width;
            int height = e.Info.Height;
            SKMatrix matrix;

            canvas.Clear();

            try
            {
                // the page is not visible yet
                if (compassSvg == null)
                    return;

                // calculate the scaling need to fit to screen
                float scaleX = width / compassSvg.Picture.CullRect.Width;
                float scaleY = height / compassSvg.Picture.CullRect.Height;
                matrix = SKMatrix.CreateScale(scaleX, scaleY);

                // draw the svg
                canvas.Clear();
                canvas.RotateDegrees((float)-rawCompassReading, width / 2, height / 2);
                canvas.DrawPicture(compassSvg.Picture, ref matrix);
                if (compassDetected)
                {
                    CompassReading = Math.Round(rawCompassReading);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void OnArrowCanvasViewPaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface;
            var canvas = surface.Canvas;
            int width = e.Info.Width;
            int height = e.Info.Height;
            SKMatrix matrix;

            canvas.Clear();

            try
            {
                // the page is not visible yet
                if (arrowSvg == null)
                    return;

                // calculate the scaling need to fit to screen
                float scaleX = width / arrowSvg.Picture.CullRect.Width;
                float scaleY = height / arrowSvg.Picture.CullRect.Height;
                matrix = SKMatrix.CreateScale(scaleX, scaleY);

                // draw the svg
                canvas.DrawPicture(arrowSvg.Picture, ref matrix);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            var data = e.Reading;
            Console.WriteLine($"Reading: {data.HeadingMagneticNorth} degrees");
            // Process Heading Magnetic North
            rawCompassReading = (float)data.HeadingMagneticNorth;
            float tempCompassReading = (float)Math.Round(rawCompassReading);
            if (tempCompassReading == 360)
            {
                CompassReading = 0;
            }
            else
            {
                CompassReading = tempCompassReading;
            }
        }
    }
}