using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora.rtc;

namespace agora.KTV
{
    public class KtvTestPanel : MonoBehaviour
    {
        private Toggle AirToggle;
        private Toggle BlurToggle;
        private Dropdown uidDropDown;
        
        private RtcEngineController _rtcEngineController = null;
        private bool enable_air_absorb = false;
        private bool enable_blur = false;
        private uint uid;

        // Start is called before the first frame update
        void Start()
        {
            
        }
        

        // Update is called once per frame
        void Update()
        {

        }

        public void SetUpUI()
        {
            AirToggle = GameObject.Find("TestPanel/AirAbsorbToggle").GetComponent<Toggle>();
            AirToggle.onValueChanged.AddListener(onAirToggle);
            AirToggle.isOn = false;
            
            BlurToggle = GameObject.Find("TestPanel/BlurToggle").GetComponent<Toggle>();
            BlurToggle.onValueChanged.AddListener(onBlurToggle);
            BlurToggle.isOn = false;
            
            uidDropDown = GameObject.Find("TestPanel/UidDropdown").GetComponent<Dropdown>();
            uidDropDown.ClearOptions();
            uidDropDown.AddOptions(GameApplication.PlayerUidList);
            uidDropDown.onValueChanged.AddListener(onDropDownChosen);
            uid = System.Convert.ToUInt32(GameApplication.PlayerUidList[0]);
        }

        private void onAirToggle(bool enable)
        {
            enable_air_absorb = enable;
        }
        
        private void onBlurToggle(bool enable)
        {
            enable_blur = enable;
        }
        
        private void onDropDownChosen(int index)
        {
            Debug.Log("onDropDownChosen " + index);
            Debug.Log("onDropDownChosen uid is:"+ GameApplication.PlayerUidList[index]);
            uid = System.Convert.ToUInt32(GameApplication.PlayerUidList[index]);
            Debug.Log("onDropDownChosen uid is:"+ uid);
        }

        public void OpenKtvTestPanel()
        {
            _rtcEngineController = RtcEngineController.GetInstance();
        }

        public void BeginTest()
        {
            SpatialAudioParams audioParams = new SpatialAudioParams();
            audioParams.enable_air_absorb = enable_air_absorb;
            audioParams.enable_blur = enable_blur;
            Debug.Log("Local Player uid is:"+ GameApplication.uid);
            if (GameApplication.isOwner && uid == 67890)
            {
                MediaPlayerController.GetInstance(_rtcEngineController.GetRtcEngine())
                    .MediaPlayerSetSpatialAudioParams(GameApplication.playerId, audioParams);
            }
            else
            {
                _rtcEngineController?.SetRemoteUserSpatialAudioParamsEx(uid, GameApplication.uid, GameApplication.ChannelId, audioParams);
            }
        }
    }
}