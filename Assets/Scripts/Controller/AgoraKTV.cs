using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using agora.rtc;
using agora.util;
using Mirror;

namespace agora.KTV
{
    public struct PlayerInfo
    {
        public uint uid;
        public Vector3 position;
        public Vector3 forward;
    }
    
    public class AgoraKTV : NetworkBehaviour, IAgoraKtvEventListener
    {
        public GameObject gameObject;
        private System.Random _random = new System.Random();
        private RtcEngineController _rtcEngineController;
        private MediaPlayerController _mediaPlayerController;
        private IAgoraRtcEngine _agoraRtcEngine;
        private SpatialAudioController _spatialAudioController;
        private int playerId = 0;

        public event System.Action<int> OnPlayerNumberChanged;
        public event System.Action<AgoraKTV, PlayerInfo> OnPlayerPositionChanged;
        public event System.Action<uint> OnPlayerUidChanged;

        private int pig = 0;
        private bool isLocal = false;

        // Players List to manage playerNumber
        internal static readonly List<AgoraKTV> playersList = new List<AgoraKTV>();

        internal static void ResetPlayerNumbers()
        {
            int playerNumber = 0;
            foreach (AgoraKTV player in playersList)
            {
                player.playerNumber = playerNumber++;
            }
        }

        [Header("Player UI")] public GameObject playerUIPrefab;
        GameObject playerUI;

        [Header("SyncVars")] [SyncVar(hook = nameof(PlayerNumberChanged))]
        public int playerNumber = 0;

        [SyncVar(hook = nameof(PlayerPositionChanges))]
        public Vector3 playerPosition;
        
        [SyncVar] public Vector3 remoteForward;

        [SyncVar] public uint playerName;
        
        // public TextMesh playerNameText;
        // public GameObject floatingInfo;
        //
        // private Material playerMaterialClone;
        //
        // [SyncVar(hook = nameof(OnNameChanged))]
        // public string name;
        //
        // [SyncVar(hook = nameof(OnColorChanged))]
        // public Color playerColor = Color.white;
        //
        // [Command]
        // public void CmdSetupPlayer(string _name, Color _col)
        // {
        //     // player info sent to server, then server updates sync vars which handles it on all clients
        //     name = _name;
        //     playerColor = _col;
        // }
        //
        // public override void OnStartLocalPlayer()
        // {
        //     string name = GameApplication.PlayerName;
        //     Color color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        //     CmdSetupPlayer(name, color);
        // }
        //
        // void OnNameChanged(string _Old, string _New)
        // {
        //     playerNameText.text = name;
        // }
        //
        // void OnColorChanged(Color _Old, Color _New)
        // {
        //     playerNameText.color = _New;
        //     playerMaterialClone = new Material(GetComponent<Renderer>().material);
        //     playerMaterialClone.color = _New;
        //     GetComponent<Renderer>().material = playerMaterialClone;
        // }

        void PlayerNumberChanged(int _, int newPlayerNumber)
        {
            OnPlayerNumberChanged?.Invoke(newPlayerNumber);
        }

        void PlayerPositionChanges(Vector3 _, Vector3 newPosition)
        {
            PlayerInfo info = new PlayerInfo();
            info.position = newPosition;
            info.uid = this.playerName;
            info.forward = remoteForward;
            OnPlayerPositionChanged?.Invoke(this, info);
        }

        void PlayerUidChanged(uint _, uint uid)
        {
            OnPlayerUidChanged?.Invoke(uid);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            // Add this to the static Players List
            playersList.Add(this);

            // set the Player position
            playerPosition = this.transform.position;
        }

        public override void OnStopServer()
        {
            CancelInvoke();
            playersList.Remove(this);
        }

        public override void OnStartClient()
        {
            PlayerInfo info = new PlayerInfo();
            info.position = playerPosition;
            info.uid = playerName;
            info.forward = remoteForward;
            
            OnPlayerNumberChanged += OnPlayerNumberChangedHandler;
            OnPlayerPositionChanged += OnPlayerPositionChangedHandler;
            OnPlayerUidChanged += OnPlayerUidChangedHandler;

            // Invoke all event handlers with the current data
            OnPlayerNumberChanged.Invoke(playerNumber);
            OnPlayerPositionChanged.Invoke(this, info);
            OnPlayerUidChanged.Invoke(playerName);
        }

        public override void OnStopClient()
        {

        }

        void OnPlayerNumberChangedHandler(int playerNumber)
        {
            Debug.Log("OnPlayerNumberChangedHandler playerNumber: " + playerNumber);
        }

        void OnPlayerPositionChangedHandler(AgoraKTV p, PlayerInfo PlayerInfo)
        {
            Debug.Log("pig is:" + pig + " isLocalPlayer " + p.isLocalPlayer + " uid:" + p.playerName +
                      " OnPlayerPositionChangedHandler x:" + PlayerInfo.position.x + " y:" + PlayerInfo.position.y +
                      " z:" + PlayerInfo.position.z);
            if (p.isLocalPlayer)
            {
                Transform transform = p.GetComponent<Transform>();
                var position = transform.position;

                float[] positionLocal = {position.x, position.y, position.z};
                float[] right = {transform.right.x, transform.right.y, transform.right.z};
                float[] up = {transform.up.x, transform.up.y, transform.up.z};
                float[] forward = {transform.forward.x, transform.forward.y, transform.forward.z};
                _spatialAudioController?.UpdateSelfPosition(positionLocal, forward, right, up, new RtcConnection(GameApplication.ChannelId, playerName));
            }
            else
            {
                var position = PlayerInfo.position;
                var forward = PlayerInfo.forward;
                float[] positionRemote = {position.x, position.y, position.z};
                float[] forwardRemote = {forward.x, forward.y, forward.z};
                _spatialAudioController?.UpdateRemotePosition(p.playerName, positionRemote, forwardRemote, new RtcConnection(GameApplication.ChannelId, GameApplication.uid));
            }
        }

