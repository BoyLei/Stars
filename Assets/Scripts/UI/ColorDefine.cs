///--------------------------------------------------------------------
/// 文件名   :   ColorDefine.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/18 10:45:15
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yoka.UnityString.Core;

[XLua.LuaCallCSharp]
[CreateAssetMenu(menuName = "Assets/Create ColorDefine")]
[System.Serializable]
[HideMonoScript]
public class ColorDefine : ScriptableObject
{

    private Dictionary<string, ColorItem> items = new Dictionary<string, ColorItem>();


    [TableList(ShowIndexLabels =true,CellPadding =3,DrawScrollView =true, AlwaysExpanded=true)]
    [ShowInInspector]
    [LabelText("颜色库")]
    [SerializeField]
    private List<ColorItem> list = new List<ColorItem>();

    public Dictionary<int, string> qualitycolor = new Dictionary<int, string>()
    {
        {1,"aaaaaa"},
        {2,"78A096"},
        {3,"7D9BCD"},
        {4,"CDA4DE"},
        {5,"e1af8c"},
    };
    private static ColorDefine define;
    public static ColorDefine Instance
    {
        get
        {
            if (define == null)
            {
                define = Resources.Load<ColorDefine>("ColorDefine");
                define.Init();
            }
            return define;
        }
    }

    private void Init()
    {
        items.Clear();
        foreach (var item in list)
        {
            if(!items.ContainsKey(item.Key))
            {
                items.Add(item.Key, item);
            }
        }
    }

    public Color GetColor(string key)
    {
        if(items.ContainsKey(key))
        {
            return items[key].color;
        }
        return Color.white;
    }
    
    
    /// <summary>
    /// 通过品质获取颜色
    /// </summary>
    /// <param name="quality"></param>
    /// <returns></returns>
    public string GetColorByQuality(int quality)
    {
        if (qualitycolor.ContainsKey(quality))
        {
            return  qualitycolor[quality];
        }
        return "aaaaaa";
    }

    /// <summary>
    /// 通过品质设置字符串颜色
    /// </summary>
    /// <param name="content"></param>
    /// <param name="quality"></param>
    /// <returns></returns>
    public string SetTextColorByQuality(string content, int quality)
    {
        if (qualitycolor.ContainsKey(quality))
        {
            using (UString.Block())
            {
                 return  UString.Format("<color=#{0}>{1}</color>", qualitycolor[quality], content).Clone();
            }
        }
        return content;
    }

    public Color GetHtmlColor(string key)
    {
        var color=Color.white;
        ColorUtility.TryParseHtmlString(key, out color);
        return color;
    }
}

[System.Serializable]
public class ColorItem
{
    [LabelText("键值")]
    public string Key;

    [LabelText("备注")]
    public string Desc;

    [LabelText("颜色")]
    public Color color;
}