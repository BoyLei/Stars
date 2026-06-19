using System.Collections;
using System.Collections.Generic;
using StarProject.Game.Entity;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// Bullet 的 所有阶段处理中间层
    /// </summary>
    public class BulletStageHandle : StageHandle
    {
        public override string TagFlag
        {
            get => $"[{OwnerEntityID}] [Bullet] RuntimeID[{RuntimeID}]";
        }

        public override void OnCreate()
        {
            base.OnCreate();
            Reset();
            playStageType = E_StageType.Bullet;
        }

        public override string FormatEffectKey(int effectID)
        {
            return $"{TagFlag} effectID[{effectID}]";
        }

        public override void OnActionExitStage(SkillStage skillStage, double exitStageTime, E_SkillStageExitType skillStageExitType)
        {
            base.OnActionExitStage(skillStage, exitStageTime, skillStageExitType);
            //  Bullet do nothing
        }

        public override bool IsJoySitckMoving()
        {
            // 服务器的子弹是非生命体, 子弹也不需要播放动作,所以此处子弹不需要 判断是否是摇杆移动 打断动画
            return false;
        }

        public override void OnActionStageStartCD(SkillStage skillStage)
        {
            base.OnActionStageStartCD(skillStage);
            //  Bullet do nothing
        }

        public override bool OnActionStageTrySetActiveMain(SkillStage skillStage, double enterStageTime, bool isActiveMain, bool force)
        {
            // Bullet 不设置活跃技能
            return true;
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}
