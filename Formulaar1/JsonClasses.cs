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

}
