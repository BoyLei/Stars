using System.Collections;
using System.Collections.Generic;
using StarProject.Game.Player;
using StarProjectDef;
using UnityEngine;
namespace StarProject.Game.TypeEffect
{
    public class CameraBaseTypeEffect : BaseTypeEffect
    {

        public override void OnEnter(ulong owneruid, int buffid)
        {
            base.OnEnter(owneruid, buffid);
            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
            if (player != null && player.Data.isMainPlayer)
            {
                HandleCameraEffect(true, player);
            }

        }

        public virtual void HandleCameraEffect(bool onEnter, EntityCtrlBase player)
        {
            SGF.Debuger.LogError($"准备处理 屏幕 效果 onEnter: {onEnter}");
        }




        public override void OnExit()
        {
            var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            if (player != null && player.Data.isMainPlayer)
            {
                HandleCameraEffect(false, player);
            }

            base.OnExit();
        }
    }
}
