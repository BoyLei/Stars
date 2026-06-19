using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.VitalSigns;
using StarProjectDef;
/// <summary>
/// 怪物显示层
/// </summary>
namespace StarProject.Game.Entity.View.VitalSign
{
    public class ViewVitalMonsterNormal : ViewVitalNPCNormal
    {
        public new MonsterEntityBase M_EntityBase
        {
            get
            {
                return m_entity as MonsterEntityBase;
            }
        }

        protected override void Create(EntityObject entity)
        {
            m_entity = entity as MonsterEntityBase;

            if (M_EntityBase == null) return;

            base.Create(entity);

            // 修改人节点的tag
            if (M_EntityBase != null)
            {
                if (M_EntityBase.EntityType == E_EntityType.Monster || M_EntityBase.EntityType == E_EntityType.GVEBoss || M_EntityBase.EntityType == E_EntityType.Robot)
                {
                    gameObject.tag = E_TagType.Enemy.ToString();
                }
            }
        }

        protected override void Release()
        {

            base.Release();

            m_entity = null;
            m_context = null;
        }

        #region 可删除的

        #region 原有逻辑
        //protected virtual void Update()// { }
        ////private void FixedUpdate()
        //{
        //    //别人也不用我碰撞，服务器都知道NavMesh，属于非重要信息，【这种中转信息就属于单向RPC本质，虽然没调用人家方法】
        //    //1,碰撞我主导，移动我主导，主导属于重要，同步属于重要，发起者属于重要
        //    //2,别人属于非重要，我重要
        //    //3,别人不能放太多到fix，别人需要平滑但视情况而定,我必须保持驱动力
        //    if (M_EntityBase != null && m_renderer != null)
        //    {
        //        m_renderer.enabled = M_EntityBase.ShowData.bodyVisible;

        //        //if (!M_EntityBase.IsDriveByServerStop)
        //        //技能特殊移动，且已经结束
        //        //if (M_EntityBase.skillMovePara.ReadyDone)
        //        //{
        //        //    m_EntityPosition = M_EntityBase.Position();

        //        //    // 第一次 怪物是直接设置坐标
        //        //    if (tempV3Pos == Vector3.zero)
        //        //    {
        //        //        this.transform.localPosition = m_EntityPosition;        //只更新XZ
        //        //        this.transform.localEulerAngles = M_EntityBase.EulerAngles;
        //        //        tempV3Pos = m_EntityPosition;
        //        //        return;
        //        //    }

        //        //    if (!m_MoveLocker && !m_ForceMoveLocker)                    //双锁判断:之前的，不用判断了，双锁也没必要
        //        //    {
        //        //        pos = m_context.EntityToViewPoint(m_EntityPosition);
        //        //        //SGF.Debuger.Log("deltaTime" + Time.deltaTime);              //当前每次deltaTime，时间，不是累计到1的递增值
        //        //        //SGF.Debuger.Log("time" + Time.time);
        //        //        //LerpTime = GameConfig.FIX_RENDER_FRAME_SEC;//PLAYER_MOVE_SPEED_PER_FIXFRAME

        //        //        if (M_EntityBase.M_CtrlByServer)//服务器驱动——但是不知道什么时候下次驱动--所以在一个渲染帧里面做完
        //        //        {
        //        //            //LerpTime = 1;//这里面从来进不来
        //        //            M_EntityBase.IsDriveByServerStop = false;
        //        //            M_EntityBase.IsJoySitckStop = false;
        //        //            M_EntityBase.M_CtrlByServer = false;//此时交给客户端了
        //        //            m_MoveLocker = true;

        //        //            //Time.deltaTime//0.5秒被服务器//之后移动速度算

