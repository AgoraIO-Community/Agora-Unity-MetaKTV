using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using agora.rtc;
using agora.util;
using Logger = agora.util.Logger;

public class movie : MonoBehaviour
{
        [FormerlySerializedAs("APP_ID")] [SerializeField]
        private string appID = "";

        [FormerlySerializedAs("TOKEN")] [SerializeField]
        private string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
        private string channelName = "YOUR_CHANNEL_NAME";

        public Text logText;
        internal Logger Logger;
        internal IAgoraRtcEngine _mRtcEngine = null;
        internal IAgoraRtcMediaPlayer _mediaPlayer = null;
        internal IAgoraRtcCloudSpatialAudioEngine cloud = null;
        internal IAgoraRtcSpatialAudioEngine local = null;
        private const float Offset = 100;
        public int playerId = 0;

        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;

        public GameObject GameObject;
        private int pig = 0;

        // Use this for initialization
        private void Start()
        {
            CheckAppId();
            //SetUpUI();
            InitEngine();
            InitMediaPlayer();
            JoinChannelEx();
            //JoinChannel();
            //InitSpatialAudioEngine();

            //cloud = _mRtcEngine.GetAgoraRtcCloudSpatialAudioEngine();
            local = _mRtcEngine.GetAgoraRtcSpatialAudioEngine();
            
            var ret = local.SetAudioRecvRange(30.5f);
            Debug.Log("local.SetAudioRecvRange returns" + ret);
            ret = local.EnableMic(true);
            Debug.Log("local.EnableMic returns" + ret);
            ret = local.EnableSpeaker(true);
            Debug.Log("local.EnableSpeaker returns" + ret);
            if (ret == 0)
            {
                Debug.Log("local returns" + ret);
                pig = 1;
            }
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();

            if (pig == 1)
            {
                var position = GameObject.Find("KTVGirl_").GetComponent<Transform>().position;
                float[] positionList = { position.x, position.y, position.z };
                float[] right = { transform.right.x, transform.right.y, transform.right.z };
                float[] up = { transform.up.x, transform.up.y, transform.up.z };
                float[] forward = { transform.forward.x, transform.forward.y, transform.forward.z };
                
                Debug.Log("position.x" + position.x + "position.y" + position.y + "position.z" + position.z);

                var transform1 = GameObject.Find("Speakers02").GetComponent<Transform>();
                var position1 = transform1.position;
                float[] positionList1 = { position1.x, position1.y, position1.z };
                float[] forward1 = { transform1.forward.x, transform1.forward.y, transform1.forward.z };
                Debug.Log("position1.x" + position1.x + "position1.y" + position1.y + "position1.z" + position1.z);
                
                //var ret = local.UpdateSelfPosition(positionList, forward, right, up);
                //Debug.Log("local.UpdateSelfPosition returns" + ret);
                //ret = local.UpdatePlayerPositionInfo(playerId, positionList1, forward1);
                //Debug.Log("local.UpdateRemotePosition returns" + ret);
            }
        }

        private void SetUpUI()
        {
            button1 = GameObject.Find("Button1").GetComponent<Button>();
            button1.onClick.AddListener(onPlayButtonPress);
            button2 = GameObject.Find("Button2").GetComponent<Button>();
            button2.onClick.AddListener(onStopButtonPress);
            button3 = GameObject.Find("Button3").GetComponent<Button>();
            button3.onClick.AddListener(onPauseButtonPress);
            button4 = GameObject.Find("Button4").GetComponent<Button>();
            button4.onClick.AddListener(onResumeButtonPress);
            button5 = GameObject.Find("Button5").GetComponent<Button>();
            button5.onClick.AddListener(onOpenButtonPress);
           
        }

        private void CheckAppId()
        {
            Logger = new Logger(logText);
            Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
        }

