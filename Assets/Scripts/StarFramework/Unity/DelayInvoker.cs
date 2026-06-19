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
using UnityEngine;
using System.Collections;
using System.Threading;

namespace SGF.Unity
{

    [XLua.LuaCallCSharp]
    public delegate void DelayFunction(object[] args);

    [XLua.LuaCallCSharp]
    public class DelayInvoker : MonoSingletonEx<DelayInvoker>
    {
        private List<DelayHelper> m_lstHelper;
        private List<DelayHelper> m_lstUnscaledHelper;
        private static WaitForEndOfFrame ms_waitForEndOfFrame = new WaitForEndOfFrame();
        private static WaitForFixedUpdate ms_waitForEndOfFixFrame = new WaitForFixedUpdate();

        class DelayHelper
        {
            public object group;
            public float delay;
            public DelayFunction func;
            public object[] args;

            public void Invoke()
            {
                if (func != null)
                {
                    try
                    {
                        func(args);
                    }
                    catch (Exception e)
                    {
                        Debuger.LogError("DelayInvoker", "Invoke() Error:{0}\n{1}", e.Message, e.StackTrace);
                    }

                }
            }
        }

        public static void DelayInvoke(object group, float delay, DelayFunction func, params object[] args)
        {
            DelayInvoker.Instance.DelayInvokeWorker(group, delay, func, args);
        }

        public static bool ContainInvoke(object group)
        {
            return DelayInvoker.Instance.ContainInvokerWorker(group);
        }


        public static void DelayInvoke(float delay, DelayFunction func, params object[] args)
        {
            DelayInvoker.Instance.DelayInvokeWorker(null, delay, func, args);
        }

        public static void UnscaledDelayInvoke(float delay, DelayFunction func, params object[] args)
        {
            DelayInvoker.Instance.UnscaledDelayInvokeWorker(null, delay, func, args);
        }

        public static void CancelInvoke(object group)
        {
            DelayInvoker.Instance.CancelInvokeWorker(group);
        }

        //====================================================================

        private void DelayInvokeWorker(object group, float delay, DelayFunction func, params object[] args)
        {
            if (m_lstHelper == null)
            {
                m_lstHelper = new List<DelayHelper>();
            }

            DelayHelper helper = new DelayHelper();
            helper.group = group;
            helper.delay = delay;
            helper.func += func;
            helper.args = args;

            m_lstHelper.Add(helper);
        }

        private void UnscaledDelayInvokeWorker(object group, float delay, DelayFunction func, params object[] args)
        {
            if (m_lstUnscaledHelper == null)
            {
                m_lstUnscaledHelper = new List<DelayHelper>();
            }

            DelayHelper helper = new DelayHelper();
            helper.group = group;
            helper.delay = delay;
            helper.func += func;
            helper.args = args;

            m_lstUnscaledHelper.Add(helper);
        }

        private bool ContainInvokerWorker(object group)
        {
            if (null == m_lstHelper)
            {
                return false;
            }
            if (null == group)
            {
                return false;
            }
            for (int i = 0; i < m_lstHelper.Count(); ++i)
            {

                DelayHelper helper = m_lstHelper[i];


                if (group.Equals(helper.group))
                {
                    return true;
                }
            }
            return false;
        }

