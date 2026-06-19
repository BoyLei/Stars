
///--------------------------------------------------------------------
/// 文件名   :   AddEffectFly
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

namespace SkillEditor
{
    public partial class AddEffectFly
    {
        public string OnValueGet()
        {
#if UNITY_EDITOR
            //技能编辑器打开直接返回中文
            if (LanguageManager.Instance.SkillEditorOpened)
            {
                return val;
            }
#endif
            //非编辑器模式返回key对应的语言文本
            return LanguageManager.Instance.GetLanguageByKey(Value_Key);
        }

        public void OnValueSet(string value)
        {
            val = value;
        }
    }

}