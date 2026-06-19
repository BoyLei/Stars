using SGF.Time;
using SGF.Unity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Entity.WithOutLife;
using StarProject.Game.Skill;
using StarProjectDef;
using Suriyun.MCS;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 这里全是显示层，，mono显示层
/// </summary>
namespace StarProject.Game.Entity.View.OutLife
{
    public class OutLifeEntityViewBase : ViewModel
    {
        [SerializeField]
        protected Vector3 m_EntityPosition;

        //别人的重力来源于位置，位置来源于服务器，来源于我的信息中转
        //我来源于碰撞，来源于重力，所以最终来源于我驱动的Fix
        //主角有就好，其他人都不用，因为服务器转达加存储，但是不算
        //private float _verticalVelocity = 0f;

        protected EntityRemoteStatic m_entity;
        protected GameContext m_context;
        protected bool m_visible = true;
        //protected MeshRenderer m_renderer;

        protected Vector3 pos;
        //private GameCamera GCamera;//也不是狙击手游戏

        private EntityOutLifeType nttType;


        protected override void Create(EntityObject entity)
        {
            //重力
            m_entity = entity as EntityRemoteStatic;

            nttType = m_entity.GetyOutLifeType;
            switch (nttType)
            {
                case EntityOutLifeType.FollowRole:
                    transform.localPosition = Vector3.zero;
                    break;
                case EntityOutLifeType.MoveStandAlong:
                    break;
                default:
                    break;
            }

            m_entity.ViewEnterFrameAction += OnEnterFrame;


            m_context = GameManager.Instance.Context;
            m_visible = true;



            //m_renderer = this.GetComponent<MeshRenderer>();
            //if (m_renderer == null)
            //{
            //    m_renderer = this.GetComponentInChildren<MeshRenderer>();
            //}

            //if (m_renderer != null)
            //{
            //    m_renderer.enabled = true;
            //    //m_renderer.color = m_context.GetUniqueColor(m_entity.TeamId);
            //    if (m_entity.Index > 0)//第几个人，第几个节点
            //    {
            //        //m_renderer.sortingOrder = 10000 - m_entity.Index;
            //    }
            //    else
            //    {
            //        //if (m_entity is SnakeTail)
            //        //{
            //        //    m_renderer.sortingOrder = 0;
            //        //}
            //        //else if (m_entity is SnakeHead)
            //        //{
            //        //    m_renderer.sortingOrder = 10000;
            //        //}
            //    }
            //}


        }


        /// <summary>
        /// FixUpdate去更新
        /// PS:这里如果不需要依赖unity的物理，如ChapterCtrl
        /// 那就不会View反馈到logic进行更正
        /// 只需要逻辑到显示推送，或者说显示服从逻辑
        /// </summary>
        /// <param name="obj"></param>
        private void OnEnterFrame(Vector3 skillmove)
        {
            //m_renderer.enabled = m_entity.ShowData.bodyVisible;
            if (m_entity != null/* && m_renderer != null*/)
            {
                if (nttType == EntityOutLifeType.MoveStandAlong)
                {
                    //if (skillmove == Vector3.zero)
                    //{
                    //更新逻辑Logic做
                        transform.position = m_entity.Position();
                    //}
                    //else
                    //{
                    //    //驱动，物理，反馈
                    //    transform.position += skillmove;
                    //    //后续的更新和反馈
                    //}
                   
                }

            }
        }



        protected override void Release()
        {
            //先解开注册，再制空
            m_entity.ViewEnterFrameAction -= OnEnterFrame;
            m_entity = null;
            m_context = null;
        }

        /// <summary>
        /// 动态帧率 60~70
        /// </summary>
        protected virtual void Update()
        {
            #region 机制复习

            //m_renderer.enabled = m_entity.ShowData.bodyVisible;
            if (m_entity != null/* && m_renderer != null*/)
            {

            }
        }


        //private float VerticalVelocity
        //{
        //    get
        //    {
        //        //If the entity is grounded
        //        if (this.IsGrounded())
        //        {
        //            _verticalVelocity = 0;
        //        }
        //        else
        //        {
        //            //重力加速度 Vt = 0 + at
        //            //通常离散时间，调用点（递增点为）1秒
        //            //每次1/30秒，增加1/30份向下速度，虽离散但更平滑的处理即刻下降速度，累加确定向下即时速度Vt
        //            //每次间隔1/N秒，共调用N次，总系数为1，调用时总距离就是最后【1秒的Vt】，每次调用就是【此帧刻的Vt】，公式嘛
        //            _verticalVelocity += Physics.gravity.y * Time.fixedDeltaTime;
        //            //-9.8f
        //            //0.033f
        //        }

        //        return _verticalVelocity;
        //    }
        //}
        /// <summary>
        /// Returns if the entity is grounded or not
        /// </summary>
        /// <returns>True if the entity is grounded, else false</returns>
        //private bool IsGrounded()
        //{
        //    if (m_CharacterController.isGrounded)
        //        return true;
        //    else
        //        return false;

        //}



        #endregion
    }
}
