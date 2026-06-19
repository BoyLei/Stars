using System.Collections;
using System.Collections.Generic;
using ProtoMsg;
using SGF.Time;
using SkillEditor;
using StarProject;
using StarProject.Game.Skill;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;


namespace EditorModeTest
{


    /// <summary>
    /// 子弹运行时.
    /// 1.子弹运行时 包含2部分， 子弹自己的运行/阶段运行时;
    /// 2.子弹的阶段 由子弹运行时 自己创建, 
    /// 3.子弹的阶段 有两种类型, 普通阶段 和 子弹阶段。 子弹阶段主要用来控制子弹位移
    /// 
    /// note:
    /// 1.阶段 效果的触发 先由客户端线 自己触发, 本地服目前没有 跑效果线的逻辑, 挪过来不实际.
    /// 2.本地服 主要控制 阶段的创建和销毁, 目前的想法是 跟服务器一样 采用定时器来时间.
    /// 3.子弹位移 服务器 是通过 路点发送给客户端, 这块 需要具体情况具体分析。 追踪形子弹 和 固定路线子弹不一样。
    ///   不过 应该是都教给 客户端模拟。 
    /// 4.子弹 飞行过程中的 碰撞 交给客户端 每帧检测，检测到碰撞的时候通知 本地服. 伤害效果 也是 客户端检测 然后告诉 本地服.
    /// 5.子弹是否 销毁 由 命中效果 和 阶段时间 共同确定. 任意一个需要销毁即 通知客户端销毁
    /// </summary>
    public class LocalBulletRuntime : BaseParentRuntime
    {
        /// <summary>
        /// 配置 id
        /// </summary>
        public int cfgID;

        BulletJson bulletJson;

        public override List<StageJson> NormalStages => bulletJson.Normals;
        public override List<StageJson> BulletStages => bulletJson.Bullets;
        public override List<StageJson> OtherStages => bulletJson.Others;


        public static BulletCreateRet bulletCreateRet = new BulletCreateRet();
        public static BulletEndRet bulletEndRet = new BulletEndRet();


        // 子弹阶段 的创建和 销毁 跟服务器一样 用定时器来做.
        // 如果 用 OnTick , 那阶段之间就需要补帧， 很麻烦
        public override void OnTick()
        {
            return;
            // 如果子弹的普通阶段都不存在了, 说明子弹正常运行结束了
            //if (normalStageRuntime == null)
            //{
            //    // 通知外面子弹运行时销毁
            //    return;
            //}

            //normalStageRuntime.OnTick();

            //// 子弹移动阶段 结束也不会导致
            //if (bulletStageRuntime != null)
            //{
            //    bulletStageRuntime.OnTick();
            //}

        }

        /// <summary>
        /// 子弹 运行时
        /// 跟 孔磊 沟通, 子弹运行时 一定 由一个运行时创建, 它不能独立凭空创建(子弹一定有个 builderID)
        /// </summary>
        /// <param name="bulletID"></param>
        /// <param name="bulletBlackBoard"></param> <summary>
        public LocalBulletRuntime(int bulletID, BaseBlackBoard bulletBlackBoard)
        {
            UID = EditorMode.Instance.localServer.localServerIDManager.GetUID();
            runtimeEnumType = RuntimeEnumType.Bullet;

            // 本地服 的时间 用客户端本地时间
            CreateTime = TimeUtils.ClientNowStampMilli;


            cfgID = bulletID;

            RuntimeID = EditorMode.Instance.localServer.localServerIDManager.GetRuntimeID();

            // 配置表已经提前预加载, 所以此处是同步
            LocalDataManager.Instance.GetBulletJson(bulletID, (BulletJson json) =>
            {
                bulletJson = json;
            });

            // 创建 子弹自己的黑板
            CreateBlackBoard(bulletBlackBoard);
        }

        public void OnEnter(ulong ownerID, ulong builderID)
        {
            OwnerEntityID = ownerID;

            BuilderID = builderID;

            // 子弹运行时创建时 发送 运行时创建 消息
            SendBulletCreate();

            // 阶段的创建 放在 运行时创建之后， 配置是之前就预加载， 所以此处同步的顺序进来配置表已经加载完成
            CreateNormalStage();

            CreateBulletStage();

            // 创建触发器, 触发器只有在触发的时候才需要 创建对应的触发器阶段
            CreateOhterStage();

        }

        public override void OnExitRuntime()
        {
            OnBulletRuntimeEnd();
        }


        /// <summary>
        /// 结束 子弹运行时 
        /// </summary>
        public void OnBulletRuntimeEnd()
        {
            if (normalStageRuntime != null)
            {
                normalStageRuntime.ExitStage(true);
                normalStageRuntime = null;
            }

            if (bulletStageRuntime != null)
            {
                bulletStageRuntime.ExitStage(true);
                bulletStageRuntime = null;
            }

            SendBulletEnd();
        }

        public void CreateBlackBoard(BaseBlackBoard bulletBlackBoard)
        {
            blackBoard = bulletBlackBoard;

            // 子弹的黑板
            // 子弹的黑板 需要由外部效果 写入数据
        }




        public void SendBulletCreate()
        {
            bulletCreateRet.OwnerEntityID = OwnerEntityID;
            bulletCreateRet.RuntimeID = RuntimeID;
            bulletCreateRet.BulletID = cfgID;
            bulletCreateRet.BuilderID = BuilderID;

            bulletCreateRet.BlackList.Clear();
            // TODO: 子弹的黑板数据

            var cusBlackBoardData = new CusBlackBoardData();
            cusBlackBoardData.RuntimeID = RuntimeID;
            cusBlackBoardData.CusBlackType = CusUpdateBlackType.BulletCraete;
            cusBlackBoardData.blackBoard = blackBoard;


            // 发送运行时创建消息
            EditorMode.Instance.localServer.SendMsg((msgData, otherData) =>
            {
                GlobalEvent.OnLocalServerEvent?.Invoke(LocalServerEventRsp.BulletCreateRet, LocalServer.RPCMsgPacker(msgData, OwnerEntityID));
                SGF.Debuger.LogWarning($"{LocalServer.TagFlag} 子弹运行时创建 bulletCreateRet: [{msgData}] ");

                GlobalEvent.OnLocalServerEvent?.Invoke(LocalServerEventRsp.UpdateCusBlackBoard, LocalServer.RPCMsgPacker(otherData, OwnerEntityID));
                SGF.Debuger.LogWarning($"{LocalServer.TagFlag} 子弹 更新创建黑板  : [{otherData}] ");

            }, bulletCreateRet, MsgType.Delay, cusBlackBoardData);


        }


        public void SendBulletEnd()
        {
            bulletEndRet.OwnerEntityID = OwnerEntityID;
            bulletEndRet.RuntimeID = RuntimeID;
            bulletEndRet.BulletID = cfgID;

            // 先子弹结束类型 用默认的结束类型
            bulletEndRet.EndType = SKillEndType.Default;

            bulletEndRet.BlackList.Clear();

            // TODO: 黑板数据 先不管

            EditorMode.Instance.localServer.SendMsg((data, otherData) =>
            {
                GlobalEvent.OnLocalServerEvent?.Invoke(LocalServerEventRsp.BulletEndRet, LocalServer.RPCMsgPacker(data, OwnerEntityID));
                SGF.Debuger.LogWarning($"{LocalServer.TagFlag} 子弹运行时结束 bulletEndRet: [{data}] ");
            }, bulletEndRet, MsgType.Delay);

        }
    }
}