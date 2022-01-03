using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DownloadImage : MonoBehaviour
{
    [Header("图片地址")] [SerializeField] private string url =
        "https://accpic.sd-rtn.com/pic/release/jpg/1/7a8941/2da8105a-10d9-4b91-99d4-37383704f2ad.jpg";
    // Use this for initialization
    void Start () {
        StartCoroutine(DownSprite());

    }
   
    IEnumerator DownSprite()
    {
        UnityWebRequest wr = new UnityWebRequest(url);
        DownloadHandlerTexture texD1 = new DownloadHandlerTexture(true);
        wr.downloadHandler = texD1;
        yield return wr.SendWebRequest();
        int width = 1920;
        int high = 1080;
        if (!wr.isNetworkError)
        {
            Texture2D tex = new Texture2D(width, high);
            tex = texD1.texture;

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            transform.Find("Image").GetComponent<Image>().sprite = sprite;
        }
    }
   
    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }
}

