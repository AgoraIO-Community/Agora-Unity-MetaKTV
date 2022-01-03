using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.NetworkRoom;
using UnityEngine;
using Mirror;

namespace agora.KTV
{

    [AddComponentMenu("")]
    public class KtvRoomExt : NetworkRoomManager
    {
        [Header("Spawner Setup")] [Tooltip("Reward Prefab for the Spawner")]
        public GameObject rewardPrefab;

        /// <summary>
        /// This is called on the server when a networked scene finishes loading.
        /// </summary>
        /// <param name="sceneName">Name of the new scene.</param>
        public override void OnRoomServerSceneChanged(string sceneName)
        {

        }

        /// <summary>
        /// Called just after GamePlayer object is instantiated and just before it replaces RoomPlayer object.
        /// This is the ideal point to pass any data like player name, credentials, tokens, colors, etc.
        /// into the GamePlayer object as it is about to enter the Online scene.
        /// </summary>
        /// <param name="roomPlayer"></param>
        /// <param name="gamePlayer"></param>
        /// <returns>true unless some code in here decides it needs to abort the replacement</returns>
        public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer,
            GameObject gamePlayer)
        {
            return true;
        }

        public override void OnRoomStopClient()
        {
            base.OnRoomStopClient();
        }

        public override void OnRoomStopServer()
        {
            base.OnRoomStopServer();
        }

        /*
            This code below is to demonstrate how to do a Start button that only appears for the Host player
            showStartButton is a local bool that's needed because OnRoomServerPlayersReady is only fired when
            all players are ready, but if a player cancels their ready state there's no callback to set it back to false
            Therefore, allPlayersReady is used in combination with showStartButton to show/hide the Start button correctly.
            Setting showStartButton false when the button is pressed hides it in the game scene since NetworkRoomManager
            is set as DontDestroyOnLoad = true.
        */

        bool showStartButton;

        public override void OnRoomServerPlayersReady()
        {
            // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
#if UNITY_SERVER
        base.OnRoomServerPlayersReady();
#else
            showStartButton = true;
#endif
        }

        public override void OnGUI()
        {
            base.OnGUI();

            if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
            {
                // set to false to hide it in the game scene
                showStartButton = false;

                ServerChangeScene(GameplayScene);
            }
        }

        // Set by UI element UsernameInput OnValueChanged
        public string PlayerName { get; set; }

        public string RoomName { get; set; }

        // Called by UI element NetworkAddressInput.OnValueChanged
        public void SetHostname(string hostname)
        {
            networkAddress = hostname;
        }

        public void SetRoomName()
        {
            GameApplication.ChannelId = RoomName;
            GameApplication.PlayerName = PlayerName;
        }

        // public struct CreatePlayerMessage : NetworkMessage
        // {
        //     public uint name;
        // }
        //
        // public override void OnStartServer()
        // {
        //     base.OnStartServer();
        //     NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
        // }
        //
        // public override void OnClientConnect(NetworkConnection conn)
        // {
        //     base.OnClientConnect(conn);
        //
        //     // tell the server to create a player with this name
        //     conn.Send(new CreatePlayerMessage {name = System.Convert.ToUInt32(PlayerName)});
        // }
        //
        // void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage createPlayerMessage)
        // {
        //     // create a gameobject using the name supplied by client
        //     GameObject playergo = Instantiate(playerPrefab);
        //     playergo.GetComponent<AgoraKTV>().playerName = createPlayerMessage.name;
        //
        //     // set it as the player
        //     NetworkServer.AddPlayerForConnection(connection, playergo);
        // }
    }
}