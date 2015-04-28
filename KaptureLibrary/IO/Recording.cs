using KaptureLibrary.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaptureLibrary.IO
{
    public class Recording : IDisposable
    {
        #region Private Fields
        private FileStream fDepth;
        private FileStream fColour;
        private byte[] mappingParams;
        private Calibration calibration;
        private int cFrameSizeBytes;
        private int dFrameSizeShorts;
        #endregion

        #region Public Properties
        public ICollection<byte> MappingParameters { get { return mappingParams; } }
        public Calibration Calibration { get { return calibration; } }
        public bool IsCalibratedRecording { get; private set; }
        public int NumberOfFrames { get; private set; }
        public ColourFormat ColourFormat { get { return ColourFormat.HighRes30Fps; } }
        public DepthFormat DepthFormat { get { return DepthFormat.HighRes30Fps; } }
        public float[] Timestamps { get; private set; }
        public DateTime CaptureDateTime { get; private set; }
        #endregion

        #region Constructors
        private Recording()
        {
            this.CaptureDateTime = DateTime.Now;
        }

        public Recording(string mappingParamsPath, string timingPath, string depthPath, DepthFormat dFormat,
            string colourPath, ColourFormat cFormat)
            : this(mappingParamsPath, timingPath, depthPath, dFormat, colourPath, cFormat, null) { }

        public Recording(string mappingParamsPath, string timingPath, string depthPath, DepthFormat dFormat,
            string colourPath, ColourFormat cFormat, string calibrationPath) : this()
        {
            // open mapping data
            var fMapping = new FileStream(mappingParamsPath, FileMode.Open);
            this.mappingParams = new byte[fMapping.Length];
            fMapping.Read(this.mappingParams, 0, this.mappingParams.Length);
            fMapping.Close();

            // get image file streams ready
            this.fDepth = new FileStream(depthPath, FileMode.Open);
            this.fColour = new FileStream(colourPath, FileMode.Open);

            // deserialise calibration if required
            if (calibrationPath == null)
            {
                this.IsCalibratedRecording = false;
            }
            else
            {
             
                this.calibration = Calibration.CreateFromFile(calibrationPath);
                this.IsCalibratedRecording = true;
            }


            // read and convert timing data to fractional seconds
            var fTiming = new FileStream(timingPath, FileMode.Open);
            var timingReader = new BinaryReader(fTiming);
            this.Timestamps = new float[fTiming.Length / sizeof(Single)];
            for (int i = 0; i < this.Timestamps.Length; i++)
                this.Timestamps[i] = timingReader.ReadSingle();
            fTiming.Close();

            // set other recording properties
            this.cFrameSizeBytes = FormatConvertor.ByteDataLength(cFormat);
            this.dFrameSizeShorts = FormatConvertor.PixelDataLength(dFormat);
            this.NumberOfFrames = (int)this.fDepth.Length / FormatConvertor.ByteDataLength(dFormat);

        }
        #endregion

        #region Destructor + Dispose
        ~Recording()
        {
            Dispose();
        }
        public void Dispose()
        {
            // release files held by streams
            this.fDepth.Close();
            this.fColour.Close();
        }
        #endregion

        #region Random Access (Sync)
        public byte[] FetchColourFrame(int frameNumber)
        {
            var oldPosition = fColour.Position;
            fColour.Seek(frameNumber * cFrameSizeBytes, SeekOrigin.Begin);
            var buffer = new byte[this.cFrameSizeBytes];
            this.fColour.Read(buffer, 0, cFrameSizeBytes);
            fColour.Position = oldPosition;
            return buffer;
        }

        public short[] FetchDepthFrame(int frameNumber)
        {
            var oldPosition = fDepth.Position;
            fDepth.Seek(frameNumber * dFrameSizeShorts * 2, SeekOrigin.Begin);
            var buffer = new byte[this.dFrameSizeShorts * 2];
            this.fDepth.Read(buffer, 0, buffer.Length);
            var shorts = new short[this.dFrameSizeShorts];
            Parallel.For(0, shorts.Length, (i) =>
            {
                shorts[i] = System.BitConverter.ToInt16(buffer, i * 2);
            });
            fDepth.Position = oldPosition;
            return shorts;
        }
        #endregion

        #region Random Access (Async)
        public async Task<byte[]> FetchColourFrameAsync(int frameNumber)
        {
            var oldPosition = fColour.Position;
            fColour.Seek(frameNumber * cFrameSizeBytes, SeekOrigin.Begin);
            var buffer = new byte[this.cFrameSizeBytes];
            await this.fColour.ReadAsync(buffer, 0, cFrameSizeBytes);
            fColour.Position = oldPosition;
            return buffer;
        }

        public async Task<short[]> FetchDepthFrameAsync(int frameNumber)
        {
            var oldPosition = fDepth.Position;
            fDepth.Seek(frameNumber * dFrameSizeShorts * 2, SeekOrigin.Begin);
            var buffer = new byte[this.dFrameSizeShorts * 2];
            await this.fDepth.ReadAsync(buffer, 0, buffer.Length);
            var shorts = new short[this.dFrameSizeShorts];
            Parallel.For(0, shorts.Length, (i) =>
                {
                    shorts[i] = System.BitConverter.ToInt16(buffer, i * 2);
                });
            fDepth.Position = oldPosition;
            return shorts;
        }
        #endregion


    }
}
