using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using StarProject.Service.SDK;
using StarProjectDef;
using UnityEngine;

using Best.HTTP;
using Google.Protobuf;

public static class HttpUtils
{
    public enum HTTPMethods
    {
        Post,
        Get

    }
    public static readonly string CHOOSE_HERO_HANDLER = "loginAfter/ChooseHeroHandler";    // 选择某个角色进入游戏111带token 1111111111
    public static readonly int RETRY_COUNT = 2;     // HTTP 发送失败重发次数

    #region  通用的 http 相关接口
    public static Dictionary<string, string> HEADER_DEFAULT = new() { { "Content-Type", "application/json" }, { "Accept", "application/json" } };

    public static void SendGetHttp<T1, T2>(string url, T1 sendJson = null, Action<T2, bool> callBack = null, Dictionary<string, string> headers = null) where T1 : class
    {
        SendHttp(url, HTTPMethods.Get, sendJson, callBack, headers);
    }

    public static void SendGetHttp<T1, T2>(string url, T1 sendJson = null, Action<T2> callBack = null, Dictionary<string, string> headers = null) where T1 : class
    {
        SendHttp(url, HTTPMethods.Get, sendJson, (T2 value, bool result) =>
        {
            callBack?.Invoke(value);
        }, headers);
    }

    public static void SendPostHttp<T1>(string url, T1 sendJson = null, Action<bool> callBack = null, Dictionary<string, string> headers = null) where T1 : class
    {
        SendHttp(url, HTTPMethods.Post, sendJson, (object value, bool result) =>
        {
            callBack?.Invoke(result);
        }, headers);
    }

    public static void SendPostHttp<T1, T2>(string url, T1 sendJson = null, Action<T2> callBack = null, Dictionary<string, string> headers = null) where T1 : class
    {
        SendHttp(url, HTTPMethods.Post, sendJson, (T2 value, bool result) =>
        {
            callBack?.Invoke(value);
        }, headers);
    }

    public static void SendPostHttp<T1, T2>(string url, T1 sendJson = null, Action<T2, bool> callBack = null, Dictionary<string, string> headers = null) where T1 : class
    {
        SendHttp(url, HTTPMethods.Post, sendJson, callBack, headers);
    }

    public static void SendPostHttp<T1, T2, T3>(string url, T1 sendJson = null, Action<T2, T3> callBack = null, T3 resultCallBack = null, Dictionary<string, string> headers = null) where T1 : class where T3 : class
    {
        SendHttp(url, HTTPMethods.Post, sendJson, (T2 value, bool result) =>
        {
            callBack?.Invoke(value, resultCallBack);
        }, headers);
    }

    public static void SendHttp<T1, T2>(string url, HTTPMethods httPMethod, T1 sendJson = null, Action<T2, bool> callBack = null, Dictionary<string, string> headers = null, float timeOut = 20) where T1 : class
    {
        OnRequestFinishedDelegate httpCallBack = (HTTPRequest originalRequest, HTTPResponse response) =>
        {
            if (originalRequest.Exception != null)
            {
                Debug.LogError($"[HttpUtils] SendHttp url: {url}, sendJson: {sendJson} \n, has exception: {originalRequest.Exception.Message}  ");

                return;
            }
            Debug.LogWarning($"[HttpUtils] SendHttp url: {url}, hTTPMethod: {httPMethod}  response.DataAsText= {response?.DataAsText}");
            T2 value = DeserializeResponse<T2>(response, out bool result);

            callBack?.Invoke(value, result);
        };

        HTTPRequest httpReq = null;

        switch (httPMethod)
        {
            case HTTPMethods.Post:
                {
                    httpReq = HTTPRequest.CreatePost(url, httpCallBack);
                }
                break;
            case HTTPMethods.Get:
                {
                    httpReq = HTTPRequest.CreateGet(url, httpCallBack);
                }
                break;
        }


        httpReq.TimeoutSettings.ConnectTimeout = TimeSpan.FromSeconds(timeOut / 2);
        httpReq.TimeoutSettings.Timeout = TimeSpan.FromSeconds(timeOut);


        if (headers == null)
        {
            headers = HEADER_DEFAULT;
        }

        foreach (var item in headers)
        {
            httpReq.SetHeader(item.Key, item.Value);
        }

        if (sendJson != null)
        {
            // Type type = typeof(T1);
            // type.GetInterfaces().Any(iface => iface == typeof(IMessage));
            var iMessage = sendJson as IMessage;
            if (iMessage == null)
            {
                httpReq.UploadSettings.UploadStream = new Best.HTTP.Request.Upload.JSonDataStream<T1>(sendJson);
            }
            else
            {
                byte[] srcStr = System.Text.Encoding.UTF8.GetBytes(sendJson.ToString());
                httpReq.UploadSettings.UploadStream = new System.IO.MemoryStream(srcStr);

            }


            Debug.LogWarning($"[HttpUtils] SendHttp url: {url}, hTTPMethod: {httPMethod}  sendJson : {sendJson}");

        }
        else
        {
            Debug.LogWarning($"[HttpUtils] SendHttp url: {url}, hTTPMethod: {httPMethod} sendJson : null");

        }

        httpReq.Send();

    }

