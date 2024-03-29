using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[AddComponentMenu("")]
public class PositionNetworkManager : NetworkManager
{
    // Set by UI element UsernameInput OnValueChanged
    public string PlayerName { get; set; }

    // Called by UI element NetworkAddressInput.OnValueChanged
    public void SetHostname(string hostname)
    {
        networkAddress = hostname;
    }

    public struct CreatePlayerMessage : NetworkMessage
    {
        public string name;
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
        conn.Send(new CreatePlayerMessage { name = PlayerName });
    }

    void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage createPlayerMessage)
    {
        // create a gameobject using the name supplied by client
        GameObject playergo = Instantiate(playerPrefab);
        playergo.GetComponent<Player>().playerName = createPlayerMessage.name;

        // set it as the player
        NetworkServer.AddPlayerForConnection(connection, playergo);
    }
}
