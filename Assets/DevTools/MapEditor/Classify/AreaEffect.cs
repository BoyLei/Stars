///--------------------------------------------------------------------
/// 文件名   :   AreaEffect
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/07/25 17:58:53
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapEditor
{
    [System.Serializable]
    public class AreaEffect
    {

        private IEnumerable _effects = new ValueDropdownList<EffectType>()
        {
            { "释放技能", EffectType.ReleaseSkill },
            { "添加Buff", EffectType.AddBuff },
            { "接收任务", EffectType.ReciveTask },
            { "完成任务", EffectType.FinishTask },
            { "终止任务", EffectType.StopTask },
        };



        [LabelText("效果类型")]
        [ValueDropdown("_effects")]
        [OnValueChanged("OnEffectTypeChange")]
        public EffectType effectType = EffectType.ReleaseSkill;

        //[DictionaryDrawerSettings(KeyLabel = "参数名称", ValueLabel = "参数值", IsReadOnly = true)]
        [LabelText("效果参数")]
        //[ShowInInspector]
        public List<EffectArg> Args = new List<EffectArg>();
        //public Dictionary<string, string> Args = new Dictionary<string, string>();

        public string GetArgs()
        {
            string args = string.Empty;
            int index = 0;
            foreach (var item in Args)
            {
                if (index == 0)
                {
                    args = item.Value;
                }
                else
                {
                    args += "," + item.Value;
                }
                index++;
            }
            return args;
        }

        public void OnEffectTypeChange()
        {
            Args.Clear();

            if (EditorConfigUtils.AreaEffectConfigs.Effects.TryGetValue((int)effectType, out var areaEffectData) && areaEffectData != null)
            {
                if (areaEffectData.Args != null && areaEffectData.Args.Count > 0)
                {
                    foreach (var item in areaEffectData.Args)
                    {
                        Args.Add(new EffectArg() { Key = item,Value="" }) ;
                    }
                }
            }

        }
    }

    [System.Serializable]
    public class EffectArg
    {
        [LabelText("参数名称")]
        public string Key;

        [LabelText("参数值")]
        public string Value;
    }
}
#endif