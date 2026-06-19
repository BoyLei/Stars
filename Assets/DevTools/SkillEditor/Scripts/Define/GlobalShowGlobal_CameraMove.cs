
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowGlobal_CameraMove
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
    /// 摄像机移动
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public  class GlobalShowGlobal_CameraMove:GlobalShowTypeSerialize 
    {
        /// <summary>
        /// 移动方式
        /// <summary>
        [LabelText("移动方式")]
        [ValueDropdown("_cameramovetype")]
        public CameraMoveType CameraMoveType= new CameraMoveType();

        /// <summary>
        /// 偏移角度
        /// <summary>
        [LabelText("偏移角度")]
        public int Value;

        /// <summary>
        /// 偏移距离
        /// <summary>
        [LabelText("偏移距离")]
        public int Distance;

        /// <summary>
        /// 进入时间
        /// <summary>
        [LabelText("进入时间")]
        public int EnterTime;

        /// <summary>
        /// 持续时间
        /// <summary>
        [LabelText("持续时间")]
        public int LoopTime;

        /// <summary>
        /// 退出时间
        /// <summary>
        [LabelText("退出时间")]
        public int EndTime;

        public IEnumerable _cameramovetype()
        {
            return EnumDefineMap._cameramovetype;
        }

    }

}