using UnityEngine;
using System.Collections.Generic;
using agora.rtc.LitJson;

namespace agora.KTV
{
    public class MusicJsonDecoder
    {
        
        public List<T> Decode<T>(string message, string key)
        {
            JsonData jsonData = JsonMapper.ToObject(message);
            string list = jsonData["data"][key].ToJson();
            List<T> musicList = JsonMapper.ToObject<List<T>>(list);
            Debug.Log("HUGOLOG Decode ");
            return musicList;
        }

        public string Decode(string message)
        {
            JsonData jsonData = JsonMapper.ToObject(message);
            string url = (string) jsonData["data"]["playUrl"];
            return url;
        }
    }
}