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
using System.Text;
using UnityEngine.Events;
using XLua;

namespace SGF.Module.Framework
{

    [LuaCallCSharp]
    /// <summary>
    /// 1参unityAction的【代理】
    /// </summary>
    public class ModuleEvent : UnityEvent<object>
    {

    }
    [LuaCallCSharp]
    public class ModuleEvent<T> : UnityEvent<T>
    {

    }

    [LuaCallCSharp]
    public class ModuleEvent<T0, T1> : UnityEvent<T0, T1>
    {

    }

    [LuaCallCSharp]
    public class ModuleEvent<T0, T1, T2> : UnityEvent<T0, T1, T2>
    {

    }


    [LuaCallCSharp]
    public class ModuleEvent<T0, T1, T2, T3> : UnityEvent<T0, T1, T2, T3>
    {

    }

    /// <summary>
    /// 不用你业务逻辑封装了，底层帮你封装了本质是一个参数，但是这个参数是数组
    /// 数组用基类存储
    /// 子类（数组不同的数据）可以用不同类型的数据进行组合，本质是合并成T0一个参数过来处理
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    [LuaCallCSharp]
    public class ModuleEvent<T0, T1, T2, T3, T4> : UnityEvent<object[]>
    {
        private object[] m_Params = new object[5];

        public void Invoke(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            m_Params[0] = t0;
            m_Params[1] = t1;
            m_Params[2] = t2;
            m_Params[3] = t3;
            m_Params[4] = t4;

            base.Invoke(m_Params);
        }
    }

    [LuaCallCSharp]
    public class ModuleEvent<T0, T1, T2, T3, T4, T5> : UnityEvent<object[]>
    {
        private object[] m_Params = new object[6];

        public void Invoke(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            m_Params[0] = t0;
            m_Params[1] = t1;
            m_Params[2] = t2;
            m_Params[3] = t3;
            m_Params[4] = t4;
            m_Params[5] = t5;

            base.Invoke(m_Params);
        }
    }

    [LuaCallCSharp]
    public class ModuleEvent<T0, T1, T2, T3, T4, T5, T6> : UnityEvent<object[]>
    {
        private object[] m_Params = new object[7];

        public void Invoke(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        {
            m_Params[0] = t0;
            m_Params[1] = t1;
            m_Params[2] = t2;
            m_Params[3] = t3;
            m_Params[4] = t4;
            m_Params[5] = t5;
            m_Params[6] = t6;

            base.Invoke(m_Params);
        }
    }

    [LuaCallCSharp]
    public class EventTable //StrKey 到 响应事件的 绑定
    {
        /// <summary>
        /// 名字，对照代理，的索引，哈希表
        /// </summary>
        private Dictionary<string, ModuleEvent> m_mapEvents;


        /// <summary>
        /// 获取Type所指定的ModuleEvent（它其实是一个EventTable）
        /// 如果不存在，则实例化一个
        /// </summary>
        /// <param name="type">事件名称</param>
        /// <returns></returns>
        public ModuleEvent GetEvent(string type)
        {
            if (m_mapEvents == null)
            {
                m_mapEvents = new Dictionary<string, ModuleEvent>();
            }
            if (!m_mapEvents.ContainsKey(type))
            {
                m_mapEvents.Add(type, new ModuleEvent());
            }
            return m_mapEvents[type];
        }

        public void Clear()
        {
            if (m_mapEvents != null)
            {
                foreach (var @event in m_mapEvents)
                {
                    @event.Value.RemoveAllListeners();
                }
                m_mapEvents.Clear();
            }
        }

    }
}
