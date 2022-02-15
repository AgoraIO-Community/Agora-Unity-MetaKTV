using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using agora.rtc;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


namespace agora.KTV
{
    public class KtvControlPanel : MonoBehaviour
    {
        public GameObject ktvControlPanel;
        public GameObject musicItem;
        
        //music List UI
        private Transform mMusicListContentTransform;
        private Scrollbar mMusicListScrollbar;
        private List<Transform> musicItemList = new List<Transform>();
        
        //Add music list UI
        private Transform mAddListContentTransform;
        private Scrollbar mAddListScrollbar;
        private List<Transform> addItemList = new List<Transform>();
        
        //KtvControlPanel UI
        private Button NextPageButton;
        private Button NextSongButton;
        private Button AddsongButton;
        private Toggle MusicToggle;
        private Slider MusicVolumeSlider;
        private Slider PeopleVolumeSlider;
        private TextMesh MusicInfoText;
        
        private RtcEngineController _rtcEngineController = null;
        
        private MusicData _musicData = new MusicData();
        private HttpRequest httpRequest = new HttpRequest();
        private MusicRequestInfo musicRequestInfo = new MusicRequestInfo();
        private List<MvInfo> mvInfoList = new List<MvInfo>();
        
        private string Mp3Url = "";
        private long page = 6375734115566230;
        private int index = 0;
        
        public void Start()
        {
            if (!GameApplication.isOwner)
            {
                ktvControlPanel.SetActive(false);
                return;
            }
            
            //register UI, Only owner can use KtvControlPanel
            mMusicListContentTransform = this.transform.Find("Parents/DrawerCommandPanel/MusicScroll/ScrollRect/ScrollView/Content");
            mMusicListScrollbar = this.transform.Find("Parents/DrawerCommandPanel/MusicScroll/Scrollbar").GetComponent<Scrollbar>();
            mAddListContentTransform = this.transform.Find("Parents/DrawerCommandPanel/AddScroll/ScrollRect/ScrollView/Content");
            mAddListScrollbar = this.transform.Find("Parents/DrawerCommandPanel/AddScroll/Scrollbar").GetComponent<Scrollbar>();
            
            _rtcEngineController = RtcEngineController.GetInstance();
            NextPageButton = GameObject.Find("Parents/DrawerCommandPanel/SettingsScrollView/SettingsPanel/AddRemoveOnePanel/NextPage").GetComponent<Button>();
            NextPageButton.onClick.AddListener(NextPage);
            NextSongButton = GameObject.Find("Parents/DrawerCommandPanel/SettingsScrollView/SettingsPanel/AddRemoveOnePanel/NextSong").GetComponent<Button>();
            NextSongButton.onClick.AddListener(NextSong);

            MusicToggle = GameObject.Find("Parents/DrawerCommandPanel/MusicControlView/SettingsPanel/AddRemoveOnePanel/Toggle").GetComponent<Toggle>();
            MusicToggle.onValueChanged.AddListener(onMusicToggle);
            MusicToggle.isOn = true;

            MusicVolumeSlider = GameObject.Find("Parents/DrawerCommandPanel/MusicControlView/SettingsPanel/AddRemoveOnePanel/MusicSlider").GetComponent<Slider>();
            MusicVolumeSlider.onValueChanged.AddListener(onMusicSlider);
            
            PeopleVolumeSlider = GameObject.Find("Parents/DrawerCommandPanel/MusicControlView/SettingsPanel/AddRemoveOnePanel/PeopleSlider").GetComponent<Slider>();
            PeopleVolumeSlider.onValueChanged.AddListener(onPeopleSlider);

            MusicInfoText = GameObject.Find("MusicInfoText").GetComponent<TextMesh>();
        }
        
        private void OnApplicationQuit()
        {
            StopAllCoroutines();
        }
        
        private void ShowItems()
        {
            for (int i = 0; i < 10; i++)
            {
                MusicInfo info = _musicData.GetMusicInfo(MUSIC_LIST_TYPE.LIST, i);
                GameObject item = Instantiate(musicItem, transform.position, transform.rotation);
                item.transform.Find("Panel/Name").GetComponent<Text>().text = info.name;
                item.transform.Find("Panel/Singer").GetComponent<Text>().text = info.singer;
                StartCoroutine(DownSprite(info.poster, item.transform.Find("Panel/Image").GetComponent<Image>()));
                item.transform.parent = mMusicListContentTransform;
                musicItemList.Add(item.transform);
            }
        }

        private void RemoveItems()
        {
            for (int i = 0; i < musicItemList.Count; i++)
            {
                if (musicItemList[i] != null)
                {
                    Transform t = musicItemList[i];
                    Destroy(t.gameObject);
                }
            }
            musicItemList.Clear();
        }

        private void AddItems(MusicInfo info)
        {
            GameObject item = Instantiate(musicItem, transform.position, transform.rotation);
            item.transform.Find("Panel/Name").GetComponent<Text>().text = info.name;
            item.transform.Find("Panel/Singer").GetComponent<Text>().text = info.singer;
            StartCoroutine(DownSprite(info.poster, item.transform.Find("Panel/Image").GetComponent<Image>()));
            item.transform.parent = mAddListContentTransform;
            addItemList.Add(item.transform);
        }

        private void NextItems()
        {
            if (addItemList.Count > 0)
            {
                Transform t = addItemList[0];
                addItemList.Remove(addItemList[0]);
                Destroy(t.gameObject);
            }
        }
        
