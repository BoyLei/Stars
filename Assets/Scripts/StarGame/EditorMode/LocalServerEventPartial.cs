using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Network;
using Sirenix.Utilities;
using SkillEditor;
using StarProject.Game.Player;
using StarProject.Game.Skill;
using StarProjectDef;
using UnityEngine;
using ProVec3 = ProtoMsg.Vector3;
using UVec3 = UnityEngine.Vector3;

namespace EditorModeTest
{
    /// <summary>
    /// 本地服处理客户端请求事件  和 本地服发送客户端事件 部分。
    /// </summary>
    public partial class LocalServer
    {
        public void OnClientReqLocalServerEvent(ClientEventReq localServerClientCMD, object[] data)
        {
            switch (localServerClientCMD)
            {
                case ClientEventReq.CreateMainPlayer:
                    {
                        CreateMainPlayer((UserMainDataNotify)data[0]);
                    }
                    break;
                case ClientEventReq.CreateMonster:
                    {
                        // 创建 一个 怪物 、
                        localEntityManager.CreateMonster((int)data[0]);
                    }
                    break;
                case ClientEventReq.CreateBullet:
                    {
                        I_EffectParam effectParam = (I_EffectParam)data[0];
                        BaseBlackBoard baseBlackBoard = (BaseBlackBoard)data[1];
                        CreateBullet(effectParam, baseBlackBoard);
                    }
                    break;
                case ClientEventReq.MoveMsg2:
                    {
                        // 本地服收到的主角 的移动, 需要做两步:
                        // 1.更新 本地服 MainPlayer 的 坐标 和 朝向 属性;
                        // 2.本底服的 属性 反向推送到 客户端的 实体身上.
                        localEntityManager.OnEntityMove(localEntityManager.MainPlayer.EntityID, (MoveMsg2)data[0]);
                    }
                    break;
                case ClientEventReq.SetBulletTargetPos:
                    {
                        ulong entityID = (ulong)data[0];
                        ulong runtimeID = (ulong)data[1];
                        UVec3 targetPos = (UVec3)data[2];
                        int serverRot = (int)data[3];
                        UVec3[] path = (UVec3[])data[4];

                        // 本地服收到客户端 设置子弹目标的请求,需要做两部:
                        // 1.将效果 产生的黑板 数据存入 运行时中, 同时 下发给客户端;
                        // 2.将目标节点 转换成 路点属性, 下发给子弹实体.(对于服务器来说 是将目标点交给移动模块)
                        SetBulletTargetPos(entityID, runtimeID, targetPos, serverRot, path);
                    }
                    break;
                case ClientEventReq.BreakCurRuntimeInBullet:
                    {
                        ulong entityID = (ulong)data[0];
                        ulong runtimeID = (ulong)data[1];
                        EffectTypeBreakCurRuntimeInBullet effect = (EffectTypeBreakCurRuntimeInBullet)data[2];
                        BreakCurRuntimeInBullet(entityID, runtimeID, effect);
                    }
                    break;
                case ClientEventReq.TriggerEvent:
                    {
                        ulong entityID = (ulong)data[0];
                        TriggerEvent triggerEvent = (TriggerEvent)data[1];
                        var triggerData = data[2];
                        localEntityManager.OnTriggerEvent(entityID, triggerEvent, triggerData);
                    }
                    break;
                case ClientEventReq.UpdatePosAndRot:
                    {
                        // 使用技能时，同步数据到本地服的 实体层
                        ulong entityID = (ulong)data[0];
                        var pos = (ProVec3)data[1];
                        var rot = (int)data[2];
                        localEntityManager.UpdatePosAndRot(entityID, pos, rot);
                    }
                    break;
                default:
                    {
                        SGF.Debuger.LogError($"{TagFlag} 本地服 没有处理: {localServerClientCMD} 的接口");
                    }
                    break;
            }
        }

        public void CreateMainPlayer(UserMainDataNotify userMainData)
        {
            localEntityManager.CreateMainPlayer(userMainData);
        }

