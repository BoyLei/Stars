using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
public class VectorUtils
{
    /// <summary>
    /// 其实 就是 v3.Set() . 博哥说 Set 可能有问题,就封装下
    /// </summary>
    /// <param name="v3"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Vector3 V3Set(Vector3 v3, float x, float y, float z)
    {
        v3.x = x;
        v3.y = y;
        v3.z = z;

        return v3;
    }

    public static Vector2 V2Set(Vector2 v2, float x, float y)
    {
        v2.x = x;
        v2.y = y;

        return v2;
    }
}