using SGF;
using StarProject.Game.Data;
using StarProject.Game.SnapShot;
using System;
using System.Collections.Generic;
//using StarProject.Game.SnapShot.Skill;

namespace StarProject.Game.Entity.Factory
{
    //帧同步来弥补同步对齐的策略；基于数据快照
    //==========================================================
    /// <summary>
    //TODO：        其实也要联通三部分，修改一下框架 ： 数据池很少有人进行系统的抽象，数据池用的少而散，，也就是说，我们所有的引擎优化和 都是基于机器性能极限 项目平衡
    //TODO:         一个是数据对其。
    //TODO：        一个是服务器数据队列
    //TODO：         1个是ecs：Unity不单单是V，可以理解都是过程化，和 持久化；unity也是Yml引擎分离了表现和数据就是数据的进一步拓展
    /// Data依赖实体创建，或推送给实体
    /// Data被资深管理器管理
    /// 理解：循环器是内存空间管理；List是内存区块编号管理；外层来定义消息是顺序都存留还是立即替换
    //工厂是预分配内存空间得，生命周期管理
    /// </summary>
    public static class DynamicDataFactory
    {
        public static bool EnableLog = false;
        private static string LOG_TAG = "DataFactory";

        private static bool m_isInit = false;

        //循环器
        private static Recycler m_recycler;

        /// <summary>
        /// 数据虽然不是UDP不用处理顺序，和排序，拆解
        /// 但是基于效果来说就是:1时间顺序，2结构顺序
        /// 再我工厂管理生命周期：招聘解雇这里，我利用循环器能力
        /// 我数据工厂更关心，服务器指令的表现，当前的应用，同步对齐。任意历史消息都无效：
        /// 有效的只有【当前】【最后一个】
        /// 【当前】：我有些动画，伤害必须要，表现完毕
        /// 【最后一个】：属性同步需要直接同步结果的情况
        /// 整体是基于过程化表现设计
        /// 服务器分别告诉你【伤害】【属性】；客户端就是剧场而已
        /// 
        /// key:ClassName + NttId ;Value 数据缓存串，广度换深度:改成并行代表不再处理分发
        /// </summary>
        //private static List<DataObject> m_listObject;
        //【缓存所有帧的数据，随时一直在记录】
        private static DictionaryEx<string, LinkedList<DynamicDataObject>> m_queueObjectMaps;
        ////【根据帧记录的一帧数据】【没办法记录一帧数据，和全部帧复杂，就是转换存储】
        ///Enter leave Update 都把所有一帧的 就是全部数据 一条
        //private static Queue<ulong> nttidExOneFrameRecorde;

        public static void Init()
        {
            if (m_isInit)
            {
                return;
            }
            m_isInit = true;

            m_queueObjectMaps = new DictionaryEx<string, LinkedList<DynamicDataObject>>();//new List<DataObject>();
            m_recycler = new Recycler();

        }

        /*    public static int GetCount<T>(ulong entityId) where T : DynamicDataObject, new()
            {
                Type type = typeof(T);
                String ClassName_NttID_Key = type.FullName + entityId;

                if (m_queueObjectMaps.ContainsKey(ClassName_NttID_Key))
                {
                    return m_queueObjectMaps[ClassName_NttID_Key].Count;
                }
                return 0;
            }
    */


        /// <summary>
        /// 这个人
        /// 这个类型
        /// 缓存的数据
        /// </summary>
        /// <returns></returns>
        public static int GetCount<T>() where T : DynamicDataObject
        {
            int initCount = 0;
            Type type = typeof(T);

            string fullName;
            fullName = typeof(T).FullName;
            String ClassName_NttID_Key = fullName;

          

            if (m_queueObjectMaps == null)
            {
                return initCount;
            }

            if (m_queueObjectMaps.ContainsKey(ClassName_NttID_Key))//开始，两种，都count== 0
            {
                if (m_queueObjectMaps[ClassName_NttID_Key].Count > 0)//缓存的数据队列
                {
                    return m_queueObjectMaps[ClassName_NttID_Key].Count;
                }
            }

            return initCount;
        }


