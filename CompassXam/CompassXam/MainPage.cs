using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

using SkiaSharp;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using SkiaSharp.Views.Forms;


namespace CompassXam
{
    public class MainPageViewModel :BaseViewModel
    {
        static SkiaSharp.Extended.Svg.SKSvg arrowSvg;
        static SkiaSharp.Extended.Svg.SKSvg compassSvg;
        static SKCanvasView compassCanvas;
        static float rawCompassReading = 0;
        static float compassReadingRounded = 0;

        public AsyncCommand AppearCommand { get; protected set; }
        public Command DisappearCommand { get; protected set; }

        // Set speed delay for monitoring changes.
        readonly SensorSpeed speed = SensorSpeed.UI;

        public MainPageViewModel()
        {

            Compass.ReadingChanged += Compass_ReadingChanged;
            if (!Compass.IsMonitoring)
            {
                try
                {
                    Compass.Start(speed, applyLowPassFilter: true);
                    CompassDetected = true;
                }
                catch (FeatureNotSupportedException ex)
                {
                    CompassInstruction = "Compass Sensor not Detected. Manually Enter Azimuth below.";
                    CompassDetected = false;
                }
            }

            AppearCommand = new AsyncCommand(async () =>
            {
                // Load compassSvg
                compassSvg = new();
                using (var stream = GetImageStream("Compass.svg"))
                {
                    compassSvg.Load(stream);
                }
                // Load arrowSvg
                arrowSvg = new();
                using (var stream = GetImageStream("CompassArrow.svg"))
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

        public float CompassReading
        {
            get => compassReadingRounded;
            set => SetProperty(ref compassReadingRounded, value);
        }
        public SKCanvasView CompassCanvas
        {
            get => compassCanvas;
            set { compassCanvas = value; }
        }


        static async Task RotateCompass()
        {
            float oldReading = 0;

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
                
            }
        }

        // Get file .svg to folder Images
        private static Stream GetImageStream(string svgName)
        {
            var type = typeof(MainPageViewModel).GetTypeInfo();
            var assembly = type.Assembly;

            var svgStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.Images.{svgName}");
            return svgStream;
        }

        public void OnCanvasViewPaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
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
                canvas.RotateDegrees(-rawCompassReading, width / 2, height / 2);
                canvas.DrawPicture(compassSvg.Picture, ref matrix);
                if (!compassDetected)
                {
                    CompassReading = rawCompassReading;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void OnArrowCanvasViewPaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
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