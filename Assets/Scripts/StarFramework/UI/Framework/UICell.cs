///--------------------------------------------------------------------
/// 文件名   :   UICell.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/25 17:25:14
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class UICell : MonoBehaviour
{

    private RectTransform rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null)
            {
                rectTransform = transform.GetComponent<RectTransform>();
            }
            return rectTransform;
        }
    }
    protected virtual void Awake()
    {

    }

    public virtual void UpdateContent(object arg)
    {
    }


    public virtual Vector2 SetItemSize()
    {
        return Vector2.one;
    }
}
