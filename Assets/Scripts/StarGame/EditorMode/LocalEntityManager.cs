using Google.Protobuf;
using ProtoMsg;
using Sirenix.Utilities;
using SkillEditor;
using StarProject;
using StarProject.Game.Skill;
using StarProject.Service.LocalData;
using StarProjectDef;
using ProVec3 = ProtoMsg.Vector3;
using UVec3 = UnityEngine.Vector3;

namespace EditorModeTest
{
    public class LocalEntityManager
    {
        public LocalEntityManager()
        {
            propSyncList1 = new PropSyncList();
            propSyncList1.Prop = new PropBaseSyncList();

            aoiMsg = new AOIMsg();
        }
        /// <summary>
        /// 本地服 创建的一份 玩家 localServerEntity
        /// </summary>
        public LocalServerEntity MainPlayer;

        /// <summary>
        /// 本地创建的实体
        /// </summary>
        /// <typeparam name="ulong"></typeparam>
        /// <typeparam name="LocalServerEntity"></typeparam>
        /// <returns></returns>
        private DictionaryEx<ulong, LocalServerEntity> localEntitys = new();

        bool dirty = false;



        public void OnTick()
        {
            // 对实体执行 tick
            localEntitys.ForEach((item) =>
            {
                item.Value.OnTick();
            });

            // 主角如果有 属性同步的话， 就先发
            DirtySendPropSyncList();

            // 每次tick 检测是否需要同步 aoiMsg
            DirtySendAoiMsg();

        }

        ProtoMsg.AOIMsg aoiMsg;
        private void DirtySendAoiMsg()
        {
            dirty = false;

            if (aoiMsg.EnterAOIs.Count > 0)
            {
                dirty = true;
            }

            if (!dirty && aoiMsg.UpdateAOIs.Count > 0)
            {
                dirty = true;
            }

            if (!dirty && aoiMsg.LeaveAOIs.Count > 0)
            {
                dirty = true;
            }

            if (dirty)
            {
                EditorMode.Instance.localServer.SendMsg((object msgData, object otherData) =>
                {
                    GlobalEvent.OnLocalServerEvent?.Invoke(LocalServerEventRsp.AOIMsg, msgData);
                    SGF.Debuger.LogWarning($"{LocalServer.TagFlag} 发送 AOIMsg {msgData}");

                }, aoiMsg, MsgType.AOI);

                aoiMsg.EnterAOIs.Clear();
                aoiMsg.UpdateAOIs.Clear();
                aoiMsg.LeaveAOIs.Clear();
            }

            dirty = false;
        }

        public void UpdateEntityAOI(UpdateAOI updateAOI)
        {
            aoiMsg.UpdateAOIs.Add(updateAOI);
        }

        public void OnActionChangeProto(ulong entityID, string entityType, PropBaseSyncList propBaseSyncList)
        {
            UpdateAOI updateAOI = new();
            updateAOI.Prop = new PropSyncList();
            updateAOI.Prop.Prop = new PropBaseSyncList();
            updateAOI.Prop.Prop.MergeFrom(propBaseSyncList);

            updateAOI.Prop.EntityID = entityID;
            updateAOI.Prop.EntityType = entityType;

            UpdateEntityAOI(updateAOI);
        }

        PropSyncList propSyncList1;

        void DirtySendPropSyncList()
        {
            dirty = false;
            if (propSyncList1.Prop.Prop.Count > 0)
            {
                dirty = true;
            }

            // 此处使用dirty 是为了以后有多种情况 发送
            if (dirty)
            {
                EditorMode.Instance.localServer.SendMsg((object msgData, object otherData) =>
                {
                    GlobalEvent.OnLocalServerEvent?.Invoke(LocalServerEventRsp.PropSyncList, msgData);
                    //SGF.Debuger.LogWarning($"{LocalServer.TagFlag} 发送 PropSyncList {msgData}");
                }, propSyncList1, MsgType.Post);
                propSyncList1.Prop.Prop.Clear();
            }

            dirty = false;
        }

