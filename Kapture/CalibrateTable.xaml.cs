using KaptureLibrary.Kinect;
using KaptureLibrary.ShapeAndMeasure;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kapture
{
    /// <summary>
    /// Interaction logic for Calibrate.xaml
    /// </summary>
    public partial class CalibrateTable : Window
    {
        #region Return Values
        public AreaInt MarkerSearchSpace { get; set; }
        #endregion

        #region Private Fields
        private Point selectionStartPoint;
        private Rectangle selectionRectangle;
        private Rectangle searchRectangle;
        private bool searchAreaSet;
        #endregion

        private WriteableBitmap stream;

        public CalibrateTable(SensorManager manager)
        {
            InitializeComponent();
            this.stream = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);
            manager.LiveColourFrameReady += manager_LiveColourFrameReady;
            this.Viewport.Source = stream;
            this.searchAreaSet = false;
            this.searchRectangle = new Rectangle()
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };
            this.RangeText.Text = "Range: [" + 0.8 + ", " + 1.5 + "] m";
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

        #region Area Selection
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectionStartPoint = e.GetPosition(this.ViewCanvas);

            selectionRectangle = new Rectangle
            {
                Fill = Brushes.LightBlue,
                Opacity = 0.5,
                Stroke = Brushes.Blue,
                StrokeThickness = 1
            };
            Canvas.SetLeft(selectionRectangle, selectionStartPoint.X);
            Canvas.SetTop(selectionRectangle, selectionStartPoint.X);
            this.ViewCanvas.Children.Add(selectionRectangle);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || selectionRectangle == null)
                return;

            var pos = e.GetPosition(this.ViewCanvas);

            var x = Math.Min(pos.X, selectionStartPoint.X);
            var y = Math.Min(pos.Y, selectionStartPoint.Y);

            var w = Math.Max(pos.X, selectionStartPoint.X) - x;
            var h = Math.Max(pos.Y, selectionStartPoint.Y) - y;

            selectionRectangle.Width = w;
            selectionRectangle.Height = h;

            Canvas.SetLeft(selectionRectangle, x);
            Canvas.SetTop(selectionRectangle, y);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.searchRectangle.Width = this.selectionRectangle.Width;
            this.searchRectangle.Height = this.selectionRectangle.Height;

            var left = Canvas.GetLeft(this.selectionRectangle);
            var top = Canvas.GetTop(this.selectionRectangle);

            Canvas.SetLeft(searchRectangle, left);
            Canvas.SetTop(searchRectangle, top);

            this.ViewCanvas.Children.Remove(selectionRectangle);
            if (!this.searchAreaSet) { this.ViewCanvas.Children.Add(searchRectangle); this.searchAreaSet = true; }
        }
        #endregion

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            // search area
            var left = (int)Canvas.GetLeft(this.searchRectangle);
            var top = (int)Canvas.GetTop(this.searchRectangle);
            var right = left + (int)this.searchRectangle.Width;
            var bottom = top + (int)this.searchRectangle.Height;
            this.MarkerSearchSpace = new AreaInt(new RangeInt(left, right), new RangeInt(top, bottom));

            this.DialogResult = true;
            this.Close();
        }
    }
}
