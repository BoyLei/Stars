///--------------------------------------------------------------------
/// 文件名   :   LevelCondition.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/17 17:05:53
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
    public class GuildLevelLimitCondition : CMCondition
    {
        [LabelText("等级")]
        public int Level;

        public override void OnSerializd(JsonConditon conditon)
        {
            base.OnSerializd(conditon);
            conditon.Args1 = Level.ToString();

        }

        public override void OnDeSerializd(JsonConditon conditon)
        {
            base.OnDeSerializd(conditon);
            Level = System.Int32.Parse(conditon.Args1);
        }
    }
}
