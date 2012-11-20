using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.MediaServices.Client;
using Newtonsoft.Json;

namespace Wams
{
    public class RestServices
    {
        private readonly string accountName;
        private readonly string accountKey;

        public RestServices(string accountName, string accountKey)
        {
            this.accountName = accountName;
            this.accountKey = accountKey;
        }

        public async void RenameAsset(IAsset asset, string newName)
        {
            // Create the request
            var client = new HttpClient();
            var resource = string.Format("https://wamsbluclus001rest-hs.cloudapp.net/API/Assets('{0}')", asset.Id);
            var request = new HttpRequestMessage(new HttpMethod("MERGE"), resource);

            // Set the request content
            var content = string.Format("{{ 'Name' : '{0}' }}", newName);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            var oDataParameter = new NameValueHeaderValue("odata", "verbose");
            stringContent.Headers.ContentType.Parameters.Add(oDataParameter);
            request.Content = stringContent;
            
            // Set the request headers
            var jsonAccessToken = await this.GetAccessToken();
            string accessToken = JsonConvert.DeserializeObject<dynamic>(jsonAccessToken).access_token.Value;
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            request.Headers.Add("DataServiceVersion", "3.0");
            request.Headers.Add("MaxDataServiceVersion", "3.0");
            request.Headers.Add("x-ms-version", "1.0");
            request.Headers.Host = "wamsbluclus001rest-hs.cloudapp.net";
            var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            mediaType.Parameters.Add(oDataParameter);
            client.DefaultRequestHeaders.Accept.Add(mediaType);

            // Make the request
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        private async Task<string> GetAccessToken()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://wamsprodglobal001acs.accesscontrol.windows.net/v2/OAuth2-13");
            var content = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=urn%3aWindowsAzureMediaServices", this.accountName, HttpUtility.UrlEncode(this.accountKey));
            request.Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded");
            request.Headers.Host = "wamsprodglobal001acs.accesscontrol.windows.net";
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}