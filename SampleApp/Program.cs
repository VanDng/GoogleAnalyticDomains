using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System.Text.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {

        }

        static void TestConnect()
        {
            var webClient = new WebClient();

            while (true)
            {
                try
                {
                    var webContent = webClient.DownloadString("https://www.google.com");
                    Console.WriteLine($"{DateTime.Now} Google");

                    Thread.Sleep(10000);

                    webContent = webClient.DownloadString("http://gamevn.com/");
                    Console.WriteLine($"{DateTime.Now} Gamevn");

                    Thread.Sleep(10000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occured. Message: {ex.ToString().Substring(0, (ex.ToString().Length > 15) ? 15 : ex.ToString().Length)}");
                }

            }
        }

        static async Task TestSomethingElseAsync()
        {
            //https://stackoverflow.com/questions/15270764/get-ssl-certificate-in-net

            var handler = new HttpClientHandler
            {
                UseDefaultCredentials = true,

                ServerCertificateCustomValidationCallback = (sender, cert, chain, error) =>
                {

                    /// Access cert object.

                    return true;
                }
            };

            using (HttpClient client = new HttpClient(handler))
            {
                using (HttpResponseMessage response = await client.GetAsync("https://mail.google.com"))
                {
                    using (HttpContent content = response.Content)
                    {

                    }
                }
            }
        }
    }
}