        void OnPlayerUidChangedHandler(uint uid)
        {
            Debug.Log("OnPlayerUidChangedHandler uid:" + uid);
        }

        // Start is called before the first frame update
        void Start()
        {
            var identity = GetComponent<NetworkIdentity>();
            isLocal = identity.isLocalPlayer;

            _rtcEngineController = RtcEngineController.GetInstance();
            _agoraRtcEngine = _rtcEngineController.GetRtcEngine();
            _spatialAudioController = SpatialAudioController.GetInstance(_agoraRtcEngine);

            if (isLocal)
            {
                _mediaPlayerController = MediaPlayerController.GetInstance(_agoraRtcEngine);
                
                _rtcEngineController.AddEventListener(this);
                _mediaPlayerController.AddEventListener(this);

                playerId = _mediaPlayerController.GetPlayerId();
                Debug.Log("HUGOLOG playerId is :"  + playerId);

                if (GameApplication.isOwner)
                {
                    Debug.Log("Owner enters room!");
                    
                    _rtcEngineController?.JoinChannelEx(GameApplication.ChannelId, playerName);
                    _rtcEngineController?.JoinChannelEx_MPK(GameApplication.ChannelId, 67890, playerId);
                    
                    //_mediaPlayerController.MediaPlayerOpen(playerId, "https://agora-adc-artifacts.s3.cn-north-1.amazonaws.com.cn/resources/jay.mkv", 0);
                    pig = 1;
                }
                else
                {
                    Debug.Log("Guest enters room!");
                    _rtcEngineController.JoinChannelEx(GameApplication.ChannelId, playerName);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            playerPosition = this.transform.position;
            remoteForward = this.transform.forward;
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");

            if (isLocalPlayer)
            {
                if (GameApplication.isOwner)
                {
                    _mediaPlayerController?.MediaPlayerStop(playerId);
                    _mediaPlayerController?.DestroyMediaPlayer(playerId);
                    _rtcEngineController?.LeaveChannel(67890, GameApplication.ChannelId);
                }
                _rtcEngineController?.LeaveChannel(playerName, GameApplication.ChannelId);
                _rtcEngineController?.Dispose();
            }
        }
        
        private void MakeVideoView(uint id, string channelId, IRIS_VIDEO_SOURCE_TYPE type)
        {
            // create a GameObject and assign to this new user
            Debug.Log("MakeVideoView");
            gameObject = GameObject.Find("Plane");
            gameObject.name = id.ToString();
            var surface = gameObject.AddComponent<AgoraVideoSurface>();
            surface.SetForUser(id, channelId, type);
            surface.SetEnable(true);
            surface.EnableFilpTextureApply(false, false);
        }

        private void DestroyVideoView(uint playerId)
        {
            var go = GameObject.Find(playerId.ToString());
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }
        
        #region AgoraKtvEventListener
        public void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            GameApplication.uid = playerName;
            //_rtcEngineController.EnableSpatialAudio(true);
        }

        public void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            
        }

        public void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            if ((!GameApplication.PlayerUidList.Contains(uid.ToString())) && uid != playerName)
            {
                GameApplication.PlayerUidList.Add(uid.ToString());
            }
            if (uid == 67890 && !GameApplication.isOwner)
            {
                Debug.Log("client MakeVideoView " + connection.localUid + " " + connection.channelId);
                MakeVideoView(67890, connection.channelId, IRIS_VIDEO_SOURCE_TYPE.kVideoSourceTypeRemote);
                
                
                var transform1 = GameObject.Find("Speakers02").GetComponent<Transform>();
                var position1 = transform1.position;
                float[] positionList1 = {position1.x, position1.y, position1.z};
                float[] forward1 = {transform1.forward.x, transform1.forward.y, transform1.forward.z};
                Debug.Log("position1.x" + position1.x + "position1.y" + position1.y + "position1.z" + position1.z);
                _spatialAudioController?.UpdateRemotePosition(67890, positionList1, forward1, new RtcConnection(GameApplication.ChannelId, playerName));
            }
        }

        public void OnUserOffline(RtcConnection connection, uint remoteUid, USER_OFFLINE_REASON_TYPE reason)
        {
            if (remoteUid == 67890)
            {
                Debug.Log("client DestroyVideoView " + remoteUid + " " + connection.channelId);
                DestroyVideoView((uint)playerId);
            }
        }


        //MediaPlayer
        public void OnPlayerSourceStateChanged(int playerId, MEDIA_PLAYER_STATE state, MEDIA_PLAYER_ERROR ec)
        {
            if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
            {
                _mediaPlayerController.MediaPlayerPlay(playerId);
                _mediaPlayerController.MediaPlayerAdjustPublishSignalVolume(playerId, 200);
                //_mediaPlayerController.MediaPlayerAdjustPlayoutVolume(playerId, 0);
                
                MakeVideoView((uint) playerId, "", IRIS_VIDEO_SOURCE_TYPE.kVideoSourceTypeMediaPlayer);
                var transform1 = GameObject.Find("Speakers02").GetComponent<Transform>();
                var position1 = transform1.position;
                float[] positionList1 = {position1.x, position1.y, position1.z};
                float[] forward1 = {transform1.forward.x, transform1.forward.y, transform1.forward.z};
                _spatialAudioController?.UpdatePlayerPosition(playerId, positionList1, forward1);
            }
            else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_STOPPED)
            {
                GameApplication.canPlay = true;
            }
        }

        public void OnCompleted(int playerId)
        {
            
        }
        #endregion
    }
}