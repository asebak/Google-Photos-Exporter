using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Discovery.v1;
using Google.Apis.Discovery.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;


namespace exporter
{
    class Program
    {
        readonly string[] _scopes = {"https://www.googleapis.com/auth/photoslibrary"};

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Google Photos Exporter");
            Console.WriteLine("================================");
            try
            {
                new Program().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task Run()
        {
            UserCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets, _scopes,
                    "user", CancellationToken.None);
            }

            var x = credential;

            // Create the service.
            var service = new PhotosService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Photos API Exporter",
            });

            await service.ExportMediaAsync();
        }
    }
}