        //        //            //Getter:可以理解成初始值，临时形式封装属性，本质返回transform.localPosition，即Dotween去你初始运算的时候
        //        //            //我的Setter： （changeVector）out形参 set给我===Update他要一直给我 ：
        //        //            //他的运行机制是：初始的时候Get你一下和目标值和时间，一直做插值通过changeVector过程一直给你
        //        //            Vector3 distXZ = pos - transform.localPosition; //移动速度是表面移动速度，类似周长，我假设模拟的是XZ平面的，爬坡移动速度变快了其实，先用服务器设定同步就没问题
        //        //            distXZ.y = 0;
        //        //            float moveTimeSec = (distXZ).magnitude / M_EntityBase.Speed; //可能提前释放锁
        //        //            SGF.Debuger.Log("StopTime：" + moveTimeSec);
        //        //            LerpMover1 = DOTween.To(() => transform.localPosition, changeVector =>
        //        //            {
        //        //                monsterMoveMotion = changeVector - transform.localPosition; //时刻根据当前，取得下次的小步长
        //        //                monsterMoveMotion.y = this.VerticalVelocity;
        //        //                m_CharacterController.Move(monsterMoveMotion);              //时刻更新当前
        //        //            }, pos, moveTimeSec).SetEase(Ease.Flash);
        //        //            LerpMover1.onComplete = () =>//Ease.Linear :https://www.cnblogs.com/ckAng/p/10684650.html ；https://www.xuanfengge.com/easeing/easeing/   所以flash是平的
        //        //            {
        //        //                m_EntityPosition = M_EntityBase.Position();//本次结束的时候客户端本地一定到目标了，此时服务器是否具有新的位置：1，有就保持移动，2，没就暂停
        //        //                //SGF.Debuger.LogError($"VVMonsterNormal Update m_EntityPosition {m_EntityPosition} curPos {transform.localPosition} , onComplete ---> {m_EntityPosition == transform.localPosition} IsJoySitckStop {M_EntityBase.IsJoySitckStop} ReadyDone {M_EntityBase.skillMovePara.ReadyDone}");
        //        //                //通常先告诉技能


        //        //                //M_EntityBase.M_CtrlByServer = false;
        //        //                //if (transform.localPosition != m_EntityPosition) //不同位置=有新目标：经过dG的Chara的同步应该相差不大（z，x）；如果有相差一定高度（排除）或 新坐标发过来了
        //        //                tempDis = transform.localPosition - m_EntityPosition;
        //        //                if (tempDis.z > 0.1 || tempDis.x > 0.1)             //X或Z差距过大（容错之外）= 新目标
        //        //                {
        //        //                    M_EntityBase.IsDriveByServerStop = false;
        //        //                    M_EntityBase.IsJoySitckStop = false;                //新目标继续走
        //        //                }
        //        //                else
        //        //                {
        //        //                    M_EntityBase.IsDriveByServerStop = true;
        //        //                    M_EntityBase.IsJoySitckStop = true;
        //        //                }

        //        //                m_MoveLocker = false;
        //        //            };
        //        //        }
        //        //        //抛弃掉的地方---
        //        //        transform.localEulerAngles = M_EntityBase.EulerAngles;
        //        //        tempV3Pos = this.transform.localPosition;
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //    skillChangeMotion = M_EntityBase.skillMovePara.M_TotleMoveDisVec;    //技能引发位移
        //        //    skillChangeMotion.y = this.VerticalVelocity;
        //        //    m_CharacterController.Move(skillChangeMotion);
        //        //    this.transform.localEulerAngles = M_EntityBase.EulerAngles;
        //        //}
        //    }
        //}
        #endregion

        #region update
        //protected virtual void Update()
        //{
        //    //别人也不用我碰撞，服务器都知道NavMesh，属于非重要信息，【这种中转信息就属于单向RPC本质，虽然没调用人家方法】
        //    //1,碰撞我主导，移动我主导，主导属于重要，同步属于重要，发起者属于重要
        //    //2,别人属于非重要，我重要
        //    //3,别人不能放太多到fix，别人需要平滑但视情况而定,我必须保持驱动力

        //    if (M_EntityBase != null/* && m_renderer != null*/)
        //    {
        //        //m_renderer.enabled = M_EntityBase.ShowData.bodyVisible;
        //        // 过去式，用不到了
        //        //if (M_EntityBase.skillMovePara.SkillMoveDone)
        //        //{
        //        //    //normalMove
        //        //}
        //        //else
        //        //{
        //        //    //原子锁，技能优先，打断移动：
        //        //    //服务器会吧路点清楚，并且给校验，并且有技能位置
        //        //    //正常来说，如果移动还在继续，网络延迟：【技能优先】 
        //        //    //技能引发位移
        //        //    //TODO：技能位移可修改成，角色位移相互循环调用的

        //        //}
        //    }
        //}

        #endregion

        #endregion

    }
}
