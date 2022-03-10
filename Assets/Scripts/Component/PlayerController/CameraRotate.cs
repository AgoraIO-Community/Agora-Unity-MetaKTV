using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    private Transform player;
    private Vector3 offsetPosition;//位置偏移
    private bool isRotating = false;
        
         
    public float distance = 0;
    public float scrollSpeed = 10;
    public float rotateSpeed = 2;
    // Start is called before the first frame update
    void Start()
    {
        // player = this.transform;
        // transform.LookAt(player.position);
        // offsetPosition = transform.position - player.position;
    }
    
    public enum RotationAxes
    {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }
 
    public RotationAxes m_axes = RotationAxes.MouseXAndY;
    public float m_sensitivityX = 10f;
    public float m_sensitivityY = 10f;
 
    // 水平方向的 镜头转向
    public float m_minimumX = -90f;
    public float m_maximumX = 90f;
    // 垂直方向的 镜头转向 (这里给个限度 最大仰角为45°)
    public float m_minimumY = -45f;
    public float m_maximumY = 45f;
 
    float m_rotationY = 0f;
    

    // Update is called once per frame
    void Update()
    {
        // transform.position = offsetPosition + player.position;
        // //处理视野的旋转
        // RotateView();
        
        Input.GetAxis("Mouse X");//得到鼠标在水平方向的滑动
        Input.GetAxis("Mouse Y");//得到鼠标在垂直方向的滑动
        if (Input.GetMouseButtonDown(1)) {
            isRotating = true;
        }
        if (Input.GetMouseButtonUp(1)) {
            isRotating = false;
        }

        if (isRotating)
        {
            if (m_axes == RotationAxes.MouseXAndY) 
            {
                float m_rotationX = transform.localEulerAngles.y + Input.GetAxis ("Mouse X") * m_sensitivityX;
                m_rotationY += Input.GetAxis ("Mouse Y") * m_sensitivityY;
                m_rotationY = Mathf.Clamp (m_rotationY, m_minimumY, m_maximumY);
     
                transform.localEulerAngles = new Vector3 (10-m_rotationY, m_rotationX, 0);
            } 
            else if (m_axes == RotationAxes.MouseX) 
            {
                transform.Rotate (0, Input.GetAxis ("Mouse X") * m_sensitivityX, 0);
            } 
            else 
            {
                m_rotationY += Input.GetAxis ("Mouse Y") * m_sensitivityY;
                m_rotationY = Mathf.Clamp (m_rotationY, m_minimumY, m_maximumY);
     
                transform.localEulerAngles = new Vector3 (-m_rotationY, transform.localEulerAngles.y, 0);
            }
        }
        else
        {
            Camera.main.transform.localPosition = new Vector3(0f, 2.5f, -2f);
            Camera.main.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
        }
        
    }

    // void RotateView() {
    //     Input.GetAxis("Mouse X");//得到鼠标在水平方向的滑动
    //     Input.GetAxis("Mouse Y");//得到鼠标在垂直方向的滑动
    //     if (Input.GetMouseButtonDown(1)) {
    //         isRotating = true;
    //     }
    //     if (Input.GetMouseButtonUp(1)) {
    //         isRotating = false;
    //     }
    //          
    //     if (isRotating) {
    //         transform.RotateAround(player.position,player.up, rotateSpeed * Input.GetAxis("Mouse X"));
    //              
    //         Vector3 originalPos = transform.position;
    //         Quaternion originalRotation = transform.rotation;
    //              
    //         transform.RotateAround(player.position,transform.right, - rotateSpeed * Input.GetAxis("Mouse Y"));//影响的属性有两个 一个是position 一个是rotation
    //         float x = transform.eulerAngles.x;
    //         if (x < 10 || x > 80) {//当超出范围之后，我们将属性归位原来的，就是让旋转无效 
    //             transform.position = originalPos;
    //             transform.rotation = originalRotation;
    //         }
    //              
    //     }
    //          
    //     offsetPosition = transform.position - player.position;
    // }
}
