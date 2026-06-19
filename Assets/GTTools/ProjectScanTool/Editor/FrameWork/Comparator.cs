/*
 * @Description: 自定义反射数据比较器，对自定义扫描规则的数据进行对比并根据规则确定是否矫正
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    /// <summary>
    /// 比较级
    /// </summary>
    public enum EnumCompareType
    {
        不等于 = 0,
        等于,
        大于等于,
        小于等于,
        大于,
        小于
    }

    /// <summary>
    /// 对比数据的结果
    /// </summary>
    public enum EnumCompareRet
    {
        正常 = 0,
        异常,
        异常并自动修正
    }


    public class Comparator
    {
        /// <summary>
        /// 对比UnityEngine.Object的数据
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="context">检测对象</param>
        /// <param name="srcObj">源数据-对象</param>
        /// <param name="srcVarName">源数据字段名称</param>
        /// <param name="dstValue">对比的数据</param>
        /// <param name="customRule">自定义的检测规则</param>
        /// <param name="compareType">比较级，如果与当前比较级不一致则表示需要修改数据</param>
        public static EnumCompareRet CompareForObject<T, U>(string methodName, UnityEngine.Object context, T srcObj, string srcVarName, U dstValue, CustomRule customRule, EnumCompareType compareType = EnumCompareType.等于) where T : UnityEngine.Object where U : IComparable
        {
            //对比两个值，如果没有处于需要的比较级情况下，表示是需要修改的
            bool needModify = !CompareObjcetValue(srcObj, srcVarName, dstValue, compareType);
            EnumCompareRet ret = EnumCompareRet.正常;
            if (needModify)
            {
                ret = EnumCompareRet.异常;
                //开启了自动修正
                if (customRule.autoCorrection)
                {
                    ret = EnumCompareRet.异常并自动修正;
                    if (srcObj.GetType().GetField(srcVarName) != null)
                    {
                        srcObj.GetType().GetField(srcVarName).SetValue(srcObj, dstValue);
                    }
                    else if (srcObj.GetType().GetProperty(srcVarName) != null)
                    {
                        srcObj.GetType().GetProperty(srcVarName).SetValue(srcObj, dstValue);
                    }
                    EditorUtility.SetDirty(context);
                }
                string path = AssetDatabase.GetAssetPath(context);
                customRule.Record(context, methodName, string.Format("{0}参数设置错误，应该{1}{2}", srcVarName, compareType.ToString(), dstValue), path, true);
            }
            return ret;
        }

        /// <summary>
        /// 对比new class的数据
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="context">检测对象</param>
        /// <param name="srcObj">源数据-对象</param>
        /// <param name="srcVarName">源数据字段名称</param>
        /// <param name="dstValue">对比的数据</param>
        /// <param name="customRule">自定义的检测规则</param>
        /// <param name="compareType">比较级，如果与当前比较级不一致则表示需要修改数据</param>
        public static EnumCompareRet CompareForClass<T, U>(string methodName, UnityEngine.Object context, T srcObj, string srcVarName, U dstValue, CustomRule customRule, EnumCompareType compareType = EnumCompareType.等于) where T : class where U : IComparable
        {
            //对比两个值，如果没有处于需要的比较级情况下，表示是需要修改的
            bool needModify = !CompareClassValue(srcObj, srcVarName, dstValue, compareType);
            EnumCompareRet ret = EnumCompareRet.正常;
            if (needModify)
            {
                ret = EnumCompareRet.异常;
                //开启了自动修正
                if (customRule.autoCorrection)
                {
                    ret = EnumCompareRet.异常并自动修正;
                    if (srcObj.GetType().GetField(srcVarName) != null)
                    {
                        srcObj.GetType().GetField(srcVarName).SetValue(srcObj, dstValue);
                    }
                    else if (srcObj.GetType().GetProperty(srcVarName) != null)
                    {
                        srcObj.GetType().GetProperty(srcVarName).SetValue(srcObj, dstValue);
                    }
                    EditorUtility.SetDirty(context);
                }
                string path = AssetDatabase.GetAssetPath(context);
                customRule.Record(context, methodName, string.Format("{0}参数设置错误，应该{1}{2}", srcVarName, compareType.ToString(), dstValue), path, true);
            }
            return ret;
        }

        /// <summary>
        /// 对比Struct的数据，注意struct的FieldInfo.SetValue涉及到装箱拆箱问题，所以需要带引用
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="context">检测对象</param>
        /// <param name="srcObj">源数据-struct</param>
        /// <param name="srcVarName">源数据字段名称</param>
        /// <param name="dstValue">对比的数据</param>
        /// <param name="customRule">自定义的检测规则</param>
        /// <param name="compareType">比较级，如果与当前比较级不一致则表示需要修改数据</param>
        public static EnumCompareRet CompareForStruct<T, U>(string methodName, UnityEngine.Object context, ref T srcObj, string srcVarName, U dstValue, CustomRule customRule, EnumCompareType compareType = EnumCompareType.等于) where T : struct where U : IComparable
        {
            //对比两个值，如果没有处于需要的比较级情况下，表示是需要修改的
            bool needModify = !CompareStructValue(srcObj, srcVarName, dstValue, compareType);
            EnumCompareRet ret = EnumCompareRet.正常;
            if (needModify)
            {
                ret = EnumCompareRet.异常;
                //开启了自动修正
                if (customRule.autoCorrection)
                {
                    ret = EnumCompareRet.异常并自动修正;
                    if (srcObj.GetType().GetField(srcVarName) != null)
                    {
                        //srcObj.GetType().GetField(srcVarName).SetValue(srcObj, dstValue); 设置无效
                        //FieldInfo.SetValue的原型是：void SetValue(object obj, object value) 当你传递一个值类型（结构体是值类型）的时候，它要转化成object，也就是要装箱。
                        // 而SetValue将作用在那个装箱产品上，而不是原来的那个结构。 解决办法就是自己装箱和拆箱
                        FieldInfo field = srcObj.GetType().GetField(srcVarName);
                        object box = srcObj;
                        field.SetValue(box, dstValue);
                        srcObj = (T)box;
                    }
                    else if (srcObj.GetType().GetProperty(srcVarName) != null)
                    {
                        srcObj.GetType().GetProperty(srcVarName).SetValue(srcObj, dstValue);
                        PropertyInfo prop = srcObj.GetType().GetProperty(srcVarName);
                        object box = srcObj;
                        prop.SetValue(box, dstValue);
                        srcObj = (T)box;
                    }
                    EditorUtility.SetDirty(context);
                }
                string path = AssetDatabase.GetAssetPath(context);
                customRule.Record(context, methodName, string.Format("{0}参数设置错误，应该{1}{2}", srcVarName, compareType.ToString(), dstValue), path, true);
            }
            return ret;
        }

        /// <summary>
        /// 对比序列化字段的数据
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="context">检测对象</param>
        /// <param name="srcObj">源数据-序列化字段</param>
        /// <param name="srcVarName">源数据字段名称-需要指定序列化类型，boolValue，intValue等</param>
        /// <param name="dstValue">对比的数据</param>
        /// <param name="customRule">自定义的检测规则</param>
        /// <param name="compareType">比较级，如果与当前比较级不一致则表示需要修改数据</param>
        public static EnumCompareRet CompareForSerializedProp<T, U>(string methodName, UnityEngine.Object context, T srcObj, string srcVarName, U dstValue, CustomRule customRule, EnumCompareType compareType = EnumCompareType.等于) where T : SerializedProperty where U : IComparable
        {
            //对比两个值，如果没有处于需要的比较级情况下，表示是需要修改的
            bool needModify = !CompareSerializedPropertyValue(srcObj, srcVarName, dstValue, compareType);
            EnumCompareRet ret = EnumCompareRet.正常;
            if (needModify)
            {
                ret = EnumCompareRet.异常;
                //开启了自动修正
                if (customRule.autoCorrection)
                {
                    ret = EnumCompareRet.异常并自动修正;
                    if (srcObj.GetType().GetField(srcVarName) != null)
                    {
                        srcObj.GetType().GetField(srcVarName).SetValue(srcObj, dstValue);
                    }
                    else if (srcObj.GetType().GetProperty(srcVarName) != null)
                    {
                        srcObj.GetType().GetProperty(srcVarName).SetValue(srcObj, dstValue);
                    }
                    EditorUtility.SetDirty(context);
                }
                string path = AssetDatabase.GetAssetPath(context);
                customRule.Record(context, methodName, string.Format("{0}参数设置错误，应该{1}{2}", srcObj.displayName, compareType.ToString(), dstValue), path, true);
            }
            return ret;
        }

        /// <summary>
        /// 值类型数据之前的对比
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="context">检测对象</param>
        /// <param name="srcObj">源数据-对象</param>
        /// <param name="srcVarName">源数据字段名称</param>
        /// <param name="dstValue">对比的数据</param>
        /// <param name="customRule">自定义的检测规则</param>
        /// <param name="compareType">比较级，如果与当前比较级不一致则表示需要修改数据</param>
        public static EnumCompareRet CompareForBaseValue<T>(string methodName, UnityEngine.Object context, ref T srcValue, string srcVarName, T dstValue, CustomRule customRule, EnumCompareType compareType = EnumCompareType.等于) where T : IComparable
        {
            //对比两个值，如果没有处于需要的比较级情况下，表示是需要修改的
            bool needModify = !CompareTowValue(srcValue, dstValue, compareType);
            EnumCompareRet ret = EnumCompareRet.正常;
            if (needModify)
            {
                ret = EnumCompareRet.异常;
                //开启了自动修正
                if (customRule.autoCorrection)
                {
                    ret = EnumCompareRet.异常并自动修正;
                    srcValue = dstValue;
                    EditorUtility.SetDirty(context);
                }
                string path = AssetDatabase.GetAssetPath(context);
                customRule.Record(context, methodName, string.Format("{0}参数设置错误，应该{1}{2}", srcVarName, compareType.ToString(), dstValue), path, true);
            }
            return ret;
        }

        private static bool CompareObjcetValue<T>(UnityEngine.Object srcObj, string srcVarName, T dstValue, EnumCompareType compareType = EnumCompareType.等于) where T : IComparable
        {
            object srcValue = null;
            if (srcObj.GetType().GetField(srcVarName) != null)
            {
                srcValue = srcObj.GetType().GetField(srcVarName).GetValue(srcObj);
            }
            else if (srcObj.GetType().GetProperty(srcVarName) != null)
            {
                srcValue = srcObj.GetType().GetProperty(srcVarName).GetValue(srcObj);
            }
            else
            {
                Debug.LogErrorFormat("{0}无法访问{1}，请确认{2}的类型(属性、方法等)", srcObj.GetType(), srcVarName, srcVarName);
                return true;
            }
            return CompareTowValue((IComparable)srcValue, dstValue, compareType);
        }

        private static bool CompareClassValue<T, U>(T srcObj, string srcVarName, U dstValue, EnumCompareType compareType = EnumCompareType.等于) where T : class where U : IComparable
        {
            object srcValue = null;
            if (srcObj.GetType().GetField(srcVarName) != null)
            {
                srcValue = srcObj.GetType().GetField(srcVarName).GetValue(srcObj);
            }
            else if (srcObj.GetType().GetProperty(srcVarName) != null)
            {
                srcValue = srcObj.GetType().GetProperty(srcVarName).GetValue(srcObj);
            }
            else
            {
                Debug.LogErrorFormat("{0}无法访问{1}，请确认{2}的类型(属性、方法等)", srcObj.GetType(), srcVarName, srcVarName);
                return true;
            }
            return CompareTowValue((IComparable)srcValue, dstValue, compareType);
        }

        private static bool CompareStructValue<T, U>(T srcObj, string srcVarName, U dstValue, EnumCompareType compareType = EnumCompareType.等于) where T : struct where U : IComparable
        {
            object srcValue = null;
            if (srcObj.GetType().GetField(srcVarName) != null)
            {
                srcValue = srcObj.GetType().GetField(srcVarName).GetValue(srcObj);
            }
            else if (srcObj.GetType().GetProperty(srcVarName) != null)
            {
                srcValue = srcObj.GetType().GetProperty(srcVarName).GetValue(srcObj);
            }
            else
            {
                Debug.LogErrorFormat("{0}无法访问{1}，请确认{2}的类型(属性、方法等)", srcObj.GetType(), srcVarName, srcVarName);
                return true;
            }

            return CompareTowValue((IComparable)srcValue, dstValue, compareType);
        }

        private static bool CompareSerializedPropertyValue<T, U>(T srcObj, string srcVarName, U dstValue, EnumCompareType compareType = EnumCompareType.等于) where T : SerializedProperty where U : IComparable
        {
            object srcValue = null;
            if (srcObj.GetType().GetField(srcVarName) != null)
            {
                srcValue = srcObj.GetType().GetField(srcVarName).GetValue(srcObj);
            }
            else if (srcObj.GetType().GetProperty(srcVarName) != null)
            {
                srcValue = srcObj.GetType().GetProperty(srcVarName).GetValue(srcObj);
            }
            else
            {
                Debug.LogErrorFormat("{0}无法访问{1}，请确认{2}的类型(属性、方法等)", srcObj.GetType(), srcVarName, srcVarName);
                return true;
            }
            return CompareTowValue((IComparable)srcValue, dstValue, compareType);
        }

        public static bool CompareTowValue<T>(T srcValue, T dstValue, EnumCompareType compareType = EnumCompareType.等于) where T : IComparable
        {
            //应对枚举和自定义IntVal的数据比较器
            if (srcValue.GetType().BaseType == typeof(Enum) && dstValue.GetType().BaseType != typeof(Enum))
            {
                return CompareTowValue(Convert.ToInt32(srcValue), dstValue, compareType);
            }

            if (compareType == EnumCompareType.不等于 && !(srcValue.CompareTo(dstValue) != 0))
            {
                return false;
            }

            if (compareType == EnumCompareType.等于 && !(srcValue.CompareTo(dstValue) == 0))
            {
                return false;
            }

            if (compareType == EnumCompareType.大于等于 && !(srcValue.CompareTo(dstValue) >= 0))
            {
                return false;
            }

            if (compareType == EnumCompareType.小于等于 && !(srcValue.CompareTo(dstValue) <= 0))
            {
                return false;
            }

            if (compareType == EnumCompareType.大于 && !(srcValue.CompareTo(dstValue) > 0))
            {
                return false;
            }

            if (compareType == EnumCompareType.小于 && !(srcValue.CompareTo(dstValue) < 0))
            {
                return false;
            }
            return true;
        }

        public static bool CompareTowValue<T>(int srcValue, T dstValue, EnumCompareType compareType = EnumCompareType.等于) where T : IComparable
        {
            if (compareType == EnumCompareType.不等于 && !(srcValue.CompareTo(dstValue) != 0))
            {
                return false;
            }

            if (compareType == EnumCompareType.等于 && !(srcValue.CompareTo(dstValue) == 0))
            {
                return false;
            }

            if (compareType == EnumCompareType.大于等于 && !(srcValue.CompareTo(dstValue) >= 0))
            {
                return false;
            }

            if (compareType == EnumCompareType.小于等于 && !(srcValue.CompareTo(dstValue) <= 0))
            {
                return false;
            }

            if (compareType == EnumCompareType.大于 && !(srcValue.CompareTo(dstValue) > 0))
            {
                return false;
            }

            if (compareType == EnumCompareType.小于 && !(srcValue.CompareTo(dstValue) < 0))
            {
                return false;
            }
            return true;
        }
    }
}
