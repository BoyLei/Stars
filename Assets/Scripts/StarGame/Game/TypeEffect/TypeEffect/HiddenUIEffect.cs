using System.Collections;
using System.Collections.Generic;
using SkillEditor;
using StarProject.Game.Player;
using StarProjectDef;
using UnityEngine;
using StarProject.Service.LocalData;
using StarProject.Game.Data;

namespace StarProject.Game.TypeEffect
{
    public class HiddenUIEffect : BaseTypeEffect
    {

        public override void OnEnter(ulong owneruid, int buffid)
        {
            base.OnEnter(owneruid, buffid);
            EntityCtrlBase player = GameManager.Instance.GetEntityCtr(owneruid);
            if (player != null && player.Data.isMainPlayer)
            {
                // List<ChangeAnim> changeAnims = BuffInfo.Cfg.ChangeAnims;
                // player.RegisterChangeAnim(RuntimeID.ToString(), changeAnims);
                HandleHiddenUIEffect(true, player);
            }

        }

        private void HandleHiddenUIEffect(bool onEnter, EntityCtrlBase player)
        {
            List<UILabel> hideLabels = EffectTypeSerialize.BUFF_HideUI.HideLabels;

            List<int> uiLabels = hideLabels.ConvertAll((UILabel uiLabel) => (int)uiLabel);

            // 找到 uiTags 标签 对应的 所有 相关的 ui 类型枚举Set
            HashSet<int> uiConfigs = new HashSet<int>();

            CompOperator compOperator = EffectTypeSerialize.BUFF_HideUI.CompOperator;

            uiConfigs = LocalDataManager.Instance.GetLabsConfigs(uiLabels, compOperator);

            HashSet<string> uiStrConfigs = new HashSet<string>();

            // 执行 UI 相关 的标签
            {
                foreach (int uiConfig in uiConfigs)
                {
                    // 通知 每一种类型的 ui 要干啥
                    string triggerKey = TriggleEventUtils.FromatUITriggleKey(uiConfig);
                    uiStrConfigs.Add(triggerKey);
                }
            }

            // 执行 技能按钮 相关的标签,此处 技能按钮 策划需要 公用一个 基础的 UIConfig 配置,
            // 然后 以此为基础, 加上技能槽 上 当前 技能 配置的 Labels 共同 确定一个 技能 的 labels.

            // 找到 uiTags 标签 对应的 所有 相关的 ui 类型枚举Set
            HashSet<string> complexUIConfigs = LocalDataManager.Instance.GetLabsComplexConfigs(uiLabels);

            // 执行 复杂UI 相关 的标签
            {
                foreach (string uiConfig in complexUIConfigs)
                {
                    // 通知 每一种类型的 ui 要干啥
                    string triggerKey = TriggleEventUtils.FromatComplexUITriggleKey(uiConfig);
                    uiStrConfigs.Add(triggerKey);
                }
            }

            player.Data.TriggerHiddenUIChange(uiStrConfigs, onEnter);
        }

        public override void OnExit()
        {
            var player = GameManager.Instance.GetEntityCtr(ownerEntityID);
            if (player != null && player.Data.isMainPlayer)
            {
                // List<ChangeAnim> changeAnims = BuffInfo.Cfg.ChangeAnims;
                // player.UnRegisterChangeAnim(RuntimeID.ToString(), changeAnims);
                HandleHiddenUIEffect(false, player);
            }

            base.OnExit();
        }

    }
}

