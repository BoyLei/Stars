using System.Collections;
using System.Collections.Generic;
using Best.HTTP.JSON.LitJson;
using Best.HTTP.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Best.HTTP.Examples
{
    public class TestHttp : MonoBehaviour
    {
        private string GameServerIpAddress = "http://10.225.254.180:8001/";

        public Button button;

        // Start is called before the first frame update
        void Start()
        {
            button.onClick.AddListener(Test);
        }
        

        public class UserLoginReq
        {
            public string Openid;       // (玩家的openid)：【玩家id随便换一个就是新号】
            public string Channel;      // (玩家的渠道)
        }

        private void Test()
        {
            UserLoginReq userLoginReq = new UserLoginReq();
            userLoginReq.Openid = "10001_210423703009";
            userLoginReq.Channel = "1";


            string msgId = "userLoginNew";

            CommonHttpServerReq<UserLoginReq>(msgId, (HTTPRequest originalRequest, HTTPResponse response) =>
            {

            }, userLoginReq);
        }


        public void CommonHttpServerReq<T>(string mgsId, OnRequestFinishedDelegate onReqFinishedDel, T json, string str = "")
        {
            string url = GameServerIpAddress + mgsId;
            UnityEngine.Debug.Log($" CommonHttpServerReq GameServerIpAddress: {GameServerIpAddress},  mgsId={mgsId},json={json} url={url} , str: {str}");

            try
            {
                HTTPRequest httpReq = HTTPRequest.CreatePost(url, (HTTPRequest originalRequest, HTTPResponse response) =>
                {
                    Debug.LogWarning($" CommonHttpServerReq  response.DataAsText={response.DataAsText}");
                    onReqFinishedDel?.Invoke(originalRequest, response);
                });
     
                httpReq.SetHeader("Content-Type", "application/json");
                httpReq.SetHeader("Accept", "application/json");
             
                
                httpReq.UploadSettings.UploadStream = new Best.HTTP.Request.Upload.JSonDataStream<T>(json);
                httpReq.Send();

            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($" CommonHttpServerReq GameServerIpAddress: {GameServerIpAddress}, mgsId: {mgsId} , url: {url} 捕获异常: {e.Message} , stack: {e.StackTrace}");
            }

        }
    }
}
