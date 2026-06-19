
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeAddBuff
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
    /// 添加BUFF
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeAddBuff:BaseEffectType 
    {
        /// <summary>
        /// 添加对象Key
        /// <summary>
        [LabelText("添加对象Key")]
        [HideReferenceObjectPicker]
        public InputKey EffectTargetArray= new InputKey();

        /// <summary>
        /// BUFFID
        /// <summary>
        [LabelText("BUFFID")]
        public int BuffID;

        /// <summary>
        /// BUFF时间
        /// <summary>
        [LabelText("BUFF时间")]
        public int BUFFTime;

        /// <summary>
        /// BUFF层数
        /// <summary>
        [LabelText("BUFF层数")]
        public int Pile=1;

        /// <summary>
        /// Buff拷贝数据
        /// <summary>
        [LabelText("Buff拷贝数据")]
        [HideReferenceObjectPicker]
        public List<CustomDictionary> BuffCopyData= new List<CustomDictionary>();

        [Button("CopyBuff")]
        public void DoCopyBuff()
        {
#if UNITY_EDITOR
            if (BuffCopyData == null)
            {
                BuffCopyData = new List<CustomDictionary>();
            }
            BuffCopyData.Clear();
            TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/DevTools/SkillEditor/Export/Json/Buff/Buff_{BuffID}.json");
            if (textAsset != null && textAsset.text != null)
            {
                BuffJson json = Newtonsoft.Json.JsonConvert.DeserializeObject<BuffJson>(textAsset.text);
                if (json != null)
                {
                    foreach (var item in json.config.BuffCopyData)
                    {
                        BuffCopyData.Add(new CustomDictionary(){ToKey=item});
                    }
                }
            }
#endif
        }

    }

}