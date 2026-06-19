using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LogUtils
{
    [Flags]
    public enum LogEnum
    {
        GameManagerTest,

        Skill,
        UseSkill,
        SkillEffect,

    }

    public static void LogError(LogEnum tag, object message, bool checkResult = true)
    {
        if (GMTestData.CheckGmIsOpen("CloseSkillLog") && tag == LogEnum.Skill)
        {
            return;
        }
        if (!checkResult)
        {
            return;
        }
        // SGF.Debuger.Log(message);
    }

    public static void LogWarning(LogEnum tag, object message, bool checkResult = true)
    {
        //if (GMTestData.CheckGmIsOpen("CloseSkillLog") && tag == LogEnum.Skill)
        //{
        //    return;
        //}
        if (!checkResult)
        {
            return;
        }
        SGF.Debuger.LogWarning(message);
    }

    public static void Log(LogEnum tag, object message, bool checkResult = true)
    {
        //if (GMTestData.CheckGmIsOpen("CloseSkillLog") && tag == LogEnum.Skill)
        //{
        //    return;
        //}
        if (!checkResult)
        {
            return;
        }
        return;
        SGF.Debuger.Log(message);
    }
}
