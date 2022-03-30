using System;
using System.Collections.Generic;
using UnityEngine;
using agora.rtc;

namespace agora.KTV
{

    public class RtcEngineController
    {
        private static RtcEngineController _rtcEngineController;
        private IAgoraRtcEngine _agoraRtcEngine;
        internal IAgoraKtvEventListener _agoraKtvEventListener;
        
        public static RtcEngineController GetInstance()
        {
            if (_rtcEngineController == null)
            {
                _rtcEngineController = new RtcEngineController();
            }

            return _rtcEngineController;
        }

        private RtcEngineController()
        {
            InitAgoraRtcEngine();
            InitRtcEngineEvent();
        }
        
        public IAgoraRtcEngine GetRtcEngine()
        {
            return _agoraRtcEngine;
        }

        private void InitAgoraRtcEngine()
        {
            _agoraRtcEngine = AgoraRtcEngine.CreateAgoraRtcEngine();
            RtcEngineContext context = new RtcEngineContext(null, GameApplication.AppId, null, true,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            _agoraRtcEngine.Initialize(context);
            _agoraRtcEngine.SetLogFile("./log.txt");
            _agoraRtcEngine.EnableAudio();
            _agoraRtcEngine.EnableVideo();
            _agoraRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            EnableSpatialAudio(true);
        }

        private void InitRtcEngineEvent()
        {
            UserEventHandler handler = new UserEventHandler(this);
            _agoraRtcEngine.InitEventHandler(handler);
        }

        public void AddEventListener(IAgoraKtvEventListener listener)
        {
            _agoraKtvEventListener = listener;
        }

        public void Dispose()
        {
            if (_agoraRtcEngine == null)
            {
                Debug.Log("RtcEngineController _agoraRtcEngine is null, you can't dispose!");
            }

            _agoraRtcEngine.Dispose(true);
            _agoraRtcEngine = null;
            _rtcEngineController = null;
        }

        public void EnableSpatialAudio(bool enable)
        {
            var ret = _agoraRtcEngine.EnableSpatialAudio(enable);
            Debug.Log("RtcEngineController EnableSpatialAudio returns: " + ret);
        }

        public void JoinChannel(uint uid, string channelId)
        {
            _agoraRtcEngine?.JoinChannel("", channelId, "", uid);
        }

        public void JoinChannelEx(string channelName, uint uid)
        {
            RtcConnection connection = new RtcConnection();
            connection.channelId = channelName;
            connection.localUid = uid;
            ChannelMediaOptions options2 = new ChannelMediaOptions();
            options2.autoSubscribeAudio = true;
            options2.autoSubscribeVideo = true;
            options2.publishAudioTrack = true;
            options2.publishCameraTrack = false;
            options2.enableAudioRecordingOrPlayout = true;
            options2.clientRoleType = CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER;
            var ret = _agoraRtcEngine?.JoinChannelEx("", connection, options2, null);
            Debug.Log("RtcEngineController JoinChannelEx returns: " + ret);
        }

        public void JoinChannelEx_MPK(string channelName, uint uid, int playerId)
        {
            RtcConnection connection = new RtcConnection();
            connection.channelId = channelName;
            connection.localUid = 67890;
            ChannelMediaOptions options2 = new ChannelMediaOptions();
            options2.autoSubscribeAudio = false;
            options2.autoSubscribeVideo = false;
            options2.publishAudioTrack = false;
            options2.publishCameraTrack = false;
            options2.publishMediaPlayerAudioTrack = true;
            options2.publishMediaPlayerVideoTrack = true;
            options2.publishMediaPlayerId = playerId;
            options2.enableAudioRecordingOrPlayout = false;
            options2.clientRoleType = CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER;
            var ret = _agoraRtcEngine?.JoinChannelEx("", connection, options2, null);
            Debug.Log("RtcEngineController JoinChannelEx_MPK returns: " + ret);
        }

        public int SetExternalVideoSource()
        {
            return _agoraRtcEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME, new EncodedVideoTrackOptions());
        }

        public int CreateDataStream()
        {
            DataStreamConfig config = new DataStreamConfig();
            config.ordered = true;
            config.syncWithAudio = true;
            return _agoraRtcEngine.CreateDataStream(config);
        }

        public int SendStreamMessage(int streamId, byte[] message)
        {
            DataStreamConfig config = new DataStreamConfig();
            config.ordered = true;
            config.syncWithAudio = true;
            return _agoraRtcEngine.SendStreamMessage(streamId, message, (uint) message.Length);
        }

        public void StartAudioMixing(string url)
        {
            var ret =  _agoraRtcEngine.StartAudioMixing(url, false, false, 1);
            Debug.Log("RtcEngineController StartAudioMixing returns: " + ret);
        }

        public void AdjustRecordingSignalVolume(int volume)
        {
            var ret = _agoraRtcEngine.AdjustRecordingSignalVolume(volume);
            Debug.Log("RtcEngineController AdjustRecordingSignalVolume returns: " + ret + " volume is: " + volume);
        }

        public void AdjustUserPlaybackSignalVolume(uint uid, int volume)
        {
            var ret = _agoraRtcEngine.AdjustUserPlaybackSignalVolume(uid, volume);
            Debug.Log("RtcEngineController AdjustUserPlaybackSignalVolume returns: " + ret + " volume is: " + volume);
        }

        public int LeaveChannel(uint uid, string channelId)
        {
            RtcConnection connection = new RtcConnection(channelId, uid);
            return _agoraRtcEngine.LeaveChannelEx(connection);
        }

        public void SetRemoteUserSpatialAudioParamsEx(uint remoteUid, uint localUid, string channelId, SpatialAudioParams param)
        {
            var ret = _agoraRtcEngine.SetRemoteUserSpatialAudioParamsEx(remoteUid, param, new RtcConnection(channelId, localUid));
            Debug.Log("RtcEngineController SetRemoteUserSpatialAudioParamsEx returns: " + ret + " uid is: " + remoteUid);
        }
    }

    internal class UserEventHandler : IAgoraRtcEngineEventHandler
    {
        private readonly RtcEngineController _rtcEngineController;

        internal UserEventHandler(RtcEngineController rtcEngineController)
        {
            _rtcEngineController = rtcEngineController;
        }

        public override void OnWarning(int warn, string msg)
        {
            Debug.Log("[KTV LOG] OnWarning uid:" + warn);
        }

        public override void OnError(int err, string msg)
        {
            Debug.Log("[KTV LOG] OnError uid:" + err);
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _rtcEngineController._agoraKtvEventListener?.OnJoinChannelSuccess(connection, elapsed);
            Debug.Log("[KTV LOG] OnJoinChannelSuccess uid:" + connection.localUid);
        }
        
        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _rtcEngineController._agoraKtvEventListener?.OnLeaveChannel(connection, stats);
            Debug.Log("[KTV LOG] OnLeaveChannel uid:" + connection.localUid);
        }
        
        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _rtcEngineController._agoraKtvEventListener?.OnUserJoined(connection, uid, elapsed);
            Debug.Log("[KTV LOG] OnUserJoined uid:" + uid);
        }

        public override void OnUserOffline(RtcConnection connection, uint remoteUid, USER_OFFLINE_REASON_TYPE reason)
        {
            _rtcEngineController._agoraKtvEventListener?.OnUserOffline(connection, remoteUid, reason);
            Debug.Log("[KTV LOG] OnUserOffline uid:" + remoteUid);
        }
    }
}