using System.IO;

namespace KaptureLibrary.IO
{
    public static class ImageWriter
    {
        /// <summary>
        /// Save an array of BGRX32 bytes as a bitmap file (PPM file format).
        /// </summary>
        /// <param name="outFrame">Array of image bytes in BGRX32 format.</param>
        /// <param name="frameWidth">Width of image.</param>
        /// <param name="frameHeight">Height of image.</param>
        /// <param name="outputPath">Location to save file.</param>
        /// <param name="ppmVersion">PPM format version.</param>
        public static void WritePPM(byte[] outFrame, int frameWidth, int frameHeight, string outputPath, int ppmVersion)
        {
            var ppm = new StreamWriter(outputPath);
            byte maxVal = 0;
            foreach (var p in outFrame) if (p > maxVal) maxVal = p;
            switch (ppmVersion)
            {
                case 1:
                    {
                        ppm.WriteLine("P1");
                        ppm.WriteLine(frameWidth + " " + frameHeight);
                        byte i;
                        for (int row = 0; row < frameHeight; row++)
                        {
                            for (int col = 0; col < frameWidth; col++)
                            {
                                i = outFrame[(row * frameWidth + col)];
                                ppm.Write(i + " ");
                            }
                            ppm.WriteLine();
                        }
                        break;
                    }
                case 2:
                    {
                        ppm.WriteLine("P2");
                        ppm.WriteLine(frameWidth + " " + frameHeight);
                        ppm.WriteLine(maxVal);
                        byte i;
                        for (int row = 0; row < frameHeight; row++)
                        {
                            for (int col = 0; col < frameWidth; col++)
                            {
                                i = outFrame[(row * frameWidth + col)];
                                ppm.Write(i + " ");
                            }
                            ppm.WriteLine();
                        }
                        break;
                    }
                case 3:
                    {
                        ppm.WriteLine("P3");
                        ppm.WriteLine(frameWidth + " " + frameHeight);
                        ppm.WriteLine("255");
                        byte r, g, b;
                        for (int row = 0; row < frameHeight; row++)
                        {
                            for (int col = 0; col < frameWidth; col++)
                            {
                                r = outFrame[(row * frameWidth + col) * 4 + 2];
                                g = outFrame[(row * frameWidth + col) * 4 + 1];
                                b = outFrame[(row * frameWidth + col) * 4];
                                ppm.Write(r + " " + g + " " + b + " ");
                            }
                            ppm.WriteLine();
                        }
                        break;
                    }
            }

            ppm.Close();
        }
    }
}