        /// 工厂自己的Release,变卖产权那种
        public static void Release()
        {
            m_isInit = false;
            foreach (var item in m_queueObjectMaps)
            {
                LinkedListNode<DynamicDataObject> node = item.Value.First;
                while (node != null)
                {
                    node.Value.ReleaseInFactory();
                    node.Value.Dispose();

                    item.Value.Remove(node);

                }
            }
            THDBEGIN = false;
            MainPlayerBEGIN = false;
            RPCBEGIN = false;

            //for (int i = 0; i < m_queueObject.Count; i++)
            //{
            //    //[工厂倒闭]
            //    //[解雇所有在职-送到后备公司(池化)]
            //    m_queueObject[i].ReleaseInFactory();         //实体子类的[直接][逻辑][释放],实体的待释放标记-待转存,
            //                                                //[所有在职-立刻kill掉=等待gc回收内存]
            //    m_queueObject[i].Dispose();                  //[*代GC][删除][或兄弟类自定义Db层清理数据库]
            //}
            //[清理在职名单]
            m_queueObjectMaps.Clear();
            //[不等到帧结束,清理后备公司实体,立刻解雇掉,清理后备公司名单]
            m_recycler.Release();

        }




        /// <summary>
        /// 服务器来的数据，不能直接用
        /// 三层：m_recycler，内存区块分配管理：所DataBoj是极简空间模板，甚至数据对齐Chip对位让内存条多Chip运行所以内存空间取整
        /// 1；[提供内存空间的集中销毁，复用，控制内存生命周期]
        /// 数据层数据区块，以及对应的顺序，这一点是看具体数据要呗如何应用
        /// 2：【如这里就规划了使用Squeue那就约束了Data的意义！】[同类：数据量级管理：【应用程生命周期】【增/删/改/查】]-这直接决定了上层数据统一的特点
        /// 3: 应用层，标记顺序，再下层2中的顺序再人为标记顺序：满足快照顺序，即时赋值：等数据的统一特点
        /// </summary>
        /// <typeparam name="T">
        /// !!!!!!!T：不同类型 *（乘以）
        /// !!!!!!!这里的数据结构：并行出现的（同类）数据量级：很多属性，很多黑板（快照），
        /// 
        /// 入口和出口：
        /// 入口是反复网络进入
        /// 出口是应用
        /// </typeparam>
        /// <returns></returns>
        //public static T InstanceData<T>(ulong entityId) where T : DynamicDataObject, new()// 【提前数据】【即时数据都可以创建】【首先通过类型就分类管理了】【快照数据向下兼容-即时数据-可以放一起】
        //{
        //    //缓存者是此时此刻的最新
        //    //赋予时是缓存转存到这里
        //    //取是取得最新缓存值
        //    //清理是清理上一个
        //    DynamicDataObject obj = null;
        //    bool useRecycler = true;


        //    Type type = typeof(T);
        //    String ClassName_NttID_Key = type.FullName + entityId;
        //    obj = m_recycler.Pop(ClassName_NttID_Key) as DynamicDataObject;
        //    if (obj == null)
        //    {
        //        //是否是池中物,金鳞?
        //        useRecycler = false;
        //        obj = new T();
        //    }

        //    //实际管理:[工厂招聘]
        //    obj.InstanceInFactory(entityId);//being Useing

        //    if (!m_queueObjectMaps.ContainsKey(ClassName_NttID_Key))
        //    {
        //        m_queueObjectMaps.Add(ClassName_NttID_Key, new LinkedList<DynamicDataObject>());
        //    }
        //    m_queueObjectMaps[ClassName_NttID_Key].AddLast(obj);
        //    nttidExOneFrameRecorde.Enqueue(entityId);
        //    //m_queueObject.Add(obj);

        //    if (EnableLog && Debuger.EnableLog)
        //    {
        //        Debuger.Log(LOG_TAG, "InstanceEntity() {0}:{1}, UseRecycler:{2}", obj.GetType().Name, obj.GetHashCode(), useRecycler);
        //    }


        //    return (T)obj;
        //}




        //public static DynamicDataObject[][][][] LasterDataObject = null;
        //public static DynamicDataObject LasterDataObject = null;
        //记录上一次的缓存
        public static Dictionary<string, DynamicDataObject> TypePervCaches = new Dictionary<string, DynamicDataObject>();
        /// <summary>
        /// 【弹出】取得最近一次需要同步的数据
        /// 同样类型的数据有一大组，很多属性，很多伤害 || 时间阻塞 时间顺序
        /// 这里依赖服务器同组必然有顺序发送 + TCP特点
        /// 循环只是管理空间，
        /// 
        /// 确实是顺序
        /// 确实是那个组
        /// 
        /// 
        /// 内存分布图：【混序标】【*规划标签】
        /// 组1  ：时序1，时序2，时序3
        /// 组2  ：时序1，时序2
        /// Fix消化时可以针对不同类型的优先级，深度，广度优先，分摊帧压力

        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        //public static T PopEarliestData<T>(ulong entityId) where T : DynamicDataObject
        //{
        //    // Doesn't work on Win 8.1
        //    Type type = typeof(T);
        //    T deq = null;
        //    string fullName;
        //    fullName = typeof(T).FullName;
        //    String ClassName_NttID_Key = fullName + entityId;

