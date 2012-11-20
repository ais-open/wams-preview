using System;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace Wams
{
    public class AssetHelper
    {
        private readonly CloudMediaContext context;
        private readonly Encoder encoder;
        private readonly RestServices restServices;
        private readonly TextWriter textWriter;
        private IAsset asset;
        private IAsset encodedAsset;
        private IFileInfo encodedFile;

        public AssetHelper(CloudMediaContext context, Encoder encoder, RestServices restServices, TextWriter textWriter)
        {
            this.context = context;
            this.encoder = encoder;
            this.restServices = restServices;
            this.textWriter = textWriter;
        }

        public void Ingest(string path)
        {
            this.textWriter.WriteLine("Ingesting video file {0}", path);
            this.textWriter.WriteLine();
            this.asset = this.context.Assets.Create(path);
        }

        public void Encode()
        {
            const string OUTPUT_ASSET_NAME = "Encoded Asset";
            this.textWriter.WriteLine("Encoding video file");
            encoder.Encode(this.asset);
            this.encodedAsset = encoder.OutputAsset;
            this.textWriter.WriteLine("Renaming the video file to '{0}' using the REST API", OUTPUT_ASSET_NAME);
            this.restServices.RenameAsset(this.encodedAsset, OUTPUT_ASSET_NAME);
            this.textWriter.WriteLine();

            this.encodedFile = this.encodedAsset.Files.Where(x => x.Name.EndsWith("." + encoder.MediaEncoding.FileExtension)).First();
        }

        public void CreateSasLocator()
        {
            this.textWriter.WriteLine("Creating SAS locator for {0}", this.encodedFile.Name);
            var accessPolicy = this.CreateAccessPolicy();
            var locator = this.context.Locators.CreateSasLocator(this.encodedAsset, accessPolicy, DateTime.UtcNow.AddMinutes(-5));
            var uriBuilder = new UriBuilder(locator.Path);
            uriBuilder.Path += "/" + this.encodedFile.Name;
            this.textWriter.WriteLine();
            this.textWriter.WriteLine("Use this locator URL in your video player of choice:");
            this.textWriter.WriteLine(uriBuilder.Uri.AbsoluteUri);
        }

        private IAccessPolicy CreateAccessPolicy()
        {
            return this.context.AccessPolicies.Create("Access Policy", TimeSpan.FromDays(30), AccessPermissions.Read);
        }

        public void Delete()
        {
            foreach (var locator in this.asset.Locators)
            {
                context.Locators.Revoke(locator);
            }

            var numberOfContentKeys = this.asset.ContentKeys.Count();

            for (var i = 0; i < numberOfContentKeys; i++)
            {
                this.asset.ContentKeys.RemoveAt(i);
            }

            context.Assets.Delete(this.asset);
        }
    }
}