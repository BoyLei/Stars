

using SGF;
using SkillEditor;
using StarProject.Game.Entity.RemoteDynamic;
using StarProjectDef;
using System;
using System.Collections.Generic;

namespace StarProject.Game.Entity.Factory
{
    //===============================================================
    /// <summary>
    /// 实体工厂，用于创建实体对象:工厂模式更在乎生命周期的管理,在生命的长河中有效的对[玩偶]进行量产并控制   +   结合池有效的结合出游戏的具体模式    =   现实中实际的例子是[水资源]
    /// 到这一层:Enity本身.在工厂中,具备的能力是<回收性><位置信息>|<增|删|改|查>
    /// 工厂是对:模式,回收器管理,回收物件具体:的实际应用
    /// 以及后续可能对其进行回收和重复利用
    /// PS：通过静态支持类的方式，做init/Release： 可以兼容【场景】【副本-战场模式】
    /// 
    /// </summary>
    public static class EntityFactory
    {
        public static bool EnableLog = false;
        private static string LOG_TAG = "EntityFactory";

        private static bool m_isInit = false;
        /// <summary>
        /// 洗衣机：池化接口的：【堆积，取，删除，分类】
        /// -------------------------------------------------------------------------------------
        /// X工厂是：对X对象工厂操作，增，删，修改，查询
        /// X具备的特性决定分类：X需要工厂提供他干什么，X的标记属性，X的增删修查，X的特性。
        /// -------------------------------------------------------------------------------------
        /// @工厂和洗衣机要合作的：这就是铁打的营房，流水的兵,：
        /// @@@所以值得注意：每个工厂有用自己的洗衣机（循环管理模式），
        /// @@@洗衣机服务于工厂（继承I循环者），所以洗衣机只包含本工厂的循环者对象数据
        /// @@@洗衣机辅助----@@@工厂管理自己的工厂实际工人（继承I循环者）
        /// </summary>
        private static Recycler m_recycler;

        /// <summary>
        /// 工厂所实例化的对象列表：X
        /// 整体是:
        /// 1,池化器: X的子集--X实例栈|X的子集--X实例栈|X的子集--X实例栈
        /// @(这里的设计意义)2,应用器: 在使用中的
        /// </summary>
        private static List<EntityObject> m_listObject;

        /// <summary>
        /// 初始化一次，实例工厂特性：状态，洗衣机，所有Enity这类兵的 实例工厂集
        /// </summary>
        public static void Init()
        {
            if (m_isInit)
            {
                return;
            }
            m_isInit = true;

            m_listObject = new List<EntityObject>();
            m_recycler = new Recycler();

        }

        /// <summary>
        /// 释放工厂所创建的所有对象，包括空闲的对象
        /// 洗衣机释放=释放洗衣机内列表，销毁，闲置洗衣机
        /// 工厂自己的Release,变卖产权那种
        /// </summary>
        public static void Release()
        {
            m_isInit = false;

            for (int i = 0; i < m_listObject.Count; i++)
            {
                //[工厂倒闭]
                //[解雇所有在职-送到后备公司(池化)]
                m_listObject[i].ReleaseInFactory();         //实体子类的[直接][逻辑][释放],实体的待释放标记-待转存,
                                                            //[所有在职-立刻kill掉=等待gc回收内存]
                m_listObject[i].Dispose();                  //[*代GC][删除][或兄弟类自定义Db层清理数据库]
            }
            //[清理在职名单]
            m_listObject.Clear();
            //[不等到帧结束,清理后备公司实体,立刻解雇掉,清理后备公司名单]
            m_recycler.Release();

        }


        /// <summary>
        /// 1Load
        /// 实例化一个实体对象:都是EnityObj的子集
        /// 实体工厂具体要做的事儿...:
        /// 1池化概念
        /// 2具体情况
        /// </summary>
        /// <returns>The entity.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T InstanceEntity<T>() where T : EntityObject, new()
        {
            EntityObject obj = null;
            bool useRecycler = true;

            //先从回收池中寻找://[不够用，或没创建]/[有的用]
            Type type = typeof(T);
            //洗衣机做二级回收:洗衣机内的分类是区分子集名字的分类
            //这里取不会错，因为真尾巴才会放在这里
            obj = m_recycler.Pop(type.FullName) as EntityObject;
            if (obj == null)
            {
                //是否是池中物,金鳞?
                useRecycler = false;
                obj = new T();
            }

            //实际管理:[工厂招聘]
            obj.InstanceInFactory();//being Useing

            m_listObject.Add(obj);

            if (EnableLog && Debuger.EnableLog)
            {
                Debuger.Log(LOG_TAG, "InstanceEntity() {0}:{1}, UseRecycler:{2}", obj.GetType().Name, obj.GetHashCode(), useRecycler);
            }

            return (T)obj;
        }

