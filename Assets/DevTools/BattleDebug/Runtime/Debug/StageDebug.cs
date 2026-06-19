///--------------------------------------------------------------------
/// 文件名   :   StageDebug.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/02 09:49:28
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
namespace BattleDebug
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class StageDebug
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string UniqueID;

        /// <summary>
        /// 类型 1 动画
        /// </summary>
        public StageEnum Type;

        public double StarTime;

        public double EndTime;

        public List<string> Args = new List<string>();

        public bool CanModify = true;

        public StageDebug()
        {
            UniqueID = "";
            StarTime = 0;
            CanModify = true;
        }

        public StageDebug(string uid, StageEnum stageEnum)
        {
            UniqueID = uid;
            Type = stageEnum;
            StarTime = BattleDebugHelper.GetTimeOffset(System.DateTime.Now);
            CanModify = true;
        }

        public bool IsNull()
        {
            return string.IsNullOrEmpty(UniqueID) && StarTime == 0;
        }

        public void Modify(string arg)
        {
            if (!CanModify)
            {
                Debug.LogError($"{Type.ToString()} {UniqueID}已经结束不能修改");
                return;
            }
            Args.Add(arg);

        }

        public void OnEnd()
        {
            CanModify = false;
            EndTime = BattleDebugHelper.GetTimeOffset(System.DateTime.Now);
        }
    }
}