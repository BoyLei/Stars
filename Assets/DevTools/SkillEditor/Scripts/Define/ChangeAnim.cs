
///--------------------------------------------------------------------
/// 文件名   :   ChangeAnim
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
    /// 动作修改
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class ChangeAnim 
    {
        /// <summary>
        /// 动作状态
        /// <summary>
        [LabelText("动作状态")]
        [ValueDropdown("_animstate")]
        public AnimState AnimState= new AnimState();

        /// <summary>
        /// 动作路径
        /// <summary>
        [LabelText("动作路径")]
        [FilePath]
        public string Anim;

        public IEnumerable _animstate()
        {
            return EnumDefineMap._animstate;
        }

    }

}