        IEnumerator DownSprite(string url, Image img)
        {
            Debug.Log("poster url: " + url);
            UnityWebRequest wr = new UnityWebRequest(url);
            DownloadHandlerTexture texD1 = new DownloadHandlerTexture(true);
            wr.downloadHandler = texD1;
            yield return wr.SendWebRequest();
            int width = 1920;
            int high = 1080;
            if (!wr.isNetworkError)
            {
                Texture2D tex = new Texture2D(width, high);
                tex = texD1.texture;

                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                img.sprite = sprite;
            }
        }

        public void NextPage()
        {
            httpRequest.Request(HttpRequestType.Get, musicRequestInfo.BuildUrlForMusicInfo(Convert.ToString(page)), MusicListResultCallback);
            Debug.Log("page is:" + page);
            RemoveItems();
            ShowItems();
            mMusicListScrollbar.value = 1.0f;
        }

        public void AddSong(Transform transform)
        {
            index = musicItemList.IndexOf(transform);
            Debug.Log("HUGOLOG index" + index);
            MusicInfo musicInfo = _musicData.GetMusicInfo(MUSIC_LIST_TYPE.LIST, index);
            
            MusicInfo preAddMusicInfo = new MusicInfo();
            preAddMusicInfo.name = musicInfo.name;
            preAddMusicInfo.singer = musicInfo.singer;
            preAddMusicInfo.songCode = musicInfo.songCode;
            AddItems(musicInfo);
            Debug.Log("HUGOLOG AddSongName:"  + preAddMusicInfo.name);
            Debug.Log("HUGOLOG AddSongCode:"  + preAddMusicInfo.songCode);
            
            if (musicInfo.mv == null)
            {
                Debug.Log("HUGOLOG This Song do not have mv");
                httpRequest.Request(HttpRequestType.Get,
                    musicRequestInfo.BuildUrlForMusicUrl(Convert.ToString(musicInfo.songCode)), MusicUrlResultCallback);
                preAddMusicInfo.url = Mp3Url;
                _musicData.Add(MUSIC_LIST_TYPE.CHOSEN, preAddMusicInfo);

                if (addItemList.Count == 1)
                {
                    MediaPlayerController.GetInstance(_rtcEngineController.GetRtcEngine()).MediaPlayerOpen(GameApplication.playerId, preAddMusicInfo.url, 0);
                }
                
                return;
            }

            httpRequest.Request(HttpRequestType.Get, musicRequestInfo.BuildUrlForMvUrl(Convert.ToString(musicInfo.songCode)), MvUrlResultCallback);
            for (int i = 0; i < mvInfoList.Count; i++)
            {
                Debug.Log("HUGOLOG mv url: " + mvInfoList[i].mvUrl);
            }
            if (mvInfoList[0] != null) preAddMusicInfo.url = mvInfoList[0].mvUrl;
            _musicData.Add(MUSIC_LIST_TYPE.CHOSEN, preAddMusicInfo);
            
            if (addItemList.Count == 1)
            {
                MusicInfoText.text = musicInfo.name + "\n" + musicInfo.singer;
                MediaPlayerController.GetInstance(_rtcEngineController.GetRtcEngine()).MediaPlayerOpen(GameApplication.playerId, preAddMusicInfo.url, 0);
            }
            Debug.Log("HUGOLOG AddSong Url:"  + preAddMusicInfo.url);
        }

        public void NextSong()
        {
            if (addItemList.Count == 0) return;
            
            MediaPlayerController.GetInstance(_rtcEngineController.GetRtcEngine()).MediaPlayerStop(GameApplication.playerId);

            if (GameApplication.canPlay == true)
            {
                NextItems();
                MusicInfo musicInfo = _musicData.Remove(MUSIC_LIST_TYPE.CHOSEN);
                Debug.Log("HUGOLOG NextSong Url:"  + musicInfo.url);
                if (GameApplication.playerId == 0) return;
                MediaPlayerController.GetInstance(_rtcEngineController.GetRtcEngine()).MediaPlayerOpen(GameApplication.playerId, musicInfo.url, 0);

                MusicInfoText.text = musicInfo.name + "\n" + musicInfo.singer;
                GameApplication.canPlay = false;
            }
        }

        public void onMusicToggle(bool isOn)
        {
            int index = 0;
            if (isOn)
                index = 1;
            else
                index = 2;
            MediaPlayerController.GetInstance(_rtcEngineController.GetRtcEngine()).MediaPlayerSelectAudioTrack(GameApplication.playerId, index);
        }

        public void onMusicSlider(float volume)
        {
            MediaPlayerController.GetInstance(_rtcEngineController.GetRtcEngine())
                .MediaPlayerAdjustPublishSignalVolume(GameApplication.playerId, (int) volume);
        }

        public void onPeopleSlider(float volume)
        {
            _rtcEngineController.AdjustRecordingSignalVolume((int)volume);
        }

        private void MusicListResultCallback(string result)
        {
            Debug.Log("HUGOLOG MusicListResultCallback " + result);
            MusicJsonDecoder decoder = new MusicJsonDecoder();
            _musicData.Update(MUSIC_LIST_TYPE.LIST, decoder.Decode<MusicInfo>(result, "list"));
            page = (long) _musicData.GetMusicInfo(MUSIC_LIST_TYPE.LIST, 9).songCode;
        }
        
        private void MvUrlResultCallback(string result)
        {
            Debug.Log("HUGOLOG MusicUrlResultCallback " + result);
            MusicJsonDecoder decoder = new MusicJsonDecoder();
            mvInfoList = decoder.Decode<MvInfo>(result, "mvList");
        }

        private void MusicUrlResultCallback(string result)
        {
            Debug.Log("HUGOLOG MusicUrlResultCallback " + result);
            MusicJsonDecoder decoder = new MusicJsonDecoder();
            string music = decoder.Decode(result);
            Mp3Url = music;
            Debug.Log("HUGOLOG music url is: " + music);
        }
    }
}