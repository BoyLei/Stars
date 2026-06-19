///--------------------------------------------------------------------
/// 文件名   :   ClipEffectAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/01 18:34:53
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Timeline;
using System.Linq;
using System.Reflection;
using Task;
using System.Text.RegularExpressions;

namespace SkillEditor
{
#if UNITY_EDITOR
    [DisplayName("效果")]
    [HideMonoScript]
    public class ClipEffectAsset : PlayableAsset, ITimelineClipAsset
    {
        //[LabelText("索引")]
        //[Sirenix.OdinInspector.ReadOnly]
        //public int Index;

        [PropertyOrder(1)]
        [Button("使所有Key增加1")]
        public void Add1ToKey()
        {
            foreach (var effect in template.frameEffect)
            {
                if (effect.EffectArgs.BaseEffect == null)
                {
                    continue;
                }
                System.Type argType = effect.EffectArgs.BaseEffect.GetType();
                FieldInfo[] fields = argType.GetFields(); // 获取类中所有的字段

                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(OutputKey)) // 检查字段类型是否为输出Key
                    {
                        OutputKey outputKey = field.GetValue(effect.EffectArgs.BaseEffect) as OutputKey; // 获取字段的值
                        int index = CheckEndsWithNumber(outputKey.Result, out string willSet);
                        if (index != -1)
                        {
                            OutputKey willSetOutputKey = new OutputKey()
                            {
                                Guid = outputKey.Guid,
                                Result = willSet + (index + 1).ToString()
                            };
                            field.SetValue(effect.EffectArgs.BaseEffect, willSetOutputKey);
                        }
                    }
                    if (field.FieldType == typeof(InputKey)) // 检查字段类型是否为输入Key
                    {
                        InputKey inputKey = field.GetValue(effect.EffectArgs.BaseEffect) as InputKey; // 获取字段的值
                        int index = CheckEndsWithNumber(inputKey.Result, out string willSet);
                        if (index != -1)
                        {
                            InputKey willSetInputKey = new InputKey()
                            {
                                Result = willSet + (index + 1).ToString()
                            };
                            field.SetValue(effect.EffectArgs.BaseEffect, willSetInputKey);
                        }
                    }
                }
            }
        }

        [PropertyOrder(2)]
        [HideLabel] public ClipEffectBehaviour template = new ClipEffectBehaviour();


        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var playable = ScriptPlayable<ClipEffectBehaviour>.Create(graph, template);
            //template.Index = Index;
            ClipEffectBehaviour behaviour = playable.GetBehaviour();

            return playable;
        }

        private int CheckEndsWithNumber(string inputString, out string stringWithoutNumber)
        {
            Regex pattern = new Regex(@"\d+$"); // 正则表达式用于匹配结尾的数字

            Match match = pattern.Match(inputString);

            if (match.Success)
            {
                stringWithoutNumber = pattern.Replace(inputString, ""); // 去除结尾的数字并赋值给输出参数
                return int.Parse(match.Value); // 将匹配的数字字符串转换为整数并返回
            }
            else
            {
                stringWithoutNumber = inputString; // 如果匹配失败，直接将输入字符串赋值给输出参数
                return -1; // 返回-1
            }
        }
    }
#endif
}