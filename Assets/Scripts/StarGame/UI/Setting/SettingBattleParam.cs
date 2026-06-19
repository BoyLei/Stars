using SGF.UI.Framework;
using Sirenix.OdinInspector;
using StarProject.Game;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;
using StarProject.Game.Skill;
using StarProject.Service.Battle;
using StarProject.Service.LocalData;
using StarProject.Service.SystemOpen;
using StarProject.Service.User;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.StarWorld
{
    public class SettingBattleParam : SettingBaseParam
    {
        private string LOG_TAG = "[SettingBattleParam]";

        [LabelText("技能Root")]
        public Transform SkillItemRoot;
        [LabelText("技能Item")]
        public GameObject SkillItem;

        private NPCEntityBase m_NPCEntityBase;
        private List<SettingSkillItem> SkillItemList = new();
        private bool isChanage = false;

        public Toggle PartnerToggle;

        public GameObject PartnerRoot;

        protected override void Awake()
        {
            base.Awake();

            // 读取战斗设置的缓存
            EntityCtrlBase mainPlayer = GameManager.Instance.M_MainPlayerCtrlBase;
            if (mainPlayer != null && mainPlayer.M_Curr != null)
            {
                m_NPCEntityBase = mainPlayer.M_Curr as NPCEntityBase;
            }

            PartnerToggle.onValueChanged.AddListener(OnPartnerToggle);

            GlobalEvent.OnSystemOpen.AddListener(OnSystemOpen);
        }

        private void OnEnable()
        {
            RefreshPartnerView();
        }

        public override void Reset()
        {
            base.Reset();
            ClearSkillItem();
            isChanage = false;
        }

        public override void Init()
        {
            base.Init();
            if (m_NPCEntityBase == null)
            {
                EntityCtrlBase mainPlayer = GameManager.Instance.M_MainPlayerCtrlBase;
                if (mainPlayer != null && mainPlayer.M_Curr != null)
                {
                    m_NPCEntityBase = mainPlayer.M_Curr as NPCEntityBase;
                }
            }
            InitSkillItemData();

            InitPartnerValue();
        }

        private void OnToggleSkillItem(bool isOn, int skillPos)
        {
            isChanage = true;
            BattleManager.Instance.ForbidSkillPos(skillPos, !isOn);
        }


        #region 技能按钮

        private void InitSkillItemData()
        {
            if (m_NPCEntityBase != null)
            {
                int index = 0;
                foreach (KeyValuePair<int, SkillContainer> item in m_NPCEntityBase.GetSkillContainerDic())
                {
                    int posID = item.Key;
                    SkillContainer skillContainer = item.Value;
                    if (!skillContainer.IsPadding)
                    {
                        SGF.Debuger.LogWarning($"{LOG_TAG} Init skillBtn.SkillContainer.isPadding=false ");
                        continue;
                    }

                    SkillPosSetDataCell skillPosSetDataCell = LocalDataManager.Instance.GetSkillPosSetDataCell(posID);
                    if (skillPosSetDataCell == null)
                    {
                        SGF.Debuger.LogWarning($"{LOG_TAG} Init skillBtn.SkillContainer.skillPosSetDataCell=null ");
                        continue;
                    }

                    // 除冲刺和普攻
                    if (skillPosSetDataCell.GetSkillType() == 1 || skillPosSetDataCell.GetSkillType() == 5)
                    {
                        continue;
                    }

                    SkillInfo firstSkillInfo = skillContainer.FirstSkillInfo;
                    if (firstSkillInfo == null)
                    {
                        continue;
                    }

                    SettingSkillItem skillItem = GetSkillItem(index);
                    skillItem.transform.name = $"{posID}";
                    bool isForbidde = BattleManager.Instance.GetIsForbiddeSkillPos(posID);
                    skillItem.SetSettingSkillItem(firstSkillInfo, posID, skillPosSetDataCell, !isForbidde, OnToggleSkillItem);
                    index++;
                }
            }
        }

        private SettingSkillItem GetSkillItem(int index)
        {
            SettingSkillItem item = GetIdleSkillItem(index);
            if (item == null)
            {
                item = AddSkillItem();
                SkillItemList.Add(item);
            }
            return item;
        }

        private SettingSkillItem GetIdleSkillItem(int index)
        {
            if (SkillItemList.Count > index)
            {
                return SkillItemList[index];
            }

            return null;
        }

        private SettingSkillItem AddSkillItem()
        {
            var gob = GameObject.Instantiate<GameObject>(SkillItem);
            gob.transform.SetParent(SkillItemRoot);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.SetLocalScale(Vector3.one);
            SettingSkillItem sp = gob.GetComponent<SettingSkillItem>();
            return sp;
        }

        private void ClearSkillItem()
        {
            for (int i = 0; i < SkillItemList.Count; i++)
            {
                var child = SkillItemList[i];
                if (child != null)
                {
                    child.Reset();
                    child.gameObject.SetActive(false);
                }
            }
        }

        #endregion

        #region 伙伴自动战斗

        private void InitPartnerValue()
        {
            bool isOn = BattleManager.Instance.GetPartnerAutoBattleCache();

            // 先刷新一次，狗曲的 init 不确定是不是在 awake 之前调用
            OnPartnerToggle(isOn);

            PartnerToggle.isOn = isOn;
        }

        /// <summary>
        /// 伙伴自动战斗开放时候显示 
        /// </summary>
        /// <param name="systemOpenType"></param>
        /// <param name="isOpen"></param> 
        private void OnSystemOpen(SystemOpenType systemOpenType, bool isOpen)
        {
            if (systemOpenType != SystemOpenType.AutoFight)
            {
                return;
            }
            PartnerRoot.SetActive(isOpen);
        }

        private void RefreshPartnerView()
        {
            bool isOpen = SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.AutoFight);

            PartnerRoot.SetActive(isOpen);
        }

        private void OnPartnerToggle(bool isOn)
        {
            SetPartnerAutoBattleCache(isOn);
            // 切换 或半自动战斗
            BattleManager.Instance.SwitchPartnerAutoBattle(isOn);
        }
        #endregion
        #region 本地缓存

        public override void SaveLocalData()
        {
            base.SaveLocalData();

            if (isChanage == false)
            {
                return;
            }

            string battleLocalKey = $"{GameConfig.SETTING_BATTLE}_{UserManager.Instance.MainUserData.playerRoleId}";
            SaveManager.Instance.Save<List<int>>(battleLocalKey, BattleManager.Instance.GetForbidSkillPoss(), "Setting");
        }


        private void SetPartnerAutoBattleCache(bool isOn)
        {
            LocalCacheManager.Instance.SetValueType(GameConfig.SETTING_BATTLE_PARTNER, isOn);
        }

        #endregion

    }
}