        private void InitEngine()
        {
            _mRtcEngine = AgoraRtcEngine.CreateAgoraRtcEngine();
            UseEventHandler handler = new UseEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(null, appID, null, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            _mRtcEngine.Initialize(context);
            _mRtcEngine.InitEventHandler(handler);
            _mRtcEngine.SetLogFile("./log.txt");
            _mRtcEngine.EnableAudio();
            _mRtcEngine.EnableVideo();
            _mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            _mRtcEngine.EnableSoundPositionIndication(true);
        }

        private void onPlayButtonPress()
        {
            var ret = _mediaPlayer.Play(playerId);
            Debug.Log("Play return" + ret);
        }
        
        private void onStopButtonPress()
        {
            var ret = _mediaPlayer.Stop(playerId);
            Debug.Log("Stop return" + ret);
        }
        
        private void onPauseButtonPress()
        {
            var ret = _mediaPlayer.Pause(playerId);
            Debug.Log("Pause return" + ret);
        }
        
        private void onResumeButtonPress()
        {
            var ret = _mediaPlayer.Resume(playerId);
            Debug.Log("Resume return" + ret);
        }
        
        private void onOpenButtonPress()
        {
            var ret = _mediaPlayer.Open(playerId, "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4", 0);
            Debug.Log("_mediaPlayer.Open returns: " + ret);
        }
        
        private void InitMediaPlayer()
        {
            _mediaPlayer = _mRtcEngine.GetAgoraRtcMediaPlayer();
            if (_mediaPlayer == null)
            {
                Debug.Log("GetAgoraRtcMediaPlayer failed!");
                return;
            }
            playerId = _mediaPlayer.CreateMediaPlayer();
            MpkEventHandler handler = new MpkEventHandler(this);
            _mediaPlayer.InitEventHandler(handler);
            Debug.Log("playerId id: " + playerId);
            var ret = _mediaPlayer.Open(playerId, "https://agora-adc-artifacts.s3.cn-north-1.amazonaws.com.cn/resources/jay.mkv", 0);
            Debug.Log("_mediaPlayer.Open returns: " + ret);
        }

        public void TestMediaPlayer()
        {
            long duration = 0;
            var ret = _mediaPlayer.GetDuration(playerId, ref duration);
            Debug.Log("_mediaPlayer.GetDuration returns: " + ret + "duration: " + duration);
            
            long pos = 0;
            ret = _mediaPlayer.GetPlayPosition(playerId, ref pos);
            Debug.Log("_mediaPlayer.GetPlayPosition returns: " + ret + "position: " + pos);

            Debug.Log("_mediaPlayer.GetState:" + _mediaPlayer.GetState(playerId));

            bool mute = true;
            ret = _mediaPlayer.GetMute(playerId, ref mute);
            Debug.Log("_mediaPlayer.GetMute returns: " + ret + "mute: " + mute);
            
            int volume = 0;
            ret = _mediaPlayer.GetPlayoutVolume(playerId, ref volume);
            Debug.Log("_mediaPlayer.GetPlayoutVolume returns: " + ret + "volume: " + volume);
            
            Debug.Log("SDK Version:" + _mediaPlayer.GetPlayerSdkVersion(playerId));
            Debug.Log("GetPlaySrc:" + _mediaPlayer.GetPlaySrc(playerId));
        }

        private void JoinChannel()
        {
            //_mRtcEngine.EnableAudio();
            //_mRtcEngine.EnableVideo();
            _mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            _mRtcEngine.JoinChannel(token, channelName);
        }

        private void JoinChannelEx()
        {
            RtcConnection connection = new RtcConnection();
            connection.channelId = channelName;
            connection.localUid = 12345;
            ChannelMediaOptions options2 = new ChannelMediaOptions();
            options2.autoSubscribeAudio = true;
            options2.autoSubscribeVideo = true;
            options2.publishAudioTrack = false;
            options2.publishCameraTrack = false;
            options2.enableAudioRecordingOrPlayout = true;
            options2.clientRoleType = CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER;
            var ret = _mRtcEngine.JoinChannelEx("", connection, options2, null);
            Debug.Log("JoinChannelEx returns: " + ret);
        }
        
        public void JoinChannelEx_MPK()
        {
            RtcConnection connection = new RtcConnection();
            connection.channelId = channelName;
            connection.localUid = 67890;
            ChannelMediaOptions options2 = new ChannelMediaOptions();
            options2.autoSubscribeAudio = true;
            options2.autoSubscribeVideo = true;
            options2.publishAudioTrack = false;
            options2.publishCameraTrack = false;
            options2.publishMediaPlayerAudioTrack = true;
            options2.publishMediaPlayerVideoTrack = true;
            options2.publishMediaPlayerId = playerId;
            options2.enableAudioRecordingOrPlayout = false;
            options2.clientRoleType = CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER;
            var ret = _mRtcEngine.JoinChannelEx("", connection, options2, null);
            Debug.Log("JoinChannelEx returns: " + ret);
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            //_mediaPlayer.DestroyMediaPlayer(playerId);
            if (_mRtcEngine == null) return;
            _mRtcEngine.LeaveChannel();
            _mRtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return channelName;
        }

        internal void MakeVideoView(uint uid, string channelId = "")
        {
            // var go = gameObject.GetComponent<Renderer>();
            // if (!ReferenceEquals(go, null))
            // {
            //     Debug
            //     return; // reuse
            // }

            // create a GameObject and assign to this new user
            Debug.Log("MakeVideoView");
            var surface = GameObject.AddComponent<AgoraVideoSurface>();
            surface.SetForUser(uid, channelId, IRIS_VIDEO_SOURCE_TYPE.kVideoSourceTypeMediaPlayer);
            surface.SetEnable(true);
            surface.EnableFilpTextureApply(false, false);
            // var videoSurface = MakeImageSurface(uid.ToString());
            // if (ReferenceEquals(videoSurface, null)) return;
            // // configure videoSurface
            // videoSurface.SetForUser(uid, channelId, IRIS_VIDEO_SOURCE_TYPE.kVideoSourceTypeMediaPlayer);
            //videoSurface.SetEnable(true);
            //videoSurface.EnableFilpTextureApply(true, false);
        }

        // VIDEO TYPE 1: 3D Object
        private AgoraVideoSurface MakePlaneSurface(string goName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            var yPos = Random.Range(3.0f, 5.0f);
            var xPos = Random.Range(-2.0f, 2.0f);
            go.transform.position = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(1.0f, 1.333f, 0.5f);

            // configure videoSurface
            var videoSurface = go.AddComponent<AgoraVideoSurface>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private static AgoraVideoSurface MakeImageSurface(string goName)
        {
            GameObject go = new GameObject();

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // to be renderered onto
            go.AddComponent<RawImage>();
            // make the object draggable
            go.AddComponent<UIElementDrag>();
            var canvas = GameObject.Find("VideoCanvas");
            if (canvas != null)
            {
                go.transform.parent = canvas.transform;
                Debug.Log("add video view");
            }
            else
            {
                Debug.Log("Canvas is null video view");
            }

            // set up transform
            go.transform.Rotate(0f, 0.0f, 180.0f);
            //var xPos = Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
            //var yPos = Random.Range(Offset, Screen.height / 2f - Offset);
            //Debug.Log("position x " + xPos + " y: " + yPos);
            go.transform.localPosition = new Vector3(Screen.width / 2f - Offset, Screen.height / 2f - Offset, 0f);
            go.transform.localScale = new Vector3(4.5f, 3f, 1f);

            // configure videoSurface
            var videoSurface = go.AddComponent<AgoraVideoSurface>();
            return videoSurface;
        }

        internal void DestroyVideoView(uint uid)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }
    }

