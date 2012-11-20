namespace Wams
{
    public class MediaEncoding
    {
        public MediaEncoding(string mediaProcessor, string configuration, string fileExtension)
        {
            this.MediaProcessor = mediaProcessor;
            this.Configuration = configuration;
            this.FileExtension = fileExtension;
        }

        public string MediaProcessor { get; private set; }
        public string Configuration { get; private set; }
        public string FileExtension { get; private set; }
    }
}