        public LocalServerEntity GetServerEntity(ulong entityID)
        {
            return localEntitys[entityID];
        }
        public bool HasServerEntity(ulong entityID)
        {
            return localEntitys.ContainsKey(entityID);
        }

        /// <summary>
        /// 创建实体
        /// </summary>
        /// <param name="e_EntityType"></param>
        /// <param name="prop"></param>
        /// <param name="entityID">实体id, 如果非0 就按传进来的 id</param>
        public LocalServerEntity CreateEntity(E_EntityType e_EntityType, PropBaseSyncList prop, ulong entityID = 0, bool isMainPlayer = false)
        {
            entityID = entityID != 0 ? entityID : EditorModeTest.EditorMode.Instance.localServer.localServerIDManager.GetEntityID();
            // 创建对应的实体
            LocalServerEntity localServerEntity = new(e_EntityType, entityID, prop);
            localEntitys.Add(localServerEntity.EntityID, localServerEntity);
            localServerEntity.ActionOnChangeProto = OnActionChangeProto;

            // 主角的创建流程先不放这块， 后续完善后 可以挪进来
            if (!isMainPlayer)
            {
                // 通知到客户端
                EnterAOI enterAOI = new();
                PropSyncList propSyncList = new();
                enterAOI.Prop = propSyncList;

                propSyncList.EntityID = localServerEntity.EntityID;
                propSyncList.EntityType = localServerEntity.EntityType;
                propSyncList.Prop = localServerEntity.PropList;

                aoiMsg.EnterAOIs.Add(enterAOI);
            }


            return localServerEntity;
        }

        public void CreateMainPlayer(UserMainDataNotify userMainData)
        {
            SGF.Debuger.LogWarning($"{LocalServer.TagFlag} 创建主角 实体ID: [{userMainData.EntityID}] ");

            MainPlayer = CreateEntity(e_EntityType: E_EntityType.Player, userMainData.Prop, userMainData.EntityID, true);
        }

        /// <summary>
        /// 创建子弹 分为2步:
        /// 1.先创建一个 子弹AOI实体
        /// 2.创建子弹运行时
        /// </summary>
        /// <param name="proVec3"></param>
        /// <param name="toward"></param>
        /// <param name="bulletBlackBoard"></param>子弹的黑板 <summary>
        /// <param name="creater"></param>创建者 <summary>
        public void CreateBullet(ProVec3 proVec3, int toward, int bulletID, BaseBlackBoard bulletBlackBoard, LocalServerEntity creater)
        {
            PropBaseSyncList propBaseSyncList = new();

            // propBaseSyncList.Prop
            // 子弹 TinyEntity.json 的属性配置
            {
                /**
                "desc": "子弹",
                "props": [
                    "Index",
                    "Faction",
                    "PathPoses",
                    "CurrPathIndex",
                    "Rot",
                    "Position",
                    "SummonHostID",
                    "BelongID"
                ]
                */
                // "Index"
                {
                    SyncBaseInfo syncBaseInfo = new();

                    syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Index);
                    syncBaseInfo.Uint32Value = (uint)bulletID;

                    propBaseSyncList.Prop.Add(syncBaseInfo);
                }

                // "Faction"
                {
                    SyncBaseInfo syncBaseInfo = new();

                    syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Faction);

                    // 子弹的 阵营 是 创建者 的 阵营
                    SyncBaseInfo createrFactionInfo = creater.GetSyncBaseInfo(syncBaseInfo.Index);
                    syncBaseInfo.Uint32Value = createrFactionInfo.Uint32Value;


                    propBaseSyncList.Prop.Add(syncBaseInfo);
                }

                // "PathPoses"
                {

                }
                // "CurrPathIndex"
                {

                }

                // "Rot"
                {
                    SyncBaseInfo syncBaseInfo = new();

                    syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Rot);
                    syncBaseInfo.Int32Value = toward;

                    propBaseSyncList.Prop.Add(syncBaseInfo);
                }

                // "Position"
                {
                    SyncBaseInfo syncBaseInfo = new();


                    syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Position);
                    syncBaseInfo.MsgValue = proVec3.ToByteString();

