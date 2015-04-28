
namespace KaptureLibrary.IO
{
    public enum StreamType { Depth, Colour, Audio, MappingParameters };

    public static class FileNameHelper
    {
        public static string DefaultExtension(StreamType type)
        {
            switch (type)
            {
                case StreamType.Depth: return "z";
                case StreamType.Colour: return "rgb";
                case StreamType.Audio: return "a";
                case StreamType.MappingParameters: return "map";
                default: throw new System.ArgumentException("Not supported stream type!");
            }
        }
        public static string DefaultExtensionWithDot(StreamType type)
        {
            return "." + DefaultExtension(type);
        }
    }
}