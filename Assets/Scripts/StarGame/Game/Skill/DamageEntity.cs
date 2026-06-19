using SGF.Unity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Player;
using StarProject.Service.Battle;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Entity.WithOutLife.InfoEntity
{
    /// <summary>
    /// 伤害信息面板：有自己的实体id，有DG动画，有生命周期管理（自动已经有了）
    /// 属性：暴击，普通
    /// 文字
    /// TxeMeshPro-中文图集
    /// 有Queue池管理，显示延迟
    /// </summary>
    public class DamageEntity : EntityRemoteStatic
    {
        private string TagFlag = "[DamageEntity]";

        private const string damagePath = "Effects/Damage/DamageViewNew";

        public DamageData DamageData;

        public ProtoMsg.HurtData hurtData;
        public List<string> addEffectFly;
        public int damageTextId;
        public string damageText;
        public EntityCtrlBase attackerCtrlBase;// 攻击者实体
        public EntityCtrlBase buildCtrlBase;// 施法者实体
        public EntityCtrlBase attackedCtrlBase;// 被攻击者实体
        public E_StageType StageType = E_StageType.None;

        private DamageViewNew damageViewNew;

        private ulong Index = 0;
        private string key = "";

        public void InitIndex(ulong index)
        {
            Index = index;
            key = $"{TagFlag}_{Index}";
        }

        public void Create(ProtoMsg.HurtData _hurtData, List<string> _addEffectFly, EntityCtrlBase _attackedCtrlBase, EntityCtrlBase _attackerCtrlBase, EntityCtrlBase _buildCtrlBase, int _damageTextId, E_StageType e_StageType)
        {
            hurtData = _hurtData;
            addEffectFly = _addEffectFly;
            attackedCtrlBase = _attackedCtrlBase;
            attackerCtrlBase = _attackerCtrlBase;
            buildCtrlBase = _buildCtrlBase;
            damageTextId = _damageTextId;
            StageType = e_StageType;

            CreateGob();
        }

        public void Create(DamageData damageData, List<string> _addEffectFly, EntityCtrlBase _attackedCtrlBase, EntityCtrlBase _attackerCtrlBase, EntityCtrlBase _buildCtrlBase, int _damageTextId, E_StageType e_StageType)
        {
            DamageData = damageData;
            addEffectFly = _addEffectFly;
            attackedCtrlBase = _attackedCtrlBase;
            attackerCtrlBase = _attackerCtrlBase;
            buildCtrlBase = _buildCtrlBase;
            damageTextId = _damageTextId;
            StageType = e_StageType;

            CreateGob();
        }

        public void Create(string _damageText, int _damageTextId, EntityCtrlBase _attackedCtrlBase, EntityCtrlBase _attackerCtrlBase, EntityCtrlBase _buildCtrlBase)
        {
            attackedCtrlBase = _attackedCtrlBase;
            attackerCtrlBase = _attackerCtrlBase;
            buildCtrlBase = _buildCtrlBase;
            damageTextId = _damageTextId;
            damageText = _damageText;

            CreateGob();
            //ViewFactory.CreateViewAddressables(damagePath, resDefaultPath, this, UIRoot.DamageUIRoot.transform, "", true, StarProjectDef.E_AssetType.Effects);
        }

        private void CreateGob()
        {
            StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(damagePath, (go) =>
            {
                if (go != null)
                {
                    damageViewNew = go.GetComponent<DamageViewNew>();
                    damageViewNew.GetComponent<CanvasGroup>().alpha = 1;
                    damageViewNew.BindDamageEntity(this);
                    DelayInvoker.DelayInvoke(key, 5, DelayShowModel, new object[] { });
                }
            });
        }

        private void DelayShowModel(object[] args)
        {
            OnRemove();
        }

        public void OnRemove()
        {
            EntityFactory.ReleaseEntity(this);
        }

        protected override void Release()
        {
            base.Release();
            hurtData = null;
            DamageData = null;
            addEffectFly = null;
            attackedCtrlBase = null;
            attackerCtrlBase = null;
            buildCtrlBase = null;
            damageTextId = 0;
            damageText = string.Empty;
            StageType = E_StageType.None;
            if (DelayInvoker.ContainInvoke(key))
            {
                DelayInvoker.CancelInvoke(key);
            }
            Index = 0;
            if (damageViewNew != null)
            {
                //SGF.Debuger.Log($"伤害飘字 实体层 销毁 222222222222222 name={damageViewNew.transform.name}");
                damageViewNew.Reset();
                StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(damagePath, damageViewNew.gameObject,GameObjectPoolType.CanvasGroupType);
            }
            damageViewNew = null;
        }

    }
}
