////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
 * 描述：
 * 工程 ：StarProject
*/
using System.Collections.Generic;
using ProtoBuf;


namespace SGF.Network.SSPLite
{
    //==========================================================
    #region SSP启动参数定义

    //[ProtoContract]
    public class SSPParam
    {

        public uint sid;

        public int clientFrameRateMultiple = 2;

        public bool enableSpeedUp = true;

        public int defaultSpeed = 1;

        public int authId = 0;


        //public SSPParam Clone()
        //{
        //    byte[] buffer = PBSerializer.NSerialize(this);
        //    return (SSPParam)PBSerializer.NDeserialize(buffer, typeof(SSPParam));
        //}
    }
    #endregion

    //==========================================================

    //==========================================================
    #region 客户端上报的数据定义

    #endregion


    //==========================================================
    #region 服务器下发的数据定义
    [ProtoContract]
    public class SSPDataS2C
    {
        /// <summary>
        /// 因为客户端监听的时间，是按照服务器逻辑处理的，对其服务器时间，计算客户端应该播放的时间
        /// NetWorkManager OnMyUpdate
        /// </summary>
        //[ProtoMember(1)]
       // public int Time//   = new List<FSPFrame>();
    }


    #endregion


    #region 公用数据结构定义

    /// <summary>
    /// 为了兼容键盘和轮盘操作，将玩家的操作抽象为【虚拟按键+参数】的【命令】形式：VKey+Arg
    /// </summary>






    //==========================================================
    //公共数据定义

    #endregion

}

