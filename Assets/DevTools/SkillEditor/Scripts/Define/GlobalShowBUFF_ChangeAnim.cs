
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowBUFF_ChangeAnim
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
    /// 修改状态动作
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class GlobalShowBUFF_ChangeAnim:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 动作修改
        /// <summary>
        [LabelText("动作修改")]
        public List<ChangeAnim> ChangeAnims= new List<ChangeAnim>();

    }

}