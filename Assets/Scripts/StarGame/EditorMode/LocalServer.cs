using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using StarProject.Service.LocalData;
using ProtoMsg;
using System;
using StarProject;


using SGF.Unity;

using Google.Protobuf;

namespace EditorModeTest
{
    public partial class LocalServer
    {
        public static string TagFlag = "LocalServer";
        /// <summary>
        /// 服务器 登录的信息
        /// </summary>
        public PickSrvAck pickSrvAck;
        /// <summary>
        /// 地图预加载的 信息
        /// </summary>
        public MapPreloadNotice mapPreloadNotice;

        /// <summary>
        /// 进入场景的通知
        /// </summary>
        public EnterSpaceNtf enterSpaceNtf;

        /// <summary>
        /// 主角全同步的 信息
        /// </summary>
        private UserMainDataNotify userMainDataNotify;

        public AllSkillNotice CurAllSkillNotice;

        public LocalEntityManager localEntityManager;

        public LocalServerIDManager localServerIDManager;

        public LocalServerMsgQueue localServerMsgQueue;

        public LocalServer()
        {
            RegReqEventListener();
            InitPickSrvAck();
            InitMapPreloadNotice();
            InitEnterSpaceNtf();

            localEntityManager = new LocalEntityManager();
            localServerIDManager = new LocalServerIDManager();
            localServerMsgQueue = new();

            MonoHelper.AddFixedUpdateListener(OnTick, MonoHelper.E_ModuleType.CommonService);
        }

        public void Dispose()
        {
            MonoHelper.RemoveFixedUpdateListener(OnTick, MonoHelper.E_ModuleType.CommonService);
        }


        public void OnTick()
        {
            localEntityManager.OnTick();

            localServerMsgQueue.OnTick();
        }

        public void SendMsg(Action<object, object> msgAc, IMessage data, MsgType msgType, object otherData = null)
        {
            localServerMsgQueue.SendMsg(msgAc, data, msgType, otherData);
        }

        /// <summary>
        /// 发送 消息时 如果时rpc 消息的封装接口
        /// </summary>
        /// <param name="msgData"></param>
        /// <param name="entityID"></param>
        /// <returns></returns>
        public static object RPCMsgPacker(object msgData, ulong entityID)
        {
            return new KeyValuePair<ulong, object>(entityID, msgData);
        }

        public void RegReqEventListener()
        {
            GlobalEvent.OnClientReqLocalServerEvent.AddListener(OnClientReqLocalServerEvent);
        }

        public void InitPickSrvAck()
        {
            pickSrvAck = new PickSrvAck();
            pickSrvAck.UID = 1;
            pickSrvAck.Result = 1;
            pickSrvAck.PlayerData = new List<PlayerLoginData>();

            var roleCreateData = LocalDataManager.Instance.M_CharacterCreateData.StaticCharacterCreateDatas;
            foreach (var item in roleCreateData)
            {
                var jobID = item.Value.GetJobID();
                var roleData = new PlayerLoginData();
                roleData.PID = (ulong)jobID;
                roleData.NickName = jobID.ToString();
                roleData.ModelID = 1;
                roleData.JobID = jobID;
                roleData.Level = 1;
                pickSrvAck.PlayerData.Add(roleData);
            }
        }

        public void InitMapPreloadNotice()
        {
            mapPreloadNotice = new MapPreloadNotice();
            mapPreloadNotice.MapID = 4;
            mapPreloadNotice.SpType = SpaceType.SpaceScene;
        }

