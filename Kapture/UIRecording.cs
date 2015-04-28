using KaptureLibrary.IO;
using KaptureLibrary.Points;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kapture
{
    public enum ProcessingStatus { Idle, Busy, Complete }

    public class UIRecording : INotifyPropertyChanged
    {
        public Recording Recording { get; private set; }
        public PointCloud ProcessedCloud { get; set; }

        private ProcessingStatus _status;
        public ProcessingStatus ProcessingStatus
        {
            get { return this._status; }
            set
            {
                this._status = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("Title");
            }
        }

        public string Title
        {
            get
            {
                var status = (ProcessingStatus == Kapture.ProcessingStatus.Complete) ? " [Processed]" : "";
                status = (ProcessingStatus == Kapture.ProcessingStatus.Busy) ? " [Busy...]" : status;
                return this.Recording.CaptureDateTime.ToShortDateString() + " "
                    + this.Recording.CaptureDateTime.ToShortTimeString() + status;
            }
        }
        public WriteableBitmap ColourImage { get; set; }
        public WriteableBitmap DepthImage { get; set; }

        private int _currentFrame;
        public int CurrentDisplayFrameNumber
        {
            get
            {
                return this._currentFrame;
            }
            set
            {
                var colourBytes = this.Recording.FetchColourFrame(value);
                var depthBytes = this.Recording.FetchDepthFrame(value);
                this.ColourImage.WritePixels(
                    new Int32Rect(0, 0, 640, 480),
                    colourBytes,
                    640 * sizeof(int),
                    0);
                this.DepthImage.WritePixels(
                    new Int32Rect(0, 0, 640, 480),
                    depthBytes,
                    640 * sizeof(short),
                    0);
                this._currentFrame = value;
            }
        }

        public UIRecording(Recording recording)
        {
            this.Recording = recording;
            this.ColourImage = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.DepthImage = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Gray16, null);
            this.CurrentDisplayFrameNumber = 0; // will cause frame 0 to be populated/drawn
            this.ProcessingStatus = Kapture.ProcessingStatus.Idle;
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
