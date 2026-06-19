/*
 * @Description: 扫描模块，统计定义当前模块的扫描规则
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    internal class ScanModeDrawItem
    {
        private ScanRulePreview[] rulesPreview;

        [HideInInspector]
        public EnumScanModes scanMode;

        public ScanModeDrawItem(EnumScanModes type)
        {
            this.scanMode = type;
            List<UnityEngine.Object> rules = new List<UnityEngine.Object>();
            foreach (var item in ProjectScanGlobalConfig.allScanModes)
            {
                if (item.scanMode == type)
                {
                    foreach (var child in item.scanRules)
                    {
                        var customRule = AssetDatabase.LoadAssetAtPath(ProjectScanGlobalConfig.scanTypeConfig_dir + child.assetType.Name + ".asset", child.assetType);
                        if (customRule.GetType().BaseType == typeof(CustomRule))
                        {
                            (customRule as CustomRule).RegisterMenuItem(customRule);
                            (customRule as CustomRule).bindMode = ProjectScanHelper.GetCustomRuleBindMode(customRule);
                            rules.Add(customRule);
                        }
                    }
                }
            }

            this.rulesPreview = new ScanRulePreview[rules.Count];
            for (int i = 0; i < rules.Count; i++)
            {
                this.rulesPreview[i] = new ScanRulePreview(rules[i]);
            }
            //扫描项排序规则，1、先按权值排序，2、再按名称排序
            this.rulesPreview.Sort((x, y) =>
            {
                var srcPri = ProjectScanHelper.GetCustomRulePriority(x.customRule);
                var dstPri = ProjectScanHelper.GetCustomRulePriority(y.customRule);
                if (srcPri != dstPri)
                {
                    return srcPri - dstPri;
                }
                return (x.customRule as CustomRule).ruleTitle.CompareTo((y.customRule as CustomRule).ruleTitle);
            });
        }

        public ScanRulePreview[] RulesPreview
        {
            get
            {
                return rulesPreview;
            }
        }

        [OnInspectorGUI]
        public void Draw()
        {
            GUILayout.BeginVertical(new GUILayoutOption[0]);
            if (this.rulesPreview.Length != 0)
            {
                foreach (ScanRulePreview attributeExamplePreview in this.rulesPreview)
                {
                    attributeExamplePreview.Draw();
                }
            }
            GUILayout.EndVertical();
        }
    }
}