using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct PlayerInfo1
{
    public uint uid;
    public Vector3 position;
}

public class Player : NetworkBehaviour
{
    public event System.Action<int> OnPlayerNumberChanged;
    public event System.Action<Player, PlayerInfo1> OnPlayerPositionChanged;
    public event System.Action<uint> OnPlayerUidChanged;

    // Players List to manage playerNumber
    internal static readonly List<Player> playersList = new List<Player>();
    
    
    internal static void ResetPlayerNumbers()
    {
        int playerNumber = 0;
        foreach (Player player in playersList)
        {
           player.playerNumber = playerNumber++;
        }
    }
    
    [Header("Player UI")]
    public GameObject playerUIPrefab;
    GameObject playerUI;

    [Header("SyncVars")]
    [SyncVar(hook = nameof(PlayerNumberChanged))]
    public int playerNumber = 0;
    
    [SyncVar(hook = nameof(PlayerPositionChanges))]
    public Vector3 playerPosition;

    [SyncVar(hook = nameof(PlayerUidChanged))]
    public uint uid = 0;
    
    [SyncVar]
    public string playerName;

    void PlayerNumberChanged(int _, int newPlayerNumber)
    {
        OnPlayerNumberChanged?.Invoke(newPlayerNumber);
    }

    void PlayerPositionChanges(Vector3 _, Vector3 newPosition)
    {
        PlayerInfo1 info = new PlayerInfo1();
        info.position = newPosition;
        info.uid = this.uid;
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
        
        // set thr Player Uid
        //uid = 67890;

        // Start generating updates
        //InvokeRepeating(nameof(UpdateData), 1, 1);
    }

    public override void OnStopServer()
    {
        CancelInvoke();
        playersList.Remove(this);
    }
    
    public override void OnStartClient()
    {
        PlayerInfo1 info = new PlayerInfo1();
        info.position = playerPosition;
        info.uid = uid;
        OnPlayerNumberChanged += OnPlayerNumberChangedHandler;
        OnPlayerPositionChanged += OnPlayerPositionChangedHandler;
        OnPlayerUidChanged += OnPlayerUidChangedHandler;

        // Invoke all event handlers with the current data
        OnPlayerNumberChanged.Invoke(playerNumber);
        OnPlayerPositionChanged.Invoke(this, info);
        OnPlayerUidChanged.Invoke(uid);
    }
    
    public override void OnStopClient()
    {

    }

    void OnPlayerNumberChangedHandler(int playerNumber)
    {
        Debug.Log("OnPlayerNumberChangedHandler playerNumber: " + playerNumber);
    }

    void OnPlayerPositionChangedHandler(Player p, PlayerInfo1 PlayerInfo)
    {
        var position = PlayerInfo.position;
        Debug.Log("uid:" + p.playerName + " OnPlayerPositionChangedHandler x:" + position.x + " y:" + position.y + " z:" + position.z);
        //Player player = NetworkClient.connection.identity.GetComponent<Player>();
    }

    void OnPlayerUidChangedHandler(uint uid)
    {
        Debug.Log("OnPlayerUidChangedHandler uid:" + uid);
    }
    
    // This only runs on the server, called from OnStartServer via InvokeRepeating
    [ServerCallback]
    void UpdateData()
    {
        playerPosition = this.transform.position;
    }

    private void Update()
    {
        playerPosition = this.transform.position;
    }
}
