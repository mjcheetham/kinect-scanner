using Microsoft.Kinect;
using System;

namespace KaptureLibrary.Kinect
{
    public enum DepthFormat { HighRes30Fps }
    public enum ColourFormat { HighRes30Fps }

    public static class FormatConvertor
    {
        #region Static Convertors
        internal static Microsoft.Kinect.DepthImageFormat ConvertToKinect(DepthFormat df)
        {
            switch (df)
            {
                case DepthFormat.HighRes30Fps: return Microsoft.Kinect.DepthImageFormat.Resolution640x480Fps30;
                default: throw new ArgumentException("Unsupported depth format!");
            }
        }
        internal static Microsoft.Kinect.ColorImageFormat ConvertToKinect(ColourFormat df)
        {
            switch (df)
            {
                case ColourFormat.HighRes30Fps: return Microsoft.Kinect.ColorImageFormat.RgbResolution640x480Fps30;
                default: throw new ArgumentException("Unsupported colour format!");
            }
        }
        #endregion

        #region Kapture Formats
        // Depth
        public static int PixelDataLength(DepthFormat dFormat)
        {
            return PixelDataLength(ConvertToKinect(dFormat));
        }
        public static int PixelWidth(DepthFormat dFormat)
        {
            return PixelWidth(ConvertToKinect(dFormat));
        }
        public static int PixelHeight(DepthFormat dFormat)
        {
            return PixelHeight(ConvertToKinect(dFormat));
        }
        public static int ByteDataLength(DepthFormat dFormat)
        {
            return ByteDataLength(ConvertToKinect(dFormat));
        }

        // Colour
        public static int PixelDataLength(ColourFormat cFormat)
        {
            return PixelDataLength(ConvertToKinect(cFormat));
        }
        public static int PixelWidth(ColourFormat cFormat)
        {
            return PixelWidth(ConvertToKinect(cFormat));
        }
        public static int PixelHeight(ColourFormat cFormat)
        {
            return PixelHeight(ConvertToKinect(cFormat));
        }
        public static int ByteDataLength(ColourFormat cFormat)
        {
            return ByteDataLength(ConvertToKinect(cFormat));
        }
        #endregion

        #region Kinect Formats
        // Depth
        internal static int BytesPerPixel(DepthImageFormat dFormat)
        {
            switch (dFormat)
            {
                case DepthImageFormat.Resolution640x480Fps30: return 2;
                default: throw new ArgumentException("Unsupported depth format!");
            }
        }
        internal static int PixelDataLength(DepthImageFormat dFormat)
        {
            int pxW, pxH;
            switch (dFormat)
            {
                case DepthImageFormat.Resolution640x480Fps30:
                    {
                        pxW = 640; pxH = 480;
                        break;
                    }
                default: throw new ArgumentException("Unsupported depth format!");
            }
            return pxW * pxH;
        }
        internal static int PixelWidth(DepthImageFormat dFormat)
        {
            switch (dFormat)
            {
                case DepthImageFormat.Resolution640x480Fps30: return 640;
                default: throw new ArgumentException("Unsupported depth format!");
            }
        }
        internal static int PixelHeight(DepthImageFormat dFormat)
        {
            switch (dFormat)
            {
                case DepthImageFormat.Resolution640x480Fps30: return 480;
                default: throw new ArgumentException("Unsupported depth format!");
            }
        }
        internal static int ByteDataLength(DepthImageFormat dFormat)
        {
            return PixelDataLength(dFormat) * BytesPerPixel(dFormat);
        }

        // Colour
        internal static int BytesPerPixel(ColorImageFormat cFormat)
        {
            switch (cFormat)
            {
                case ColorImageFormat.RgbResolution640x480Fps30: return 4;
                default: throw new ArgumentException("Unsupported colour format!");
            }
        }
        internal static int PixelDataLength(ColorImageFormat cFormat)
        {
            int pxW, pxH;
            switch (cFormat)
            {
                case ColorImageFormat.RgbResolution640x480Fps30:
                    {
                        pxW = 640; pxH = 480;
                        break;
                    }
                default: throw new ArgumentException("Unsupported colour format!");
            }
            return pxW * pxH;
        } 
        internal static int PixelWidth(ColorImageFormat cFormat)
        {
            switch (cFormat)
            {
                case ColorImageFormat.RgbResolution640x480Fps30: return 640;
                default: throw new ArgumentException("Unsupported colour format!");
            }
        }
        internal static int PixelHeight(ColorImageFormat cFormat)
        {
            switch (cFormat)
            {
                case ColorImageFormat.RgbResolution640x480Fps30: return 480;
                default: throw new ArgumentException("Unsupported colour format!");
            }
        }
        internal static int ByteDataLength(ColorImageFormat cFormat)
        {
            return PixelDataLength(cFormat) * BytesPerPixel(cFormat);
        }
        #endregion

    }
}
