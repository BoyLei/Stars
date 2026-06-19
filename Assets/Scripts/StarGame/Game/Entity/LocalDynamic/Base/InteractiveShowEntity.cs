using StarProject.Game.Entity.Factory;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Entity.LocalDynamic
{
    //交互表现
    public class InteractiveShowEntity : EntityLocalDynamic
    {
        public override void Create(string entityKey, E_LocalEntityType type, long id, string triggerKey, Vector3 pos, float scale)
        {
            base.Create(entityKey, type, id, triggerKey, pos, scale);
        }

        protected override void Reset()
        {
            base.Reset();
        }

        protected override void Release()
        {
            base.Release();
        }

        public override void EnterFrame(int frameIndex)
        {
            base.EnterFrame(frameIndex);

        }


    }
}