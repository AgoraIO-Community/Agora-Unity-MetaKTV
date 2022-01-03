using System.Collections;
using System.Collections.Generic;
using System.Linq;
using agora.rtc;
using UnityEngine;

namespace agora.KTV
{

    public class SpatialAudioController
    {
        private static SpatialAudioController _SpatialAudioController;
        private IAgoraRtcEngine _agoraRtcEngine;
        private IAgoraRtcSpatialAudioEngine _spatialAudioEngine;

        public static SpatialAudioController GetInstance(IAgoraRtcEngine agoraRtcEngine)
        {
            if (_SpatialAudioController == null)
            {
                _SpatialAudioController = new SpatialAudioController(agoraRtcEngine);
            }

            return _SpatialAudioController;
        }

        private SpatialAudioController(IAgoraRtcEngine agoraRtcEngine)
        {
            _agoraRtcEngine = agoraRtcEngine;
            InitLocalSpatialAudioEngine();
        }

        private int InitLocalSpatialAudioEngine()
        {
            _spatialAudioEngine = _agoraRtcEngine.GetAgoraRtcSpatialAudioEngine();
            if (_spatialAudioEngine == null)
            {
                Debug.Log("SpatialAudioController: AgoraRtcEngine is null, please SetAgoraRtcEngine first!");
                return -100;
            }

            _spatialAudioEngine?.SetAudioRecvRange(1000.5f);
            _spatialAudioEngine?.SetDistanceUnit(0.9f);
            _spatialAudioEngine?.EnableSpeaker(true);
            _spatialAudioEngine?.EnableMic(true);
            return 0;
        }

        public void Dispose()
        {
            _spatialAudioEngine?.Dispose();
        }

        public void UpdateSelfPosition(float[] position, float[] axisForward, float[] axisRight, float[] axisUp, RtcConnection connection)
        {
            var ret = _spatialAudioEngine?.UpdateSelfPositionEx(position, axisForward, axisRight, axisUp, connection);
            Debug.Log("UpdateSelfPosition return:" + ret + " position" + position);
        }

        public void UpdateRemotePosition(uint remoteUid, float[] position, float[] forward, RtcConnection connection)
        {
            RemoteVoicePositionInfo info = new RemoteVoicePositionInfo(position, forward);
            var ret = _spatialAudioEngine?.UpdateRemotePositionEx(remoteUid, position, forward, connection);
            Debug.Log("UpdateRemotePosition return:" + ret + " position" + position + " remoteuid" + remoteUid);
        }

        public void UpdatePlayerPosition(int playerId, float[] position, float[] forward)
        {
            RemoteVoicePositionInfo info = new RemoteVoicePositionInfo(position, forward);
            var ret = _spatialAudioEngine?.UpdatePlayerPositionInfo(playerId, position, forward);
            Debug.Log("position1.x" + position[0] + "position.y" + position[1] + "position.z" + position[2]);
            Debug.Log("UpdatePlayerPosition return:" + ret + " position" + position);
        }
    }

}