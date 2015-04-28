using System;

namespace KaptureLibrary.Processing.ImageAnalysis
{
    /// <summary>
    /// Facilitates calculation of 360 degree Hue values from RGB values.
    /// </summary>
    public class ColourConvertor
    {
        public static float CalculateHue(byte R, byte G, byte B)
        {

            float r, g, b;
            float h, s, v;
            r = R / 255.0f;
            g = G / 255.0f;
            b = B / 255.0f;

            float min, max, delta;
            min = Min3(r, g, b);
            max = Max3(r, g, b);
            v = max;				        // v
            delta = max - min;

            if (max != 0)
            {
                s = delta / max;		    // s
            }
            else
            {
                // r = g = b = 0		    // s = 0, v is undefined
                s = 0;
                h = -1;
                return h;
            }

            if (r == max)
            {
                h = (g - b) / delta;		// between yellow & magenta
            }
            else if (g == max)
            {
                h = 2 + (b - r) / delta;	// between cyan & yellow
            }
            else
            {
                h = 4 + (r - g) / delta;	// between magenta & cyan
            }

            h *= 60;				        // degrees

            if (h < 0)
            {
                h += 360;
            }

            return h;
        }
        private static float Min3(float A, float B, float C)
        {
            return Math.Min(Math.Min(A, B), C);

        }
        private static float Max3(float A, float B, float C)
        {
            return Math.Max(Math.Max(A, B), C);
        }
    }
}
