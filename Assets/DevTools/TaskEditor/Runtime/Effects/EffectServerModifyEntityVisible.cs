///--------------------------------------------------------------------
/// 文件名   :   EffectServerModifyEntityVisible.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/04/12 09:56:58
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    public class EffectServerModifyEntityVisible : BaseEffect
    {
        [LabelText("地图ID")]
        public int MapID;

        [LabelText("对象类型")]
        [ValueDropdown("GetEntityType")]
        public TaskEntityType EntityType;

        [LabelText("唯一ID")]
        public int Index;


        [LabelText("显示、隐藏")]
        [SuffixLabel("false- 隐藏 ,true- 显示")]
        public bool Visibility;
        
        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = MapID.ToString();
            effectJson.Args2 =((int)EntityType).ToString();
            effectJson.Args3 = Index.ToString();
            effectJson.Args4 = Visibility?"1":"0";

        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            MapID = ToInt(effectJson.Args1);
            EntityType = (TaskEntityType)ToInt(effectJson.Args2);
            Index = ToInt(effectJson.Args3);
            Visibility = ToBoolean(effectJson.Args4);
        }
        
        public IEnumerable GetEntityType()
        {
            return TaskEnumUtils._taskentitytypes;
        }

    }
}
