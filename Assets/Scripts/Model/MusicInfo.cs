namespace agora.KTV
{
    public class MusicInfo
    {
        public long songCode { set; get; }
        public string name { set; get; }
        public string singer { set; get; }
        public string poster { set; get; }
        public long duration { set; get; }
        public int[] lyricType { set; get; }
        public long type { set; get; }
        public string releaseTime { set; get; }
        public long status { set; get; }
        public long updateTime { set; get; }
        public byte[] mv { set; get; }
        public string url { set; get; }
    }
}