        /// <summary>
        /// 2UnLoad
        /// 释放一个实例
        /// </summary>
        /// <param name="entity">EntityObject.</param>
        public static void ReleaseEntity(EntityObject obj)
        {
            if (obj != null)
            {
                if (EnableLog && Debuger.EnableLog)
                {
                    Debuger.Log(LOG_TAG, "ReleaseEntity() {0}:{1}", obj.GetType().Name, obj.GetHashCode());
                }
                //[工厂解雇到后备公司]
                obj.ReleaseInFactory();
                //这里不立即从listObject中删除，
                //而是在下一个逻辑循环统一进行删除
                //这样做可以提高效率
                //[统一组]逻辑;在[进][持续][出*]的情况下,对实际压池逻辑,操作实现缓存压栈[其实是工厂先开除,名单延迟批量交给后备公司]
            }
        }



        /// <summary>
        /// Clears the released objects.
        /// 清理已经被释放的实例，并且对其进行回收
        /// *缓存到本帧结束,按照逻辑帧来处理逻辑
        /// %%%现象[优先逻辑执行Release(销毁状态)(销毁逻辑)(销毁标记)]---(对象内存一直存在)---延迟到EndFrame[(运行时集移除)(对象转存到回收池集合)(完成集合中转)]
        /// </summary>
        public static void ClearReleasedObjects()
        {
            //倒叙回收缓存
            for (int i = m_listObject.Count - 1; i >= 0; i--)
            {
                if (m_listObject[i].IsReleased)
                {
                    EntityObject obj = m_listObject[i];
                    m_listObject.RemoveAt(i);

                    //将对象加入对象池
                    m_recycler.Push(obj);
                }
            }
        }

    }

    public static class EntityFactoryUtils
    {
        //TODO: dl 改名，没想到一个好的名字

        public static bool CheckIsTriggleAOIEntity(AOIEntityObject AOIEntity, ulong curEntityID, int factionType, SelectType selectType, bool isForbidSelect = true)
        {
            //目标相当，说明是当前的entity，要找能释放技能的其它实体
            if (AOIEntity.EntityId == curEntityID)
            {
                return false;
            }
            // 如果我没有阵营，那谁也打不到
            if (factionType == -1)
            {
                return false;
            }
            //// 跟自己同阵营--不计算
            //if (AOIEntity.Faction == factionType)
            //{
            //    return false;
            //}
            // 如果是目标是死亡状态就过滤掉
            if (!AOIEntity.M_IsAlive)
            {
                return false;
            }
            // 如果是子弹的话就过滤掉-》后期技能配置加参数，这个技能会选中那些实体的类型
            if (AOIEntity.EntityType == E_EntityType.BulletEntity || AOIEntity.EntityType == E_EntityType.Npc)
            {
                return false;
            }
            if (AOIEntity.Data != null && AOIEntity.Data.IsForbidSelect && isForbidSelect)
            {
                return false;
            }
            switch (selectType)
            {
                case SelectType.Friend:
                    {
                        if (AOIEntity.FriendlyFaction == null)
                        {
                            return false;
                        }
                        for (int i = 0; i < AOIEntity.FriendlyFaction.Count; i++)
                        {
                            if (factionType == AOIEntity.FriendlyFaction[i])
                            {
                                return true;
                            }
                        }
                    }
                    break;
                case SelectType.Enemy:
                    {
                        if (AOIEntity.OpposingFaction == null)
                        {
                            return false;
                        }
                        for (int i = 0; i < AOIEntity.OpposingFaction.Count; i++)
                        {
                            if (factionType == AOIEntity.OpposingFaction[i])
                            {
                                return true;
                            }
                        }
                    }
                    break;
                case SelectType.All:
                    {
                        return true;
                    }
                    break;
                    ///////////---------------------- 以下后面做
                case SelectType.FriendPlayer:
                    break;
                case SelectType.EnemyPlayer:
                    break;
                case SelectType.AllPlayer:
                    break;

                case SelectType.FriendBullet:
                    break;
                case SelectType.EnemyBullet:
                    break;
                case SelectType.AllBullet:
                    break;
                case SelectType.FriendlyTeam:
                    break;

                default:
                    break;
            }
            return false;
        }

    }
}