        //    if (type.IsSubclassOf(typeof(EntityBaseData)) || type == typeof(EntityBaseData))
        //    {
        //        //实体的基础数据有自己的销毁
        //    }
        //    else if (type.IsSubclassOf(typeof(ServiceSnapshotData)) || type == typeof(SerNttSnapData))
        //    {
        //        //过程化的数据（围绕实体）必然销毁【上一个】
        //        //所有过程化数据不可能立刻销毁上一个？要分类型？：
        //        //1，答案是有些过程化可能没处理完（联想多线程）---必须分类
        //        //2，分类就够了么
        //        //3，角色id就够了么
        //        //所以是三维数组命中替换逻辑---农场检测建筑的增维版
        //        //但同样效果就必须释放么，如果虚空飞的过程中，被屠夫勾了，水人W时候跳刀了
        //        //本质上不应该有限制
        //        //1，这个值缓存给别人，或 ，2效果自己控制Release
        //        //3，打开条件，策划有顶替规则时
        //        //不说入队的时候是异步list也有这个能力，出队的时候就算同一个效果都能再一个人身上持续叠加
        //        ///但是请注意任意过程数据都有，1，表现载体gob，2，数据载体nttdata-更新，3协同逻辑载体Tween--所以释放自己的约束就是要让你们找到载体
        //        ///我必须释放，所以我不会分类，过程太消耗了必须找到载体，我必须处理，不用多维命中


        //        ///【上一个缓存的如果还有就释放】
        //        if (TypePervCaches.ContainsKey(ClassName_NttID_Key) && TypePervCaches[ClassName_NttID_Key] != null)
        //        {
        //            DynamicDataObject ddo = TypePervCaches[ClassName_NttID_Key];
        //            // 不必【管理】列表，DataQueue特性
        //            ReleaseData(ddo); //【上一个要标记】                                  //EntityObject obj = m_listObject[i];//不必缓存
        //                              // m_listObject.RemoveAt(i);       //同样不必处理顺序
        //                              // 【回收】将对象加入对象池
        //            m_recycler.Push(ddo);
        //        }
        //    }


        //    if (m_queueObjectMaps == null)
        //    {
        //        return deq;
        //    }
        //    if (m_queueObjectMaps.ContainsKey(ClassName_NttID_Key))
        //    {
        //        if (m_queueObjectMaps[ClassName_NttID_Key].Count > 0)
        //        {
        //            //【找到当前应该推出的】
        //            deq = (T)m_queueObjectMaps[ClassName_NttID_Key].First.Value;
        //            m_queueObjectMaps[ClassName_NttID_Key].Remove(deq);

        //            //【缓存给下一次释放】
        //            if (TypePervCaches.ContainsKey(ClassName_NttID_Key))
        //            {
        //                TypePervCaches[ClassName_NttID_Key] = deq;
        //            }
        //            else
        //            {
        //                TypePervCaches.Add(ClassName_NttID_Key, deq);
        //            }
        //        }

        //    }

        //    return deq;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        /// <param name="logicFrameIndex">LogicFrameIndex 到max你自己变成0</param>
        /// <returns></returns>
        public static T InstanceData<T>(/*ulong entityId*/) where T : DynamicDataObject, new()// 【提前数据】【即时数据都可以创建】【首先通过类型就分类管理了】【快照数据向下兼容-即时数据-可以放一起】
        {
            //缓存者是此时此刻的最新
            //赋予时是缓存转存到这里
            //取是取得最新缓存值
            //清理是清理上一个
            DynamicDataObject obj = null;
            bool useRecycler = true;


            Type type = typeof(T);
            if (type == typeof(SerNttAOIOneFrameSnapALLDataTHD) && !THDBEGIN)
            {
                THDBEGIN = true;
            }
            else if (type == typeof(SerNttAOIOneFrameSnapALLDataMainPlayer)  && !MainPlayerBEGIN)
            {
                MainPlayerBEGIN = true;
            }
            else if (type == typeof(SerRPCOneFrameSnapALLData)  && !RPCBEGIN)
            {
                RPCBEGIN = true;
            }


            String ClassName_NttID_Key = type.FullName/* + entityId*/;
            obj = m_recycler.Pop(ClassName_NttID_Key) as DynamicDataObject;
            if (obj == null)
            {
                //是否是池中物,金鳞?
                useRecycler = false;
                obj = new T();
            }

            //实际管理:[工厂招聘]
            obj.InstanceInFactory();//being Useing ；这个是设置值；不是内存缓存池中找对应类型

            if (!m_queueObjectMaps.ContainsKey(ClassName_NttID_Key))
            {
                m_queueObjectMaps.Add(ClassName_NttID_Key, new LinkedList<DynamicDataObject>());
            }
            m_queueObjectMaps[ClassName_NttID_Key].AddLast(obj);
            /*nttidExOneFrameRecorde.Enqueue(entityId);*/
            //m_queueObject.Add(obj);

            if (EnableLog && Debuger.EnableLog)
            {
                Debuger.Log(LOG_TAG, "InstanceEntity() {0}:{1}, UseRecycler:{2}", obj.GetType().Name, obj.GetHashCode(), useRecycler);
            }

            return (T)obj;
        }


