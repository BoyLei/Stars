using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnFreshGrid(Transform transform, int index);

public delegate void OnQueryData(bool isEnd);



public class SuperScrollViewController : MonoBehaviour
{

    private UIVertical mList;
    private UIHorizontal mList2;
    private List<int> intlist = new List<int>();

    private OnFreshGrid _refesh;

    private OnQueryData _query;

    private RectTransform rect_grid;

    public TweenerCore<Vector2, Vector2, VectorOptions> dotweener;

    private Vector2 tempV2 = Vector2.zero;

    private bool NeedQuery = false;

    public int PageCount
    {
        get
        {
            if(mList!=null)
            {
                int row = 1;
                if (mList.rowCount > 0)
                {
                    row = mList.rowCount;
                }
                
                int col = 1;
                if (mList.columnCount > 0)
                {
                    col = mList.columnCount;
                }
                return row*col;
            }
            
            if(mList2!=null)
            {
                int row = 1;
                if (mList2.rowCount > 0)
                {
                    row = mList2.rowCount;
                }
                
                int col = 1;
                if (mList2.columnCount > 0)
                {
                    col = mList2.columnCount;
                }
                return row*col;
            }
            return 0;
        }
    }

    public int TotalCount
    {
        get
        {
            if (mList != null)
            {
                return mList.TotalCount;
            }
            if (mList2 != null)
            {
                return mList2.TotalCount;
            }
            return 0;
        }
    }
    public void SetRefeshDelegate(OnFreshGrid _refesh)
    {
        this._refesh = _refesh;
    }

    public void BinderQueryDataDelegate(OnQueryData queryData)
    {
        _query = queryData;
        NeedQuery = true;
    }

    public void NewScroll(GameObject _perfab, int _count, OnFreshGrid _lunc)
    {
        rect_grid= transform.Find("Mask/List").GetComponent<RectTransform>();

        this.SetRefeshDelegate(_lunc);
        if (transform.GetComponent<UIVertical>() != null)
        {
            mList = transform.GetComponent<UIVertical>();
            mList.InitData(_perfab);
            mList.InitList(_count, (item, index) =>
            {
                //子物体属性赋值
                if (this._refesh != null)
                {
                    this._refesh.Invoke(item, index);
                }
            },BoundCheck);
        }

        if (transform.GetComponent<UIHorizontal>() != null)
        {
            mList2 = transform.GetComponent<UIHorizontal>();
            mList2.InitData(_perfab);
            mList2.InitList(_count, (item, index) =>
            {
                //子物体属性赋值
                if (this._refesh != null)
                {
                    this._refesh.Invoke(item, index);
                }
            },BoundCheck);
        }
    }

    private float delayTime = 0;
    
    private void BoundCheck(float vaule)
    {
        if (!NeedQuery)
        {
            return;
        }
        
        if (Time.time - delayTime < 0.33f)
        {
            return;
        }
        
        if (vaule <= 0.2f)
        {
            _query?.Invoke(true);
            delayTime = Time.time;
            return;
        }
        
        if (vaule >= 0.8f)
        {
            _query?.Invoke(false);
            delayTime = Time.time;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_count">总数</param>
    /// <param name="_needScrollToTop">是否置顶</param>
    /// <param name="startIdx">开始的下标</param>
    public void ContinueScroll(int _count, bool _needScrollToTop = true, int startIdx = 0)
    {
        if (dotweener != null)
        {
            dotweener.Kill();
        }
        // 纵向
        {
            if (transform.GetComponent<UIVertical>() != null)
            {
                mList = transform.GetComponent<UIVertical>();
                if (_needScrollToTop)
                {
                    SetVector2(rect_grid.anchoredPosition.x, 0);
                    rect_grid.anchoredPosition = tempV2;
                }
                else
                {
                    if (startIdx > 0)
                    {
                        SetVector2(rect_grid.anchoredPosition.x, startIdx * mList.itemHeight + startIdx * mList.offsetY);
                        rect_grid.anchoredPosition = tempV2;
                    }
                }
                mList.Refresh(_count, (item, index) =>
                {
                    //子物体属性赋值
                    if (this._refesh != null)
                    {
                        this._refesh.Invoke(item, index);
                    }
                });
            }
        }
        // 横向
        {
            if (transform.GetComponent<UIHorizontal>() != null)
            {
                mList2 = transform.GetComponent<UIHorizontal>();
                if (_needScrollToTop)
                {
                    SetVector2(0, rect_grid.anchoredPosition.y);
                    rect_grid.anchoredPosition = tempV2;
                }
                else
                {
                    if (startIdx > 0)
                    {
                        SetVector2(-(startIdx * mList2.itemWidth + startIdx * mList2.offsetX), rect_grid.anchoredPosition.y);
                        rect_grid.anchoredPosition = tempV2;
                    }
                }
                mList2.Refresh(_count, (item, index) =>
                {
                    //子物体属性赋值
                    if (this._refesh != null)
                    {
                        this._refesh.Invoke(item, index);
                    }
                });
            }
        }
    }
    // 自动锁定到下标
    public void NevigateByIdx(int startIdx)
    {
        if (dotweener != null)
        {
            dotweener.Kill();
        }
        if (transform.GetComponent<UIHorizontal>() != null && mList2 != null)
        {
            SetVector2(-(startIdx * mList2.itemWidth + startIdx * mList2.offsetX), rect_grid.anchoredPosition.y);
            dotweener = DOTween.To(() => rect_grid.anchoredPosition, x => rect_grid.anchoredPosition = x, tempV2, 0.8f);
        }
        else if (transform.GetComponent<UIVertical>() != null && mList != null)
        {
            SetVector2(rect_grid.anchoredPosition.x, startIdx * mList.itemHeight + startIdx * mList.offsetY);
            dotweener = DOTween.To(() => rect_grid.anchoredPosition, y => rect_grid.anchoredPosition = y, tempV2, 0.8f);
        }        
    }

    private void SetVector2(float x, float y)
    {
        tempV2.x = x;
        tempV2.y = y;
    }

    //private void OnDestroy()
    //{
    //    //清理事件
    //    if (this._refesh != null)
    //    {
    //        //this._refesh.Dispose();
    //    }
    //}
}