        /// <summary>
        /// 本地服创建一个子弹的效果, 此处需要模拟一个 子弹的创建流程.
        /// 具体 可以 分为:
        /// 1. AOI 实体的创建
        /// 2. 子弹运行时的创建
        /// 3. 子弹相关属性、黑板数值的 创建
        /// 4. 子弹 AOI 属性的同步
        /// 5. 子弹 阶段的创建 
        /// 6. 子弹阶段 运行时同步
        /// 7. 子弹的命中检测
        /// 8. 子弹的销毁
        /// 9. 子弹AOI实体的销毁
        /// </summary>
        /// <param name="effectParam">创建子弹的效果参数</param>
        /// <param name="createrBlackBoard">创建者 的 黑板运行时 </param>
        public bool CreateBullet(I_EffectParam effectParam, BaseBlackBoard createrBlackBoard)
        {
            EffectTypeCreateBullet createBullet = effectParam.BaseEffect as EffectTypeCreateBullet;

            // 中心点坐标
            RepeatedField<ProVec3> posArr = EffectUtils.GetCenterPosArray(effectParam, createBullet.CenterPosArray, createrBlackBoard);

            if (posArr.Count == 0)
            {
                return false;
            }

            // 朝向
            RepeatedField<int> towardArr = EffectUtils.GetTowardArray(effectParam, createBullet.TowardArray, createrBlackBoard);

            if (towardArr.Count == 0)
            {
                return false;
            }

            List<CustomDictionary> bulletUseKey = createBullet.BulletCopyData;

            // 提前查找出 需要给子弹赋值的 黑板数据存下来，免得在子弹创建的时候 还要再去找.
            Dictionary<string, object> useKeyData = new();
            {
                string fromKey = "";
                string toKey = "";

                for (int k = 0; k < bulletUseKey.Count; k++)
                {
                    var usekey = bulletUseKey[k];
                    fromKey = usekey.FromKey.Result;
                    toKey = usekey.ToKey.Result;

                    object v = createrBlackBoard.GetKey<object>(fromKey);
                    if (v == null)
                    {
                        SGF.Debuger.LogError($"{TagFlag} 创建子弹: {createBullet.BulletID} 缺少黑板数据 fromKey: {fromKey} ");
                    }
                    else
                    {
                        SGF.Debuger.LogWarning($"{TagFlag} 创建子弹: {createBullet.BulletID} 设置黑板数据 fromKey: {fromKey} ---> toKey: {toKey} ");

                        // 都是客户端模拟的服务器数据, 所以此处 也是存入 client 黑板
                        // bulletBlackBoard.Set(toKey, v, E_BlackBoardTag.Client);
                        useKeyData[toKey] = v;
                    }
                }
            }


            for (int i = 0; i < posArr.Count; i++)
            {
                var pos = posArr[i];
                for (int j = 0; j < towardArr.Count; j++)
                {
                    var torward = towardArr[j];
                    var bulletBlackBoard = new BaseBlackBoard();

                    // 每个 子弹 都需要一个 自己独立的黑板, 虽然 下面的查找有点浪费
                    useKeyData.ForEach((item) =>
                    {
                        bulletBlackBoard.Set(item.Key, item.Value, E_BlackBoardTag.Client);
                    });

                    // 创建 一个子弹
                    localEntityManager.CreateBullet(pos, torward, createBullet.BulletID, bulletBlackBoard, localEntityManager.MainPlayer);
                }
            }

            return true;
        }

        public bool SetBulletTargetPos(ulong entityID, ulong runtimeID, UVec3 targetPos, int serverRot, UVec3[] path)
        {
            return localEntityManager.SetBulletTargetPos(entityID, runtimeID, targetPos, serverRot, path);
        }

        public bool BreakCurRuntimeInBullet(ulong entityID, ulong runtimeID, EffectTypeBreakCurRuntimeInBullet effect)
        {
            return localEntityManager.BreakCurRuntimeInBullet(entityID, runtimeID, effect);
        }

    }
}
