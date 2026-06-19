using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public static class ExtensionUtil
{
    static public T AddComp<T>(this GameObject target) where T : Component
    {
        T m = target.GetComponent<T>();
        if (m == null)
            m = target.gameObject.AddComponent<T>();
        return m;
    }
    static public T AddComp<T>(this Component target) where T : Component
    {
        T m = target.GetComponent<T>();
        if (m == null)
            m = target.gameObject.AddComponent<T>();
        return m;
    }
    public static T GetCompInParent<T>(this GameObject go)
    {
        T t = default(T);
        Transform node = go.transform.parent;
        while (node != null)
        {
            t = node.GetComponent<T>();
            if (t != null)
                break;
            node = node.parent;
        }

        return t;
    }
#if UNITY_EDITOR
    static DrivenRectTransformTracker tracker = new DrivenRectTransformTracker();
#endif

    public static void AddDriven(this RectTransform rect, DrivenTransformProperties drivenProperties)
    {
#if UNITY_EDITOR
        if (rect.gameObject != UnityEditor.Selection.activeGameObject)
            return;
        tracker.Clear();
        tracker.Add(rect.gameObject, rect, drivenProperties);
#endif
    }
    public static void TransReset(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    public static CanvasRenderer Caret(this InputField input)
    {
        CanvasRenderer rend = input.transform.GetChild(0).GetComponent<CanvasRenderer>();
        if (rend != null && rend.gameObject.name == input.transform.name + " Input Caret")
        {
            return rend;
        }
        return null;
    }
    public static void SetMouseEnable(this UIBehaviour ts, bool b)
    {
        CanvasGroup canvasGroup = ts.gameObject.AddComp<CanvasGroup>();
        canvasGroup.interactable = b;
        canvasGroup.blocksRaycasts = b;
    }
    public static void SetAlpha(this UIBehaviour ts, float alpha)
    {
        CanvasGroup canvasGroup = ts.gameObject.AddComp<CanvasGroup>();
        canvasGroup.alpha = alpha;
        //if (ts != null)
        //{
        //    CanvasRenderer[] crs = ts.GetComponentsInChildren<CanvasRenderer>();
        //    foreach (var cr in crs)
        //    {
        //        cr.SetAlpha(alpha);
        //    }
        //}
    }
    public static void CrossFadeAlpha(this UIBehaviour ts, float alpha, float time, bool ignoreTimeScale = true)
    {
        if (ts != null)
        {
            //CanvasRenderer aa = Content.GetComponent<CanvasRenderer>();
            //aa.SetAlpha(alpha);
            Graphic[] crs = ts.GetComponentsInChildren<Graphic>();
            foreach (var cr in crs)
            {
                cr.CrossFadeAlpha(alpha, time, ignoreTimeScale);
            }
        }
    }
    public static float GetHeight(this GridLayoutGroup grid, int count)
    {
        if (grid != null && count > 0)
        {
            if (grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
                count = grid.constraintCount;
            if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                count = Mathf.CeilToInt((float)count / grid.constraintCount);
            return (grid.cellSize.y + grid.spacing.y) * count + grid.padding.top - grid.spacing.y;
        }
        return 0f;
    }
    public static float GetWidth(this GridLayoutGroup grid, int count)
    {
        if (grid != null && count > 0)
        {
            if (grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
                count = Mathf.CeilToInt((float)count / grid.constraintCount);
            if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                count = grid.constraintCount;
            return (grid.cellSize.x + grid.spacing.x) * count + grid.padding.left - grid.spacing.x;
        }
        return 0f;
    }
    public static Vector2 GetPosByIndex(this GridLayoutGroup Grid, int index)
    {
        Vector2 vec = Vector2.zero;
        if (Grid != null)
        {
            int xOff = index;
            int yOff = index;
            if (Grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
            {
                xOff = Mathf.FloorToInt((float)index / Grid.constraintCount);
                yOff = index % Grid.constraintCount;
            }
            else if (Grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                xOff = index % Grid.constraintCount;
                yOff = Mathf.FloorToInt((float)index / Grid.constraintCount);
            }
            vec.x = Grid.cellSize.x * (xOff + 0.5f) + Grid.spacing.x * xOff + Grid.padding.left;// - Grid.spacing.x;
            vec.y = -(Grid.cellSize.y * (yOff + 0.5f) + Grid.spacing.y * yOff) - Grid.padding.top;// - Grid.spacing.y;
        }
        return vec;
    }
    public static List<T> GetPageList<T>(this List<T> list, int _curPage, int _pageNum, bool autoAdd = true)
    {
        _curPage = ((_curPage >= 1) ? _curPage : 1);
        int index = (_curPage - 1) * _pageNum;
        return list.GetPageListByIndex(index, _pageNum, autoAdd);
    }

    public static List<T> GetPageListByIndex<T>(this List<T> list, int _index, int _pageNum, bool autoAdd = true)
    {
        int num = _index;
        List<T> list2 = new List<T>();
        if (!autoAdd)
        {
            num = ((num + _pageNum <= list.Count || list.Count < _pageNum) ? num : (list.Count - _pageNum));
        }
        for (int i = num; i < list.Count; i++)
        {
            if (list2.Count >= _pageNum)
            {
                break;
            }
            list2.Add(list[i]);
        }
        int count = list2.Count;
        for (int j = count; j < _pageNum; j++)
        {
            list2.Add(default(T));
        }
        return list2;
    }

    public static int GetTotalPage(this ICollection list, int num)
    {
        if (list == null)
        {
            return 1;
        }
        int num2 = Mathf.CeilToInt((float)list.Count / (float)num);
        return (num2 >= 1) ? num2 : 1;
    }

    /// <summary>
    /// Array添加
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    /// <param name="array">Array</param>
    /// <param name="item">需要添加项</param>
    /// <returns>返回新的Array</returns>
    public static T[] Add<T>(this T[] array, T item)
    {
        int _count = array.Length;
        Array.Resize<T>(ref array, _count + 1);
        array[_count] = item;
        return array;
    }
    /// <summary>
    /// Array添加
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    /// <param name="sourceArray">Array</param>
    /// <param name="addArray">Array</param>
    /// <returns>返回新的Array</returns>
    public static T[] AddRange<T>(this T[] sourceArray, T[] addArray)
    {
        int _count = sourceArray.Length;
        int _addCount = addArray.Length;
        Array.Resize<T>(ref sourceArray, _count + _addCount);
        //foreach (T t in addArray)
        //{
        //  sourceArray[_count] = t;
        //  _count++;
        //}
        addArray.CopyTo(sourceArray, _count);
        return sourceArray;
    }

    public static T[] Shift<T>(this T[] array, T item)
    {
        int _count = array.Length;
        Array.Resize<T>(ref array, _count + 1);
        for (int i = _count - 1; i >= 0; i--)
        {
            array[i + 1] = array[i];
        }
        array[0] = item;

        return array;
    }
}
