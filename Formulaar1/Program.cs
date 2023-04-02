using APIv3SonarrDotcore.Api;
using APIv3SonarrDotcore.Client;
using APIv3SonarrDotcore.Model;
using Microsoft.AspNetCore.Http.Extensions;
using QBittorrent.Client;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Bugsnag.AspNet.Core;
using static Formulaar1.Helpers;
using Bugsnag;
using Configuration = APIv3SonarrDotcore.Client.Configuration;
using System.Net;

namespace Formulaar1
{
    public class Program
    {
        private static Bugsnag.Client _bugsnag;

        private static SeriesApi? _seriesApi;
        private static EpisodeApi? _episodeApi;
        //private static SeasonPassApi? _seasonPassApi;
        private static ReleasePushApi? _releasePushApi;
        //private static QueueStatusApi? _queueStatusApi;
        //private static QueueDetailsApi? _queueDetailsApi;
        private static CommandApi? _commandApi;
        private static HistoryApi? _historyApi;
        private static HttpClient _httpClient = new();

        private static QBittorrentClient? _qBittorrentClient;

        private static List<ReleaseResource> _hashes = new List<ReleaseResource>();

        private static System.Timers.Timer _timer = new System.Timers.Timer();

        private static string? TorrentClient, BaseSonarPath, BaseqBitPath, SonarApiKey, qBitUsername, qBitPassword, bugsnagApiKey;

        private static bool running = false;
        private static bool bugsnagEnabled = true;

        public static void Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args).Build();

            Microsoft.Extensions.Configuration.IConfiguration config = host.Services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();

            SonarApiKey = config.GetValue<string>("APICredentials:Sonarr:ApiKey");
            BaseSonarPath = config.GetValue<string>("APICredentials:Sonarr:BasePath");
            TorrentClient = config.GetValue<string>("TorrentClient");
            qBitUsername = config.GetValue<string>("APICredentials:qBittorrentClient:Username");
            qBitPassword = config.GetValue<string>("APICredentials:qBittorrentClient:Password");
            BaseqBitPath = config.GetValue<string>("APICredentials:qBittorrentClient:BasePath");
            bugsnagEnabled = config.GetValue<bool>("APICredentials:bugsnag:enabled");
            bugsnagApiKey = config.GetValue<string>("APICredentials:bugsnag:apiKey");

            if (bugsnagEnabled)
            {
                _bugsnag = new Bugsnag.Client(bugsnagApiKey);
            }


            //Configuring Sonarr API
            if (BaseSonarPath != null && SonarApiKey != null)
            {

                Configuration.Default.BasePath = BaseSonarPath;
                Configuration.Default.ApiKey.Add("X-Api-Key", SonarApiKey);
                Configuration.Default.UserAgent = "Formulaar1";

                _seriesApi = new SeriesApi();
                //_seasonPassApi = new SeasonPassApi();
                _episodeApi = new EpisodeApi();
                _episodeApi = new EpisodeApi();
                _releasePushApi = new ReleasePushApi();
                //_queueStatusApi = new QueueStatusApi();
                //_queueDetailsApi = new QueueDetailsApi();
                _historyApi = new HistoryApi();
                _commandApi = new CommandApi();
            }
            else
            {
                Console.WriteLine("#####################################################################################");
                Console.WriteLine("## !!Please check the Sonarr section is configured correctly in appsettings.json!! ##");
                Console.WriteLine("#####################################################################################");

            }

