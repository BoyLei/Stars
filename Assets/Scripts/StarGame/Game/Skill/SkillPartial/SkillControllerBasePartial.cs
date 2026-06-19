using System.Collections;
using System.Collections.Generic;
using ProtoMsg;
using UnityEngine;

using StarProject.Service.LocalData;
using StarProject.Game.Data;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 技能控制器，用来控制和处理:
    ///     1: 主动技能
    ///     2：被动技能
    /// </summary>

    public partial class SkillController
    {
        #region runtimeID 生成逻辑
        /// <summary>
        /// 客户端的 runtimeID 生成记录
        /// </summary>
        /// <typeparam name="ulong"></typeparam>
        /// <returns></returns>
        private HashSet<ulong> clientRuntimeIDRecord = new HashSet<ulong>();

        /// <summary>
        /// 客户端 runtimeID的基础字段
        /// </summary>
        private ulong BASE_RUMTIME_ID = 200000;

        private ulong nextID = 0;

        public ulong GetNextID()
        {
            nextID = nextID <= BASE_RUMTIME_ID ? BASE_RUMTIME_ID : nextID;

            nextID++;

            if (nextID > BASE_RUMTIME_ID + 99999)
            {
                nextID -= 99999;
            }

            //按服务器逻辑翻译过来,如果 record全部存在,可能存在死循环的风险
            //但 runtimeID 不可能存在 99999 个, 所有风险不存在
            while (clientRuntimeIDRecord.Contains(nextID))
            {
                nextID++;
                if (nextID > BASE_RUMTIME_ID + 99999)
                {
                    nextID -= 99999;
                }
            }

            return nextID;
        }
        #endregion

        private LocalDataManager ldm;

        private Transform container;

        private SkillDispatcher dispatcher;

        private VitalSignData playerData;

        public VitalSignData PlayerData => playerData;

        private ulong entityId = 0;
        public ulong EntityId => entityId;

        public void Create(SkillDispatcher skillDispatcher, VitalSignData data, Transform parent)
        {
            dispatcher = skillDispatcher;
            playerData = data;
            entityId = data.M_EntityID;
            ldm = LocalDataManager.Instance;
            container = parent;

            // GlobalEvent.onNoRecordRunBlack.AddListener(onNoRecordRunBlack);
        }

        public SkillContainer GetSkillContainer(int skillId)
        {
            return dispatcher.SkillUnitController.GetSkillUnit(skillId);
        }

    }
}
