///--------------------------------------------------------------------
/// 文件名   :   UITabMenu.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/24 17:01:34
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using StarProject.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
public class StarUITabMenu : MonoBehaviour
{
    public delegate void OnTabChange(int index);



    private Dictionary<int, StarUITabItem> UITabItems = new Dictionary<int, StarUITabItem>();
    public int CurrentIndex { get; private set; }

    private OnTabChange onTabChange;

    [Button("初始化")]
    public void InitMenuByEditor()
    {

        StarUITabItem[] tabItems = transform.GetComponentsInChildren<StarUITabItem>();
        if (tabItems != null && tabItems.Length > 0)
        {
            for (int i = 0; i < tabItems.Length; i++)
            {
                tabItems[i].InitByEditor(i + 1);
            }
        }

    }

    public int GetCount()
    {
        if (UITabItems == null)
        {
            return 0;
        }
        return UITabItems.Count;
    }

    public void Init(int index = 0, OnTabChange tabChange = null)
    {
        int CurIndex = index;   
        bool isBeginnerMap = false;
        if (GameManager.Instance.GetCurMapId() == 10004)
        {
            isBeginnerMap = true;
            CurIndex = 1;
        }
        onTabChange = tabChange;
        UITabItems.Clear();
        StarUITabItem[] tabItems = transform.GetComponentsInChildren<StarUITabItem>();
        if (tabItems != null && tabItems.Length > 0)
        {
            for (int i = 0; i < tabItems.Length; i++)
            {
                if (isBeginnerMap && (tabItems[i].gameObject.name == "Game"|| tabItems[i].gameObject.name == "Battle")) 
                {
                    tabItems[i].gameObject.SetActive(false);
                    continue;
                }
                tabItems[i].Init(OnClickButtonHandler);
                if (!UITabItems.ContainsKey(tabItems[i].GetIndex()))
                {
                    UITabItems.Add(tabItems[i].GetIndex(), tabItems[i]);
                }
                else
                {
                    Debug.LogError($"存在相同的Tab Index {transform.name} /{tabItems[i].transform.name}  ");
                }
            }
        }
        OnSelected(CurIndex);
        CurrentIndex = CurIndex;
    }

    private void OnSelected(int Index)
    {
        if (UITabItems != null)
        {
            foreach (var item in UITabItems)
            {
                item.Value.OnSelected(item.Key == Index);
            }
        }
    }

    private void OnClickButtonHandler(int index)
    {
        if (CurrentIndex != index)
        {
            onTabChange?.Invoke(index);
        }
    }



    public void Check(int index)
    {
        CurrentIndex = index;
        OnSelected(CurrentIndex);
    }
}
