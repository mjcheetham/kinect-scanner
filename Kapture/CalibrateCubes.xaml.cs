using KaptureLibrary.Kinect;
using KaptureLibrary.Processing.ImageAnalysis;
using KaptureLibrary.Tracking;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kapture
{
    /// <summary>
    /// Interaction logic for Calibrate.xaml
    /// </summary>
    public partial class CalibrateCubes : Window
    {
        #region Return Values
        public List<TrackingMarker> Markers { get; set; }
        #endregion

        #region Private Fields
        private MarkerName currentlySampling;
        private TrackingMarker coreHueR;
        private TrackingMarker coreHueG;
        private TrackingMarker coreHueB;
        private TrackingMarker coreHueP;
        #endregion

        private WriteableBitmap stream;

        public CalibrateCubes(SensorManager manager)
        {
            InitializeComponent();
            this.stream = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);
            manager.LiveColourFrameReady += manager_LiveColourFrameReady;
            this.Viewport.Source = this.stream;
            // set up initial hues
            this.coreHueR = new TrackingMarker(MarkerName.Red, 0, 10, -Math.PI / 4); // TODO: add ording to UI
            this.coreHueG = new TrackingMarker(MarkerName.Green, 123, 10, 0);
            this.coreHueB = new TrackingMarker(MarkerName.Blue, 213, 10, Math.PI / 4);
            this.coreHueP = new TrackingMarker(MarkerName.Purple, 316, 10, Math.PI / 2);

        }

        void manager_LiveColourFrameReady(object sender, byte[] frame)
        {
            Dispatcher.Invoke(() =>
            {
                this.stream.WritePixels(
                        new Int32Rect(0, 0, 640, 480),
                        frame,
                        640 * sizeof(int),
                        0);
            });
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            this.Markers = new List<TrackingMarker>();
            if (this.RedCheckbox.IsChecked == true) this.Markers.Add(coreHueR);
            if (this.GreenCheckbox.IsChecked == true) this.Markers.Add(coreHueG);
            if (this.BlueCheckbox.IsChecked == true) this.Markers.Add(coreHueB);
            if (this.PurpleCheckbox.IsChecked == true) this.Markers.Add(coreHueP);

            if (this.Markers.Count == 0)
            {
                MessageBox.Show("Please select at least one cube!", "Calibration Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void RedSample_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.currentlySampling = MarkerName.Red;
        }
        private void GreenSample_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.currentlySampling = MarkerName.Green;
        }
        private void BlueSample_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.currentlySampling = MarkerName.Blue;
        }
        private void PurpleSample_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.currentlySampling = MarkerName.Purple;
        }

        private void ViewCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

            var selectionPoint = e.GetPosition(this.ViewCanvas);
            var x0 = (int)selectionPoint.X;
            var y0 = (int)selectionPoint.Y;

            // TODO: make better use of neighbours!
            var c = GetColorAtPoint(x0, y0);

            var aveHue = (int)ColourConvertor.CalculateHue(c.R, c.G, c.B);

            switch (this.currentlySampling)
            {
                case MarkerName.Red:
                    {
                        this.coreHueR.Hue = aveHue;
                        this.RedSample.Fill = new SolidColorBrush(c);
                        break;
                    }
                case MarkerName.Green:
                    {
                        this.coreHueG.Hue = aveHue;
                        this.GreenSample.Fill = new SolidColorBrush(c);
                        break;
                    }
                case MarkerName.Blue:
                    {
                        this.coreHueB.Hue = aveHue;
                        this.BlueSample.Fill = new SolidColorBrush(c);
                        break;
                    }
                case MarkerName.Purple:
                    {
                        this.coreHueP.Hue = aveHue;
                        this.PurpleSample.Fill = new SolidColorBrush(c);
                        break;
                    }
                default:
                    return;
            }

        }

        private Color GetColorAtPoint(int x0, int y0)
        {

            var img = this.Viewport.Source as WriteableBitmap;
            int stride = (img.PixelWidth * img.Format.BitsPerPixel + 7) / 8;

            byte[] pixels = new byte[img.PixelHeight * stride];

            img.CopyPixels(pixels, stride, 0);

            var index = (x0 + (y0 * img.PixelWidth)) * 4;

            var c = new Color();

            c.R = pixels[index + 2];
            c.G = pixels[index + 1];
            c.B = pixels[index + 0];
            c.A = 255;

            return c;
        }

    }
}
