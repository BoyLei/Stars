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
/// <summary>
/// 联通关键点：1IP地址的唯一指向（当然局域网穿透需要Nat）
/// 2Port，通讯分支的分发端口
/// 3协议id，用于标识双方按照哪一类规范来解析
/// 4PB,自定义：解析规范
/// 5二进制流传输（包体）
/// 6字符长度/类型描述（枚举，固定长度）
/// 7对应解析：序列化反序列化（内存取值），反射内存赋值（深拷贝）
/// 8自己封装部分，消息加密解密，自定义消息读取方式
/// 9结果调用，****和过程化调用
/// </summary>

using SGF.Network.KCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
//using SGF.Network.KCP;


/// <summary>
/// 1,包含不关心姓名的注册方式
/// 2,直接调用远程（C，S）的调用方式
/// 
/// 绑定 ：接收者约定的名称  和 方法的绑定
/// 发送：对1特定方法的发送，2约定映射方法的发送
/// 接受:Socket 1，自己的反射 , 2 内部映射处理的反射
/// TODO:[段磊]Socket修改成KcpSocket（优先），或，替换成Socket；在做战场陪陪/副本模式的时候做
/// </summary>
namespace SGF.Network.RPCLite
{
    public class RPCService
    {
        private string LOG_TAG = "RPCService";
        /// <summary>
        /// 自定义 委托
        /// </summary>
        /// <param name="args">参数</param>
        /// <param name="targetAddress">Add家族协议族，端口Max，ip地址，端口属性</param>
        public delegate void CustomRPC(object[] args, IPEndPoint targetAddress);

        //KCPSocket：TODO段磊
        private KCPSocket m_Socket;
        //启动标记
        private bool m_IsRunning = false;



        /// <summary>
        /// 自定义绑定名，[名字，注册的N方法s[]，调用器具] = 匿名绑定方式
        /// 自定义绑定名，[名字，注册的M方法s[]，调用器具]
        /// 设计关键：1名为绝对引用，N参方法根据实际，多函数可累加缓存
        /// 缓存了很多，
        /// </summary>
        private Dictionary<string, RPCMethodHelper> m_MapRPCBind;
        //=================================================================================
        #region 构造和析构

        public RPCService(int port = 0)
        {
            m_MapRPCBind = new Dictionary<string, RPCMethodHelper>();

            //创建Socket
            m_Socket = new KCPSocket(port, 1);//BindPort,KcpKey
            m_Socket.AddReceiveListener(OnReceive);
            m_Socket.EnableBroadcast = true;
            m_IsRunning = true;

            port = m_Socket.SelfPort;
            LOG_TAG = LOG_TAG + "[" + port + "]";
            Debuger.Log(LOG_TAG, "RPCSocket() port:{0}", port);
        }


        public virtual void Dispose()
        {
            Debuger.Log(LOG_TAG, "Dispose()");

            m_IsRunning = false;

            if (m_Socket != null)
            {
                m_Socket.Dispose();
                m_Socket = null;
            }

            m_MapRPCBind.Clear();

        }

        #endregion
        //=================================================================================
        public IPEndPoint SelfEndPoint { get { return m_Socket.SelfEndPoint; } }
        public int SelfPort { get { return m_Socket.SelfPort; } }
        public string SelfIP { get { return m_Socket.SelfIP; } }

        //=================================================================================


        //=================================================================================
        #region 主线程驱动