        private void CancelInvokeWorker(object group)
        {
            if (null != m_lstHelper)
            {
                if (group == null)
                {

                    for (int i = 0; i < m_lstHelper.Count; i++)
                    {
                        m_lstHelper[i] = null;
                    }
                    m_lstHelper.Clear();

                    return;
                }

                for (int i = 0; i < m_lstHelper.Count(); ++i)
                {

                    DelayHelper helper = m_lstHelper[i];

                    /// <summary>
                    /// bug fix:  group无法取消
                    /// 原因: helper.group == group 在 由字符串转换为 object 后, 使用  ==  直接比较可能根据 使用的方式，返回 false/True
                    /// exp:
                    ///     int temp = 199;
                    ///     string a = $"{temp}_aaa";
                    ///     int temp1 = 199;
                    ///     string a1 = $"{temp1}_aaa"; ;
                    ///     string a2 = a; ;
                    /// 
                    ///     object b = (object)a;
                    ///     object c = (object)a1;
                    ///     object c2 = (object)a2;
                    ///     
                    ///     b == c  ======》》》 False
                    ///     b == C2 ======》》》 True
                    /// </summary>
                    if (group.Equals(helper.group))
                    {
                        //Debug.LogError($"[DelayInvoker] remove group : [{group}]");
                        m_lstHelper.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        private void CancelAllInvokeWorkers()
        {
            if (m_lstHelper != null)
            {
                for (int i = 0; i < m_lstHelper.Count; i++)
                {
                    m_lstHelper[i] = null;
                }
                m_lstHelper.Clear();
            }

            if (m_lstUnscaledHelper != null)
            {
                for (int i = 0; i < m_lstUnscaledHelper.Count; i++)
                {
                    m_lstUnscaledHelper[i] = null;
                }
                m_lstUnscaledHelper.Clear();
            }
         
        }

        //void Update()
        //{
        //    //逻辑层的所有延迟执行都从 Update 挪到了 FixUpdate 中
        //    //表现也需要Fix同步（逻辑驱动表现）
        //    //逻辑也需要fix
        //    //这里整体说一下：逻辑就是30帧，表现也是30帧数，但是表现过度（1-unity动画帮忙补充到60帧，2-是过度到60帧数，3-全是30驱动60有引擎帮忙的有dg三方帮忙的也有自己写帮忙的才可控
        //}
        private void FixedUpdate()
        {
            if (null != m_lstHelper)
            {
                for (int i = 0; i < m_lstHelper.Count(); ++i)
                {
                    DelayHelper helper = m_lstHelper[i];
                    helper.delay -= UnityEngine.Time.deltaTime;
                    if (helper.delay <= 0)
                    {
                        m_lstHelper.RemoveAt(i);
                        i--;

                        helper.Invoke();
                    }
                }
            }

            if (null != m_lstUnscaledHelper)
            {
                for (int i = 0; i < m_lstUnscaledHelper.Count(); ++i)
                {
                    DelayHelper helper = m_lstUnscaledHelper[i];
                    helper.delay -= UnityEngine.Time.unscaledDeltaTime;
                    if (helper.delay <= 0)
                    {
                        m_lstUnscaledHelper.RemoveAt(i);
                        i--;

                        helper.Invoke();
                    }
                }

            }
        }

        //====================================================================

        void Update()
        {

        }

        void OnDisable()
        {
            //            Debug.Log("DelayInvoker Release!!!");
            CancelAllInvokeWorkers();
            this.StopAllCoroutines();
        }

        //====================================================================

        public static void DelayInvokerOnEndOfFrame(DelayFunction func, params object[] args)
        {
            Instance.StartCoroutine(DelayInvokerOnEndOfFrameWorker(func, args));
        }

        public static void DelayInvokerOnEndOfFixFrame(DelayFunction func, params object[] args)
        {
            Instance.StartCoroutine(DelayInvokerOnEndOfFixFrameWorker(func, args));
        }


        // public static void DelayInvokerOnTime(float delayTime, DelayFunction func, params object[] args)
        // {
        //     Instance.StartCoroutine(DelayInvokerOnTimeWorker(delayTime, func, args));
        // }
        private static IEnumerator DelayInvokerOnEndOfFrameWorker(DelayFunction func, params object[] args)
        {
            yield return ms_waitForEndOfFrame;
            //WaitForFixedUpdate
            //Profiler.BeginSample("DelayInvoker_DelayInvokerOnEndOfFrame");

            try
            {
                func(args);
            }
            catch (Exception e)
            {
                Debuger.LogError("DelayInvoker", "DelayInvokerOnEndOfFrame() Error:{0}\n{1}", e.Message, e.StackTrace);
            }

            //Profiler.EndSample();
        }

        private static IEnumerator DelayInvokerOnEndOfFixFrameWorker(DelayFunction func, params object[] args)
        {
            yield return ms_waitForEndOfFixFrame;
            //WaitForFixedUpdate
            //Profiler.BeginSample("DelayInvoker_DelayInvokerOnEndOfFrame");

            try
            {
                func(args);
            }
            catch (Exception e)
            {
                Debuger.LogError("DelayInvoker", "DelayInvokerOnEndOfFrame() Error:{0}\n{1}", e.Message, e.StackTrace);
            }

            //Profiler.EndSample();
        }

        // private static IEnumerator DelayInvokerOnTimeWorker(float delayTime, DelayFunction func, params object[] args)
        // {
        //     yield return new WaitForSeconds(delayTime); ;

        //     try
        //     {
        //         func(args);
        //     }
        //     catch (Exception e)
        //     {
        //         Debuger.LogError("DelayInvoker", "DelayInvokerOnEndOfFrame() Error:{0}\n{1}", e.Message, e.StackTrace);
        //     }
        //     yield return ms_waitForEndOfFixFrame;
        //     //Profiler.EndSample();
        // }

        public static void DelayTimeInvoke(float delayTime, DelayFunction func, params object[] args)
        {
            Timer timer = null;
            timer = new System.Threading.Timer((object v) =>
             {
                 try
                 {
                     func(args);
                 }
                 catch (Exception e)
                 {
                     Debuger.LogError("DelayInvoker", "DelayInvokerOnEndOfFrame() Error:{0}\n{1}", e.Message, e.StackTrace);
                 }

                 timer.Dispose();
             }, null, (int)delayTime * 1000, Timeout.Infinite);



            //Profiler.EndSample();
        }


        public static void FixedTimeInvoke(int hours, int minitue)
        {

        }
    }



}