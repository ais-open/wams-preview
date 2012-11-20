using System;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace Wams
{
    class Program
    {
        static void Main(string[] args)
        {
            var assetHelper = GetAssetHelper(Console.Out);

            // Ingest the video.  This creates an asset, adds the video file to it and then uploads the asset.
            assetHelper.Ingest(Config.VideoFile);

            // Encode the video file that was ingested.
            assetHelper.Encode();

            // Create a locator to the encoded file.  Video players will use the locator URL to play the encoded video file.
            assetHelper.CreateSasLocator();
            
            Console.ReadLine();
        }

        private static AssetHelper GetAssetHelper(TextWriter textWriter)
        {
            textWriter.WriteLine("Instantiating the classes required to ingest, encode and locate a video file.");
            textWriter.WriteLine();

            var cloudMediaContext = new CloudMediaContext(Config.MEDIA_ACCOUNT_NAME, Config.MEDIA_ACCESS_KEY);
            var restServices = new RestServices(Config.MEDIA_ACCOUNT_NAME, Config.MEDIA_ACCESS_KEY);
            var storageCredentials = new StorageCredentialsAccountAndKey(Config.STORAGE_ACCOUNT_NAME, Config.STORAGE_ACCESS_KEY);
            var encoder = new Encoder(cloudMediaContext, storageCredentials, MediaEncodings.H264_HD_720p_Vbr, textWriter);

            return new AssetHelper(cloudMediaContext, encoder, restServices, textWriter);
        }
    }
}