        public UserMainDataNotify GetUserMainDataNotify(ulong entityID, uint jobID)
        {
            userMainDataNotify = new UserMainDataNotify();
            ProtoMsg.PropBaseSyncList propBaseSyncList = new();
            // 临时的 entityID, 测试的时候 需要根据 选择的 角色去重新设置
            userMainDataNotify.EntityID = entityID;
            // 职业
            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Job);
                syncBaseInfo.Uint32Value = jobID;
                propBaseSyncList.Prop.Add(syncBaseInfo);
            }
            // 等级
            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.PlayerLevel);
                syncBaseInfo.Int64Value = 1;
                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.TruthSpeed);
                syncBaseInfo.Int64Value = 500;
                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Faction);
                syncBaseInfo.Uint32Value = 1;
                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Name);
                syncBaseInfo.StringValue = jobID.ToString();
                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.TruthSpectral1);
                syncBaseInfo.Int64Value = 6;
                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.TruthSpectral2);
                syncBaseInfo.Int64Value = 6;
                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.TruthSpectral3);
                syncBaseInfo.Int64Value = 6;
                propBaseSyncList.Prop.Add(syncBaseInfo);


            }
            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.curSpectral1);
                syncBaseInfo.Int64Value = 6;
                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.curSpectral2);
                syncBaseInfo.Int64Value = 6;
                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            {
                SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.curSpectral3);
                syncBaseInfo.Int64Value = 6;
                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            // "Position"
            {
                SyncBaseInfo syncBaseInfo = new();


                syncBaseInfo.Index = LocalDataManager.Instance.GetAttrPropIdxByName(AOIAttrDefine.Position);
                var pos = new ProtoMsg.Vector3();
                pos.X = 10;
                pos.Y = 0;
                pos.Z = 10;

                syncBaseInfo.MsgValue = pos.ToByteString();

                propBaseSyncList.Prop.Add(syncBaseInfo);
            }

            userMainDataNotify.Prop = propBaseSyncList;

            return userMainDataNotify;
        }





        public void InitEnterSpaceNtf()
        {
            enterSpaceNtf = new EnterSpaceNtf();
            enterSpaceNtf.SpaceID = 4;
            enterSpaceNtf.MapID = 4;
            enterSpaceNtf.SpType = SpaceType.SpaceScene;

            enterSpaceNtf.ServerID = 1;

            enterSpaceNtf.StartPos = new ProtoMsg.Vector3();
            enterSpaceNtf.StartPos.X = 10;
            enterSpaceNtf.StartPos.Y = 0;
            enterSpaceNtf.StartPos.Z = 10;

            enterSpaceNtf.StartRot = 0;

        }

        public void RefreshJobID()
        {

        }

        public AllSkillNotice GetAllSkillNotice(int jobID)
        {
            var jobData = LocalDataManager.Instance.GetJobDataCell(jobID);

            return FormateSkillNotice(jobData.GetAttackJobSkill(), jobData.BornJobSkills, jobData.GetUltimateJobSkills(), jobData.GetDashJobSkills());

        }

        public AllSkillNotice FormateSkillNotice(int attackJobSkill, List<int> activeSkills, int ultimateSkill, int dashJobSkill)
        {
            var allSkillNotice = new AllSkillNotice();
            allSkillNotice.CaseID = 1;

            Func<int, bool> InitSingleSkillInfo = (int skillID) =>
            {
                var skills = allSkillNotice.Skilllist;
                for (int i = 0; i < skills.Count; i++)
                {
                    if (skills[i].SkillDBID == skillID)
                    {
                        SGF.Debuger.LogError($"拥有重复的 技能ID: {skillID} ");
                        return false;
                    }
                }

                SingleSkillInfo singleSkillInfo = new SingleSkillInfo();
                singleSkillInfo.SkillDBID = skillID;
                singleSkillInfo.SkillLevel = 1;
                var listTalents = LocalDataManager.Instance.GetJobSkillId2TalentDataCell(skillID);
                if (listTalents != null && listTalents.Count > 0)
                {
                    singleSkillInfo.TalentID = listTalents[0].GetID();
                }
                else
                {
                    singleSkillInfo.TalentID = 0;
                }

                allSkillNotice.Skilllist.Add(singleSkillInfo);
                return true;
            };

            Action<int, int> InitSkillPos = (int skillID, int posID) =>
            {
                SkillPos skillPos = new SkillPos();
                skillPos.PosID = posID;
                skillPos.JobSkillID = skillID;
                skillPos.Islocked = false;
                allSkillNotice.Skillposlist.Add(skillPos);
            };

            // 职业普工技能
            {
                var skillID = attackJobSkill;
                InitSingleSkillInfo(skillID);
                InitSkillPos(skillID, 2);
            }


            // 初始职业技能
            {
                int idx = 2;
                bool result = false;
                activeSkills.ForEach((int skillID) =>
                {
                    result = InitSingleSkillInfo(skillID);
                    if (idx < 6 && result)
                    {
                        // 3/4/5/6
                        InitSkillPos(skillID, ++idx);
                    }
                });
            }

            // 大招， 没有大招的时候, id 设置为0
            {
                int skillID = ultimateSkill;
                if (skillID <= 0)
                {
                    skillID = 0;
                }
                InitSingleSkillInfo(skillID);
                InitSkillPos(skillID, 7);
            }

            // 职业冲刺技能
            {
                int skillID = dashJobSkill;
                InitSingleSkillInfo(skillID);
                InitSkillPos(skillID, 1);
            }

            return allSkillNotice;
        }

        /// <summary>
        /// 切换到 指定的 怪物, 切换 逻辑 分两块:
        /// 1. 切换模型 model;
        /// 2. 刷新 怪物技能;
        /// 3. 考虑要不要显示 所有技能
        /// </summary>
        /// <param name="monsterId"></param>
        /// <returns></returns>
        public bool SwitchMonster(long monsterId)
        {
            MonsterDataCell cfg = LocalDataManager.Instance.GetMonsterDataCell(monsterId);
            if (cfg == null)
            {
                return false;
            }
            AvatarID = cfg.GetAvatarID();

            var skills = cfg.ActiveSkills;

            // 刷新 当前的技能
            CurAllSkillNotice = FormateSkillNotice(0, skills, 0, 0);
            EditorMode.Instance.localServer.SendMsg((object msgData, object otherData) =>
            {
                // 通知外面 切换了 怪物
                GlobalEvent.OnLocalServerEvent?.Invoke(LocalServerEventRsp.SwitchMonster, null);
            }, null, MsgType.Post);

            return true;
        }

        public int AvatarID = 0;

        public LocalServerEntity GetServerEntity(ulong entityID)
        {
            return localEntityManager.GetServerEntity(entityID);
        }

    }
}