        //保证先inst 后 pop:111保证逻辑正确
        static bool THDBEGIN = false;
        static bool MainPlayerBEGIN = false;
        static bool RPCBEGIN = false;

        /// ===================20230308=======================
        /// 修改队列从每个人的数据自己请求，到GameMgr推送，效率有节省
        /// 结构上数据是所有人集合不用分摊到没人身上，数据层，不应该有Update
        /// 那其实我并不需要确定每个nttid来请求，而是自己记录nttID
        /// 不是一定先Inst 后 pop的 因为inst是网络时间
        /// ==================================================
        public static T PopEarliestDataByRecorde<T>() where T : DynamicDataObject
        {
            T deq = null;
            /*ulong entityId;*/
            //           /*  //下面是取得本数据，缓存到下一次，然后清理上一次缓存的。
            //             //同步数据在下一次的时候对本数据没影响，异步移动数据转向数据也已经转存了没问题。
            //             //【取的时候依赖空】
            //             if (nttidExOneFrameRecorde.Count > 0)
            //             {
            //                 entityId = nttidExOneFrameRecorde.Dequeue();
            //             }
            //             else
            //             {
            //                 //存是一直随便存，所以对于数据不代表本帧数据退不出来，这个人的数据就受到什么影响.
            //                 return deq;
            //             }*/


            /*===怎么知道一帧的数据*/

            // Doesn't work on Win 8.1
            Type type = typeof(T);

            string fullName;
            fullName = typeof(T).FullName;
            String ClassName_NttID_Key = fullName/* + entityId*/;

            if (type.IsSubclassOf(typeof(EntityBaseData)) || type == typeof(EntityBaseData))
            {
                //实体的基础数据有自己的销毁
            }
            else if (/*type.IsSubclassOf(typeof(ServiceSnapshotData)) || */
                (type == typeof(SerNttAOIOneFrameSnapALLDataTHD) && THDBEGIN)
                ||
                (type == typeof(SerNttAOIOneFrameSnapALLDataMainPlayer)  && MainPlayerBEGIN)
                ||
                (type == typeof(SerRPCOneFrameSnapALLData)  && RPCBEGIN)
                )
            {
                ///【上一个缓存的如果还有就释放】
                if (TypePervCaches.ContainsKey(ClassName_NttID_Key) && TypePervCaches[ClassName_NttID_Key] != null)
                {
                    DynamicDataObject ddo = TypePervCaches[ClassName_NttID_Key];
                    TypePervCaches[ClassName_NttID_Key] = null;//222保证异常情况逻辑正确，和先后关系，和局部正确
                    //如果实际情况是先popEarly，清理，放进去
                    //被inst初始化数据拿到，2
                    //再pop然后我缓存里面有，还缓存新给2的，我给他清了
                    //全给清了
                    //实际情况由于插入是网络所以不正保证是【先】，我处理也未必是【后】
                    //我这里默认逻辑是清理上一次，可以空跑，允许单独清理人，thd，和混合，允许空清一次，允许无清理插入一次
                    //但是你拿着这个值一定是下面逻辑给你的，但是下面逻辑找不到就没发给你缓存，inst后你有，然后我清理了，然后这个有拿到给我
                    //相当于inst，二级缓存，大缓存，三个用到的全是一个值
                    //这里是中断一次是你【你找到了。你清理了（处理一次释放），给洗衣机了,你就没权限处理了（找不到）；就等到下次给你再处理】
                    // 不必【管理】列表，DataQueue特性
                    //就只是清理数据
                    ReleaseData(ddo);
                    // m_listObject.RemoveAt(i);       //同样不必处理顺序,不必清理k或者V 后面代码再下一次执行的时候会重新给他值（需要释放的那一个）
                    // 【回收】将对象加入对象池
                    m_recycler.Push(ddo);
                }
            }

            if (m_queueObjectMaps == null)
            {
                return deq;
            }

            if (m_queueObjectMaps.ContainsKey(ClassName_NttID_Key))//开始，两种，都count== 0
            {
                if (m_queueObjectMaps[ClassName_NttID_Key].Count > 0)//缓存的数据队列
                {
                    //【找到当前应该推出的】
                    //【新增：如果未拼装完成数据，不可推出，但不影响前面一个数据的释放】【LinkList是多个Start-End】[如果第一个都没有End后面都不能用]【Saver的合并属于SerNttAOIOneFrameSnapALLDataTHD的合并】
                    var localDeq = (T)m_queueObjectMaps[ClassName_NttID_Key].First.Value;//访问
                    if (localDeq.IsFullMessage)
                    {
                        deq = localDeq;
                        m_queueObjectMaps[ClassName_NttID_Key].Remove(deq);
                        //【缓存给下一次释放】
                        if (TypePervCaches.ContainsKey(ClassName_NttID_Key))
                        {
                            TypePervCaches[ClassName_NttID_Key] = deq;
                        }
                        else
                        {
                            TypePervCaches.Add(ClassName_NttID_Key, deq);
                        }
                    }
                }
            }

            return deq;
        }

