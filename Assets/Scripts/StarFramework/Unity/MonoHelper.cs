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
using StarProjectDef;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SGF.Unity
{
    [XLua.LuaCallCSharp]
    public delegate void StarBaseDelegate();//虽然是容器，但是设计单函数指针，因为里面全是get没法操作

    /// <summary>
    /// 处理invoke
    /// 删除dele
    /// 添加add
    /// 
    /// 
    /// 1，代理他无法立刻删除，我也不应该让他立刻删除；
    /// 2，数据当时都立刻移除了：
    /// 2.1要么就立刻可以让他删除掉
    /// 2.2要么数据延迟删除，比较麻烦
    /// 
    /// gamerestar 释放；具体某个东西的释放；释放链
    /// 事件，和数据都是release释放的
    /// 
    /// 不去重的允许添加，删除删除首个，不然增加运算成本
    /// </summary>
    [XLua.LuaCallCSharp]
    public class SingleMehtodPoint
    {
        public SingleMehtodPoint(StarBaseDelegate _monoUpdaterEvent, MonoHelper.E_ModuleType _e_ModuleType1)
        {
            monoUpdaterEvent = _monoUpdaterEvent;
            e_ModuleType1 = _e_ModuleType1;
            use = true;
        }


        public MonoHelper.E_ModuleType e_ModuleType1;
        public StarBaseDelegate monoUpdaterEvent;
        public bool use = true;

        public override bool Equals(object obj)
        {
            if (obj is SingleMehtodPoint other)
            {
                // 比较字段值
                return Equals(monoUpdaterEvent, other.monoUpdaterEvent) &&
                       e_ModuleType1.Equals(other.e_ModuleType1) &&
                       use == other.use;
            }
            return false;
        }

        public override int GetHashCode()//哈希为的containKey为o1的基础；但是遍历就很大
        {
            int hashCode = 17;
            hashCode = (hashCode * 23) + (monoUpdaterEvent != null ? monoUpdaterEvent.GetHashCode() : 0);
            hashCode = (hashCode * 23) + e_ModuleType1.GetHashCode();
            hashCode = (hashCode * 23) + use.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// 先不池化了，进和出不频繁
    /// 1,list是变长的存储一个不浪费
    /// 2,代理的单个指针封装的也是代理
    /// 3,代理呗保护了过程中不能删除，也不能标记
    /// </summary>
    [XLua.LuaCallCSharp]
    public class MessageEvent
    {
        private List<SingleMehtodPoint> starBaseDelegates = new();

        public MessageEvent(SingleMehtodPoint listener)
        {
            if (!starBaseDelegates.Contains(listener))
            {
                starBaseDelegates.Add(listener);
            }
        }

        public void AddListener(SingleMehtodPoint listener)
        {
            //也不管理重复
            starBaseDelegates.Add(listener);
            //也不管理重复
            /*  if (!starBaseDelegates.Contains(listener))
              {
                  starBaseDelegates.Add(listener);
              }*/
        }



        /// <summary>
        /// 删除标记用处
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(StarBaseDelegate listener)
        {
            foreach (var item in starBaseDelegates)
            {
                //不处理重复，只要发现首个，不删除
                if (item.monoUpdaterEvent == listener && item.use != false)
                {
                    item.use = false;
                    // starBaseDelegates.Remove(item);
                    break;
                    //不去重的添加，删除也删除首个
                }
                //这里不涉及安全删除
                /* var itemToRemove = starBaseDelegates.FirstOrDefault(item => item.monoUpdaterEvent == listener);
                 if (itemToRemove != null)
                 {
                     starBaseDelegates.Remove(itemToRemove);
                 }*/
            }
        }

        /// <summary>
        /// 删除标记用处
        /// </summary>
        /// <param name="listener"></param>
        public void ClearUnuse()
        {
            var itemsToRemove = starBaseDelegates.Where
                (item => !item.use ||
                item.monoUpdaterEvent == null ||
                   item.monoUpdaterEvent.Target == "null" ||
                   item.monoUpdaterEvent.Method == null)
                .ToList();


            foreach (var item in itemsToRemove)
            {
                starBaseDelegates.Remove(item);
            }
        }

        public void Invoke()
        {
            var delListCache = starBaseDelegates.ToList();
            foreach (var del in delListCache)
            {

                try
                {
                    if (del?.use == true)
                    {
                        /*     Debug.LogError($"monoUpdaterEvent: {del.monoUpdaterEvent != null}");
                             Debug.LogError($"Target: {del.monoUpdaterEvent?.Target != null}");
                             Debug.LogError($"Method: {del.monoUpdaterEvent?.Method != null}");*/
                        // 检查 monoUpdaterEvent 是否有效
                        if (del.monoUpdaterEvent.Target != null && del.monoUpdaterEvent.Method != null)
                        {
                            // 检查 del.monoUpdaterEvent 关联的对象是否仍然有效
                            var targetObject = del.monoUpdaterEvent.Target;

                            // 根据 targetObject 的实际类型进行检查
                            if (targetObject is MonoBehaviour)
                            {
                                MonoBehaviour mono = (MonoBehaviour)targetObject;
                                // 如果是 MonoBehaviour 类型，检查其 GameObject 是否有效
                                if (mono.gameObject != null && mono.gameObject.activeInHierarchy)
                                {
                                    del.monoUpdaterEvent.Invoke();
                                }
                            }
                            else
                            {
                                // 处理其他类型的情况（如果需要）
                                // 例如：检查对象是否已被销毁或其他条件
                                // 此处的处理取决于 targetObject 的实际类型和需求
                                del.monoUpdaterEvent?.Invoke();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (del != null)
                    {
                        del.use = false;
                    }
                    // 处理异常或记录日志
                    Debug.LogError($"Invoke failed: {ex.Message}, {ex.StackTrace}  ,{ex?.InnerException},{ex?.InnerException?.StackTrace}");
                }
            }
        }

        // 重载事件的 + 和 - 操作符
        public static MessageEvent operator +(MessageEvent me, SingleMehtodPoint listener)
        {
            me.AddListener(listener);
            return me;
        }

        public static MessageEvent operator -(MessageEvent me, StarBaseDelegate listener)
        {
            me.RemoveListener(listener);
            return me;
        }
    }




    [XLua.LuaCallCSharp]
    public class MonoHelper : MonoSingletonEx<MonoHelper>
    {
        public enum E_Priority
        {
            First,
            Middle,
            Last
        }

        public enum E_ModuleType
        {
            Net = 0,
            CommonService = 1,
            Battle = 2,
            CommonBusiness = 3,
            PathFinding = 4,
            Render = 5,
            Resource = 6,
            Count//
        }



        public T[] FindObjectsOfType<T>() where T : Component
        {
            return GameObject.FindObjectsOfType<T>();//全场景的挂载的
            //Instance.FindObjectsOfType<T>();一个自己身上的,是小写的，是自己身上挂载的的gameobject
        }
        //===========================================================
        /// <summary>
        /// 一个list理解一个代理
        /// 处理器优化: 现代处理器和编译器都有优化技术，如预取（prefetching）和缓存优化，这些技术能减少内存访问延迟。因此，即使 Dictionary 的元素分布可能不连续，现代处理器的优化也能有效缓解这些影响。
        //内存管理: 操作系统和内存管理单元（MMU）会进行内存分页和缓存优化，这在一定程度上减轻了不同数据结构之间的性能差异。
        //实际上就算没有多倍，怎么也有二倍以上。
        //要严谨！！！
        //ConcurrentList 不存在的点在于他很难线程安全
        //我需要支持多线程
        //ConcurrentQueue 维度上类似list，可实际游戏里增删不频繁都是在游戏启动结束对驱动注册比较多，【3d2d，管理类】大多性能重头在于通知，增删比较少 随机访问少，遍历多，删除不固定位置，内存拓展好Queue更合适点但是稳定的时候在替换吧
        /// </summary>
        private ConcurrentDictionary<E_ModuleType, MessageEvent> UpdateEvents = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> UpdateEventsCache = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> FirstFixedUpdateEvents = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> FirstFixedUpdateEventsCache = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> FixedUpdateEvents = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> FixedUpdateEventsCache = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> LastFixedUpdateEvents = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> LastFixedUpdateEventsCache = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> LateUpdateEvents = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> LateUpdateEventsCache = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> TimeSecUpdateEvents = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> TimeSecUpdateEventsCache = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> TimeTenSecUpdateEvents = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> TimeTenSecUpdateEventsCache = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> TimeMinutesUpdateEvents = new();
        private ConcurrentDictionary<E_ModuleType, MessageEvent> TimeMinutesUpdateEventsCache = new();

        //添加：运行中不想添加让他多加数据；所以结束时一起给延迟也无所谓。
        //删除：段磊需要我帮忙立刻处理标记即可；也不关心状态；需要采用标注，处理标注，结尾一起删除 是O（3n）=有可能remove的没通知属于快速remove这次remove不通知【最后一次少一次通知为代价能接受都要推出了】，或者通知的没remove正常；缓存是稳On2+on；大量数据运算上秒的时候，结构是时间复杂度，空间复杂度，欺骗用户，分类，其他维度解决，能估量的损失都能选取；
        //3n的处理方式，所以锁定缓存加是需要的，缓存删除是不用的用标记，状态标记会直接删除不行会影响删除两个东西锁定操作额外东西没有了---不锁定操作当前删除不行所以锁和删除list都不要

        private void Awake()
        {
            //TODO:整合成一个，不用unity的，全自己写
            //TODO:拓展协同
            InvokeRepeating("OnSecTick", 0, GameConfig.SEC_RATE);//每1秒执行,但是这样就不精准
            InvokeRepeating("OnTenSecTick", 0, GameConfig.TEN_SEC_RATE);//每10秒执行,但是这样就不精准
            InvokeRepeating("OnMinutesTick", 0, GameConfig.MINUTES_RATE);//每60秒执行,但是这样就不精准


            //有内存地址删除，条目没有删除，各种维度的注册监听没有删除
            /* for (int i = 0,maxcount = (int)E_ModuleType.Count; i < maxcount; i++)
             {
                 AddUpdateListener(null, (E_ModuleType)i);
             }*/

        }


        /// <summary>
        /// 缓存维度，时间维度，模块维度，【不用检测重复的情况下 list增加O1=字典增加也是O1】字典 == list ；执行这块换成list节省一半以上；(如果list这块要检测重复增加删除会O1变成On) 整体是O2n 也 没字典耗；【要不要字典换稳===不必】
        /// 基于分类是否损耗的讨论，这里如果去掉key会变成list，
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="e_ModuleType"></param>
        public static void AddUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness)
        {
            if (Instance != null)
            {
                if (!Instance.UpdateEvents.ContainsKey(e_ModuleType))
                {
                    if (!Instance.UpdateEventsCache.ContainsKey(e_ModuleType))
                    {
                        MessageEvent me = new(new SingleMehtodPoint(listener, e_ModuleType));
                        Instance.UpdateEventsCache.TryAdd(e_ModuleType, me);
                    }
                    else
                    {
                        Instance.UpdateEventsCache[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                    }

                }
                else
                {
                    Instance.UpdateEvents[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                }

            }
        }
        /// <summary>
        /// 处理invoke期
        /// 减少期                         缓存
        /// 增加期
        ///                                
        /// 
        ///                                 直接处理
        /// 
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="e_ModuleType"></param>
        public static void RemoveUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness)
        {
            if (Instance != null)
            {
                if (!Instance.UpdateEvents.ContainsKey(e_ModuleType))
                {
                    //Instance.UpdateEvents.remove(e_ModuleType, listener);
                }
                else
                {
                    Instance.UpdateEvents[e_ModuleType] -= listener;
                    //不处理删除 

                }

            }
        }

        /// <summary>
        /// LateUpdate执行频率依赖update
        /// </summary>
        /// <param name="listener"></param>
        public static void AddLaterUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness)
        {

            if (Instance != null)
            {
                if (!Instance.LateUpdateEvents.ContainsKey(e_ModuleType))
                {
                    if (!Instance.LateUpdateEventsCache.ContainsKey(e_ModuleType))
                    {
                        MessageEvent me = new(new SingleMehtodPoint(listener, e_ModuleType));
                        Instance.LateUpdateEventsCache.TryAdd(e_ModuleType, me);
                    }
                    else
                    {
                        Instance.LateUpdateEventsCache[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                    }

                }
                else
                {
                    Instance.LateUpdateEvents[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                }

            }
        }

        public static void RemoveLaterUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness)
        {
            if (Instance != null)
            {
                if (!Instance.LateUpdateEvents.ContainsKey(e_ModuleType))
                {
                    //Instance.UpdateEvents.remove(e_ModuleType, listener);
                }
                else
                {
                    Instance.LateUpdateEvents[e_ModuleType] -= listener;
                    //不处理删除 

                }

            }
        }

        public static void AddFixedUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness, E_Priority e_Priority = E_Priority.Middle)
        {
            if (Instance != null)
            {
                switch (e_Priority)
                {
                    case E_Priority.First:

                        if (!Instance.FirstFixedUpdateEvents.ContainsKey(e_ModuleType))
                        {
                            if (!Instance.FirstFixedUpdateEventsCache.ContainsKey(e_ModuleType))
                            {
                                MessageEvent me = new(new SingleMehtodPoint(listener, e_ModuleType));
                                Instance.FirstFixedUpdateEventsCache.TryAdd(e_ModuleType, me);
                            }
                            else
                            {
                                Instance.FirstFixedUpdateEventsCache[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                            }

                        }
                        else
                        {
                            Instance.FirstFixedUpdateEvents[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                        }


                        break;
                    case E_Priority.Middle:

                        if (!Instance.FixedUpdateEvents.ContainsKey(e_ModuleType))
                        {
                            if (!Instance.FixedUpdateEventsCache.ContainsKey(e_ModuleType))
                            {
                                MessageEvent me = new(new SingleMehtodPoint(listener, e_ModuleType));
                                Instance.FixedUpdateEventsCache.TryAdd(e_ModuleType, me);
                            }
                            else
                            {
                                Instance.FixedUpdateEventsCache[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                            }

                        }
                        else
                        {
                            Instance.FixedUpdateEvents[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                        }

                        break;
                    case E_Priority.Last:

                        if (!Instance.LastFixedUpdateEvents.ContainsKey(e_ModuleType))
                        {
                            if (!Instance.LastFixedUpdateEventsCache.ContainsKey(e_ModuleType))
                            {
                                MessageEvent me = new(new SingleMehtodPoint(listener, e_ModuleType));
                                Instance.LastFixedUpdateEventsCache.TryAdd(e_ModuleType, me);
                            }
                            else
                            {
                                Instance.LastFixedUpdateEventsCache[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                            }

                        }
                        else
                        {
                            Instance.LastFixedUpdateEvents[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                        }

                        break;
                    default:
                        break;
                }

            }
        }

        public static void RemoveFixedUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness, E_Priority e_Priority = E_Priority.Middle)
        {
            if (Instance != null)
            {
                switch (e_Priority)
                {
                    case E_Priority.First:

                        if (!Instance.FirstFixedUpdateEvents.ContainsKey(e_ModuleType))
                        {
                        }
                        else
                        {
                            Instance.FirstFixedUpdateEvents[e_ModuleType] -= listener;
                            //不处理删除 

                        }
                        break;
                    case E_Priority.Middle:

                        if (!Instance.FixedUpdateEvents.ContainsKey(e_ModuleType))
                        {
                        }
                        else
                        {
                            Instance.FixedUpdateEvents[e_ModuleType] -= listener;
                            //不处理删除 

                        }

                        break;
                    case E_Priority.Last:
                        if (!Instance.LastFixedUpdateEvents.ContainsKey(e_ModuleType))
                        {
                        }
                        else
                        {
                            Instance.LastFixedUpdateEvents[e_ModuleType] -= listener;
                            //不处理删除 

                        }
                        break;
                    default:
                        break;
                }

            }
        }

        public static void AddSecTimeUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness)
        {

            if (!Instance.TimeSecUpdateEvents.ContainsKey(e_ModuleType))
            {
                if (!Instance.TimeSecUpdateEventsCache.ContainsKey(e_ModuleType))
                {
                    MessageEvent me = new(new SingleMehtodPoint(listener, e_ModuleType));
                    Instance.TimeSecUpdateEventsCache.TryAdd(e_ModuleType, me);
                }
                else
                {
                    Instance.TimeSecUpdateEventsCache[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                }

            }
            else
            {
                Instance.TimeSecUpdateEvents[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
            }
        }

        public static void RemoveSecTimeUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness)
        {

            if (Instance != null)
            {
                if (!Instance.TimeSecUpdateEvents.ContainsKey(e_ModuleType))
                {
                    //Instance.UpdateEvents.remove(e_ModuleType, listener);
                }
                else
                {
                    Instance.TimeSecUpdateEvents[e_ModuleType] -= listener;
                    //不处理删除 

                }

            }
        }
        /// <summary>
        /// 新增，剔除，invoke，有差异的进入和出，A和B之间的误差最小颗粒是1秒
        /// 其他用Delayinvoke细致的
        /// 具体常驻时间自己调
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="e_ModuleType"></param>
        public static void AddTenSecTimeUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness)
        {
            if (!Instance.TimeTenSecUpdateEvents.ContainsKey(e_ModuleType))
            {
                if (!Instance.TimeTenSecUpdateEventsCache.ContainsKey(e_ModuleType))
                {
                    MessageEvent me = new(new SingleMehtodPoint(listener, e_ModuleType));
                    Instance.TimeTenSecUpdateEventsCache.TryAdd(e_ModuleType, me);
                }
                else
                {
                    Instance.TimeTenSecUpdateEventsCache[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                }

            }
            else
            {
                Instance.TimeTenSecUpdateEvents[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
            }
        }

        public static void RemoveTenSecTimeUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness)
        {

            if (Instance != null)
            {
                if (!Instance.TimeTenSecUpdateEvents.ContainsKey(e_ModuleType))
                {
                }
                else
                {
                    Instance.TimeTenSecUpdateEvents[e_ModuleType] -= listener;
                    //不处理删除 

                }

            }
        }


        public static void AddMinutesTimeUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness)
        {
            if (!Instance.TimeMinutesUpdateEvents.ContainsKey(e_ModuleType))
            {
                if (!Instance.TimeMinutesUpdateEventsCache.ContainsKey(e_ModuleType))
                {
                    MessageEvent me = new(new SingleMehtodPoint(listener, e_ModuleType));
                    Instance.TimeMinutesUpdateEventsCache.TryAdd(e_ModuleType, me);
                }
                else
                {
                    Instance.TimeMinutesUpdateEventsCache[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
                }
            }
            else
            {
                Instance.TimeMinutesUpdateEvents[e_ModuleType] += new SingleMehtodPoint(listener, e_ModuleType);
            }
        }

        public static void RemoveMinutesTimeUpdateListener(StarBaseDelegate listener, E_ModuleType e_ModuleType = E_ModuleType.CommonBusiness)
        {
            if (Instance != null)
            {
                if (!Instance.TimeMinutesUpdateEvents.ContainsKey(e_ModuleType))
                {
                }
                else
                {
                    Instance.TimeMinutesUpdateEvents[e_ModuleType] -= listener;
                    //不处理删除 
                }
            }
        }

        //public static void StartCoroutine()
        //{
        //    if (Instance != null)
        //    {
        //        // Instance.StartCoroutine()
        //    }
        //}

        /* 先删除，再执行，再添加
                   需求是，处理的时候，前面告诉我删除的我就不调用了
                   因为删除是删除通知和数据，此时数据已经没了，通知先删除后然后调用不会调用函数进而不会调用到空数据*/
        void Update()
        {
            //remove perv 上面锁同时处理：1，后面添加的后一帧处理；2，如果同一毫秒有就是有没有就是没有；3，如果前面添加就直接对
            //如果有值，invoke执行中一直在减小temp不会影响他到此时直接给他正常的；如果触发给的时候减少不会影响；如果结束的时发现这个东西我这里清空同时在增加时候新需求过来就会触发新的减少缓存
            //理论上没有逻辑碰撞
            /*   if (tempUpdateEventsDelayRemove != null)//TTTTTTage
               {
                   UpdateEvents[tempUpdateEventsDelayRemove.e_ModuleType1] = tempUpdateEventsDelayRemove.monoUpdaterEvent;
                   tempUpdateEventsDelayRemove = null;  什么意识
               }*/
            //噢以前的意识是，外面调用就删除缓存，然后执行，然后同步删除后的信息：删除重复了

            //标注是删除的时候，
            //处理删除内部，
            //防错执行内部
            if (UpdateEvents != null)
            {
                HandleUpdateEvents(UpdateEvents, "UpdateEvents");
            }


            if (UpdateEventsCache.Count > 0)
            {
                foreach (var item in UpdateEventsCache)
                {
                    try
                    {
                        UpdateEvents.TryAdd(item.Key, item.Value);
                    }
                    catch (Exception e)
                    {
                        SGF.Debuger.LogError($"Update error: {e.Message}, stack: {e.StackTrace}, InnerException: {e?.InnerException}, StackTrace: {e?.InnerException?.StackTrace}");
                    }
                }
                UpdateEventsCache.Clear();
            }

        }

        private void LateUpdate()
        {
            if (LateUpdateEvents != null)
            {
                HandleUpdateEvents(LateUpdateEvents, "LateUpdateEvents");
            }



            if (LateUpdateEventsCache.Count > 0)
            {
                foreach (var item in LateUpdateEventsCache)
                {
                    try
                    {
                        LateUpdateEvents.TryAdd(item.Key, item.Value);
                    }
                    catch (Exception e)
                    {
                        SGF.Debuger.LogError($"LateUpdate error: {e.Message}, stack: {e.StackTrace}, InnerException: {e?.InnerException}, StackTrace: {e?.InnerException?.StackTrace}");
                    }
                }
                LateUpdateEventsCache.Clear();
            }
        }


        public byte FrameIndex = 0;//自己定义的（本秒数的帧）

        //private float lastTime = 0;

        void FixedUpdate()
        {

            if (FirstFixedUpdateEvents != null)
            {
                HandleUpdateEvents(FirstFixedUpdateEvents, "FirstFixedUpdateEvents");

                if (FirstFixedUpdateEventsCache.Count > 0)
                {
                    foreach (var item in FirstFixedUpdateEventsCache)
                    {
                        try
                        {
                            FirstFixedUpdateEvents.TryAdd(item.Key, item.Value);
                        }
                        catch (Exception e)
                        {
                            SGF.Debuger.LogError($"FirstFixedUpdateEvents error: {e.Message}, stack: {e.StackTrace}, InnerException: {e?.InnerException}, StackTrace: {e?.InnerException?.StackTrace}");
                        }
                    }
                    FirstFixedUpdateEventsCache.Clear();
                }
            }


            if (FixedUpdateEvents != null)
            {
                HandleUpdateEvents(FixedUpdateEvents, "FixedUpdateEvents");

                if (FixedUpdateEventsCache.Count > 0)
                {
                    foreach (var item in FixedUpdateEventsCache)
                    {
                        try
                        {
                            FixedUpdateEvents.TryAdd(item.Key, item.Value);
                        }
                        catch (Exception e)
                        {
                            SGF.Debuger.LogError($"FixedUpdateEvents error: {e.Message}, stack: {e.StackTrace}, InnerException: {e?.InnerException}, StackTrace: {e?.InnerException?.StackTrace}");
                        }
                    }
                    FixedUpdateEventsCache.Clear();
                }
            }

            if (LastFixedUpdateEvents != null)
            {
                HandleUpdateEvents(LastFixedUpdateEvents, "LastFixedUpdateEvents");

                if (LastFixedUpdateEventsCache.Count > 0)
                {
                    foreach (var item in LastFixedUpdateEventsCache)
                    {
                        try
                        {
                            LastFixedUpdateEvents.TryAdd(item.Key, item.Value);
                        }
                        catch (Exception e)
                        {
                            SGF.Debuger.LogError($"LastFixedUpdateEvents error: {e.Message}, stack: {e.StackTrace}, InnerException: {e?.InnerException}, StackTrace: {e?.InnerException?.StackTrace}");
                        }
                    }
                    LastFixedUpdateEventsCache.Clear();
                }
            }
        }

        void OnSecTick()
        {
            if (TimeSecUpdateEvents != null)
            {
                HandleUpdateEvents(TimeSecUpdateEvents, "TimeSecUpdateEvents");

                if (TimeSecUpdateEventsCache.Count > 0)
                {
                    foreach (var item in TimeSecUpdateEventsCache)
                    {
                        try
                        {
                            TimeSecUpdateEvents.TryAdd(item.Key, item.Value);
                        }
                        catch (Exception e)
                        {
                            SGF.Debuger.LogError($"OnSecTick error: {e.Message}, stack: {e.StackTrace}, InnerException: {e?.InnerException}, StackTrace: {e?.InnerException?.StackTrace}");
                        }
                    }
                    TimeSecUpdateEventsCache.Clear();
                }
            }

        }
        void OnTenSecTick()
        {
            if (TimeTenSecUpdateEvents != null)
            {
                HandleUpdateEvents(TimeTenSecUpdateEvents, "TimeTenSecUpdateEvents");

                if (TimeTenSecUpdateEventsCache.Count > 0)
                {
                    foreach (var item in TimeTenSecUpdateEventsCache)
                    {
                        try
                        {
                            TimeTenSecUpdateEvents.TryAdd(item.Key, item.Value);
                        }
                        catch (Exception e)
                        {
                            SGF.Debuger.LogError($"OnTenSecTick error: {e.Message}, stack: {e.StackTrace}, InnerException: {e?.InnerException}, StackTrace: {e?.InnerException?.StackTrace}");
                        }
                    }
                    TimeTenSecUpdateEventsCache.Clear();
                }
            }
        }
        void OnMinutesTick()
        {
            if (TimeMinutesUpdateEvents != null)
            {
                HandleUpdateEvents(TimeMinutesUpdateEvents, "TimeMinutesUpdateEvents");

                if (TimeMinutesUpdateEventsCache.Count > 0)
                {
                    foreach (var item in TimeMinutesUpdateEventsCache)
                    {
                        try
                        {
                            TimeMinutesUpdateEvents.TryAdd(item.Key, item.Value);
                        }
                        catch (Exception e)
                        {
                            SGF.Debuger.LogError($"OnMinutesTick error: {e.Message}, stack: {e.StackTrace}, InnerException: {e?.InnerException}, StackTrace: {e?.InnerException?.StackTrace}");
                        }
                    }
                    TimeMinutesUpdateEventsCache.Clear();
                }
            }
        }




        //===========================================================

        public static void StartCoroutine(IEnumerator routine)
        {
            MonoBehaviour mono = Instance;
            mono.StartCoroutine(routine);
        }

        List<E_ModuleType> keysToRemove = new();/*
        System.Delegate[] singleModulePointers;
        System.Delegate[] waitToRemoveModulePointers;*/
        /// <summary>
        /// 驱动增加维度，时间维度，模块维度
        /// 驱动/时间维度，模块维度，
        /// 标注，处理标注，结尾一起删除
        /// </summary>
        /// <param name="updateEventClasses"></param>
        /// <param name="beforeInvokingRemoved">记录要删除的，和删除功能的tag，循环查找，：问题是只有一个，结构不对，现在打标记了</param>
        /// <param name="key"></param>
        private void HandleUpdateEvents(ConcurrentDictionary<E_ModuleType, MessageEvent> updateEventClasses, string key)
        {
            if (updateEventClasses.Count > 0)
            {
                foreach (var moduleTimesItem in updateEventClasses)
                {
                    try
                    {
                        //模块时间维度
                        //清理1
                        moduleTimesItem.Value.ClearUnuse();
                        //unuse的使用。
                        //判空执行2
                        moduleTimesItem.Value?.Invoke(); // 处理空值，不处理 count
                    }
                    catch (System.Exception e)
                    {
                        SGF.Debuger.LogError($"{key} HandleUpdateEvent error: {e.Message}, stack: {e.StackTrace}, InnerException: {e?.InnerException}, StackTrace: {e?.InnerException?.StackTrace}, System: {moduleTimesItem.Key.ToString()}");
                    }
                }
            }
        }
    }
}