                    propBaseSyncList.Prop.Add(syncBaseInfo);
                }

                // 由谁创建的id ，比如 我创建一个召唤物, 召唤物创建一个子弹,那 SummonHostID 就是 召唤物的 id， BelongID 就是我的 id
                // "SummonHostID"
                {
                    SyncBaseInfo syncBaseInfo = new();

                    syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.SummonHostID);

                    // 子弹的 创建者 id 是 创建者的 id
                    syncBaseInfo.Uint64Value = creater.EntityID;

                    propBaseSyncList.Prop.Add(syncBaseInfo);
                }

                // "BelongID"
                {
                    SyncBaseInfo syncBaseInfo = new();

                    syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.BelongID);

                    // 子弹的 belongID 是 创建者 的 belongID
                    SyncBaseInfo createrBelongIDInfo = creater.GetSyncBaseInfo(syncBaseInfo.Index);
                    if (createrBelongIDInfo != null)
                    {
                        syncBaseInfo.Uint64Value = createrBelongIDInfo.Uint64Value;
                        propBaseSyncList.Prop.Add(syncBaseInfo);
                    }
                    else if (creater != null)
                    {
                        syncBaseInfo.Uint64Value = creater.EntityID;
                        propBaseSyncList.Prop.Add(syncBaseInfo);
                    }
                    else
                    {
                        // 凭空创建的 实体，没有 belongID 这个 属性
                    }

                }

                // 子弹配置 在 loading 已经预加载，此处应该为同步
                LocalDataManager.Instance.GetBulletJson(bulletID, (SkillEditor.BulletJson json) =>
                {
                    // "TruthSpeed"
                    {
                        SyncBaseInfo syncBaseInfo = new();

                        syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.TruthSpeed);

                        syncBaseInfo.Int32Value = json.config.MoveSpeed;

                        propBaseSyncList.Prop.Add(syncBaseInfo);
                    }
                });

            }

            LocalServerEntity bulletEntity = CreateEntity(E_EntityType.BulletEntity, propBaseSyncList);
            // SGF.Debuger.LogWarning($"{LocalServer.TagFlag} 创建子弹_[{bulletID}] 实体ID: [{bulletEntity.EntityID}] 成功");


            LocalBulletRuntime bulletRuntime = bulletEntity.CreateBulletRuntime(bulletID, bulletBlackBoard);

            // SGF.Debuger.LogWarning($"{LocalServer.TagFlag} 创建子弹_[{bulletID}] 运行时ID: [{bulletRuntime.RuntimeID}] 成功");

            bulletRuntime.OnEnter(bulletEntity.EntityID, creater.EntityID);
            // SGF.Debuger.LogWarning($"{LocalServer.TagFlag} 设置子弹_[{bulletID}] ownerID: [{bulletEntity.EntityID}] builderID: [{creater.EntityID}]");


        }

        public void CreateMonster(int monsterId)
        {
            // "Index",
            // "Position",
            // "PathPoses",
            // "CurrPathIndex",
            // "MonsterAIState",
            // "CreateTime",
            // "Rot",
            // "Faction",
            // "AIState",
            // "TargetId",
            // "TinyEntityFlag",
            // "SpawnInfluenceID",
            // "EntityLevel",
            // "Alias",
            // "Title",
            // "ControlID",
            // "ActMark"
            PropBaseSyncList propBaseSyncList = new();


            // "Index"
            {
                SyncBaseInfo syncBaseInfo = new();

                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Index);
                syncBaseInfo.Uint32Value = (uint)monsterId;

                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            // "Faction"
            {
                SyncBaseInfo syncBaseInfo = new();

                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Faction);
                syncBaseInfo.Uint32Value = 0;

                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            // "Position"
            {
                SyncBaseInfo syncBaseInfo = new();


                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Position);
                var pos = new ProtoMsg.Vector3(MainPlayer.Pos);
                pos.Z = pos.Z + 3;
                // 坐标 就先设置在玩家脚下
                syncBaseInfo.MsgValue = pos.ToByteString();

                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            // "Rot"
            {
                SyncBaseInfo syncBaseInfo = new();

                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Rot);
                syncBaseInfo.Int32Value = 0;

                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            // "TruthSpeed"
            {
                SyncBaseInfo syncBaseInfo = new();

                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.TruthSpeed);

                syncBaseInfo.Int32Value = 500;

                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            CreateEntity(E_EntityType.Monster, propBaseSyncList);
        }


        /// <summary>
        /// 执行 实体的移动(MoveMsg2)逻辑. 更新 对应实体的属性接口.
        /// note:
        ///     1.跟曲 沟通， 主角的属性同步 走的单独的 PropSyncList 消息通知;
        ///     2.其他人的 属性体同步,走 aoi 的 updateAOI 逻辑
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="moveMsgData"></param>
        public void OnEntityMove(ulong entityID, object moveMsgData)
        {
            if (entityID == MainPlayer.EntityID)
            {
                OnMianPlayerMove(moveMsgData);
            }
            else
            {
                if (localEntitys.TryGetValue(entityID, out var entity))
                {
                    OnOtherEntityMove(entity, moveMsgData);
                }
            }
        }

        /// <summary>
        /// 主角的移动, 此处 更新属性,同时 同步到客户端
        /// </summary>
        /// <param name="moveMsgData"></param>
        private void OnMianPlayerMove(object moveMsgData)
        {
            propSyncList1.Prop.Prop.Clear();
            MoveMsg2 moveMsg2 = (MoveMsg2)moveMsgData;

            UpdateMainPlayerPosAndRot(moveMsg2.Pos, moveMsg2.Rot);
        }

        private void UpdateMainPlayerPosAndRot(ProVec3 proVec3, int rot)
        {
            // "Rot"
            {
                SyncBaseInfo syncBaseInfo = new();

                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Rot);
                syncBaseInfo.Int32Value = rot;

                propSyncList1.Prop.Prop.Add(syncBaseInfo);
            }

            // "Position"
            {
                SyncBaseInfo syncBaseInfo = new();


                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Position);
                syncBaseInfo.MsgValue = proVec3.ToByteString();

                propSyncList1.Prop.Prop.Add(syncBaseInfo);
            }
            propSyncList1.EntityID = MainPlayer.EntityID;
            propSyncList1.EntityType = MainPlayer.EntityType;

            MainPlayer.UpdatePropList(propSyncList1);
        }

        private void OnOtherEntityMove(LocalServerEntity entity, object moveMsgData)
        {

        }
        public bool SetBulletTargetPos(ulong entityID, ulong runtimeID, UVec3 targetPos, int serverRot, UVec3[] path)
        {
            if (!HasServerEntity(entityID))
            {
                SGF.Debuger.LogError($" 子弹 SetBulletTargetPos 时 子弹实体[{entityID}] 已经销毁");
                return false;
            }

            localEntitys[entityID].SetBulletTargetPos(runtimeID, targetPos, serverRot, path);

            return true;
        }

        public bool OnTriggerEvent(ulong entityID, TriggerEvent triggerEvent, object triggerData)
        {
            if (!HasServerEntity(entityID))
            {
                return false;
            }
            localEntitys[entityID].OnTriggerEvent(triggerEvent, triggerData);

            return true;
        }

        public bool BreakCurRuntimeInBullet(ulong entityID, ulong runtimeID, EffectTypeBreakCurRuntimeInBullet effect)
        {
            if (!HasServerEntity(entityID))
            {
                return false;
            }
            return localEntitys[entityID].BreakCurRuntimeInBullet(runtimeID, effect);
        }

        public bool UpdatePosAndRot(ulong entityID, ProVec3 proVec3, int rot)
        {
            if (!HasServerEntity(entityID))
            {
                return false;
            }

            //如果是主句, 走主角 单独的 属性同步 消息
            if (entityID == MainPlayer.EntityID)
            {
                UpdateMainPlayerPosAndRot(proVec3, rot);
                return true;
            }

            return localEntitys[entityID].UpdatePosAndRot(proVec3, rot);
        }
    }
}
