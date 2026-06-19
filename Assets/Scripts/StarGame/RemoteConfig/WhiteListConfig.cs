using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text;

/// <summary>
/// 远程配置
/// </summary>
public static class WhiteListConfig
{
    static public List<string> WhiteListsLocal = new List<string>();
    

    public static async UniTask Init(string url)
    {
        //读取本地Config.xml
        ParseXml(Resources.Load<TextAsset>("WhiteList").text);
        //请求远程
        Debug.Log($"[RemoteConfig] init url: {url}");
        await RequestConfig(url);
    }


    /// <summary>
    /// 解析Xml
    /// </summary>
    /// <param name="xmlContent"></param>
    static void ParseXml(string xmlContent)
    {
        XmlDocument document = new XmlDocument();
        document.LoadXml(xmlContent);
        XmlNode xmlNode = document.SelectSingleNode("config");
        if (xmlNode != null && xmlNode.ChildNodes.Count > 0)
        {
            // IP列表
            var WhiteLists = xmlNode.SelectSingleNode("WhiteList");
            WhiteListsLocal.Clear();
            if (WhiteLists != null && WhiteLists.ChildNodes.Count > 0)
            {
                foreach (var item in WhiteLists)
                {
                    XmlElement node = item as XmlElement;
                    if (node == null || node.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }

                    // IP地址
                    string name = node.GetAttribute("val");

                    if (!WhiteListsLocal.Contains(name))
                    {
                        WhiteListsLocal.Add(name);
                    }
                    else
                    {
                        Debug.LogError($"有相同的IMEA值:{name}");
                    }
                }
            }

         
        }
    }

    static async UniTask RequestConfig(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.timeout = 3000;
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            await webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                ParseXml(webRequest.downloadHandler.text);
            }
        }
    }
}