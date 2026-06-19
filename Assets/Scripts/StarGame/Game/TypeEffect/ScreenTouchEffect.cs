using System.Collections;
using System.Collections.Generic;
using StarProject.Game.Player;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.TypeEffect
{
    /// <summary>
    /// 屏幕 点击的效果 , 目前 此处 作为 屏幕点击基类的效果
    /// </summary>
    public class ScreenTouchEffect : BaseTypeEffect
    {

        public override void OnEnter(ulong owneruid, int buffid)
        {
            base.OnEnter(owneruid, buffid);
            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
            if (player != null && player.Data.isMainPlayer)
            {
                HandleScreenTouchEffect(true, player);
            }

        }

        public void HandleScreenTouchEffect(bool onEnter, EntityCtrlBase player)
        {
            string regKey = GameConfig.TOUCH_EVENT;
            // 屏幕点击 效果 gl 的需求是 后面的 屏幕效果 替换前面的屏幕效果,  但是 buff 不一定替换.
            // 对于 已经被替换过的 屏幕效果, 并不会 因为 后续 buff 效果的 结束 而 恢复.
            // 所以 此处 所有的 buff 屏幕点击效果 采用相同的 tag
            string tag = this.GetHashCode().ToString();

            player.Data.UnRegChangeListenersByRegKey(regKey);

            if (onEnter)
            {
                player.Data.RegChangeListener(regKey, (object v) =>
                {
                    OnTouchScreen(v, player);
                }, tag);
            }
            else
            {
                player.Data.UnRegChangeListeners(regKey, tag);
            }

        }

        public virtual void OnTouchScreen(object v, EntityCtrlBase player)
        {
            // 屏幕点击的逻辑
            SGF.Debuger.Log($"屏幕 点击了 : {v.ToString()}");
        }


        public override void OnExit()
        {
            var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            if (player != null && player.Data.isMainPlayer)
            {
                HandleScreenTouchEffect(false, player);
            }

            base.OnExit();
        }

    }
}
