using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using Sirenix.OdinInspector;
using StarProject.Service.Language;
using StarProject;

[System.Serializable]
[XLua.LuaCallCSharp]
public class TutorialConfig
{
    //编号
    [LabelText("编号")] [FoldoutGroup("基础配置")]
    public int ID;
    
    [LabelText("下一步编号")] [FoldoutGroup("基础配置")]
    public int NextID;
    //引导名
    [LabelText("引导名")] [FoldoutGroup("基础配置")]
    public string Name;

    [LabelText("标记位索引")] [FoldoutGroup("基础配置")]
    [SuffixLabel(@"$GetFlags")]
    public int FlagIndex;
    
    //引导组
    [LabelText("引导组")] [FoldoutGroup("基础配置")]
    public int GroupID;

    //引导类型
    //[LabelText("引导类型")] [FoldoutGroup("基础配置")] [ValueDropdown("GetTutorialTypes")]
    //public E_TutorialType TutorialType;

    //引导类型
    [LabelText("是否强引导")] [FoldoutGroup("基础配置")]
    public bool IsForceGuide;
    
    //中断是否从头引导
    //[LabelText("是否为断点")] [FoldoutGroup("基础配置")]
    /*public E_TutorialPointType PointType;*/
   // public bool IsPoint = false;
    
    //[LabelText("是否服务器存储")] [FoldoutGroup("基础配置")]
    //public bool IsServerSave;

    [LabelText("设置标记位索引")] [FoldoutGroup("基础配置")]
    [SuffixLabel("$GetSetFlag")]
    public int SetFlagIndex;
    

    /*
    //是否显示黑底
    [LabelText("是否显示黑底")] [FoldoutGroup("引导表现")]
    public bool ShowMask;
    */

    /*//黑底透明度
    [LabelText("黑底透明度")] [FoldoutGroup("引导表现")]
    [Range(0,1)]
    public float MaskAlpha;*/

    [LabelText("箭头方向")]
    [FoldoutGroup("引导表现")]
    [ValueDropdown("GetDirections")]
    public Vector2Int ArrowDirection=TutorialDefine.center;

    [LabelText("是否显示箭头")] [FoldoutGroup("引导表现")]
    public bool ShowArrow;

    [LabelText("引导形状")]
    [FoldoutGroup("引导表现")]
    [ValueDropdown("GetMaskShapeTypes")]
    [JsonConverter(typeof(StringEnumConverter))]
    public E_MaskShapeType MaskShapeType;

    [LabelText("文本框样式")]
    [FoldoutGroup("引导表现")]
    [ValueDropdown("GetTextStyles")]
    [JsonConverter(typeof(StringEnumConverter))]
    public E_TextStyle TextStyle;
    
    //文字
    [LabelText("文字标题")] [FoldoutGroup("引导表现")]
    public string TipTitleText;

    private string tipText;

    [LabelText("文字")] [FoldoutGroup("引导表现")]
    [ShowInInspector]
    public string TipText
    {
        get
        {
#if UNITY_EDITOR
            //编辑器模式直接返回中文
            if (!Application.isPlaying)
            {
                return tipText;
            }
            else
            {
                if (AppConfig.IsLanguageEditorOpen("(TutorialWindow)"))
                {
                    return tipText;
                }
                else
                {
                    return LanguageManager.Instance.GetLanguageByKey(TipText_Key);
                }
            }
#else
            //非编辑器模式返回key对应的语言文本
            return LanguageManager.Instance.GetLanguageByKey(TipText_Key);
#endif
        }
        set { tipText = value; }
    }
    [HideInInspector]
    public string TipText_Key;


    [LabelText("图片引导组")] [FoldoutGroup("引导表现")]
    public int ImageGroup;
    /*
         //手指拖动时长
    [LabelText("手指拖动时长")] [FoldoutGroup("引导表现")]
    public int FingerSiderTime;


    //黑底长宽
    [LabelText("黑底长宽")] [FoldoutGroup("引导表现")]
    public Vector2Int MaskArea;

    //是否缩圈
    [LabelText("是否缩圈")] [FoldoutGroup("引导表现")]
    public bool CycleFx;

    //文本框偏移量
    [LabelText("文本框偏移量")] [FoldoutGroup("引导表现")]
    public Vector2Int TextOffset;

    //文本头像id
    [LabelText("文本头像id")] [FoldoutGroup("引导表现")]
    public int TextRoleID;

    //语音
    [LabelText("语音ID")] [FoldoutGroup("引导表现")]
    public int Voice;

    //循环时间
    [LabelText("循环时间")] [FoldoutGroup("引导表现")]
    public int CyclicTime;
  */
    [LabelText("触发条件")] [FoldoutGroup("触发条件")]
    public List<TutorialConditionConfig> TriggerConditions = new List<TutorialConditionConfig>();

    [LabelText("引导目标")] [FoldoutGroup("引导目标")]
    public List<TutorialTargetConfig> TutorialTargets = new List<TutorialTargetConfig>();

    [LabelText("完成条件")] [FoldoutGroup("完成条件")]
    public TutorialCompleteConfig CompleteConfig;

    public IEnumerable GetTutorialTypes()
    {
        return TutorialDefine.e_tutorialtypes;
    }

    public IEnumerable GetTutorialPointTypes()
    {
        return TutorialDefine.e_tutorialpointtypes;
    }
    
    public string GetSetFlag()
    {
        if (SetFlagIndex > 0 && SetFlagIndex < TutorialFlagConfig.Instance.Flags.Count)
        {
            return  TutorialFlagConfig.Instance.Flags[SetFlagIndex].FlagName;
        }

        return "未知";
    }
    
    public IEnumerable GetFlags()
    {
        if (FlagIndex > 0 && FlagIndex < TutorialFlagConfig.Instance.Flags.Count)
        {
            return  TutorialFlagConfig.Instance.Flags[FlagIndex].FlagName;
        }

        return "未知";
    }


    public IEnumerable GetDirections()
    {
        return TutorialDefine.arrowdirections;
    }

    public IEnumerable GetMaskShapeTypes()
    {
        return TutorialDefine.e_maskshapetypes;
    }

    public IEnumerable GetTextStyles()
    {
        return TutorialDefine.e_textstyles;
    }
    
    public string GetViewName()
    {
        return $"{GroupID}_{ID}_{Name}";
    }

    public string GetText()
    {
        if (string.IsNullOrEmpty(TipText))
        {
            return string.Empty;
        }
        return TipText.Replace("[-n]", "\n");
    }
}