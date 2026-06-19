///--------------------------------------------------------------------
/// 文件名   :   EffectSerialize.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 17:50:03
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using MessagePack;
using Sirenix.OdinInspector;
using StarProject.Service.Language;
using System;
using System.Collections;
using UnityEngine;

namespace Task
{
    [System.Serializable]
    public class EffectSerialize
    {
        [LabelText("效果类型")]
        [ValueDropdown("GetEffectTypes")]
        [OnValueChanged("OnEffectTypeChanged")]
        public FunctionType EffectType = FunctionType.None;

#if UNITY_EDITOR      

        private void OnEffectTypeChanged(Sirenix.OdinInspector.Editor.InspectorProperty property, FunctionType value)
        {
            if (TaskEnumUtils.EffectTypeFactory.TryGetValue(value, out var tp))
            {
                baseEffect = (BaseEffect)Activator.CreateInstance(tp);
                baseEffect.curType = value;
            }
        }
#endif
        [LabelText("任务参数")]
        [SerializeReference]
        [HideReferenceObjectPicker]
        public BaseEffect baseEffect;

        public BaseEffect GetBaseEffect()
        {
            return baseEffect;
        }


        public EffectSerialize(EffectJsonData effectJsonData)
        {
            EffectType = effectJsonData.EffectType;
            if (TaskEnumUtils.EffectTypeFactory.TryGetValue(EffectType, out var tp))
            {
                baseEffect = (BaseEffect)Activator.CreateInstance(tp);
                baseEffect.curType = EffectType;
                baseEffect.OnDeSerialized(effectJsonData);
            }
        }

        public IEnumerable GetEffectTypes()
        {
            return TaskEnumUtils._effecttypetypes;
        }
    }

    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    public class EffectJsonData
    {
        [Key(0)]
        public bool WaitEnd;
        [Key(1)]
        //public string ShowText;
        public string ShowText
        {
            get
            {
#if UNITY_EDITOR
                //任务编辑器打开直接返回中文
                if (LanguageManager.Instance.TaskEditorOpened)
                {
                    return showText;
                }
#endif
                //非编辑器模式返回key对应的语言文本
                return LanguageManager.Instance.GetLanguageByKey(ShowText_Key);
            }
            set { showText = value; }
        }
        private string showText;
        [Key(2)]
        public string ShowText_Key;
        [Key(3)]
        public int OutTime;
        [Key(4)]
        public string Args1;
        [Key(5)]
        public string Args2;
        [Key(6)]
        public string Args3;
        [Key(7)]
        public string Args4;
        [Key(8)]
        public string Args5;
        [Key(9)]
        public string Args6;
        [Key(10)]
        public string Args7;
        [Key(11)]
        public string Args8;
        [Key(12)]
        public string Args9;
        [Key(13)]
        public string Args10;
        [Key(14)]
        public FunctionType EffectType;

        private void Init()
        {
            WaitEnd = false;
            ShowText = null;
            OutTime = 0;
            Args1 = null;
            Args2 = null;
            Args3 = null;
            Args4 = null;
            Args5 = null;
            Args6 = null;
            Args7 = null;
            Args8 = null;
            Args9 = null;
            Args10 = null;
        }

        public EffectJsonData()
        {
            Init();
        }

        public EffectJsonData(EffectSerialize effectSerialize)
        {
            Init();
            EffectType = effectSerialize.EffectType;
            var effect = effectSerialize.GetBaseEffect();
            effect?.OnSerialized(this);
        }
    }
}