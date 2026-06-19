///--------------------------------------------------------------------
/// 文件名   :   InterState.cs
/// 内  容   :   交互状态
/// 说  明   :  
/// 创建日期 :   2023/05/04 16:10:23
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProject.Game;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Player;
using StarProjectDef;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClientNpc
{
    public class InterState : BaseState
    {
        public override StateEnum GetStateEnum()
        {
            return StateEnum.Inter;
        }

        public InterState(ClientNpc npc) : base(npc) { }

        public override void OnEnter()
        {
            if (mClientNpc.CtrlGroup != null)
            {
                I_AnimParam animParam = mClientNpc.CtrlGroup.M_Curr.GetAnimParamByState(E_ULayerSubState.Idle);
                mClientNpc.CtrlGroup.M_Curr.ChangeState(GameKeyCommand.BattleIdle, animParam, false);
                mClientNpc.SetMoveSpeed(0);
            }
        }

        public override void OnUpdate()
        {
        }

        public override void OnExit()
        {
        }
    }
}