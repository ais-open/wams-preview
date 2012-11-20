using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.StorageClient;

namespace Wams
{
    public class Encoder
    {
        const int JOB_PROGRESS_INTERVAL = 500;

        private readonly CloudMediaContext context;
        private readonly StorageCredentialsAccountAndKey storageCredentials;
        private readonly TextWriter textWriter;

        public MediaEncoding MediaEncoding { get; private set; }
        public IAsset OutputAsset { get; private set; }

        public Encoder(CloudMediaContext context, StorageCredentialsAccountAndKey storageCredentials, MediaEncoding mediaEncoding, TextWriter textWriter)
        {
            this.context = context;
            this.storageCredentials = storageCredentials;
            this.MediaEncoding = mediaEncoding;
            this.textWriter = textWriter;
        }

        public void Encode(IAsset asset)
        {
            var processor = this.context.MediaProcessors.Where(p => p.Name == this.MediaEncoding.MediaProcessor && p.Version == "1.5.3").First();
            var job = this.context.Jobs.Create(CreateKey("EncodingJob"));
            var task = job.Tasks.AddNew(CreateKey("EncodingTask"), processor, this.MediaEncoding.Configuration, TaskCreationOptions.ProtectedConfiguration);
            task.InputMediaAssets.Add(asset);
            task.OutputMediaAssets.AddNew(CreateKey("OutputAsset"), true, AssetCreationOptions.None);

            job.Submit();
            this.CheckProgress(job.Id);
        }

        private IJob GetJob(string jobId)
        {
            return context.Jobs.Where(j => j.Id == jobId).First();
        }

        private void CheckProgress(string jobId)
        {
            var currentJobState = JobState.Queued;
            var jobStateRunningTime = new Stopwatch();
            jobStateRunningTime.Start();
            var job = this.GetJob(jobId);

            while (job.State != JobState.Finished)
            {
                if (currentJobState != job.State)
                {
                    currentJobState = job.State;
                    jobStateRunningTime.Restart();
                    this.textWriter.WriteLine();
                }

                this.ReportProgress(job.State, jobStateRunningTime.Elapsed);
                Thread.Sleep(JOB_PROGRESS_INTERVAL);
                job = this.GetJob(jobId);
            }

            this.OutputAsset = job.OutputMediaAssets[0];
            this.SetContentType();
            this.textWriter.WriteLine();
            this.textWriter.WriteLine();
        }

        private void ReportProgress(JobState jobState, TimeSpan elapsedTime)
        {
            this.textWriter.Write("\rState: {0}.  Duration (seconds): {1}", jobState, Math.Round(elapsedTime.TotalSeconds, 2));
        }

        private void SetContentType()
        {
            var policy = this.context.AccessPolicies.Create("policy", TimeSpan.FromDays(365), AccessPermissions.Read);
            var locator = this.context.Locators.CreateSasLocator(this.OutputAsset, policy);
            var sasUrl = new Uri(locator.Path); // doing this only to get the asset container name        
            var blobClient = new CloudBlobClient("https://" + sasUrl.Host, this.storageCredentials);
            var container = blobClient.GetContainerReference(sasUrl.Segments[1]);

            foreach (CloudBlob blobItem in container.ListBlobs())
            {
                var blob = container.GetBlobReference(blobItem.Name);

                if (blob.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    blob.Attributes.Properties.ContentType = "video/mp4";
                }
                //else if … for wmv, flv, etc

                blob.SetProperties();
            }

            this.context.Locators.Revoke(locator);
        }

        private static string CreateKey(string keyType)
        {
            return string.Format(keyType, "_", Guid.NewGuid());
        }
    }
}