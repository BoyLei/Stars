///--------------------------------------------------------------------
/// 文件名   :   IsNullOrEmptyExtension.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/01/10 10:17:14
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public static class IsNullOrEmptyExtension
{
    public static bool IsNullOrEmpty(this IEnumerable source)
    {
        if (source != null)
        {
            foreach (object obj in source)
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
    {
        if (source != null)
        {
            foreach (T obj in source)
            {
                return false;
            }
        }
        return true;
    }
}
