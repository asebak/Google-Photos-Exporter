using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Services;
using Newtonsoft.Json;

namespace exporter
{
    internal class PhotosService : Google.Apis.Services.BaseClientService
    {
        private BaseClientService.Initializer initializer;
        private PhotosResource resource;
        public PhotosService(Initializer initializer) : base(initializer)
        {
            this.initializer = initializer;
            this.resource  = new PhotosResource(this);

        }

        public override string Name => "photos";

        public override string BaseUri => "https://photoslibrary.googleapis.com/";

        public override string BasePath => "";

        public override System.Collections.Generic.IList<string> Features => new string[0];

        private async Task<List<MediaItem>> GetMediaAsync()
        {
            var results = new MediaList { MediaItems = new List<MediaItem>() };
            var data = await resource.GetMedia();
            results.MediaItems.AddRange(data.MediaItems);
            while (!string.IsNullOrEmpty(data.NextPageToken))
            {
                data = await resource.GetMedia(data.NextPageToken);
                results.MediaItems.AddRange(data.MediaItems);
            }

            return results.MediaItems;
        }
        public async Task ExportMediaAsync()
        {
            var directory = PrepareDirectory();
            var mediaList = await GetMediaAsync();

            foreach (var item in mediaList)
            {
                DownloadMedia(item, directory);
            }


        }

        private DirectoryInfo PrepareDirectory()
        {
            var downloadsFolder = System.Environment.ExpandEnvironmentVariables("%userprofile%/downloads/Google Photos Exported");
           var directoryInfo = System.IO.Directory.CreateDirectory(downloadsFolder);
           return directoryInfo;
        }

        private void DownloadMedia(MediaItem mediaItem, DirectoryInfo info)
        {
            var url = mediaItem.MimeType.Contains("image") ? $"{mediaItem.BaseUrl}=d" : $"{mediaItem.BaseUrl}=dv";
            var downloadPath = $"{info.FullName}\\{mediaItem.Filename}";
            var client = new WebClient();
            if (!File.Exists(downloadPath))
            {
                Console.WriteLine($"Exporting Image: {mediaItem.Filename}");
                client.DownloadFile(url, downloadPath);
            }
        }


        internal class PhotosResource
        {
            private readonly Google.Apis.Services.IClientService service;

            /// <summary>Constructs a new resource.</summary>
            public PhotosResource(Google.Apis.Services.IClientService service)
            {
                this.service = service;
            }

            public async Task<MediaList> GetMedia(string pageToken = "")
            {

                //https://photoslibrary.googleapis.com/v1/albums
                var result = new HttpResponseMessage();
                if (string.IsNullOrEmpty(pageToken))
                {
                    result = await this.service.HttpClient.GetAsync(new Uri($"{service.BaseUri}/v1/mediaItems?pageSize=100"));
                }
                else
                {
                    result = await this.service.HttpClient.GetAsync(new Uri($"{service.BaseUri}/v1/mediaItems?pageSize=100&pageToken={pageToken}"));
                }

                var data = JsonConvert.DeserializeObject<MediaList>(result.Content.ReadAsStringAsync().Result);
                return data;
            }

        }
        public class Photo
        {
            [JsonProperty("cameraMake")]
            public string CameraMake { get; set; }

            [JsonProperty("cameraModel")]
            public string CameraModel { get; set; }

            [JsonProperty("focalLength")]
            public double FocalLength { get; set; }

            [JsonProperty("apertureFNumber")]
            public double ApertureFNumber { get; set; }

            [JsonProperty("isoEquivalent")]
            public int IsoEquivalent { get; set; }
        }

        public class MediaMetadata
        {
            [JsonProperty("creationTime")]
            public DateTime CreationTime { get; set; }

            [JsonProperty("width")]
            public string Width { get; set; }

            [JsonProperty("height")]
            public string Height { get; set; }

            [JsonProperty("photo")]
            public Photo Photo { get; set; }
        }

        public class MediaItem
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("productUrl")]
            public string ProductUrl { get; set; }

            [JsonProperty("baseUrl")]
            public string BaseUrl { get; set; }

            [JsonProperty("mimeType")]
            public string MimeType { get; set; }

            [JsonProperty("mediaMetadata")]
            public MediaMetadata MediaMetadata { get; set; }

            [JsonProperty("filename")]
            public string Filename { get; set; }
        }

        public class MediaList
        {
            [JsonProperty("mediaItems")]
            public List<MediaItem> MediaItems { get; set; }

            [JsonProperty("nextPageToken")]
            public string NextPageToken { get; set; }
        }
    }
}