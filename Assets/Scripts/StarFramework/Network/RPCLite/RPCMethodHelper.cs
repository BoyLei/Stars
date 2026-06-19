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
using System.Linq;
using System.Net;
using System.Text;
/// <summary>
/// C2C,C2S,S2S,S2C
/// 方法调用方法，除了网络压缩传输外，本质是反射
/// 除了网路组（IP，port，msgid，msgbody-{内容，方法参数}）
/// 实际应用有：直接调用，注册等回调，调用网络派发
/// 【本方法是回调组的拓展】
/// </summary>
namespace SGF.Network.RPCLite
{
    //定义了，0~9泛型参的 + Ip端口组，的代理，10种肯定够了
    //设计意义：【某来源】注册给我，我（延迟，集中）调用，通知响应【来源】
    public delegate void RPCMethod(IPEndPoint target);
    public delegate void RPCMethod<T0>(T0 arg0, IPEndPoint target);
    public delegate void RPCMethod<T0, T1>(T0 arg0, T1 arg1, IPEndPoint target);
    public delegate void RPCMethod<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2, IPEndPoint target);
    public delegate void RPCMethod<T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, IPEndPoint target);
    public delegate void RPCMethod<T0, T1, T2, T3, T4>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, IPEndPoint target);
    public delegate void RPCMethod<T0, T1, T2, T3, T4, T5>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, IPEndPoint target);
    public delegate void RPCMethod<T0, T1, T2, T3, T4, T5, T6>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, IPEndPoint target);
    public delegate void RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, IPEndPoint target);
    public delegate void RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, IPEndPoint target);
    public delegate void RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, IPEndPoint target);

    /// <summary>
    /// 【就是一个方法，和方法调用的封装】
    /// 接下来所有子类都包含
    /// 名字，回掉，调用最小组合
    /// 【固】1，名称属性
    /// 
    /// 【拓展】2，子类必然拓展的的 N 参（如7参）委托
    /// 【拓展】3，特殊的执行方式
    /// 
    /// 【可二级拓展】4可以开放拓展的自定义：ip处理
    /// </summary>
    public class RPCMethodHelper
    {
        /// <summary>
        /// 无用
        /// </summary>
        public string name;
        /// <summary>
        /// 类类型，0参泛型委托，最最最基础的委托
        /// </summary>
        public RPCMethod method;

        /// <summary>
        /// 虚方法，必须执行RPC基础方法帮助器，其他子类自定义对IP组的处理
        /// </summary>
        /// <param name="args"></param>
        /// <param name="target"></param>
        public virtual void Invoke(object[] args, IPEndPoint target)
        {
            method.Invoke(target);
        }
    }

    public class RPCMethodHelper<T0> : RPCMethodHelper
    {
        new public RPCMethod<T0> method;

        /// <summary>
        /// 拆箱
        /// 健壮：你给我一组，我也只用第一个，即使你方法掉错了（请及时更正使用多参方法）
        /// </summary>
        /// <param name="args"></param>
        /// <param name="target"></param>
        public override void Invoke(object[] args, IPEndPoint target)
        {
            method((T0)args[0], target);
        }
    }

    /// <summary>
    /// 泛型顺序，正如传参顺序
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    public class RPCMethodHelper<T0, T1> : RPCMethodHelper
    {
        new public RPCMethod<T0, T1> method;

        public override void Invoke(object[] args, IPEndPoint target)
        {
            method.Invoke((T0)args[0], (T1)args[1], target);

        }
    }

    /// <summary>
    /// 如发现数据不够，可封装成类传进来
    /// 但尽量不要，10个参数基本够用
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class RPCMethodHelper<T0, T1, T2> : RPCMethodHelper
    {
        new public RPCMethod<T0, T1, T2> method;

        public override void Invoke(object[] args, IPEndPoint target)
        {
            method.Invoke((T0)args[0], (T1)args[1], (T2)args[2], target);
        }
    }

    public class RPCMethodHelper<T0, T1, T2, T3> : RPCMethodHelper
    {
        new public RPCMethod<T0, T1, T2, T3> method;

        public override void Invoke(object[] args, IPEndPoint target)
        {
            method.Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], target);
        }
    }

    public class RPCMethodHelper<T0, T1, T2, T3, T4> : RPCMethodHelper
    {
        new public RPCMethod<T0, T1, T2, T3, T4> method;

        public override void Invoke(object[] args, IPEndPoint target)
        {
            method.Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], target);
        }
    }

    public class RPCMethodHelper<T0, T1, T2, T3, T4, T5> : RPCMethodHelper
    {
        new public RPCMethod<T0, T1, T2, T3, T4, T5> method;

        public override void Invoke(object[] args, IPEndPoint target)
        {
            method.Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], target);
        }
    }

    public class RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6> : RPCMethodHelper
    {
        new public RPCMethod<T0, T1, T2, T3, T4, T5, T6> method;

        public override void Invoke(object[] args, IPEndPoint target)
        {
            method.Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], target);
        }
    }

    public class RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7> : RPCMethodHelper
    {
        new public RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7> method;

        public override void Invoke(object[] args, IPEndPoint target)
        {
            method.Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], target);
        }
    }
    public class RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8> : RPCMethodHelper
    {
        new public RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8> method;
        public override void Invoke(object[] args, IPEndPoint target)
        {
            method.Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], target);
        }
    }

    public class RPCMethodHelper<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : RPCMethodHelper
    {
        new public RPCMethod<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> method;

        public override void Invoke(object[] args, IPEndPoint target)
        {
            method.Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], target);
        }
    }
}
