using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Data;

using agora.rtc;
using agora.rtc.LitJson;

namespace agora.KTV
{
    public class MvInfo
    {
        public string resolution { set; get; }
        public string mvUrl { set; get; }
    }

    public enum HttpRequestType
    {
        Get
    }

    public class HttpRequest
    {
        public void Request(HttpRequestType type, string url, Action<string> result_callback)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            // 拼接客户 ID 和客户密钥
            string plainCredential = GameApplication.CustomerKey + ":" + GameApplication.CustomerSecret;

            // 使用 base64 进行编码
            var plainTextBytes = Encoding.UTF8.GetBytes(plainCredential);
            string encodedCredential = Convert.ToBase64String(plainTextBytes);

            // 创建 authorization header
            string authorizationHeader = "Authorization: Basic " + encodedCredential;

            try
            {
                Debug.Log("MusicRequestLog：请求准备");
                request = (HttpWebRequest) WebRequest.Create(url);
                request.Method = "GET";

                // 添加 authorization header
                request.Headers.Add(authorizationHeader);
                request.ContentType = "application/json";

                response = (HttpWebResponse) request.GetResponse();

                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    string result = reader.ReadToEnd();
                    Debug.Log("MusicRequestLog: " + result);
                    result_callback(result);
                }

                Debug.Log("MusicRequestLog: 请求完成");
            }
            catch (Exception ex)
            {
                Debug.LogError("MusicRequestLog: 错误==>>" + ex.Message);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }

                if (request != null)
                {
                    //System.Threading.Thread.Sleep(2000);
                    //request.Abort();
                }
            }
        }
    }
}