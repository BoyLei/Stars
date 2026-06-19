using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;
using SGF.Network;
using Sirenix.Utilities;
using SkillEditor;
using StarProject;
using StarProject.Game.Skill;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;
using UVec3 = UnityEngine.Vector3;
using ProVec3 = ProtoMsg.Vector3;


namespace EditorModeTest
{
    /// <summary>
    /// 本地服创建的 实体. 像子弹 召唤物等, 由本地服创建并统一驱动
    /// </summary>
    public class LocalServerEntity
    {
        public ulong EntityID;
        public string EntityType;

        public E_EntityType E_EntityType;

        public PropBaseSyncList PropList;

        public ProtoMsg.Vector3 Pos;

        /// <summary>
        /// 发生变化的 protoList
        /// </summary>
        private PropBaseSyncList changePropList;

        /// <summary>
        /// 每个实体身上都有自己的运行时Manager. 
        /// </summary>
        public LocalRuntimeManager localRuntimeManager;

        public Action<ulong, string, PropBaseSyncList> ActionOnChangeProto;

        public LocalServerEntity(E_EntityType e_EntityType, ulong entityID, PropBaseSyncList propList)
        {
            localRuntimeManager = new LocalRuntimeManager();

            EntityID = entityID;
            E_EntityType = e_EntityType;
            EntityType = Enum.GetName(typeof(E_EntityType), e_EntityType);
            PropList = propList;

            changePropList = new PropBaseSyncList();
        }

        private SyncBaseInfo GetSyncBaseInfoFromPropList(uint propIndex, Google.Protobuf.Collections.RepeatedField<SyncBaseInfo> props)
        {
            foreach (var item in props)
            {
                if (item.Index == propIndex)
                {
                    return item;
                }
            }
            return null;
        }

        public SyncBaseInfo GetSyncBaseInfo(uint propIndex)
        {
            return GetSyncBaseInfoFromPropList(propIndex, PropList.Prop);
        }

        SyncBaseInfo tempSyncBaseInfo;
        public void UpdatePropList(PropSyncList propSyncList)
        {
            propSyncList.Prop.Prop.ForEach((SyncBaseInfo syncBaseInfo) =>
            {
                UpdateSyncBaseInfo(syncBaseInfo);
            });
        }

        /// <summary>
        /// 更新 单个的 属性信息
        /// </summary>
        /// <param name="syncBaseInfo"></param>
        public void UpdateSyncBaseInfo(SyncBaseInfo syncBaseInfo)
        {
            tempSyncBaseInfo = GetSyncBaseInfo(syncBaseInfo.Index);
            if (tempSyncBaseInfo == null)
            {
                PropList.Prop.Add(syncBaseInfo);
            }
            else
            {
                // 将传过来的属性 赋值
                tempSyncBaseInfo.MergeFrom(syncBaseInfo);
            }

            // 如果是 坐标的属性同步:
            if (syncBaseInfo.Index == LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Position))
            {
                ByteString vector3Msg = (ByteString)syncBaseInfo.MsgValue;
                byte[] msgData = vector3Msg.ToByteArray();
                IMessage pbMessage = ProtoUtils.Deserialize((int)MsgIDEnum.Vector3ID, msgData);
                Pos = (ProtoMsg.Vector3)pbMessage;
            }
        }

        /// <summary>
        /// 更新 PropBaseSyncList
        /// </summary>
        /// <param name="propBaseSyncList"></param>
        public void UpdateBasePropList(PropBaseSyncList propBaseSyncList)
        {
            propBaseSyncList.Prop.ForEach((syncBaseInfo) =>
            {
                UpdateSyncBaseInfo(syncBaseInfo);
            });
        }

        public void OnTick()
        {
            localRuntimeManager.OnTick();
        }

        /// <summary>
        /// 创建 子弹运行时
        /// </summary>
        public LocalBulletRuntime CreateBulletRuntime(int bulletID, BaseBlackBoard bulletBlackBoard)
        {
            return localRuntimeManager.CreateBulletRuntime(bulletID, bulletBlackBoard);
        }

