namespace Formulaar1
{
    public class POSTReleasePush
    {
        public string? Title { get; set; }
        public string? SeriesTitle { get; set; }
        public string? DownloadUrl { get; set; }
        public long Size { get; set; }
        public string? Indexer { get; set; }
        public string? DownloadProtocol { get; set; }
        public string? Protocol { get; set; }
        public DateTime PublishDate { get; set; }
        public int MappedSeasonNumber { get; set; }
        public int[]? MappedEpisodeNumbers { get; set; }
    }
    public class PUTManualImport
    {
        public Class1[] Property1 { get; set; }
    }

    public class Class1
    {
        public string path { get; set; }
        public string relativePath { get; set; }
        public string folderName { get; set; }
        public string name { get; set; }
        public long size { get; set; }
        public Series series { get; set; }
        public Quality quality { get; set; }
        public Language language { get; set; }
        public int qualityWeight { get; set; }
        public Rejection[] rejections { get; set; }
        public int id { get; set; }
    }

    public class Series
    {
        public string title { get; set; }
        public string sortTitle { get; set; }
        public string status { get; set; }
        public bool ended { get; set; }
        public string overview { get; set; }
        public string network { get; set; }
        public string airTime { get; set; }
        public Image[] images { get; set; }
        public Season[] seasons { get; set; }
        public int year { get; set; }
        public string path { get; set; }
        public int qualityProfileId { get; set; }
        public int languageProfileId { get; set; }
        public bool seasonFolder { get; set; }
        public bool monitored { get; set; }
        public bool useSceneNumbering { get; set; }
        public int runtime { get; set; }
        public int tvdbId { get; set; }
        public int tvRageId { get; set; }
        public int tvMazeId { get; set; }
        public DateTime firstAired { get; set; }
        public string seriesType { get; set; }
        public string cleanTitle { get; set; }
        public string imdbId { get; set; }
        public string titleSlug { get; set; }
        public string[] genres { get; set; }
        public object[] tags { get; set; }
        public DateTime added { get; set; }
        public Ratings ratings { get; set; }
        public int id { get; set; }
    }

    public class Quality
    {
        public Quality1 quality { get; set; }
        public Revision revision { get; set; }
    }

    public class Quality1
    {
        public int id { get; set; }
        public string name { get; set; }
        public string source { get; set; }
        public int resolution { get; set; }
    }

    public class Revision
    {
        public int version { get; set; }
        public int real { get; set; }
        public bool isRepack { get; set; }
    }

    public class Language
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Rejection
    {
        public string reason { get; set; }
        public string type { get; set; }
    }


    public class Episode
    {
        public int seriesId { get; set; }
        public int tvdbId { get; set; }
        public int episodeFileId { get; set; }
        public int seasonNumber { get; set; }
        public int episodeNumber { get; set; }
        public string? title { get; set; }
        public string? airDate { get; set; }
        public DateTime airDateUtc { get; set; }
        public bool hasFile { get; set; }
        public bool monitored { get; set; }
        public bool unverifiedSceneNumbering { get; set; }
        public int id { get; set; }
    }
    public class JsonClasses
    {
        public string title { get; set; }
        public Alternatetitle[] alternateTitles { get; set; }
        public string sortTitle { get; set; }
        public string status { get; set; }
        public bool ended { get; set; }
        public string overview { get; set; }
        public DateTime nextAiring { get; set; }
        public DateTime previousAiring { get; set; }
        public string network { get; set; }
        public string airTime { get; set; }
        public Image[] images { get; set; }
        public Season[] seasons { get; set; }
        public int year { get; set; }
        public string path { get; set; }
        public int qualityProfileId { get; set; }
        public int languageProfileId { get; set; }
        public bool seasonFolder { get; set; }
        public bool monitored { get; set; }
        public bool useSceneNumbering { get; set; }
        public int runtime { get; set; }
        public int tvdbId { get; set; }
        public int tvRageId { get; set; }
        public int tvMazeId { get; set; }
        public DateTime firstAired { get; set; }
        public string seriesType { get; set; }
        public string cleanTitle { get; set; }
        public string imdbId { get; set; }
        public string titleSlug { get; set; }
        public string rootFolderPath { get; set; }
        public string[] genres { get; set; }
        public object[] tags { get; set; }
        public DateTime added { get; set; }
        public Ratings ratings { get; set; }
        public Statistics statistics { get; set; }
        public int id { get; set; }
    }

    public class Ratings
    {
        public int votes { get; set; }
        public float value { get; set; }
    }

    public class Statistics
    {
        public int seasonCount { get; set; }
        public int episodeFileCount { get; set; }
        public int episodeCount { get; set; }
        public int totalEpisodeCount { get; set; }
        public long sizeOnDisk { get; set; }
        public object[] releaseGroups { get; set; }
        public float percentOfEpisodes { get; set; }
    }

    public class Alternatetitle
    {
        public string title { get; set; }
        public int seasonNumber { get; set; }
    }

    public class Image
    {
        public string coverType { get; set; }
        public string url { get; set; }
        public string remoteUrl { get; set; }
    }

    public class Season
    {
        public int seasonNumber { get; set; }
        public bool monitored { get; set; }
        public Statistics1 statistics { get; set; }
    }

    public class Statistics1
    {
        public int episodeFileCount { get; set; }
        public int episodeCount { get; set; }
        public int totalEpisodeCount { get; set; }
        public long sizeOnDisk { get; set; }
        public object[] releaseGroups { get; set; }
        public float percentOfEpisodes { get; set; }
        public DateTime nextAiring { get; set; }
        public DateTime previousAiring { get; set; }
    }

    public class APICredentialsConfig
    {
        public const string APICredentials = "APICredentials";
        public string ApiKey { get; set; } = String.Empty;
        public string BasePath { get; set; } = String.Empty;
    }

}
