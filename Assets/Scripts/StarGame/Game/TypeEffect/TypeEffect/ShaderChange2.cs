///--------------------------------------------------------------------
/// 文件名   :   FreezeBuffEffect
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/01 15:20:53
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using StarProject.Game;
using StarProject.Game.Player;
using StarProjectDef;
using UnityEngine;
using SkillEditor;

namespace StarProject.Game.TypeEffect
{
    public class ShaderChangeEffect2 : BaseTypeEffect
    {
        /// <summary>
        /// 外发光效果
        /// </summary>


        public override void OnEnter(ulong owneruid, int buffid)
        {
            base.OnEnter(owneruid, buffid);
            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
            player?.EdgeLight(true, this);

            // SGF.Debuger.LogError($"OnEnter:: owneruid={owneruid} buffid={buffid} Time={Time.time}");
        }



        public override void OnExit()
        {
            var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            player?.EdgeLight(false, this);

            // SGF.Debuger.LogError($"OnExit::owneruid={ownerEntityID} buffid={BuffID} Time={Time.time}");
            base.OnExit();
        }
    }
}