        /// <summary>
        /// 实体 设置子弹的 目标节点.
        /// 对于本地服来说，此处 需要 将 客户端线传过来的 路点信息作为属性 存下来，同时 通过 UpdateAOI 的方式, 推送给指定实体
        /// </summary>
        /// <param name="stageUID"></param>
        /// <param name="effectParam"></param>
        /// <param name="serverRot"> 转换为 给 本地服推送的 服务器坐标 </param> 
        /// <param name="blackBoard"></param>
        public void SetBulletTargetPos(ulong runtimeID, UVec3 targetPos, int serverRot, UVec3[] path)
        {
            // 运行时的 目标坐标 是否设置进入黑板 后续再说


            changePropList.Prop.Clear();
            // 先设置 服务器朝向的属性

            // "Rot"
            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();

                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Rot);
                syncBaseInfo.Int32Value = serverRot;

                changePropList.Prop.Add(syncBaseInfo);
            }

            // "PathPoses"
            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();

                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.PathPoses);

                ArrayVector3 arrayVector3 = new ArrayVector3();
                path.ForEach((point) =>
                {
                    arrayVector3.ArrayVector3_.Add(ProtoUtils.ConvertUnityVec3ToProtoVec3(point));
                });

                syncBaseInfo.MsgValue = arrayVector3.ToByteString();

                changePropList.Prop.Add(syncBaseInfo);

            }

            // 路点数据 为0，表明路点没了， 那就发送 -1
            int pathIdx = path.Length > 0 ? 0 : -1;
            // "CurrPathIndex"
            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();

                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.CurrPathIndex);

                // 路点数据 为0，表明路点没了， 那就发送 -1
                syncBaseInfo.Int32Value = pathIdx;

                changePropList.Prop.Add(syncBaseInfo);

            }

            // 
            // {
            //     SyncBaseInfo syncBaseInfo = new SyncBaseInfo();

            //     syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.TruthSpeed);

            //     // syncBaseInfo.Int32Value = 500;

            //     changePropList.Prop.Add(syncBaseInfo);

            // }


            UpdateBasePropList(changePropList);

            localRuntimeManager.SetRuntimeTargetPos(runtimeID, targetPos);

            PostChangePropList();

            // 如果 pathIdx =-1 , 说明设置的目标坐标就是玩家当前坐标, 此时,就认为
            if (pathIdx == -1)
            {
                GlobalEvent.OnClientReqLocalServerEvent.Invoke(ClientEventReq.TriggerEvent, new object[] { EntityID, TriggerEvent.PathMoveEnd, targetPos });
            }
        }

        /// <summary>
        /// 增加 变化的属性, 防止 相同属性 多次变化
        /// </summary>
        /// <param name="syncBaseInfo"></param>
        public void AddChangeProp(SyncBaseInfo syncBaseInfo)
        {
            tempSyncBaseInfo = GetSyncBaseInfoFromPropList(syncBaseInfo.Index, changePropList.Prop);
            if (tempSyncBaseInfo == null)
            {
                changePropList.Prop.Add(syncBaseInfo);
            }
            else
            {
                // 将传过来的属性 赋值
                tempSyncBaseInfo.MergeFrom(syncBaseInfo);
            }
        }

        /// <summary>
        /// 通知 属性发生了变化
        /// </summary>
        public void PostChangePropList()
        {
            ActionOnChangeProto?.Invoke(EntityID, EntityType, changePropList);
        }

        public bool OnTriggerEvent(TriggerEvent triggerEvent, object triggerData)
        {
            localRuntimeManager.TriggerEvent(triggerEvent, triggerData);

            return true;
        }

        public bool BreakCurRuntimeInBullet(ulong runtimeID, EffectTypeBreakCurRuntimeInBullet effect)
        {
            localRuntimeManager.GetRuntime(runtimeID)?.BreakCurRuntimeInBullet(effect);

            return true;
        }

        public bool UpdatePosAndRot(ProVec3 proVec3, int rot)
        {

            changePropList.Prop.Clear();
            // 先设置 服务器朝向的属性

            // "Rot"
            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();

                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Rot);
                syncBaseInfo.Int32Value = rot;

                changePropList.Prop.Add(syncBaseInfo);
            }

            // "Position"
            {
                SyncBaseInfo syncBaseInfo = new();


                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Position);
                var pos = proVec3;


                changePropList.Prop.Add(syncBaseInfo);
            }

            UpdateBasePropList(changePropList);

            PostChangePropList();

            return true;
        }
    }
}
