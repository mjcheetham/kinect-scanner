using KaptureLibrary.IO;
using KaptureLibrary.Kinect;
using KaptureLibrary.Points;
using KaptureLibrary.ShapeAndMeasure;
using KaptureLibrary.Tracking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaptureLibrary.Processing.ImageAnalysis
{
    public class HueThresholder
    {

        #region Private Fields
        private ICollection<TrackingMarker> markers;
        private AreaInt searchSpace;
        #endregion

        #region Constructors
        public HueThresholder(ICollection<TrackingMarker> markers, AreaInt markerSearchSpace)
        {
            this.markers = markers;
            this.searchSpace = markerSearchSpace;
        }
        #endregion

        #region Processing
        internal Dictionary<TrackingMarker, Point2D> IdentifyMarkers(Frame frame)
        {
            int x0 = searchSpace.RangeX.Low, x1 = searchSpace.RangeX.High;
            int y0 = searchSpace.RangeY.Low, y1 = searchSpace.RangeY.High;
            int clipWidth = x1 - x0;
            int clipHeight = y1 - y0;

            var output = new Dictionary<TrackingMarker, Point2D>();
            foreach (TrackingMarker marker in this.markers)
            {
                // threshold
                var hueTolerance = marker.HueTolerance;
                var thresh = ClipAndThreshold(
                    frame.Colour,
                    FormatConvertor.PixelWidth(frame.ColourFormat),
                    FormatConvertor.PixelHeight(frame.ColourFormat),
                    x0, x1, y0, y1,
                    marker.Hue - hueTolerance,
                    marker.Hue + hueTolerance);

                if (DebugSettings.Default.DebugMode) ImageWriter.WritePPM(
                    thresh, clipWidth, clipHeight, DebugSettings.Default.LogRoot + @"\cv\" +
                            marker.Name.ToString() + ".thresh" + frame.FrameNumber + ".ppm", 1);

                // remove outliers
                var cleanThresh = CleanImage(thresh, clipWidth, clipHeight);

                if (DebugSettings.Default.DebugMode) ImageWriter.WritePPM(
                    cleanThresh, clipWidth, clipHeight, DebugSettings.Default.LogRoot + @"\cv\" +
                            marker.Name.ToString() + ".thresh" + frame.FrameNumber + ".clean.ppm", 1);

                // calculate centre
                var momentX = CalcMomentXOrder1(cleanThresh, clipWidth, clipHeight);
                var momentY = CalcMomentYOrder1(cleanThresh, clipWidth, clipHeight);
                var momentAv = CalcMomentXYOrder00(cleanThresh, clipWidth, clipHeight);
                int x = (int)(momentX / momentAv); int y = (int)(momentY / momentAv);
                var pointXY = new Point2D(x0 + x, y0 + y);

                // test confidence
                var count = CountPixelsSet(cleanThresh);
                if (count > ImageAnalysis.Settings.Default.MinimumThresholdPixels) output.Add(marker, pointXY);
            }
            return output;
        }
        #endregion

        #region Set-Pixel Counter
        private static int CountPixelsSet(byte[] image)
        {
            int i = 0;
            foreach (byte p in image)
                if (p == 1) i++;
            return i;
        }
        #endregion

        #region Image Moments
        private static double CalcMomentXOrder1(byte[] image, int width, int height)
        {
            double acc = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    acc += x * image[(int)(y * width + x)];
                }
            }
            return acc;
        }
        private static double CalcMomentYOrder1(byte[] image, int width, int height)
        {
            double acc = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    acc += y * image[(int)(y * width + x)];
                }
            }
            return acc;
        }
        private static double CalcMomentXYOrder00(byte[] image, int width, int height)
        {
            double acc = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    acc += image[(int)(y * width + x)];
                }
            }
            return acc;
        }
        #endregion

        #region Thresholding
        private static byte[] ClipAndThreshold(byte[] srcImage, int width, int height, int x0, int x1, int y0, int y1, int lowHue, int highHue)
        {
            int copyLength = (x1 - x0);
            int clippedImageSize = (x1 - x0) * (y1 - y0);

            byte[] clippedImage = new byte[clippedImageSize * 4];

            // perform clipping
            for (int row = y0; row < y1; row++)
            {
                int srcIndex = 4 * ((width * row) + x0);
                int dstIndex = 4 * ((row - y0) * (x1 - x0));

                Array.ConstrainedCopy(srcImage, srcIndex, clippedImage, dstIndex, copyLength * 4);
            }

            byte[] thresholdImage = new byte[clippedImageSize];

            // perform thresholding
            float hue;
            byte r, g, b;
            for (int i = 0; i < clippedImageSize; i++)
            {
                r = clippedImage[4 * i + 2];
                g = clippedImage[4 * i + 1];
                b = clippedImage[4 * i];
                // calc hue
                hue = ColourConvertor.CalculateHue(r, g, b);

                thresholdImage[i] = (byte)((lowHue < hue && hue < highHue) ? 1 : 0);

            }

            return thresholdImage;

        }
        #endregion

        #region Clustering Convolution
        private static byte[] CleanImage(byte[] input, int w, int h)
        {
            byte[,] O = new byte[w, h];
            int[,] kernel = new int[,] { { 2, 2,   2, 2, 2 },
                                         { 2, 1,   1, 1, 2 },
                                         { 2, 1, -10, 1, 2 },
                                         { 2, 1,   1, 1, 2 },
                                         { 2, 2,   2, 2, 2 } };

            // TODO: remove need for wrapper!
            var I = new Image2DArrayWrapper(input, w, h);

            // convolve to find lone points and subtract from image
            byte[] output = new byte[input.Length];

            Parallel.For(2, w - 2, (x) =>
                {
                    Parallel.For(2, h - 2, (y) =>
                    {
                        int sum = 0;
                        for (int u = -2; u < 3; u++)
                        {
                            for (int v = -2; v < 3; v++)
                            {
                                int k_uv = kernel[2 + u, 2 + v];
                                sum += I[x + u, y + v] * k_uv;
                            }
                        }
                        O[x, y] = (sum < 0) ? (byte)1 : (byte)0;
                        output[y * w + x] = (byte)(I[x, y] - O[x, y]);
                    });
                });

            return output;
        }
        #endregion

    }

    /// <summary>
    /// Wrapper for a one-dimensional array of bytes.
    /// </summary>
    internal struct Image2DArrayWrapper
    {
        private byte[] _e;
        private int h;
        private int w;
        public byte this[int i, int j]
        {
            get
            {
                return _e[j * w + i];
            }
        }
        public Image2DArrayWrapper(byte[] input, int width, int height)
        {
            this._e = input;
            this.w = width;
            this.h = height;
        }
    }

}
