///--------------------------------------------------------------------
/// 文件名   :   CMCondition.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/17 16:55:35
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    public abstract class CMCondition
    {
        [LabelText("子组ID")]
        [HideInInspector]
        public int SubID;

        [LabelText("描述")]
        [HideInInspector]
        public string Desc;

        [LabelText("不满足时是否显示")]
        [HideInInspector]
        public bool IsShow;

        [LabelText("是否取反")]
        public bool Flag;

        [HideInInspector]
        public ConditionType ConditionType { get; private set; }


        public void SetConditionType(ConditionType type)
        {
            ConditionType = type;
        }

        public virtual void OnSerializd(JsonConditon conditon)
        {
            conditon.ConditionType = ConditionType;
            conditon.SubGroupID = SubID;
            conditon.Desc = Desc;
            conditon.IsShow = IsShow;
            conditon.Flag = Flag;
        }

        public virtual void OnDeSerializd(JsonConditon conditon)
        {
            ConditionType = conditon.ConditionType;
            SubID = conditon.SubGroupID;
            Desc = conditon.Desc;
            IsShow = conditon.IsShow;
            Flag = conditon.Flag ;
        }

        public int ToInt(string arg)
        {
            int result = 0;
            System.Int32.TryParse(arg,out result);
            return result;
        }
    }



    public interface IJsonType
    {
        void OnJsonSerializer(JsonSerializer jsonSerializer);
    }

}

