using StarProject.Game.Entity;
using UnityEngine;

namespace StarProject.Game.Player.Component
{
    public abstract class PlayerComponent
    {
        public abstract MonoBehaviour GetView();
        public PlayerComponent(EntityCtrlBase entity)
        {
        }

        public PlayerComponent(EntityLocalDynamic entity)
        {
        }

        public abstract void Release();

        public abstract void EnterFrame(int frameIndex);

        public abstract void SetFlashHide(bool isHide);

        public abstract void InitRefreshState();
    }
}
