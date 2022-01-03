using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using System.Net;
using agora.rtc.LitJson;

public class GameApplication : MonoBehaviour
{
    public GameObject gameCanvas;
    
    [SerializeField]
    public static string AppId = "";

    [SerializeField]
    public static string ChannelId = "";
    
    [SerializeField]
    public static string CustomerKey = "";
    
    [SerializeField]
    public static string CustomerSecret = "";
    
    public static int playerId = 0;

    public static bool canPlay = false;

    public static string PlayerName;
    
    public static uint uid;
    
    public static List<string> PlayerUidList = new List<string>();


    public static bool isOwner = false;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        Debug.Log("GameApplication Awake");

#if UNITY_ANDROID || UNITY_IOS && !UNITY_EDITOR_OSX
        
#else
        gameCanvas.SetActive(false);
#endif
    }

    private void Update()
    {
        PermissionHelper.RequestMicrophontPermission();
        PermissionHelper.RequestCameraPermission();
    }

    /// <summary>
    /// Callback sent to all game objects before the application is quit.
    /// </summary>
    void OnApplicationQuit()
    {

    }
}