            //Attempt to confiugure download client API's.
            try
            {
                if (TorrentClient == "qBittorrent" && qBitUsername != null && qBitPassword != null)
                {
                    Console.WriteLine($"Detected qBittorrent Client, attempting to login");
                    _qBittorrentClient = new QBittorrentClient(new Uri(BaseqBitPath));
                    _qBittorrentClient.LoginAsync(qBitUsername, qBitPassword).GetAwaiter().GetResult();
                    var result = _qBittorrentClient.GetQBittorrentVersionAsync().GetAwaiter().GetResult();
                    Console.WriteLine($"Logged in to {result}");
                }
                else
                {
                    Console.WriteLine("###########################################################################################");
                    Console.WriteLine("##  !!Please check the qBittorrent section is configured correctly in appsettings.json!! ##");
                    Console.WriteLine("###########################################################################################");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (bugsnagEnabled)
                {
                    _bugsnag.Notify(ex);
                }
            }

            _timer.Interval = 60000;
            _timer.Elapsed += _checkEvents;
            _timer.Enabled = true;
            _timer.AutoReset = true;

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            WebApplication app = builder.Build();

            _ = app.Use(async (context, next) =>
            {
                string pathAndQuery = context.Request.GetEncodedPathAndQuery();

                const string apiEndpoint = "/api";
                if (!pathAndQuery.StartsWith(apiEndpoint))
                {
                    //continues through the rest of the pipeline
                    await next();
                }
                else
                {
                    if (!_httpClient.DefaultRequestHeaders.Contains("X-Api-Key"))
                    {
                        _httpClient.DefaultRequestHeaders.Accept.Clear();
                        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Formulaar1");
                        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", SonarApiKey);
                    }

                    if (context.Request.Method == "GET")
                    {
                        HttpResponseMessage response = await _httpClient.GetAsync(BaseSonarPath + "/api" + pathAndQuery.Replace(apiEndpoint, ""));

                        string result = await response.Content.ReadAsStringAsync();

                        context.Response.StatusCode = (int)response.StatusCode;
                        await context.Response.WriteAsync(result);
                    }
                    else if (context.Request.Method == "POST")
                    {

                        var tmpReleasePost = await context.Request.ReadFromJsonAsync<POSTReleasePush>();

                        Console.WriteLine($"Processing {tmpReleasePost.SeriesTitle}");

                        if (tmpReleasePost != null && tmpReleasePost.Protocol != null)
                        {

                            _ = Enum.TryParse(char.ToUpper(tmpReleasePost.Protocol[0]) + tmpReleasePost.Protocol.Substring(1), out DownloadProtocol Protocol);

                            var ReleasePost = new ReleaseResource
                            {
                                Title = tmpReleasePost.Title,
                                DownloadUrl = tmpReleasePost.DownloadUrl,
                                Protocol = Protocol,
                                Indexer = tmpReleasePost.Indexer,
                                PublishDate = tmpReleasePost.PublishDate,
                                Size = tmpReleasePost.Size,
                                SeriesTitle = tmpReleasePost.SeriesTitle,
                            };

                            try
                            {

                                var Series = await _seriesApi.ApiV3SeriesGetAsync(387219);
                                if (ReleasePost != null && ReleasePost.Title != null && (ReleasePost.Title.Contains("Formula 1") || ReleasePost.Title.Contains("Formula1")))
                                {
                                    _ = int.TryParse(Regex.Match(ReleasePost.Title, @"(?:(?:18|19|20|21)[0-9]{2})").ToString(), out int SeasonID);
                                    var Country = Countries.FirstOrDefault(x => ReleasePost.Title.ToLower().Contains(x.Key.ToLower()) || ReleasePost.Title.ToLower().Contains(x.Key.ToLower())).Value;
                                    var ShowType = Regex.Match(ReleasePost.Title, @"(Qualifying|Race|Sprint)|((Practice|Practise)((.One|.Two|.Three|[0-9]|.[0-9])|(One|Two|Three|[0-9]|.[0-9])))", RegexOptions.IgnoreCase).ToString();
                                    Console.WriteLine($"ShowType: {ShowType}");
                                    ShowType = ShowType.Replace("One", "1");
                                    ShowType = ShowType.Replace("Two", "2");
                                    ShowType = ShowType.Replace("Three", "3");
                                    ShowType = ShowType.Replace("one", "1");
                                    ShowType = ShowType.Replace("two", "2");
                                    ShowType = ShowType.Replace("three", "3");

                                    if (string.IsNullOrWhiteSpace(ShowType))
                                    {
                                        //Lets assume its a Race
                                        ShowType = "Race";
                                    }
                                    Console.WriteLine($"ShowType: {ShowType}");

                                    if (Country != null)
                                    {
                                        var Episode = (await _episodeApi.ApiV3EpisodeGetAsync(Series[0].Id)).FirstOrDefault(x => x.SeasonNumber
                                        == SeasonID && x.Title.Contains(Country) && x.Title.Contains(ShowType) ||
                                        x.SeasonNumber
                                        == SeasonID && x.Title.Contains(Country.Trim(' ')) && x.Title.Contains(ShowType)
                                        );

                                        if (Episode != null)
                                        {

                                            var Quality = Regex.Match(ReleasePost.Title, @"(1080P|1080P|720P|480P|240P)", RegexOptions.IgnoreCase);

                                            var SeriesMap = await _seriesApi.ApiV3SeriesIdGetAsync(Episode.SeriesId);

                                            if (SeriesMap != null)
                                            {
                                                if (SeriesMap.Title == "Formula 1")
                                                {
                                                    var SceneMapping = new AlternateTitleResource() { Title = Episode.Title, SeasonNumber = Episode.SeasonNumber, SceneSeasonNumber = Episode.SceneSeasonNumber };

                                                    ReleasePost.SceneMapping = SceneMapping;
                                                    ReleasePost.TvdbId = SeriesMap.TvdbId;
                                                    ReleasePost.Title = $"{SeriesMap.Title} - S{Episode.SeasonNumber}E{string.Format("{0:00}", Episode.EpisodeNumber)} - {Episode.Title} {Quality}";
                                                    ReleasePost.SeriesId = SeriesMap.Id;
                                                    ReleasePost.SeasonNumber = Episode.SeasonNumber;
                                                    ReleasePost.EpisodeNumbers = new List<int?>() { Episode.EpisodeNumber };
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                if (bugsnagEnabled)
                                {
                                    _bugsnag.Notify(ex);
                                }
                            }


                            try
                            {

                                var response = await _releasePushApi.ApiV3ReleasePushPostAsync(ReleasePost);

                                if (response != null)
                                {
                                    foreach (var r in response)
                                    {
                                        if (r.Rejected == false)
                                        {
                                            _hashes.Add(r);
                                        }
                                    }
                                }

                                var result = response;

                                Console.WriteLine($"Pushing to Sonarr: {ReleasePost.Title}");

                                await context.Response.WriteAsJsonAsync(result);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                if (bugsnagEnabled)
                                {
                                    _bugsnag.Notify(ex);
                                }
                            }
                        }
                    }
                }
            });

            app.Run();
        }

        [DllImport("libc")]
        static extern int link(string oldpath, string newpath);
        private static async void _checkEvents(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!running)
            {
                running = true;
                ////
                ///Used to monitor qBit and then Hardlink once the download is complete 
                ///and will also trigger a media import og the torrent folder in Sonarr.
                ////
                ///

                foreach (var r in _hashes.ToList())
                {
                    if (r.InfoHash == null)
                    {
                        var history = await _historyApi.ApiV3HistoryGetAsync(null, true);

                        foreach (var h in history.Records.Where(x => x.SourceTitle == r.Title && x.Date < DateTime.Now.AddMinutes(-1)))
                        {
                            if (h.EventType == EpisodeHistoryEventType.Grabbed)
                            {
                                Console.WriteLine($"Added Grabbed Torrent from Sonarr history {h.DownloadId}");
                                r.InfoHash = h.DownloadId.ToLower();
                            }
                        }
                        await Task.Delay(1000);
                    }

                    if (r.InfoHash != null)
                    {
                        try
                        {
                            var query = new TorrentListQuery() { Hashes = new string[] { r.InfoHash } };
                            var result = await _qBittorrentClient.GetTorrentListAsync(query);

                            if (result.Count > 0)
                            {
                                var torrent = result.FirstOrDefault();
                                if (torrent != null && torrent.CompletionOn != null)
                                {
                                    var sonarrItem = _hashes.Where(x => x.InfoHash == torrent.Hash).FirstOrDefault();
                                    if (sonarrItem != null)
                                    {
                                        FileAttributes attr = File.GetAttributes(Path.Combine(torrent.SavePath, torrent.Name));

                                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                                        {
                                            var files = Directory.GetFiles(Path.Combine(torrent.SavePath, torrent.Name));

                                            //Attempt to Hardlink files.
                                            foreach (var file in files)
                                            {
                                                var ofInfo = new FileInfo(file);
                                                var nfInfo = new FileInfo($"{ofInfo.DirectoryName}/{sonarrItem.Title} - {ofInfo.Name}");

                                                if (!File.Exists(nfInfo.ToString()))
                                                {
                                                    Console.WriteLine($"Hard Linking {ofInfo.Name} to {nfInfo.Name}");
                                                    int linkResult = link(ofInfo.ToString(), nfInfo.ToString());
                                                }
                                            }

                                            var commandResource = new CommandResource
                                            {
                                                Name = "DownloadedEpisodesScan",
                                                Path = Path.Combine(torrent.SavePath, torrent.Name),
                                                ImportMode = CommandResource.ImportModeEnum.Auto
                                            };

                                            await _commandApi.ApiV3CommandPostAsync(commandResource);

                                            Console.WriteLine($"Sending Command:{commandResource.Name} Mode:{commandResource.ImportMode} Torrent:{torrent.Name} for path \"{commandResource.Path}\"");
                                        }
                                        else
                                        {
                                            var targetDirectory = Path.Combine(torrent.SavePath, sonarrItem.Title);
                                            Directory.CreateDirectory(targetDirectory);
                                            var file = Path.Combine(torrent.SavePath, torrent.Name);

                                            var ofInfo = new FileInfo(file);
                                            var nfInfo = new FileInfo($"{targetDirectory}/{sonarrItem.Title} - {ofInfo.Name}");

                                            if (!File.Exists(nfInfo.ToString()))
                                            {
                                                Console.WriteLine($"Hard Linking File {ofInfo.Name} to {nfInfo.Name}");
                                                int linkResult = link(ofInfo.ToString(), nfInfo.ToString());
                                                Console.WriteLine(linkResult);
                                            }

                                            var commandResource = new CommandResource
                                            {
                                                Name = "DownloadedEpisodesScan",
                                                Path = targetDirectory,
                                                ImportMode = CommandResource.ImportModeEnum.Auto
                                            };

                                            await _commandApi.ApiV3CommandPostAsync(commandResource);

                                            Console.WriteLine($"Sending Command:{commandResource.Name} Mode:{commandResource.ImportMode} Torrent:{torrent.Name} for path \"{commandResource.Path}\"");
                                        }

                                        _hashes.Remove(r);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Attempting Simple Reauth to torrent Client");

                            await _qBittorrentClient.LoginAsync(qBitUsername, qBitPassword);

                            Console.WriteLine(ex.ToString());
                            if (bugsnagEnabled)
                            {
                                _bugsnag.Notify(ex);
                            }
                        }
                    }
                }
                running = false;
            }
        }

    }
}