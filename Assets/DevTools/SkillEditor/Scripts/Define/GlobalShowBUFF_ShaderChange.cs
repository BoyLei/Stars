
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowBUFF_ShaderChange
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
    /// 修改Shader
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class GlobalShowBUFF_ShaderChange:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// Shader枚举
        /// <summary>
        [LabelText("Shader枚举")]
        [ValueDropdown("_shaderenum")]
        public ShaderEnum ShaderEnum= new ShaderEnum();

        public IEnumerable _shaderenum()
        {
            return EnumDefineMap._shaderenum;
        }

    }

}