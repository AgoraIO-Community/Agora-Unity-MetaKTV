using UnityEngine;

namespace agora.KTV
{
    public class MusicRequestInfo
    {
        private const string basic_data = "https://api.agora.io/cn/v1.0/projects/";
        private const string service_data = "/ktv-service/api/serv/";
        private const string request_data = "?requestId=";
        private const string page_type = "&pageType=";
        private const string page_data = "&page=";
        private const string service_song = "songs";
        private const string size_data = "&size=";
        private const string service_mv_url = "mv-url";
        private const string song_code_data = "&songCode=";
        private const string lyric_type_data = "&lyricType=";
        private const string service_music_url = "song-url";
        
        public string BuildUrlForMusicInfo(string pageNum)
        {
            string basic = basic_data + GameApplication.AppId + service_data + service_song;
            string requestId = request_data + "lzTZsruXVL3VUi2UVHHDTPE0PRvF8P4V";
            string lastUpdateTime = "&lastUpdateTime=" + "1635229837";
            string pagetype = page_type + "0";
            string page = page_data + pageNum;
            string size = size_data + "10";
            string pageCode = "&pageCode=" + pageNum;
            string url = basic + requestId + pagetype + lastUpdateTime+ pageCode + size;
            Debug.Log("BuildUrlForMusicInfo is: " + url);
            return url;
        }

        public string BuildUrlForMvUrl(string code)
        {
            string basic = basic_data + GameApplication.AppId + service_data + service_mv_url;
            string requestId = request_data + "lzTZsruXVL3VUi2UVHHDTPE0PRvF8P4V";
            string songCode = song_code_data + code;
            string lyricType = lyric_type_data + "0";
            string url = basic + requestId + songCode + lyricType;
            Debug.Log("BuildUrlForMvUrl is: " + url);
            return url;
        }

        public string BuildUrlForMusicUrl(string code)
        {
            string basic = basic_data + GameApplication.AppId + service_data + service_music_url;
            string requestId = request_data + "lzTZsruXVL3VUi2UVHHDTPE0PRvF8P4V";
            string songCode = song_code_data + code;
            string lyricType = lyric_type_data + "0";
            string url = basic + requestId + songCode + lyricType;
            Debug.Log("BuildUrlForMusicUrl is: " + url);
            return url;
        }
    }
}