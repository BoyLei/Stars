using System.Collections;
using System.Collections.Generic;
using StarProject.Game.Player;
using UnityEngine;

namespace StarProject.Game.TypeEffect
{
    /// <summary>
    /// 摄像机 移动的效果
    /// </summary>
    public class CameraMoveTypeEffect : CameraBaseTypeEffect
    {
        public override void HandleCameraEffect(bool onEnter, EntityCtrlBase player)
        {
            base.HandleCameraEffect(onEnter, player);

            GlobalEvent.OnCameraMoveEvent.Invoke(player.Data.M_EntityID, EffectTypeSerialize.Global_CameraMove, onEnter);
        }
    }
}
