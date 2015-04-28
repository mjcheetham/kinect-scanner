using System;

namespace KaptureLibrary.Kinect
{
    public class NotConnectedException : Exception
    {
        public NotConnectedException(string message) : base(message) { }
    }
}
