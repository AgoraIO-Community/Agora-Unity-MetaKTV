using System;
using UnityEngine;
using UnityEngine.UI;
using agora.rtc;

namespace agora.KTV
{
    public class MediaPlayerController
    {
        private static MediaPlayerController _mediaPlayerController;
        
        private IAgoraRtcEngine _mRtcEngine = null;
        private IAgoraRtcMediaPlayer _mediaPlayer = null;
        internal IAgoraKtvEventListener _agoraKtvEventListener;
        
        private int playerId = 0;

        public static MediaPlayerController GetInstance(IAgoraRtcEngine agoraRtcEngine)
        {
            if (_mediaPlayerController == null)
            {
                Debug.Log("[AgoraMeta] MediaPlayerController: GetInstance");
                _mediaPlayerController = new MediaPlayerController(agoraRtcEngine);
            }

            return _mediaPlayerController;
        }

        private MediaPlayerController(IAgoraRtcEngine agoraRtcEngine)
        {
            _mRtcEngine = agoraRtcEngine;
            InitMediaPlayer();
        }

        private int InitMediaPlayer()
        {
            Debug.Log("MediaPlayerController: init");
            if (_mRtcEngine == null)
            {
                Debug.Log("MediaPlayerController: AgoraRtcEngine is null, please SetAgoraRtcEngine first!");
                return -100;
            }

            _mediaPlayer = _mRtcEngine?.GetAgoraRtcMediaPlayer();

            playerId = _mediaPlayer.CreateMediaPlayer();
            GameApplication.playerId = playerId;
            
            MpkEventHandler handler = new MpkEventHandler(this);
            _mediaPlayer.InitEventHandler(handler);

            return playerId;
        }
        
        public void AddEventListener(IAgoraKtvEventListener listener)
        {
            _agoraKtvEventListener = listener;
        }

        public void DestroyMediaPlayer(int playerId)
        {
            _mediaPlayer?.DestroyMediaPlayer(playerId);
        }

        public int GetPlayerId()
        {
            return playerId;
        }

        public int TestMediaPlayer(int playerId)
        {
            if (_mediaPlayer == null)
            {
                Debug.Log("AgoraRtcEngine: GetAgoraRtcMediaPlayer failed!");
                return -100;
            }

            long duration = 0;
            var ret = _mediaPlayer.GetDuration(playerId, ref duration);
            Debug.Log("MediaPlayer GetDuration returns: " + ret + "duration: " + duration);

            long pos = 0;
            ret = _mediaPlayer.GetPlayPosition(playerId, ref pos);
            Debug.Log("MediaPlayer GetPlayPosition returns: " + ret + "position: " + pos);

            Debug.Log("MediaPlayer GetState:" + _mediaPlayer.GetState(playerId));

            bool mute = true;
            ret = _mediaPlayer.GetMute(playerId, ref mute);
            Debug.Log("MediaPlayer GetMute returns: " + ret + "mute: " + mute);

            int volume = 0;
            ret = _mediaPlayer.GetPlayoutVolume(playerId, ref volume);
            Debug.Log("MediaPlayer GetPlayoutVolume returns: " + ret + "volume: " + volume);

            Debug.Log("MediaPlayer SDK Version:" + _mediaPlayer.GetPlayerSdkVersion(playerId));
            Debug.Log("MediaPlayer GetPlaySrc:" + _mediaPlayer.GetPlaySrc(playerId));

            return ret;
        }

        public void MediaPlayerPlay(int playerId)
        {
            var ret = _mediaPlayer?.Play(playerId);
            Debug.Log("MediaPlayerController Play returns: " + ret);
        }

        public void MediaPlayerStop(int playerId)
        {
            var ret = _mediaPlayer?.Stop(playerId);
            Debug.Log("MediaPlayerController Stop returns: " + ret);
        }

        public void MediaPlayerPause(int playerId)
        {
            var ret = _mediaPlayer?.Pause(playerId);
            Debug.Log("MediaPlayerController Pause returns: " + ret);
        }

        public void MediaPlayerResume(int playerId)
        {
            var ret = _mediaPlayer?.Resume(playerId);
            Debug.Log("MediaPlayerController Resume returns: " + ret);
        }

        public void MediaPlayerOpen(int playerId, string url, long startPos)
        {
            var ret = _mediaPlayer?.Open(playerId, url, startPos);
            Debug.Log("MediaPlayer Open returns: " + ret);
        }

        public void MediaPlayerMuteAudio(int playerId)
        {
            var ret = _mediaPlayer?.MuteAudio(playerId, true);
            Debug.Log("MediaPlayer MuteAudio returns: " + ret);
        }

        public void MediaPlayerUnMuteAudio(int playerId)
        {
            var ret = _mediaPlayer?.MuteAudio(playerId, false);
            Debug.Log("MediaPlayer MuteAudio returns: " + ret);
        }

        public void MediaPlayerMuteVideo(int playerId)
        {
            var ret = _mediaPlayer?.MuteVideo(playerId, true);
            Debug.Log("MediaPlayer UnMuteVideo returns: " + ret);
        }

        public void MediaPlayerUnMuteVideo(int playerId)
        {
            var ret = _mediaPlayer?.MuteVideo(playerId, false);
            Debug.Log("MediaPlayer UnMuteVideo returns: " + ret);
        }
        
        public void MediaPlayerAdjustPlayoutVolume(int playerId, int volume)
        {
            var ret = _mediaPlayer?.AdjustPlayoutVolume(playerId, volume);
            Debug.Log("MediaPlayer AdjustPlayoutVolume returns: " + ret);
        }
        
        public void MediaPlayerAdjustPublishSignalVolume(int playerId, int volume)
        {
            var ret = _mediaPlayer?.AdjustPublishSignalVolume(playerId, volume);
            Debug.Log("MediaPlayer AdjustPublishSignalVolume returns: " + ret + " volume: " + volume);
        }

        public void MediaPlayerSelectAudioTrack(int playerId, int index)
        {
            var ret = _mediaPlayer?.SelectAudioTrack(playerId, index);
            Debug.Log("MediaPlayer SelectAudioTrack returns: " + ret);
        }

        public void MediaPlayerSetSpatialAudioParams(int playerId, SpatialAudioParams spatial_audio_params)
        {
            var ret = _mediaPlayer?.SetSpatialAudioParams(playerId, spatial_audio_params);
            Debug.Log("MediaPlayer SetSpatialAudioParams returns: " + ret);
        }
    }

    internal class MpkEventHandler : IAgoraRtcMediaPlayerEventHandler
    {
        private readonly MediaPlayerController _mediaPlayerController;

        internal MpkEventHandler(MediaPlayerController mediaPlayerController)
        {
            _mediaPlayerController = mediaPlayerController;
        }

        public override void OnPlayerSourceStateChanged(int playerId, MEDIA_PLAYER_STATE state, MEDIA_PLAYER_ERROR ec)
        {
            _mediaPlayerController._agoraKtvEventListener?.OnPlayerSourceStateChanged(playerId, state, ec);
            Debug.Log("[KTV LOG] OnPlayerSourceStateChanged: playerId " + playerId + "MEDIA_PLAYER_STATE: " + state);
        }

        public override void OnCompleted(int playerId)
        {
            _mediaPlayerController._agoraKtvEventListener?.OnCompleted(playerId);
            Debug.Log("[KTV LOG] OnCompleted: playerId " + playerId);

        }
    }

}