    /// <summary>
    /// 根据Resp信息，获取一个T类型的实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="response"></param>
    public static T DeserializeResponse<T>(HTTPResponse response, out bool result)
    {
        /*string errorCode = "";
        string statusCode = "";*/
        if (response == null)
        {
            result = false;
            SGF.Debuger.Log($"[HttpUtils] DeserializeResponse {typeof(T).Name} 返回 null");
            return default(T);
        }
        string str = System.Text.Encoding.Default.GetString(response.Data);
        //SGF.Debuger.LogWarning($"{TagFlag} ServiceListCallBack str={str}");
        //Dictionary<string, object> responseDic = BestHTTP.JSON.Json.Decode(str) as Dictionary<string, object>;
        result = true;
        return JsonConvert.DeserializeObject<T>(str);
        //JsonReader js = new JsonReader(str);
        //return JsonMapper.ToObject<T>(js);
    }

    #endregion


    #region  登录相关的 http 接口 ,登录 相关的 请求需要包含 登录的 头信息
    /// <summary>
    /// 登录相关的 头信息
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, string> Header_Login = new();

    /// <summary>
    /// 得到 登录相关需要的 头文件
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string> GetLoginHeaders()
    {
        Header_Login.Clear();

        Header_Login.Add("isadult", GameLoginInfo.M_UserLoginNAck.IsAdult.ToString());
        Header_Login.Add("token", GameLoginInfo.M_UserLoginNAck.AccessToken);
        Header_Login.Add("uid", GameLoginInfo.M_UserLoginNAck.UID + "");
        Header_Login.Add("channel", GameLoginInfo.M_UserLoginReq.Channel);

        // 增加 userAge 协议头, 默认 0
        {
            int userAge = 18;
            if (SDKManager.Instance.sdkLoginResult != null)
            {
                userAge = SDKManager.Instance.sdkLoginResult.userAge;
            }

            Header_Login.Add("userAge", userAge.ToString());
            SGF.Debuger.Log($"[sdk] 设置 userAge {userAge} ");

        }
        Header_Login.Add("ts", GameLoginInfo.M_UserLoginNAck.TS);
        Header_Login.Add("Content-Type", "application/json");
        Header_Login.Add("Accept", "application/json");

        return Header_Login;
    }

    public static void SendLoginHeaderHttpReq<T1, T2>(string url, T1 sendJson = default, Action<T2> callBack = null) where T1 : class
    {
        SendPostHttp(url, sendJson, callBack, GetLoginHeaders());
    }
    public static void SendLoginHeaderHttpReq<T1, T2, T3>(string url, T1 sendJson = default, Action<T2, T3> callBack = null, T3 resultCallBack = null) where T1 : class where T3 : class
    {
        SendPostHttp(url, sendJson, callBack, resultCallBack, GetLoginHeaders());
    }

