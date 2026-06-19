using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GMTestData
{
    public static Dictionary<string, object> GMDataMap = new Dictionary<string, object>();

    /// <summary>
    /// 检查 cusCmd 对于的gm 是否开启
    /// </summary>
    /// <param name="cusCmd"></param>
    /// <returns></returns>
    public static bool CheckGmIsOpen(string cusCmd)
    {
        if (!GMDataMap.ContainsKey(cusCmd))
        {
            return false;
        }
        return (bool)GMDataMap[cusCmd];
    }
}
