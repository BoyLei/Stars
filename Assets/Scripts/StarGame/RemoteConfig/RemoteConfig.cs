using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using SGF;
using StarProject.Service.SDK;
using StarProjectDef;

/// <summary>
/// 远程配置
/// </summary>
public static class RemoteConfig
{
    static public Dictionary<int, IpConfig> LoginUrls = new Dictionary<int, IpConfig>();
    static public int ActiveUrl = 0;    // 当前使用的登陆服IP下标
    static public string ResUrl = string.Empty;
    static public bool OpenSDK = true;
    static public bool IsHasOpenSDK = false;

    static public string LoginUrl
    {
        get
        {
            if (ActiveUrl < 0 || ActiveUrl >= LoginUrls.Count)
            {
                Debug.LogError($"LoginUrl Not found login url,ActiveUrl={ActiveUrl} Count={LoginUrls.Count}");
                return string.Empty;
            }

            return LoginUrls[ActiveUrl].ip;
        }
    }

    static public string LoginUrlDesc
    {
        get
        {
            if (ActiveUrl < 0 || ActiveUrl >= LoginUrls.Count)
            {
                Debug.LogError($"LoginUrlDesc Not found login url,ActiveUrl={ActiveUrl} Count={LoginUrls.Count}");
                return string.Empty;
            }

            return LoginUrls[ActiveUrl].E_IP;
        }
    }

    public static async UniTask Init(string url,System.Action<bool> cb)
    {
        //请求远程
        Debug.Log($"[RemoteConfig] init url: {url}");
        await RequestConfig(url,cb).ToUniTask();
    }

    // 读取本地的 配置
    public static void InitLocal()
    {
        Debug.Log($"[RemoteConfig] 准备读取本地 配置");
        //读取本地Config.xml
        ParseXml(Resources.Load<TextAsset>("Config").text);
        Debug.Log($"[RemoteConfig] 读取本地 配置 成功!!!");

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
            var LoginUrlNodes = xmlNode.SelectSingleNode("LoginUrl");
            LoginUrls.Clear();
            if (LoginUrlNodes != null && LoginUrlNodes.ChildNodes.Count > 0)
            {
                foreach (var item in LoginUrlNodes)
                {
                    XmlElement node = item as XmlElement;
                    if (node == null || node.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }

                    // 下标
                    string strkey = node.GetAttribute("key");
                    int key = 0;
                    System.Int32.TryParse(strkey, out key);
                    // IP地址
                    string name = node.GetAttribute("vaule");
                    // IP地址描述
                    string desc = node.GetAttribute("desc");

                    if (!LoginUrls.ContainsKey(key))
                    {
                        IpConfig ipConfig = new IpConfig();
                        ipConfig.ip = name;
                        ipConfig.E_IP = desc;
                        LoginUrls.Add(key, ipConfig);
                    }
                    else
                    {
                        Debug.LogError($"有相同的key值:{key},存在不同的value值:{name}======{LoginUrls[key]}");
                    }
                }
            }

            // 默认选用的登录ip
            var ActiveUrlNode = xmlNode.SelectSingleNode("ActiveUrl");
            if (ActiveUrlNode != null)
            {
                XmlElement ActiveUrlElement = ActiveUrlNode as XmlElement;
                if (ActiveUrlElement != null)
                {
                    string activeStr = ActiveUrlElement.GetAttribute("vaule");
                    System.Int32.TryParse(activeStr, out ActiveUrl);
                }
            }

            var ResUrlNode = xmlNode.SelectSingleNode("ResUrl");
            if (ResUrlNode != null)
            {
                XmlElement ResUrlElement = ResUrlNode as XmlElement;
                if (ResUrlElement != null)
                {
                    ResUrl = ResUrlElement.GetAttribute("vaule");
                }
            }

            //OpenSDK
            var OpenSDKNode = xmlNode.SelectSingleNode("OpenSDK");
            if (OpenSDKNode != null)
            {
                XmlElement OpenSDKElement = OpenSDKNode as XmlElement;
                if (OpenSDKElement != null)
                {
                    string openSDKStr = OpenSDKElement.GetAttribute("vaule").Trim();
                    Debug.Log($"[RemoteConfig]  openSDKStr : {openSDKStr}");
                    OpenSDK = openSDKStr == "1";
                    IsHasOpenSDK = true;
                }
            }
        }
    }

    static IEnumerator  RequestConfig(string url,System.Action<bool> cb)
    {
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.timeout = 20000;
            webRequest.downloadHandler = new DownloadHandlerBuffer();
        
            // 等待请求完成
            yield return webRequest.SendWebRequest();

            // 检查请求是否成功
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // 请求成功，解析返回的数据
                ParseXml(webRequest.downloadHandler.text);
                cb?.Invoke(true);
            }
            else
            {
                SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.NET_WORK_ERROR_1);
                Debuger.LogError($"请求失败: { webRequest.error} responseCode:{webRequest.responseCode} url:{url}");
                cb?.Invoke(false);
                
                /*bool loop = true;
                while (loop)
                {
                    UIAPI.ShowSystemMsgBox(GameUpdate.GetTipsTextByKey("NetworkTipName"),GameUpdate.GetTipsTextByKey("NetworkTip"),
                        GameUpdate.GetTipsTextByKey("NetworkBtnSure"),() =>
                        {
                            RequestConfig(url)  ;
                            loop = false;
                            //重连
                        }
                        ,GameUpdate.GetTipsTextByKey("NetworkBtnFalse"),() =>
                        {
                            Application.Quit();
                           loop = false;
                        }  );
                    // 可选：在重试前暂停一段时间，避免频繁请求
                    yield return  null;
                }*/
  
            }
        }
    }
}