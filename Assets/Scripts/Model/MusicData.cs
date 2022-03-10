using System.Collections.Generic;
using System.Collections.Concurrent;

namespace agora.KTV
{
    public enum MUSIC_LIST_TYPE
    {
        LIST,
        CHOSEN
    }
    
    public class MusicData
    {
        private const int max_size = 10;
        private List<MusicInfo> MusicList = new List<MusicInfo>(max_size);
        private List<MusicInfo> ChosenMusicList = new List<MusicInfo>(max_size);
        
        public void Update(MUSIC_LIST_TYPE type, List<MusicInfo> info)
        {
            if (type == MUSIC_LIST_TYPE.LIST)
                MusicList = info;
            else
                ChosenMusicList = info;
        }

        public void Add(MUSIC_LIST_TYPE type, MusicInfo info)
        {
            if (type == MUSIC_LIST_TYPE.LIST)
                MusicList.Add(info);
            else
                ChosenMusicList.Add(info);
        }

        public MusicInfo Remove(MUSIC_LIST_TYPE type)
        {
            if (type == MUSIC_LIST_TYPE.LIST)
            {
                if (MusicList.Count == 0) return null;
                if (MusicList.Count == 1)
                {
                    MusicList.RemoveAt(0);
                    return null;
                }
                MusicList.RemoveAt(0);
                return MusicList[0];
            }
            else
            {
                if (ChosenMusicList.Count == 0) return null;
                if (ChosenMusicList.Count == 1)
                {
                    ChosenMusicList.RemoveAt(0);
                    return null;
                }
                ChosenMusicList.RemoveAt(0);
                return ChosenMusicList[0];
            }
        }

        public MusicInfo GetMusicInfo(MUSIC_LIST_TYPE type, int index)
        {
            return MusicList.Count >= (index + 1) ? MusicList[index] : null;
        }
        
        public void Clear()
        {
            MusicList.Clear();
            ChosenMusicList.Clear();
        }
    }
}