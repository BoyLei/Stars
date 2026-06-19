
///--------------------------------------------------------------------
/// 文件名   :   Materials
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
    /// 材质球列表
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class Materials:BaseCustomData 
    {
        /// <summary>
        /// 材质球
        /// <summary>
        [LabelText("材质球")]
        [FilePath]
        public List<string> ChangeMaterials= new List<string>();

    }

}