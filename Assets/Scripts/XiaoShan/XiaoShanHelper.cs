///--------------------------------------------------------------------
/// 文件名   :   XiaoShanHelper.cs
/// 内  容   :   
/// 说  明   :  用于给小闪发消息
/// 创建日期 :   2022/09/22 09:35:25
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;

public static class XiaoShanHelper 
{
    /// <summary>
    /// 小闪机器人Token串
    /// </summary>
    public const string WEB_HOOK = @"https://wapi.zhimagame.net:6543/robot/webhook/v2?access_token=2024229:14891:cH4qj0uKR5xyJ8pOz5BesjqKNZglWcHxUIGu";

    /// <summary>
    /// 
    /// </summary>
    public static void SendTextMsg(string content,string keyword)
    {

        XiaoShanMessage xiaoShan = new XiaoShanMessage();

        xiaoShan.msgtype = "text";
        XiaoShanContent shanContent = new XiaoShanContent();
        shanContent.content = keyword + ":" + content;
        xiaoShan.text = shanContent;
        string textMsg=  Newtonsoft.Json.JsonConvert.SerializeObject(xiaoShan);
        Debug.Log(textMsg);
        string s = Post(textMsg, null);

    }


    public static void SendLink(string title,string content, string keyword,string messageUrl)
    {
        XiaoShanLinkMessage xiaoShan = new XiaoShanLinkMessage();

        xiaoShan.link.title = title;
        xiaoShan.link.text = keyword + ":" + content;
        xiaoShan.link.messageUrl = messageUrl;
        xiaoShan.link.photoId = 1575051148295929856;

        string textMsg = Newtonsoft.Json.JsonConvert.SerializeObject(xiaoShan);
        Debug.Log(textMsg);
        string s = Post(textMsg, null);
    }

    #region Post
    /// <summary>
    /// 以Post方式提交命令
    /// </summary>
    /// <param name="apiurl">请求的URL</param>
    /// <param name="jsonstring">请求的json参数</param>
    /// <param name="headers">请求头的key-value字典</param>
    private static string Post(string jsonstring, Dictionary<string, string> headers = null)
    {
        //远程证书无效
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate,
                     X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };

        WebRequest request = WebRequest.Create(WEB_HOOK);
        request.Method = "POST";
        request.ContentType = "application/json";
        if (headers != null)
        {
            foreach (var keyValue in headers)
            {
                if (keyValue.Key == "Content-Type")
                {
                    request.ContentType = keyValue.Value;
                    continue;
                }
                request.Headers.Add(keyValue.Key, keyValue.Value);
            }
        }

        if (string.IsNullOrEmpty(jsonstring))
        {
            request.ContentLength = 0;
        }
        else
        {
            byte[] bs = Encoding.UTF8.GetBytes(jsonstring);
            request.ContentLength = bs.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(bs, 0, bs.Length);
            newStream.Close();
        }


        WebResponse response = request.GetResponse();
        Stream stream = response.GetResponseStream();
        Encoding encode = Encoding.UTF8;
        StreamReader reader = new StreamReader(stream, encode);
        string resultJson = reader.ReadToEnd();
        return resultJson;
    }
    #endregion

}

public class XiaoShanMessage
{
    public string msgtype;

    public XiaoShanContent text;

}

public class XiaoShanContent
{
    public string content;

}

public class XiaoShanLinkMessage
{
    public string msgtype="link";
    public XiaoShanLink link = new XiaoShanLink();
}

public class XiaoShanLink
{
    public string title;
    public string text;
    public string messageUrl;
    public string picUrl;
    public ulong photoId;

}



