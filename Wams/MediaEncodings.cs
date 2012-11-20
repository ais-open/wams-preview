namespace Wams
{
    public static class MediaEncodings
    {
        public static MediaEncoding Mp4ToSmoothStreams { get { return new MediaEncoding("MP4 to Smooth Streams Task", EncodingPresets.Mp4ToSmoothStreams, "ism"); } }
        public static MediaEncoding SmoothStreamsToAppleHls { get { return new MediaEncoding("Smooth Streams to Apple HLS Task", EncodingPresets.SmoothStreamsToAppleHls, "m3u8"); } }
        public static MediaEncoding H264_HD_720p_Vbr { get { return new MediaEncoding("Windows Azure Media Encoder", "H.264 HD 720p VBR", "mp4"); } }
    }
}