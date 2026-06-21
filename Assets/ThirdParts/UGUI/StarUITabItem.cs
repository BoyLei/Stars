///--------------------------------------------------------------------
/// 文件名   :   StarUITabItem.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/24 17:07:21
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[XLua.LuaCallCSharp]
public class StarUITabItem : MonoBehaviour
{
    [SerializeField]
    [LabelText("索引")]
    private int Index;

    [SerializeField]
    [LabelText("选中标记")]
    public Transform select;
    [SerializeField]
    [LabelText("未选中标记")]
    public Transform unselect;

    [SerializeField]
    [LabelText("额外选中")]
    public Transform ExtendSelect;

    [SerializeField]
    [LabelText("按钮")]
    private Button button;

    [SerializeField]
    [LabelText("节点自己是否是按钮")]
    public bool IsSelfBtn = false;

    private StarUITabMenu.OnTabChange onTabCallBack;

    [SerializeField]
    [LabelText("换色组件")]
    private Graphic ChangeGraphic;

    public string SelectColor;
    private Color Selectcolor;

    public string NormalColor;
    private Color Normalcolor;

    public int GetIndex() { return Index; }

    public void Init(StarUITabMenu.OnTabChange tabChange)
    {
        ColorUtility.TryParseHtmlString(NormalColor, out Normalcolor);
        ColorUtility.TryParseHtmlString(SelectColor, out Selectcolor);

        onTabCallBack = tabChange;
        button.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        onTabCallBack?.Invoke(Index);
    }

    public void OnSelected(bool sel)
    {
        if (ChangeGraphic != null)
        {
            ChangeGraphic.color = sel ? Selectcolor : Normalcolor;
        }
        // 选中时需要显示的 节点
        if (select != null)
        {
            select.gameObject.SetActive(sel);
        }
        // 未选中时需要显示的 节点
        if (unselect != null)
        {
            unselect.gameObject.SetActive(!sel);
        }

        if (ExtendSelect != null)
        {
            ExtendSelect.gameObject.SetActive(sel);
        }


    }

    public void OnDestroy()
    {
        onTabCallBack = null;
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    public void InitByEditor(int index)
    {

        this.Index = index;
        if (!IsSelfBtn)
        {
            button = transform.GetComponentInChildren<Button>();
        }
        else
        {
            button = transform.GetComponent<Button>();
        }
        if (select == null)
        {
            select = transform.Find("select");
        }
        if (unselect == null)
        {
            unselect = transform.Find("unselect");
        }

    }


}
