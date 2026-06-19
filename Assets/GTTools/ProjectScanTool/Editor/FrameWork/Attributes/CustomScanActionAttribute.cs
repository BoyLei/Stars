/*
 * @Description: 自定义扫描函数，在自定义扫描类里面，添加了当前特性的函数将被批量执行，避免手动去调度每一个函数
 */
using System;
using System.Diagnostics;

namespace CasualEngine.ProjectScanTool
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class CustomScanActionAttribute : Attribute
    {
        public CustomScanActionAttribute()
        {
        }
    }
}