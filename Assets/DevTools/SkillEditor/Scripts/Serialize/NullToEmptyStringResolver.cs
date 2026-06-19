///--------------------------------------------------------------------
/// 文件名   :   NullToEmptyStringResolver.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/16 17:59:55
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class NullToEmptyStringResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        return type.GetProperties()
                .Select(p => {
                    var jp = base.CreateProperty(p, memberSerialization);
                    jp.ValueProvider = new NullToEmptyStringValueProvider(p);
                    return jp;
                }).ToList();
    }
}

public class NullToEmptyStringValueProvider : IValueProvider
{
    PropertyInfo _MemberInfo;
    public NullToEmptyStringValueProvider(PropertyInfo memberInfo)
    {
        _MemberInfo = memberInfo;
    }

    public object GetValue(object target)
    {
        object result = _MemberInfo.GetValue(target);
        if (_MemberInfo.PropertyType == typeof(string) && result == null) result = "";
        return result;

    }

    public void SetValue(object target, object value)
    {
        _MemberInfo.SetValue(target, value);
    }
}