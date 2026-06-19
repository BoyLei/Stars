
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeSerialize
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   
/// 创建人   :   Create By BaseDataConfig.xml
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SkillEditor
{
    /// <summary>
    /// 效果序列化
    /// </summary>
    [System.Serializable]
    
    public class EffectTypeSerialize
    {
        /// <summary>
        /// 效果类型
        /// <summary>
        [LabelText("效果类型")]
        [ValueDropdown("_effecttype")]
        [OnValueChanged("InitEffect")]
        public EffectType EffectType = new EffectType();

        [LabelText("效果参数")]
        [SerializeReference]
        [HideReferenceObjectPicker]
        public BaseEffectType BaseEffect;


        public void InitEffect()
        {
            var type = System.Type.GetType($"SkillEditor.EffectType{EffectType.ToString()}");
            BaseEffect = (BaseEffectType)Activator.CreateInstance(type);
        }

        public List<string> GetOutputKey()
        {
            List<string> outputKeys = new List<string>();
            if (BaseEffect == null)
            {
                return outputKeys;
            }
            System.Type argType = BaseEffect?.GetType();
            FieldInfo[] fields = argType.GetFields(); // 获取类中所有的字段

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(OutputKey)) // 检查字段类型是否为输出Key
                {
                    OutputKey outputKey = field.GetValue(BaseEffect) as OutputKey; // 获取字段的值
                    outputKeys.Add(outputKey.Result);
                }
            }
            List<string> result = outputKeys.Where(str => !String.IsNullOrEmpty(str)).Distinct().ToList();
            return result;
        }

        public List<string> GetInputKey()
        {
            List<string> inputKeys = new List<string>();
            if (BaseEffect == null)
            {
                return inputKeys;
            }
            System.Type argType = BaseEffect.GetType();
            FieldInfo[] fields = argType.GetFields(); // 获取类中所有的字段

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(InputKey)) // 检查字段类型是否为输入Key
                {
                    InputKey inputKey = field.GetValue(BaseEffect) as InputKey; // 获取字段的值
                    inputKeys.Add(inputKey.Result);
                }
                else//否则要再去里面找一下
                {
                  
                    if (field.GetValue(BaseEffect)==null)
                    {
                        if (field.Name == "EventPath")
                        {
                            field.SetValue(BaseEffect, "");
                            Debug.LogError($"帮你把{field.Name}初始化了");
                        }
                        else
                        {
                            Debug.LogError($"{BaseEffect}的{field.Name}为空");
                        }
                        //  Debug.DebugBreak();
                    }

                    if (field.GetValue(BaseEffect) == null)
                    {
                        continue;
                    }
                    System.Type curType = field.GetValue(BaseEffect).GetType();
                    FieldInfo[] curFields = curType.GetFields(); // 获取类中所有的字段
                    foreach (FieldInfo curField in curFields)
                    {
                        if (curField.FieldType == typeof(InputKey)) // 检查字段类型是否为输入Key
                        {
                            InputKey inputKey = curField.GetValue(field.GetValue(BaseEffect)) as InputKey; // 获取字段的值
                            inputKeys.Add(inputKey.Result);
                        }
                    }
                }
            }
            List<string> result = inputKeys.Where(str => !String.IsNullOrEmpty(str)).Distinct().ToList();
            return result;
        }

        public IEnumerable _effecttype()
        {
            return EnumDefineMap._effecttype;
        }
    }
}