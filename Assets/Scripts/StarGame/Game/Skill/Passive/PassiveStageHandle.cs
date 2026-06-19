using System.Collections;
using System.Collections.Generic;
using StarProject.Game.Data;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 被动的 StageHandle
    /// </summary>
    public class PassiveStageHandle : StageHandle
    {
        public override string TagFlag => $"[{OwnerEntityID}] [Passive] RuntimeID[{RuntimeID}]";

        private VitalSignData _playerData;

        public override void OnCreate()
        {
            base.OnCreate();
            Reset();
            playStageType = E_StageType.Passive;
        }

        /// <summary>
        /// 对于服务器来说:
        ///     被动可以挂在所有有生命体 实体身上.
        ///     而对于子弹来说,不一定会挂上被动(服务器子弹是非生命体,但是客户端子弹 是用召唤物做的,所以不太一样)
        /// </summary>
        /// <param name="playerData"></param>
        public void SetVitalSignData(VitalSignData playerData)
        {
            _playerData = playerData;
        }

        public override string FormatEffectKey(int effectID)
        {
            return $"{TagFlag} effectID[{effectID}]";
        }

        public override void OnActionExitStage(SkillStage skillStage, double exitStageTime, E_SkillStageExitType skillStageExitType)
        {
            base.OnActionExitStage(skillStage, exitStageTime, skillStageExitType);
            //  Passive do nothing
        }

        public override bool IsJoySitckMoving()
        {

            // 被动如果有动作,那跟buff一样,移动能够打断被动的 动作
            if (_playerData == null)
            {
                return false;
            }
            return _playerData.M_Is_Moveing;
        }

        public override void OnActionStageStartCD(SkillStage skillStage)
        {
            base.OnActionStageStartCD(skillStage);
            //  Passive do nothing
        }

        public override bool OnActionStageTrySetActiveMain(SkillStage skillStage, double enterStageTime, bool isActiveMain, bool force)
        {
            // Passive 不设置活跃技能,直接返回执行的 结果 为true.
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            _playerData = null;
        }
    }
}
