
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeAddPassive
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
using MessagePack;
using Newtonsoft.Json.Converters;
namespace SkillEditor
{
    /// <summary>
    /// 添加被动
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeAddPassive:BaseEffectType 
    {
        /// <summary>
        /// 添加对象Key
        /// <summary>
        [LabelText("添加对象Key")]
        [HideReferenceObjectPicker]
        public InputKey InputKey= new InputKey();

        /// <summary>
        /// 被动ID
        /// <summary>
        [LabelText("被动ID")]
        public int PassiveID;

        /// <summary>
        /// 被动时间
        /// <summary>
        [LabelText("被动时间")]
        public int PassiveTime;

        /// <summary>
        /// Passive拷贝数据
        /// <summary>
        [LabelText("Passive拷贝数据")]
        public List<CustomDictionary> PassiveCopyData= new List<CustomDictionary>();

        [Button("CopyPassive")]
        public void DoCopyPassive()
        {
#if UNITY_EDITOR
            if (PassiveCopyData == null)
            {
                PassiveCopyData = new List<CustomDictionary>();
            }
            PassiveCopyData.Clear();
            TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/DevTools/SkillEditor/Export/Json/Passive/Passive_{PassiveID}.json");
            if (textAsset != null && textAsset.text != null)
            {
                PassiveJson json = Newtonsoft.Json.JsonConvert.DeserializeObject<PassiveJson>(textAsset.text);
                if (json != null)
                {
                    foreach (var item in json.config.PassiveCopyData)
                    {
                        PassiveCopyData.Add(new CustomDictionary(){ToKey=item});
                    }
                }
            }
#endif
        }

    }

}