using System;
using agora.rtc;

namespace agora.KTV
{
    public interface IAgoraKtvEventListener
    {
        //RtcEngine
        public void OnJoinChannelSuccess(RtcConnection connection, int elapsed);
        
        public void OnLeaveChannel(RtcConnection connection, RtcStats stats);
        
        public void OnUserJoined(RtcConnection connection, uint uid, int elapsed);

        public void OnUserOffline(RtcConnection connection, uint remoteUid, USER_OFFLINE_REASON_TYPE reason);
        
        //MediaPlayer
        public void OnPlayerSourceStateChanged(int playerId, MEDIA_PLAYER_STATE state, MEDIA_PLAYER_ERROR ec);

        public void OnCompleted(int playerId);
    }
}