        public void RPCTick()
        {
            if (m_IsRunning)
            {
                m_Socket.Update();
            }
        }
        #endregion

        
        //=================================================================================
        //接收
        #region 消息接收处理: ACK, SYN, Broadcast： 收到消息，反射处理消息，绑定处理消息
        //①  SYN(synchronous建立联机)；②  ACK(acknowledgement 确认) ③  PSH(push传送) ④  FIN(finish结束) ⑤  RST(reset重置) ⑥  URG(urgent紧急)
        //接受消息自动处理
        private void OnReceive(byte[] buffer, int size, IPEndPoint remotePoint)
        {
            try
            {
                var msg = PBSerializer.NDeserialize<RPCMessage>(buffer);
                HandleRPCMessage(msg, remotePoint);
            }
            catch (Exception e)
            {
                Debuger.LogError(LOG_TAG, "OnReceive()->HandleMessage->Error:" + e.Message + "\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// 处理消息
        /// 优先查找反射消息，然后查找绑定（是一种（匿名）绑定调用的方式）
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="target"></param>
        private void HandleRPCMessage(RPCMessage msg, IPEndPoint target)
        {
            MethodInfo mi = this.GetType().GetMethod(msg.methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (mi != null)
            {
                Debuger.Log(LOG_TAG, "HandleRPCMessage() DefaultRPC:{0}, Target:{1}", msg.methodName, target);
                try
                {
                    var args = msg.args.ToList();
                    args.Add(target);

                    mi.Invoke(this, BindingFlags.NonPublic, null, args.ToArray(), null);
                }
                catch (Exception e)
                {
                    Debuger.LogError(LOG_TAG, "HandleRPCMessage() DefaultRPC<" + msg.methodName + ">响应出错:" + e.Message + "\n" + e.StackTrace + "\n");
                }
            }
            else
            {
                OnBindingRPCInvoke(msg, target);
            }
        }


        private void OnBindingRPCInvoke(RPCMessage msg, IPEndPoint target)
        {
            //查看是否绑定过
            if (m_MapRPCBind.ContainsKey(msg.methodName))
            {
                Debuger.Log(LOG_TAG, "OnBindingRPCInvoke() RPC:{0}, Target:{1}", msg.methodName, target);

                RPCMethodHelper rpc = m_MapRPCBind[msg.methodName];


                try
                {
                    rpc.Invoke(msg.args, target);
                }
                catch (Exception e)
                {
                    Debuger.LogError(LOG_TAG, "OnBindingRPCInvoke() RPC<" + msg.methodName + ">响应出错:" + e.Message + "\n" + e.StackTrace + "\n");
                }

            }
            else
            {
                Debuger.LogError(LOG_TAG, "OnBindingRPCInvoke() 收到未知的RPC:{0}", msg.methodName);
            }
        }


        #endregion











        //=================================================================================
        //RPC 调用 
        /// 单ip调用
        public void RPC(IPEndPoint target, string name, params object[] args)
        {
            Debuger.Log(LOG_TAG, "RPC() 1对1调用, name:{0}, target:{1}", name, target);

            RPCMessage msg = new RPCMessage();
            msg.methodName = name;
            msg.args = args;
            SendMessage(target, msg);

        }
        /// 多ip调用
        public void RPC(List<IPEndPoint> listTargets, string name, params object[] args)
        {
            Debuger.Log(LOG_TAG, "RPC() 1对多调用, Begin, msg:{0}", name);

            RPCMessage msg = new RPCMessage();
            msg.methodName = name;
            msg.args = args;
            SendMessage(listTargets, msg);

            Debuger.Log(LOG_TAG, "RPC() 1对多调用, End!");
        }
        /// 批量端口调用
        public void RPC(int beginPort, int endPort, string name, params object[] args)
        {
            Debuger.Log(LOG_TAG, "RPC() 广播调用, PortRange:{0}-{1}, Begin, msg:{2}", beginPort, endPort, name);

            RPCMessage msg = new RPCMessage();
            msg.methodName = name;
            msg.args = args;
            SendBroadcast(beginPort, endPort, msg);
        }

        /// <summary>
        /// C2c/s2s的调用，本地不通过网络
        /// </summary>
        /// <param name="target"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        public void RPC(RPCService target, string name, params object[] args)
        {
            //对RpcServive调用
            RPCMessage msg = new RPCMessage();
            msg.methodName = name;
            msg.args = args;
            target.HandleRPCMessage(msg, null);
        }

        //==========================================================================
        //单RPC发送
        #region 消息发送处理
        //发送SYN
        //直接发送RPC消息，的支持
        //地址，信息
        //调用Socket
        private void SendMessage(IPEndPoint target, RPCMessage msg)
        {
            byte[] buffer = PBSerializer.NSerialize(msg);
            m_Socket.SendTo(buffer, buffer.Length, target);
        }
        /// <summary>
        /// 群发RPC的支持
        /// </summary>
        /// <param name="listTargets"></param>
        /// <param name="msg"></param>
        private void SendMessage(List<IPEndPoint> listTargets, RPCMessage msg)
        {
            byte[] buffer = PBSerializer.NSerialize(msg);

            for (int i = 0; i < listTargets.Count; i++)
            {
                IPEndPoint target = listTargets[i];
                if (target != null)
                {
                    m_Socket.SendTo(buffer, buffer.Length, target);
                }
            }
        }

        //端口广播的支持
        private void SendBroadcast(int beginPort, int endPort, RPCMessage msg)
        {
            byte[] buffer = PBSerializer.NSerialize(msg);

            for (int i = beginPort; i < endPort; i++)
            {
                m_Socket.SendTo(buffer, buffer.Length, new IPEndPoint(IPAddress.Broadcast, i));
            }
        }

        #endregion


        //=================================================================================
      
        ///不关心姓名的绑定方式
        ///注册的方式
        ///
        /// <summary>
        /// 绑定时。
        /// </summary>
        /// <param name="name">绑定函数被调用的名字</param>
        /// <param name="rpc">主机作为绑定者的函数作为，方法传递进来</param>
        public void Bind(string name, RPCMethod rpc)
        {
            RPCMethodHelper helper = new RPCMethodHelper();
            helper.method = rpc;//方法调用其
            m_MapRPCBind[name] = helper;//名字，委托类，委托：关联
           
        }

        public void Bind<T0>(string name, RPCMethod<T0> rpc)
        {
            RPCMethodHelper<T0> helper = new RPCMethodHelper<T0>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1>(string name, RPCMethod<T0, T1> rpc)
        {
            RPCMethodHelper<T0, T1> helper = new RPCMethodHelper<T0, T1>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2>(string name, RPCMethod<T0, T1, T2> rpc)
        {
            RPCMethodHelper<T0, T1, T2> helper = new RPCMethodHelper<T0, T1, T2>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3>(string name, RPCMethod<T0, T1, T2, T3> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3> helper = new RPCMethodHelper<T0, T1, T2, T3>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4>(string name, RPCMethod<T0, T1, T2, T3, T4> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4> helper = new RPCMethodHelper<T0, T1, T2, T3, T4>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4, T5>(string name, RPCMethod<T0, T1, T2, T3, T4, T5> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4, T5> helper = new RPCMethodHelper<T0, T1, T2, T3, T4, T5>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6> helper = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6, T7>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7> helper = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8> helper = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }

        public void Bind<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name, RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> rpc)
        {
            RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> helper = new RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>();
            m_MapRPCBind[name] = helper;
            helper.method = rpc;
        }


    }
}
