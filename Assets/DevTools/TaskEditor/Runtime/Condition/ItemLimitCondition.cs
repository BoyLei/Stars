///--------------------------------------------------------------------
/// 文件名   :   ItemLimitCondition.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 10:29:20
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    public class ItemLimitCondition : CMCondition
    {
        [LabelText("道具ID")]
        [Newtonsoft.Json.JsonProperty("Args1")]
        public int ItemID;

        [LabelText("道具数量")]
        [Newtonsoft.Json.JsonProperty("Args2")]
        public int ItemCount;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = ItemID.ToString();
            conditon.Args2 = ItemCount.ToString();
  
        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            ItemID = ToInt(conditon.Args1);
            ItemCount = ToInt(conditon.Args2);
        }
    }
}