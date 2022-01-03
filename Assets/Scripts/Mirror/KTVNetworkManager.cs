using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace agora.KTV
{

    public class KTVNetworkManager : NetworkManager
    {
        // Set by UI element UsernameInput OnValueChanged
        public string PlayerName { get; set; }
        
        public string RoomName { get; set; }
        
        private System.Random _random = new System.Random();
        private Dropdown dropDown;
        private List<string> IdentityList = new List<string> {"Audience", "Broadcaster"};  

        // Called by UI element NetworkAddressInput.OnValueChanged

        public override void Awake()
        {
            networkAddress = "58.211.16.78";
            
            dropDown = GameObject.Find("Dropdown").GetComponent<Dropdown>();
            dropDown.ClearOptions();
            dropDown.AddOptions(IdentityList);
            dropDown.onValueChanged.AddListener(onDropDownChosen);
        }

        private void onDropDownChosen(int index)
        {
            Debug.Log("onDropDownChosen " + index);
            switch (index)
            {
                case 0:
                    GameApplication.isOwner = false;
                    break;
                case 1:
                    GameApplication.isOwner = true;
                    break;
            }
        }
        
        public void SetHostname(string hostname)
        {
            networkAddress = hostname;
        }

        public void SetRoomName()
        {
            GameApplication.ChannelId = RoomName;
            GameApplication.PlayerName = PlayerName;
        }

        public void SetClientRole()
        {
            GameApplication.isOwner = true;
        }

        public struct CreatePlayerMessage : NetworkMessage
        {
            public uint name;
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
        }
        
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
        
            // tell the server to create a player with this name
            conn.Send(new CreatePlayerMessage {name = (uint) _random.Next(10000, 100000000)});
        }
        
        void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage createPlayerMessage)
        {
            // create a gameobject using the name supplied by client
            GameObject playergo = Instantiate(playerPrefab);
            playergo.GetComponent<AgoraKTV>().playerName = createPlayerMessage.name;
        
            // set it as the player
            NetworkServer.AddPlayerForConnection(connection, playergo);
        }
    }
}