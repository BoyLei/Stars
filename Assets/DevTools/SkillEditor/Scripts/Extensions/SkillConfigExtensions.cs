
///--------------------------------------------------------------------
/// 文件名   :   SkillConfig
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   
/// 创建人   :   Create By BaseDataConfig.xml
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StarProject.Service.Language;
using System.Runtime.Serialization;

namespace SkillEditor
{
    public partial class SkillConfig : BaseConfig
    {
        public string OnSkillIconDescGet()
        {
#if UNITY_EDITOR
            //技能编辑器打开直接返回中文
            if (LanguageManager.Instance.SkillEditorOpened)
            {
                return skillIconDesc;
            }
#endif
            //非编辑器模式返回key对应的语言文本
            return LanguageManager.Instance.GetLanguageByKey(SkillIconDesc_Key);
        }
        public void OnSkillIconDescSet(string value)
        {
            skillIconDesc = value;
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            if (!string.IsNullOrEmpty(SkillIconDesc))
            {
                SkillIconDesc_Key = $"SkillIconDesc_{ID}";
            }
            else
            {
                SkillIconDesc_Key = "";
            }
        }
    }
}

