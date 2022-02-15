using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{

    
    
    public override void OnStartLocalPlayer()
    {
        Camera.main.orthographic = false;
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0f, 3f, -4f);
        Camera.main.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
        
        player = this.transform;
        transform.LookAt(player.position);
        offsetPosition = transform.position - player.position;

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
    private Transform player;
    private Vector3 offsetPosition;//位置偏移
    private bool isRotating = false;
        
         
    public float distance = 0;
    public float scrollSpeed = 10;
    public float rotateSpeed = 2;
    void Update()
    {
        if (!isLocalPlayer) { return; }
        
        transform.position = offsetPosition + player.position;
        //处理视野的旋转
        RotateView();
        //处理视野的拉近和拉远效果
        ScrollView();

        // float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
        // float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;
        //
        // transform.Rotate(0, moveX, 0);
        // transform.Translate(0, 0, moveZ);
    }
    
    void ScrollView() {
        //print(Input.GetAxis("Mouse ScrollWheel"));//向后 返回负值 (拉近视野) 向前滑动 返回正值(拉远视野)
        distance = offsetPosition.magnitude;
        distance += Input.GetAxis("Mouse ScrollWheel")*scrollSpeed;
        distance = Mathf.Clamp(distance, 2, 18);
        offsetPosition = offsetPosition.normalized * distance;
    }
         
    void RotateView() {
        Input.GetAxis("Mouse X");//得到鼠标在水平方向的滑动
        Input.GetAxis("Mouse Y");//得到鼠标在垂直方向的滑动
        if (Input.GetMouseButtonDown(1)) {
            isRotating = true;
        }
        if (Input.GetMouseButtonUp(1)) {
            isRotating = false;
        }
             
        if (isRotating) {
            transform.RotateAround(player.position,player.up, rotateSpeed * Input.GetAxis("Mouse X"));
                 
            Vector3 originalPos = transform.position;
            Quaternion originalRotation = transform.rotation;
                 
            transform.RotateAround(player.position,transform.right, -rotateSpeed * Input.GetAxis("Mouse Y"));//影响的属性有两个 一个是position 一个是rotation
            float x = transform.eulerAngles.x;
            if (x < 10 || x > 80) {//当超出范围之后，我们将属性归位原来的，就是让旋转无效 
                transform.position = originalPos;
                transform.rotation = originalRotation;
            }
                 
        }
             
        offsetPosition = transform.position - player.position;
    }
    
}