    internal class MpkEventHandler : IAgoraRtcMediaPlayerEventHandler
    {
        private readonly movie _mediaPlayerTest;

        internal MpkEventHandler(movie videoSample)
        {
            _mediaPlayerTest = videoSample;
        }

        public override void OnPlayerSourceStateChanged(int playerId, MEDIA_PLAYER_STATE state, MEDIA_PLAYER_ERROR ec)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format(
                "OnPlayerSourceStateChanged state: {0}, ec: {1}, playId: {2}", state, ec, playerId));
            Debug.Log("OnPlayerSourceStateChanged");
            if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
            {
                var ret = _mediaPlayerTest._mediaPlayer.Play(_mediaPlayerTest.playerId);
                //_mediaPlayerTest._mediaPlayer.MuteAudio(playerId, false);
                _mediaPlayerTest._mediaPlayer.AdjustPlayoutVolume(playerId, 0);
                Debug.Log("Play return" + ret);
                _mediaPlayerTest.TestMediaPlayer();
                _mediaPlayerTest.MakeVideoView((uint)_mediaPlayerTest.playerId, "");
                _mediaPlayerTest.JoinChannelEx_MPK();
            }
            else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_STOPPED)
            {
                //var ret = _mediaPlayerTest._mediaPlayer.Play(_mediaPlayerTest.playerId);
                //Debug.Log("Play return" + ret);
                _mediaPlayerTest.DestroyVideoView((uint)_mediaPlayerTest.playerId);
            }
        }

        public override void OnPlayerEvent(int playerId, MEDIA_PLAYER_EVENT @event)
        {
            // _mediaPlayerTest.Logger.UpdateLog(string.Format(
            //     "OnPlayerEvent state: {0}", @event));
        }
    }

    internal class UseEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly movie _mediaPlayerTest;

        internal UseEventHandler(movie videoSample)
        {
            _mediaPlayerTest = videoSample;
        }

        public override void OnWarning(int warn, string msg)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format("OnWarning warn: {0}, msg: {1}", warn, msg));
        }

        public override void OnError(int err, string msg)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format("sdk version: ${0}",
                _mediaPlayerTest._mRtcEngine.GetVersion()));
            _mediaPlayerTest.Logger.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _mediaPlayerTest.Logger.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _mediaPlayerTest.Logger.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
            CLIENT_ROLE_TYPE newRole)
        {
            _mediaPlayerTest.Logger.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _mediaPlayerTest.Logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int) reason));
        }
    }

