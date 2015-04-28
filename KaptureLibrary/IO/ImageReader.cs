using System;
using System.IO;

namespace KaptureLibrary.IO
{
    public static class ImageReader
    {
        /// <summary>
        /// Open an existing PPM bitmap file as an array of bytes.
        /// </summary>
        /// <param name="inputPath">Existing PPM file.</param>
        /// <param name="frameWidth">Width of image.</param>
        /// <param name="frameHeight">Height of image.</param>
        /// <param name="v">Version of PPM which was opened.</param>
        /// <returns></returns>
        public static byte[] ReadPPM(string inputPath, out int frameWidth, out int frameHeight, out int v)
        {
            var ppm = new StreamReader(inputPath);
            byte[] frame;
            var ppmVersion = ppm.ReadLine();
            switch (ppmVersion)
            {
                case "P1":
                    {
                        v = 1;
                        var dim = ppm.ReadLine().Split(' ');
                        frameWidth = Int32.Parse(dim[0]);
                        frameHeight = Int32.Parse(dim[1]);
                        frame = new byte[frameWidth * frameHeight];
                        int i = 0;
                        while (!ppm.EndOfStream)
                        {
                            var row = ppm.ReadLine().Split(' ');
                            foreach (var pixelStr in row)
                            {
                                try
                                {
                                    frame[i] = Byte.Parse(pixelStr);
                                }
                                catch (FormatException)
                                {
                                    continue;
                                }
                                i++;
                            }
                        }
                        break;
                    }
                case "P2":
                    {
                        v = 2;
                        var dim = ppm.ReadLine().Split(' ');
                        frameWidth = Int32.Parse(dim[0]);
                        frameHeight = Int32.Parse(dim[1]);
                        frame = new byte[frameWidth * frameHeight];
                        ppm.ReadLine(); // skip 255 value
                        int i = 0;
                        while (!ppm.EndOfStream)
                        {
                            var row = ppm.ReadLine().Split(' ');
                            foreach (var pixelStr in row)
                            {
                                try
                                {
                                    frame[i] = Byte.Parse(pixelStr);
                                }
                                catch (FormatException)
                                {
                                    continue;
                                }
                                i++;
                            }
                        }
                        break;
                    }
                case "P3":
                    {
                        v = 3;
                        var dim = ppm.ReadLine().Split(' ');
                        frameWidth = Int32.Parse(dim[0]);
                        frameHeight = Int32.Parse(dim[1]);
                        frame = new byte[4 * frameWidth * frameHeight];
                        ppm.ReadLine(); // skip 255 value
                        int i = 0;
                        while (!ppm.EndOfStream)
                        {
                            var row = ppm.ReadLine().Split(' ');
                            for (int j = 0; j < frameWidth; j++)
                            {
                                try
                                {
                                    frame[4 * (i * frameWidth + j)] = Byte.Parse(row[(3 * j) + 2]);
                                    frame[4 * (i * frameWidth + j) + 1] = Byte.Parse(row[(3 * j) + 1]);
                                    frame[4 * (i * frameWidth + j) + 2] = Byte.Parse(row[(3 * j)]);
                                }
                                catch (FormatException)
                                {
                                    continue;
                                }
                            }
                            i++;
                        }
                        break;
                    }
                default:
                    {
                        throw new IOException("Incompatible file format!");
                    }
            }

            ppm.Close();
            return frame;

        }
    }
}
