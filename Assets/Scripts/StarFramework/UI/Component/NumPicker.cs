using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 条目选择器
/// 支持循环滑动
/// </summary>
[XLua.LuaCallCSharp]
public class NumPicker : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject _itemObj;
    public Transform _contentTrans;
    public int _itemNum = 5;
    int _minNum = 2;
    int _maxNum = 99;
    int _interval = 5;
    List<int> _allNums = new List<int>();
    int curSelNum = 0;
    float _itemHeight;
    Vector3 oldDragPos;
    bool _change = false;

    void Awake()
    {
        _itemObj.SetActive(false);
        _itemHeight = _itemObj.GetComponent<RectTransform>().sizeDelta.y;
    }

    // void Start()
    // {
    //     Init(0, 100, _itemNum);
    // }

    public void Init(int min, int max, int inter, bool change = false)
    {
        _minNum = min;
        _maxNum = max;
        _interval = inter;
        _change = change;

        _allNums.Clear();
        if (_minNum >= _maxNum)
        {
            Debug.LogError("min is bigger than max!!!");
            return;
        }
        int newMin = _minNum;
        int newMax = _maxNum;
        if (newMin % _interval != 0)
        {
            _allNums.Add(newMin);
            newMin = (_minNum / _interval + 1) * _interval;
        }
        if (newMax % _interval != 0)
        {
            newMax = (newMax / _interval) * _interval;
        }
        for (int i = newMin; i <= newMax; i += _interval)
        {
            _allNums.Add(i);
        }
        if (_maxNum % _interval != 0)
        {
            _allNums.Add(_maxNum);
        }

        for (int i = 0; i < _itemNum; i++)
        {
            SpawnItem();
        }
        if (_change)
        {
            RefreshDateList2();
        }
        else
        {
            RefreshDateList();
        }
    }

    GameObject SpawnItem()
    {
        GameObject _item = Instantiate(_itemObj);
        _item.SetActive(true);
        _item.transform.SetParent(_contentTrans);
        _item.transform.localScale = new Vector3(1, 1, 1);
        _item.transform.localEulerAngles = Vector3.zero;
        var pos = _item.transform.localPosition;
        _item.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        return _item;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        oldDragPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateSelectNum(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _contentTrans.localPosition = Vector3.zero;
    }

    void UpdateSelectNum(PointerEventData eventData)
    {
        if (Mathf.Abs(eventData.position.y - oldDragPos.y) >= _itemHeight * 0.5f)
        {
            curSelNum += eventData.position.y > oldDragPos.y ? 1 : -1;
            curSelNum = SetInRange(curSelNum);

            oldDragPos = eventData.position;

            if (_change)
            {
                RefreshDateList2();
            }
            else
            {
                RefreshDateList();
            }
        }
    }

    public void RefreshDateList()
    {
        int index = 0;
        int itemCount = 0;
        for (int i = -_itemNum / 2; i <= -1; i++)
        {
            index = curSelNum + i;
            index = SetInRange(index);

            var tex1 = _contentTrans.GetChild(itemCount).GetComponentInChildren<Text>();
            tex1.text = _allNums[index].ToString();
            tex1.color = new Color(0x77/255.0f, 0x77/255.0f, 0x77/255.0f);

            itemCount++;
        }
        _contentTrans.GetChild(itemCount).GetComponentInChildren<Text>().text = _allNums[curSelNum].ToString();
        _contentTrans.GetChild(itemCount).GetComponentInChildren<Text>().color = new Color(0x3A/255.0f, 0x39/255.0f, 0x37/255.0f);
        itemCount++;
        for (int i = 1; i <= _itemNum / 2; i++)
        {
            index = curSelNum + i;
            index = SetInRange(index);

            var tex1 = _contentTrans.GetChild(itemCount).GetComponentInChildren<Text>();
            tex1.text = _allNums[index].ToString();
            tex1.color = new Color(0x77/255.0f, 0x77/255.0f, 0x77/255.0f);

            itemCount++;
        }
    }

    public void RefreshDateList2()
    {
        int index = 0;
        int itemCount = 0;
        for (int i = -_itemNum / 2; i <= -1; i++)
        {
            index = curSelNum + i;
            index = SetInRange(index);

            var data = StarProject.Service.LocalData.LocalDataManager.Instance.GetAdvGradeExpDataCell(_allNums[index]);
            _contentTrans.GetChild(itemCount).GetComponentInChildren<Text>().text = data.AdvClass + GetRomaNum((data.GetAdvOrder()));
            _contentTrans.GetChild(itemCount).GetComponentInChildren<Text>().color = new Color(0x77/255.0f, 0x77/255.0f, 0x77/255.0f);
            itemCount++;
        }
        var data2 = StarProject.Service.LocalData.LocalDataManager.Instance.GetAdvGradeExpDataCell(_allNums[curSelNum]);
        _contentTrans.GetChild(itemCount).GetComponentInChildren<Text>().text = data2.AdvClass + GetRomaNum((data2.GetAdvOrder()));
        _contentTrans.GetChild(itemCount).GetComponentInChildren<Text>().color = new Color(0x3A/255.0f, 0x39/255.0f, 0x37/255.0f);
        itemCount++;
        for (int i = 1; i <= _itemNum / 2; i++)
        {
            index = curSelNum + i;
            index = SetInRange(index);
            var data = StarProject.Service.LocalData.LocalDataManager.Instance.GetAdvGradeExpDataCell(_allNums[index]);
            _contentTrans.GetChild(itemCount).GetComponentInChildren<Text>().text = data.AdvClass + GetRomaNum((data.GetAdvOrder()));
            _contentTrans.GetChild(itemCount).GetComponentInChildren<Text>().color = new Color(0x77/255.0f, 0x77/255.0f, 0x77/255.0f);
            itemCount++;
        }
    }

    string GetRomaNum(int type)
    {
        if (type == 1)
            return "Ⅰ";
        else if (type == 2)
            return "Ⅱ";
        else if (type == 3)
            return "Ⅲ";
        else if (type == 4)
            return "Ⅳ";
        else if (type == 5)
            return "Ⅴ";
        else if (type == 6)
            return "Ⅵ";
        else if (type == 7)
            return "Ⅶ";
        else if (type == 8)
            return "Ⅷ";
        else if (type == 9)
            return "Ⅸ";
        else if (type == 10)
            return "Ⅹ";

        return "";
    }

    public int SetInRange(int index)
    {
        int ret = index;
        if (index < 0)
        {
            ret = _allNums.Count + index;
        }
        if (index >= _allNums.Count)
        {
            ret = index - _allNums.Count;
        }
        ret = Math.Clamp(ret, 0, _allNums.Count - 1);
        return ret;
    }

    public int GetCurSel()
    {
        return _allNums[curSelNum];
    }
}