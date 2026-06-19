using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;

/// <summary>
/// //=========================================================================================================================================================================== 
/// ---[远近]分离，[动静]分离---
/// 表现层的：动态和批，静态是Static
/// ---[通讯项:进入-更新-离开][结构:awake，destroy，显示，隐藏][可选:其他拓展如怪物挂掉]   ：[通讯有一个准则:客户端不具备完全逻辑的，通过通知;客户端有完全逻辑的：通过属性]
/// 这里是逻辑层：
/// //=========================================================================================================================================================================== 
/// 【状态同步】
/// 远动态：服务器控制其完整生命周期【C关心，进入-更新-离开】                 【特性：随时可能初始化，结束化，过程更新化属性，通知变化】 
/// 远静态：【生命周期相对静态：全局创建天气（不在乎范围的触发）】【不更新】  【特性：初始化创建作为提前数据，结束销毁：更接近一个提前消息：技能（通知只是【效果】和【数据】）】 
/// 【模拟】
/// 本地静态：静态资源
/// 本地动态：动态资源，具备交互的，客户端掌管其完整生命周期的
/// //=========================================================================================================================================================================== 
/// IRecyclableObject               回收标识
/// EntityObject                    基础实体:有行为的逻辑元体，【初始化，在乎其生命周期，消亡化】，【关键工人】，【有位置】，【IO】，
/// 远程动态，                                                                                      远程静态                                                                      ，本地动态，本地静态
/// AOIEntityObject                 [通讯项:进入-更新-离开][结构:继承生死][可选:显示隐藏]{远+动态} 【PassiveSkillEntity      SkillContainer      SkillEntity】  
/// VitalSignsBase（NPC）                AOIEntityObjectAttribute(删)                         ObjectSignDataBase
///HeroEntityBase MonsterEntityBase /VitalSingnsObjectEntityBase/ /WithOutLifeEntityBase(远程动态）/
///                                                                                  AuxiliarySummoner /InfoEntity/ BulletEntity
/// //=========================================================================================================================================================================== 
/// 是否可以运动，
/// 
/// 
/// 驱动接口：控制接口，Ai接口
/// 
/// 阵营属性===
/// 
/// /// </summary>
namespace StarProject.Game.Entity.Factory
{
    public abstract class EntityObject : IRecyclableObject
    //,IRelease，释放约束，重复代码，哪层的东西哪层处理根据特性
    {
        //【释放标记-，帧尾释放检测逻辑，通过list和标记做处理】【不会对其进行检测是否可以pop】---------------------------------------------------------------------
        private bool m_isReleased = false;//怕被释放，本质是true，已经释放的未出生
        public bool IsReleased { get { return m_isReleased; } }

        /// <summary>
        /// 一个entity 关联的 实体clistDic
        /// 1我们假设，2任意个实体都，3可以包含分组的，4组内有很多成员的实体
        //   //1里面会存储，诸如：火熊猫护盾技能id = 1 得ui，场景，主角，boss唯一得buff；2虽然来源于同一个数据，但是必须为池实体；3所以没必要分组是同一个id技能不同得显示也是一个逻辑实体最后呈现为一个数据N个事件N个logic个N个View
        ///
        /// 这里容纳了结构的统一性，和生命周期的处理性
        /// 相比较于顶一个结构类【封装类】
        /// 【-封装更在乎其中组合之间的【相互，生命，逻辑，操作】关系-如PlayerCtrlGroup】封装类对于方法的拓展操作性较好，自我维护性较差，需要创建组合如果组合没有很多逻辑，和相互关系就没用--
        /// 【数据结构那种，更好维护，不需要处理，底层提供容纳性质】更在乎logic自我的自我迭代，虽然简单，但是适用于子节点的低耦合情况
        /// *目前采用是结构，我假设的都是没相对关系的，假设Buff1销毁，Buff2不会销毁，排布你管你排布的那是uiMgr的事情即可
        /// 如果有强大的互相依赖关联之后考虑【再拓展】类PlayerCtrlGroup结构
        /// </summary>
        private DictionaryEx<EnumEnityListKey, List<EntityObject>> entityListDic = new DictionaryEx<EnumEnityListKey, List<EntityObject>>();

        /// <summary>
        /// [工厂录用]
        /// </summary>
        internal void InstanceInFactory()
        {
            //开始才能标记为不释放
            m_isReleased = false;//beingUseing
        }

        //通过具体工厂释放：引用----------------------------------------------------------------------
        /// <summary>
        /// [工厂解雇]
        /// </summary>
        internal void ReleaseInFactory()
        {
            //初始化 = 运行用  ===转换为===> 准备释放标记 + 处理子类Release
            if (!m_isReleased)
            {
                //清理数据
                Release();
                //准备释放，并且一定帧尾释放
                m_isReleased = true;
            }
        }
        ~EntityObject()
        {

        }



        /// <summary>
        /// 无定规则的抽象者
        /// 如有共有逻辑则修改为虚函数
        /// [解雇附加条件]
        /// </summary>
        protected virtual void Release()
        {
            if (entityListDic != null && entityListDic.Count != 0)
            {
                foreach (var lists in entityListDic)
                {
                    if (lists.Value != null && lists.Value.Count != 0)
                    {
                        for (int i = 0; i < lists.Value.Count; i++)
                        {
                            lists.Value[i].Release();
                        }
                        lists.Value.Clear();
                    }
                }
                entityListDic.Clear();

            }
        }


        //【位置信息】！----------------------------------------------------------------------
        public virtual Vector3 Position()
        {
            return Vector3.zero;
        }




        //【加减EntityList】----------list处理在生命体，非生命体中具体处理--------------------------------------------
        protected List<EntityObject> GetEntityList(EnumEnityListKey key)
        {
            //取他就说明你肯定想用，
            //EnsureEntityListInit();
            //Get：Key，List
            if (entityListDic.ContainsKey(key))
            {
                return entityListDic[key];
            }
            else
            {
                entityListDic.Add(key, new List<EntityObject>());
            }
            return entityListDic[key];
        }





        //innerface----------------------------------------------------------------------

        public string GetRecycleType()
        {
            return this.GetType().FullName;
        }
        /// <summary>
        /// [社会移除掉这个实体-_-!!!]
        /// </summary>
        public void Dispose()
        {
            //由系统的GC机制来处理
            //Do nothing!
        }
        //----------------------------------------------------------------------
    }
}