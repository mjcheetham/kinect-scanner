using KaptureLibrary.Kinect;

namespace KaptureLibrary.Processing
{
    public struct Frame
    {
        public int FrameNumber { get; set; }
        public float Timestamp { get; set; }
        public byte[] Colour { get; set; }
        public short[] Depth { get; set; }
        public ColourFormat ColourFormat { get; set; }
        public DepthFormat DepthFormat { get; set; }

        public Frame(int frameNumber, float timestamp, short[] depth, DepthFormat dFormat, byte[] colour, ColourFormat cFormat)
            : this()
        {
            this.FrameNumber = frameNumber;
            this.Timestamp = timestamp;
            this.Depth = depth;
            this.Colour = colour;
            this.DepthFormat = dFormat;
            this.ColourFormat = cFormat;
        }

    }
}
