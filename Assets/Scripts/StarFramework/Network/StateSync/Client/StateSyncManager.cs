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
using System;
using System.Collections.Generic;


/// <summary>
/// 状态同步，也含有帧对其的概念
/// 帧锁定同步，上下文具备强烈的关系
/// 帧非锁定同步，【只是按照时间---帧---进行快照对齐】
/// 状态同步：【有必要同步的相应频次（NoLock），（必要的上传），有必要的数据同步SetInitAttr，同步数据的简化（片段数据）】
/// 时间对其，帧映射，数据映射
/// 同步频次/和同步数据强(有强逻辑，有强大的通知同步富余性，和可选性:所以事件信息都不连续）
/// 状态同步关键点：时间=>帧驱动Logic/View 
/// 客户端必要知道所有的时间
/// 根据服务器去对其，达到同步效果，所以追帧肯定存在
/// </summary>
namespace SGF.Network.SSPLite.Client
{
    public class SSPManager
    {
        public string LOG_TAG = "SSPManager";

        private SSPParam m_Param;
        //private SSPClient m_Client;
        //客户端对服务器给定时间的帧数对其处理
        private bool m_IsRunning = false;

        private ulong m_MinePlayerId = 0;

        //接收逻辑
        //private Action<SSPFrame> m_RecvListener;

        //本地表现
        //TODO：通过心跳，客户端记录帧，通过时间模拟帧数应该做的事情
        //不必模拟服务器，所以只有Client


        public void Start(SSPParam param, ulong playerId)
        {
            //FSPManager
            m_Param = param;
            m_MinePlayerId = playerId;
            LOG_TAG = "FSPManager[" + playerId + "]";
            //SetFSPListener(OnFSPListener);
           
        }

        public void Stop()
        {
           
        }
        //internal void SetFrameListener(Action<SSPFrame> listener)
        //{
        //    m_RecvListener = listener;
        //}






        /// <summary>
        /// 监听来自SSPClient的帧数据
        /// </summary>
        /// <param name="frame"></param>
        private void OnSSPListener(SSPParam frame)
        {
           
        }

        /// <summary>
        /// 由外界驱动
        /// </summary>
        public void EnterFrame()
        {
            if (!m_IsRunning)
            {
                return;
            }

          
        }

    














        //======================================================================

        //public string ToDebugString()
        //{
        //    string str = "";
        //    if (m_FrameCtrl != null)
        //    {
        //        str += ("NewestFrameId:" + m_FrameCtrl.NewestFrameId) + "; ";
        //        str += ("PlayedFrameId:" + m_CurrentFrameIndex) + "; ";
        //        str += ("IsInBuffing:" + m_FrameCtrl.IsInBuffing) + "; ";
        //        str += ("IsInSpeedUp:" + m_FrameCtrl.IsInSpeedUp) + "; ";
        //        str += ("FrameBufferSize:" + m_FrameCtrl.FrameBufferSize) + "; ";
        //    }

        //    if (m_Client != null)
        //    {
        //        str += m_Client.ToDebugString();
        //    }

        //    return str;
        //}
    }
}
