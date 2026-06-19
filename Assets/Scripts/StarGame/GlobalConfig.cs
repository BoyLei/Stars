using System;
/// <summary>
/// 全局配置，在app 和game之间的
/// </summary>
namespace StarProjectDef
{
    //---【内网IP区分】---
    //192.168.0.1
    //172.
    //10.
    //---【格式：Json】
    //---【策略：Post：Form】
    //---【端口：8001】
    //---【登录模式：内网通过OpenId + 渠道】
    //---【设计模式（老）1，一个永久的【公告】服务器，2,永久选服务服务器，3，Login中转Gate，4，Logic，5，人物同步，6，场景同步】
    //---【设计模式（新）2,永久选服务服务器，3，Login中转Gate，4，Logic，5，人物同步，6，场景同步，7主城堡，8野外，9副本，0跨服】
    //---【还有一个协议：上线的协议【服务器注释掉了】】
    public static class GlobalData
    {
        #region 部署地址
        // 【------外网版署环境------】

        //public const string sGetGameNoticeIpAddress = "http://172.31.3.213:9201/getbroad/";

        //------内外网分割线----------------

        //【-----内网环境------】

        //内网：公共服务器链接地址：压根不用改链接地址，选择不同人的服务器直接链接即可登录时候，服务器会自己统一中转  
        //public const string sGetGameServerIpAddress = "http://10.225.254.180:8001/";
        // 海星地址
        //public const string sGetGameServerIpAddress = "http://10.191.72.37:8001/";
        // 孔磊地址
        public const string sGetGameServerIpAddress = "http://10.191.73.20:8001/";

        //内网-连调：AAA
        // public const string sGetGameServerIpAddress = "http://10.225.254.180:8001/";

        //内网-连调：BBB
        //public const string sGetGameServerIpAddress = "http://10.225.254.180:8001/";

        //内网-连调：CCC
        //public const string sGetGameServerIpAddress = "http://10.225.254.180:8001/";

        #endregion





















        /*------外网版署环境------【旧的】
       * //【获取公告,停服,cg播放等状态】
      public const string sGetGameNoticeIpAddress = "http://172.31.3.213:9201/getbroad/";//"http://47.89.243.83:6600/";//许旋 "http://172.31.0.191:8090/"
      // viking - 原逻辑
      //public const string sGetGameServerIpAddress = "http://47.89.243.83:6601/";
      // viking - 新逻辑:因为要把accountId，链接到url后面，所以不能用const了
      //public const string sGetGameServerIpAddress = "http://172.31.3.213:9101/login?clusterid=2&acct=";
      public const string sGetGameServerIpAddress = "http://58.246.249.178:9101/login?clusterid=101&acct="; //"http://58.246.249.178:9301/login?clusterid=101&acct="; //http://172.31.3.213:9101/login?clusterid=1&acct=
      */
    }
}