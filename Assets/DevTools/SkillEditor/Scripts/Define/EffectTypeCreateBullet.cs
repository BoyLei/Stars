
///--------------------------------------------------------------------
/// 文件名   :   EffectTypeCreateBullet
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
    /// 创造子弹
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class EffectTypeCreateBullet:BaseEffectType 
    {
        /// <summary>
        /// 中心点
        /// <summary>
        [LabelText("中心点")]
        [HideReferenceObjectPicker]
        public CenterPosArray CenterPosArray= new CenterPosArray();

        /// <summary>
        /// 朝向
        /// <summary>
        [LabelText("朝向")]
        [HideReferenceObjectPicker]
        public TowardArray TowardArray= new TowardArray();

        /// <summary>
        /// 子弹ID
        /// <summary>
        [LabelText("子弹ID")]
        public int BulletID;

        /// <summary>
        /// BulletUseKey
        /// <summary>
        [LabelText("BulletUseKey")]
        [HideReferenceObjectPicker]
        public List<CustomDictionary> BulletCopyData= new List<CustomDictionary>();

        /// <summary>
        /// 输出Key
        /// <summary>
        [LabelText("输出Key")]
        [HideReferenceObjectPicker]
        public OutputKey OutputKey= new OutputKey();

        [Button("CopyBullet")]
        public void DoCopyBullet()
        {
#if UNITY_EDITOR
            if (BulletCopyData == null)
            {
                BulletCopyData = new List<CustomDictionary>();
            }
            BulletCopyData.Clear();
            TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/DevTools/SkillEditor/Export/Json/Bullet/Bullet_{BulletID}.json");
            if (textAsset != null && textAsset.text != null)
            {
                BulletJson json = Newtonsoft.Json.JsonConvert.DeserializeObject<BulletJson>(textAsset.text);
                if (json != null)
                {
                    foreach (var item in json.config.BulletCopyData)
                    {
                        BulletCopyData.Add(new CustomDictionary(){ToKey=item});
                    }
                }
            }
#endif
        }

    }

}