        ///// <summary>
        ///// 弹出但是不删除
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="runtimeId"></param>
        ///// <param name="nttId"></param>
        ///// <param name="runtimeType"></param>
        ///// <param name="BlackBoardType"></param>
        ///// <returns></returns>
        //public static T GetData<T>(ulong runtimeId, ulong nttId, RuntimeEnumType runtimeType, EnumBlackBoardType BlackBoardType) where T : RuntimeAllEffect
        //{
        //    Type type = typeof(T);
        //    T deq = null;
        //    string fullName;
        //    fullName = typeof(T).FullName;

        //    //if get from cache
        //    if (TypePervCaches.ContainsKey(fullName) && TypePervCaches[fullName] != null)
        //    {
        //        return (T)TypePervCaches[fullName];
        //    }



        //    if (m_queueObjectMaps == null)
        //    {
        //        return deq;
        //    }
        //    if (m_queueObjectMaps.ContainsKey(fullName))
        //    {
        //        if (m_queueObjectMaps[fullName].Count > 0)
        //        {

        //            deq = (T)m_queueObjectMaps[fullName].LinkedListFind(
        //                (DynamicDataObject d) =>
        //                {
        //                    T t = (T)d;
        //                    return t.RuntimeID == runtimeId
        //                    &&
        //                    t.M_EntityID == nttId
        //                    &&
        //                    t.runtimeEnumType == runtimeType
        //                    &&
        //                    t.BlackBoardType == BlackBoardType
        //                    ;
        //                }
        //                );

        //        }

        //    }

        //    return deq;
        //}


        /// <summary>
        /// 只有提供自己用了
        /// </summary>
        /// <param name="entity">DataObject.</param>
        private static void ReleaseData(DynamicDataObject obj)
        {
            if (obj != null)
            {
                if (EnableLog && Debuger.EnableLog)
                {
                    Debuger.Log(LOG_TAG, "ReleaseData() {0}:{1}", obj.GetType().Name, obj.GetHashCode());
                }
                //内存空间的释放标记
                //[工厂解雇到后备公司]
                //特性：View是瞬时Active|Entity是帧末组GC|Data当时GC
                obj.ReleaseInFactory();
                //这里不立即从listObject中删除，
                //而是在下一个逻辑循环统一进行删除
                //这样做可以提高效率
                //[统一组]逻辑;在[进][持续][出*]的情况下,对实际压池逻辑,操作实现缓存压栈[其实是工厂先开除,名单延迟批量交给后备公司]
            }
        }




        //public static void ClearReleasedObjects()
        //{
        //    //倒叙回收缓存
        //    for (int i = m_queueObjectMaps.Count - 1; i >= 0; i--)
        //    {
        //        if (m_queueObjectMaps[i].IsReleased)
        //        {
        //            //缓存
        //            DataObject obj = m_queueObjectMaps[i];
        //            //管理
        //            m_queueObjectMaps.RemoveAt(i);

        //            //将对象加入对象池

        //            m_recycler.Push(obj);
        //        }
        //    }
        //}





    }
}