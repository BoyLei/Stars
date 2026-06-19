///--------------------------------------------------------------------
/// 文件名   :   BattleDebugHelper.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/02 09:49:28
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDebug
{
    public delegate void BattleDebug(DebugData debugData);

    public enum StageEnum
    {
        SkillState=1,
        Animation = 2,
        Effect = 3,
        Audio = 4,
        ClientEvent = 5,
        ServerEvent = 6,
    }

    public enum DEBUGTYPE
    {
        DEBUG_SKILL = 1,             //技能              技能ID 、开关
        DEBUG_ANIMATION = 2,         //动作              技能ID 、唯一ID、开关、参数
        DEBUG_EFFECT = 3,            //特效              技能ID 、唯一ID、开关、参数
        DEBUG_AUDIO = 4,             //音效              技能ID 、唯一ID、开关、参数
        DEBUG_CLIENT_EVENT = 5,      //客户端执行效果    技能ID 、唯一ID、参数
        DEBUG_SERVER_EVENT = 6,      //服务器执行效果    技能ID 、唯一ID、参数
        DEBUG_SKILL_STATE_EVENT= 7   //技能状态同步      技能ID、开关、客户端/服务器
    }

    public static class BattleDebugHelper
    {

        private static BattleDebug BattleDebugDelegate = null;

        private static BattleRecord record;


        public static BattleRecord GetBattleRecord()
        {
            return record;
        }

        public static double GetTimeOffset(System.DateTime date)
        {
            TimeSpan timeSpan = date.Subtract(record.LocalTime);
            return timeSpan.TotalSeconds;
        }

        public static void SetBattleRecord(BattleRecord battle)
        {
            record = battle;
        }

        public static void SetDelegate(BattleDebug debug)
        {
            BattleDebugDelegate = debug;
        }


        public static void Debug(DebugData debugData)
        {
            BattleDebugDelegate?.Invoke(debugData);
        }

    }
}