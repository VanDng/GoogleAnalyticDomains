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
            var webClient = new WebClient();

            while (true)
            {
                try
                {
                    var webContent = webClient.DownloadString("https://www.google.com");
                    Console.WriteLine($"{DateTime.Now} Google");

                    Thread.Sleep(10000);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occured. Message: {ex.ToString().Substring(0, (ex.ToString().Length > 15) ? 15 : ex.ToString().Length)}");
                }

            }
        }
    }
}
