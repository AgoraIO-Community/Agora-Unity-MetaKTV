using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
public class iosHttp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if (UNITY_IOS)
        HttpWebRequest request = (HttpWebRequest) WebRequest.Create("https://www.baidu.com");
        request.Method = "GET";

        HttpWebResponse response = (HttpWebResponse) request.GetResponse();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
