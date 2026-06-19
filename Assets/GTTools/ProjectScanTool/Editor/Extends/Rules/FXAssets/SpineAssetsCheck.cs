/*
 * @Description: Spine资源检查
 */
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CasualEngine.ProjectScanTool
{
    //使用到的材质数据检查
    [Serializable, HideLabel]
    public class SpineUsedMaterialCheck
    {
        public StringVal shader = new StringVal("Spine/SkeletonAlpha"){
            dropDownMethod = "@ProjectScanGlobalConfig.allShaders"
        };

        public BoolVal straightAlphaInput = new BoolVal(true);

        public EnumVal srcBlendMode = new EnumVal(Convert.ToInt32(BlendMode.One), typeof(BlendMode));

        public EnumVal dstBlendMode = new EnumVal(Convert.ToInt32(BlendMode.OneMinusSrcAlpha), typeof(BlendMode));
    }


    [Serializable, HideLabel]
    public class SpineAssetsCheckDetail : CustomCheckDetail
    {
        [FoldoutGroup("纹理数据检查", Expanded = true)]
        public DefaultTextureFormatDetail texCheck = new DefaultTextureFormatDetail();

        [FoldoutGroup("材质数据检查", Expanded = true)]
        public SpineUsedMaterialCheck matCheck = new SpineUsedMaterialCheck();
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 0)]
    public class SpineAssetsCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "Spine资源检查";
            ruleDescription = "[说明]：包括检查Spine资源使用到的的纹理和材质参数设置。此检查项提供自动修正和后处理功能。";
        }


        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<SpineAssetsCheckDetail> checkDetailList = new List<SpineAssetsCheckDetail>() { new SpineAssetsCheckDetail() };

        [CustomScanAction]
        public void Do_SpineAssetsCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_SpineAssetsCheck, this);
        }
    }
}
