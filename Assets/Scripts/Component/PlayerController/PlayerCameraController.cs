using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    public override void OnStartClient()
    {
        if (!hasAuthority) return;
        Camera.main.orthographic = false;
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0f, 2.5f, -2f);
        Camera.main.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
    }
}

