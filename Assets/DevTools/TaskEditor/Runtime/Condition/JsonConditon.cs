///--------------------------------------------------------------------
/// 文件名   :   JsonConditon.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/04/05 18:23:52
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using MessagePack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    public class JsonConditon
    {
        [Key(0)]
        public ConditionType ConditionType;
        [Key(1)]
        public int SubGroupID;
        [Key(2)]
        public string Desc;
        [Key(3)]
        public bool IsShow;
        [Key(4)]
        public bool Flag;
        [Key(5)]
        public string Args1;
        [Key(6)]
        public string Args2;
        [Key(7)]
        public string Args3;
        [Key(8)]
        public string Args4;
        [Key(9)]
        public string Args5;
        [Key(10)]
        public string Args6;
        [Key(11)]
        public string Args7;
        [Key(12)]
        public string Args8;
        [Key(13)]
        public string Args9;
        [Key(14)]
        public string Args10;



        public JsonConditon()
        {
            ConditionType = ConditionType.LevelLimit;
            SubGroupID = 0;
            Desc = string.Empty;
            IsShow = false;
            Flag = false;
            Args1 = null;
            Args2 = null;
            Args3 = null;
            Args4 = null;
            Args5 = null;
            Args6 = null;
            Args7 = null;
            Args8 = null;
            Args9 = null;
            Args10 = null;
        }

        public JsonConditon(ConditionSerialize serialize)
        {
            ConditionType = ConditionType.LevelLimit;
            SubGroupID = 0;
            Desc = string.Empty;
            IsShow = false;
            Flag = false;
            Args1 = null;
            Args2 = null;
            Args3 = null;
            Args4 = null;
            Args5 = null;
            Args6 = null;
            Args7 = null;
            Args8 = null;
            Args9 = null;
            Args10 = null;
            var condition = serialize.GetMCondition();
            if(condition!=null)
            {
                condition.OnSerializd(this);
            }
            ConditionType = serialize.ConditionType;
        }

    }
}
