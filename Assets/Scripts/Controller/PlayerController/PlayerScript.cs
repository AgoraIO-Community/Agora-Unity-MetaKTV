using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    // public TextMesh playerNameText;
    // public GameObject floatingInfo;
    //
    //  private Material playerMaterialClone;
    //
    //  [SyncVar(hook = nameof(OnNameChanged))]
    //  public string name;
    //
    //  [SyncVar(hook = nameof(OnColorChanged))]
    //  public Color playerColor = Color.white;
    //
    //  void OnNameChanged(string _Old, string _New)
    //  {
    //      playerNameText.text = name;
    //  }
    //
    //  void OnColorChanged(Color _Old, Color _New)
    //  {
    //      playerNameText.color = _New;
    //      playerMaterialClone = new Material(GetComponent<Renderer>().material);
    //      playerMaterialClone.color = _New;
    //      GetComponent<Renderer>().material = playerMaterialClone;
    //  }
    
    
    public override void OnStartLocalPlayer()
    {
        Camera.main.orthographic = false;
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0f, 3f, -4f);
        Camera.main.transform.localEulerAngles = new Vector3(10f, 0f, 0f);

        // string name = GameApplication.PlayerName;
        // Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        // CmdSetupPlayer(name, color);
    }
    
    // [Command]
    // public void CmdSetupPlayer(string _name, Color _col)
    // {
    //     // player info sent to server, then server updates sync vars which handles it on all clients
    //     name = _name;
    //     playerColor = _col;
    // }

    void Update()
    {
        if (!isLocalPlayer) { return; }

        // float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
        // float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;
        //
        // transform.Rotate(0, moveX, 0);
        // transform.Translate(0, 0, moveZ);
    }
    
}