    /// <summary>
    /// 发送包含重试 接口的 登录头 http
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public static void SendRetryLoginHeaderHttpReq<T1, T2>(string serverIpAddress, string mgsId, T1 sendJson, Action<T2, bool> ac, int retryTime = -1, float timeOut = 20) where T1 : class
    {
        SendRetryHttpPostReq(serverIpAddress, mgsId, sendJson, ac, GetLoginHeaders(), retryTime, timeOut);
    }

    #endregion


    #region 发送 包含重试次数的 http 请求接口
    /// <summary>
    /// 简化的 发送 http 请求的接口
    /// </summary>
    /// <param name="serverIpAddress"></param>
    /// <param name="mgsId"></param>
    /// <param name="sendJson"></param>
    /// <param name="ac"></param>
    /// <param name="httpMethods"></param>
    /// <typeparam name="T2"></typeparam>
    public static void SendHttpReq<T1, T2>(string serverIpAddress, string mgsId, T1 sendJson, Action<T2, bool> ac, Dictionary<string, string> headers = null, HTTPMethods httpMethods = HTTPMethods.Post, float timeOut = 20) where T1 : class
    {
        string url = serverIpAddress + mgsId;
        SGF.Debuger.LogWarning($"[HttpUtils] SendHttpReq mgsId={mgsId},paras={sendJson} url={url}");

        SendHttp(url, httpMethods, sendJson, ac, headers, timeOut);
    }

    /// <summary>
    /// 发送重试的 http 请求
    /// </summary>
    /// <typeparam name="T2"></typeparam>
    /// <param name="serverIpAddress"></param>
    /// <param name="mgsId"></param>
    /// <param name="sendJson"></param>
    /// <param name="ac"></param>
    /// <param name="retryTime">失败重试次数, -1 表示默认重试次数</param>
    /// <param name="httpMethods"></param> 
    public static void SendRetryHttpReq<T1, T2>(string serverIpAddress, string mgsId, T1 sendJson, Action<T2, bool> ac, Dictionary<string, string> headers = null, int retryTime = -1, HTTPMethods httpMethods = HTTPMethods.Post, float timeOut = 20) where T1 : class
    {
        // 重试次数, -1 表示按默认 的通用 重试次数
        retryTime = retryTime < 0 ? RETRY_COUNT : retryTime;

        var curTryTime = 0;

        TrySendHttpReq(serverIpAddress, mgsId, sendJson, ac, curTryTime, retryTime, headers, httpMethods, timeOut);
    }

    public static void SendRetryHttpPostReq<T1, T2>(string serverIpAddress, string mgsId, T1 sendJson, Action<T2, bool> ac, Dictionary<string, string> headers = null, int retryTime = -1, float timeOut = 20) where T1 : class
    {
        SendRetryHttpReq(serverIpAddress, mgsId, sendJson, ac, headers, retryTime, HTTPMethods.Post, timeOut);
    }

    private static void TrySendHttpReq<T1, T2>(string serverIpAddress, string mgsId, T1 sendJson, Action<T2, bool> ac, int curTryTimes, int retryTime, Dictionary<string, string> headers = null, HTTPMethods httpMethods = HTTPMethods.Post, float timeOut = 20) where T1 : class
    {
        if (curTryTimes > retryTime)
        {
            ac?.Invoke(default(T2), false);
            return;
        }
        curTryTimes++;

        SendHttpReq(serverIpAddress, mgsId, sendJson, (T2 v, bool reqResult) =>
        {
            if (!reqResult)
            {
                TrySendHttpReq(serverIpAddress, mgsId, sendJson, ac, curTryTimes, retryTime, headers, httpMethods);
                return;
            }

            // 正确拿到数据
            ac?.Invoke(v, true);

        }, headers, httpMethods, timeOut);
    }

    #endregion
}
