using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

[XLua.LuaCallCSharp]
public class ScrollViewNevigation : MonoBehaviour
{

    private ScrollRect scrollRect;
    private RectTransform scrollRectTransform;
    private RectTransform viewport;
    private RectTransform content;

    public TweenerCore<Vector2, Vector2, VectorOptions> dotweener;

    public TweenerCore<Vector3, Vector3, VectorOptions> dotweenerV3;

    // Use this for initialization
    void Start()
    {

        Init();
        //Nevigate(content.GetChild(45).GetComponent<RectTransform>());
    }


    private void Init()
    {
        if (scrollRect == null)
        {
            scrollRect = this.GetComponent<ScrollRect>();
        }

        if (scrollRectTransform == null)
        {
            scrollRectTransform = this.GetComponent<RectTransform>();
        }

        if (viewport == null)
        {
            viewport = this.transform.Find("Viewport").GetComponent<RectTransform>();
        }

        if (content == null)
        {
            Transform tr = this.transform.Find("Viewport/Content");
            if (tr != null)
            {
                content = tr.GetComponent<RectTransform>();
            }
            else 
            {
                content = this.transform.Find("Viewport/ContentRoot/Content").GetComponent<RectTransform>();
            }
        }
        //if (content == null)
        //{
        //    content = this.transform.Find("Viewport/ContentRoot/Content").GetComponent<RectTransform>();
        //}
    }

    #region 方法1
    public void Nevigate(RectTransform item, bool isTween = true)
    {
        if (scrollRect == null)
        {
            Init();
        }
        if (dotweener != null)
        {
            dotweener.Kill();
        }
        // InverseTransformPoint: Transforms position from world space to local space, 和TransformPoint左右相反
        // 这步的意义是把 item、viewport的localPosition转换到同一个父节点下，才能计算出需要移动的差值
        // 看图1
        Vector3 itemCurrentLocalPostion = scrollRectTransform.InverseTransformPoint(ConvertLocalPosToWorldPos(item));
        Vector3 itemTargetLocalPos = scrollRectTransform.InverseTransformPoint(ConvertLocalPosToWorldPos(viewport));
        // 计算需要移动的距离
        Vector3 diff = itemTargetLocalPos - itemCurrentLocalPostion;

        // 当第一个item处于viewport最上时 verticalNormalizedPosition = 0，处于最左时 horizontalNormalizedPosition = 0，看图2

        // 计算需要移动的距离占的比，即需要移动的距离占可移动长度的百分比，看图3
        // 如果你的 viewport 中只能显示一个item的话，这样就行了，但是超过了1个的话需要计算偏移：
        // 以数值方向为例，当verticalNormalizedPosition = 0，第一个 item 距离 viewport的中心位置其实有一段距离的，
        // 所以要减去这段距离，diff.y - offset，再计算 newNormalizedPosition，看图4
        var newNormalizedPosition = new Vector2(
            diff.x / (content.rect.width - viewport.rect.width),
            diff.y / (content.rect.height - viewport.rect.height)
            );
        // 当时 normalizedPosition - 需要移动的占比newNormalizedPosition，得到最终的位置normalizedPosition
        newNormalizedPosition = scrollRect.normalizedPosition - newNormalizedPosition;

        newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
        newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);
        // 也可以只设置水平方向 horizontalNormalizedPosition，或竖直方向 verticalNormalizedPosition
        // 设置 normalizedPosition 等于同时设置 horizontalNormalizedPosition， verticalNormalizedPosition
        // normalizedPosition.x == horizontalNormalizedPosition
        // normalizedPosition.y == verticalNormalizedPosition
        // normalizedPosition == new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition)
        //scrollRect.GetComponent<ScrollRect>().normalizedPosition = newNormalizedPosition;


        if (isTween)
        {
            dotweener = DOTween.To(() => scrollRect.normalizedPosition, x => scrollRect.normalizedPosition = x, newNormalizedPosition, 0.8f);
        }
        else
        {
            scrollRect.normalizedPosition = newNormalizedPosition;
        }

        // 如果有分类功能，比如点击type1,scrollview中显示type1中的数据,点击type2，scrollview中显示type2中的数据，需要在给 
        // normalizedPosition 赋值前先赋值 (0, 0)，否则最后一行代码获取的 normalizedPosition 还是上一类型 type 中的 
        // normalizedPosition，导致出bug
    }

    // 这个方法的作用是消除Pivot数值的影响，Pivot不是(0.5，0.5)时，最后所计算出来的结果会有误差
    // 如果是(0.5, 0.5)的话可以直接返回 target.parent.TransformPoint(localPosition)
    private Vector3 ConvertLocalPosToWorldPos(RectTransform target)
    {
        var pivotOffset = new Vector3(
            (0.5f - target.pivot.x) * target.rect.size.x,
            (0.5f - target.pivot.y) * target.rect.size.y,
            0f);

        var localPosition = target.localPosition + pivotOffset;
        // TransformPoint: Transforms position from local space to world space
        return target.parent.TransformPoint(localPosition);
    }
    #endregion



    #region 方法二

    public void CenterPoint(RectTransform item, bool isTween = true)
    {
        if (scrollRect == null)
        {
            Init();
        }
        if (dotweenerV3 != null)
        {
            dotweenerV3.Kill();
        }
        Vector3 targetPosition = scrollRectTransform.InverseTransformPoint(Clear_Pivot_Offset(item));
        Vector3 viewportPosition = scrollRectTransform.InverseTransformPoint(Clear_Pivot_Offset(content));
        Vector3 distance_vec = viewportPosition - targetPosition;
        Vector3 newPos = viewport.transform.position + distance_vec;

        Vector3 scrollOffset = new(
            scrollRect.horizontal ? newPos.x : content.position.x,
            scrollRect.vertical ? newPos.y : content.position.y,
            0
            );
        if (isTween)
        {
            Vector3 startPos = content.position;
            dotweenerV3 = DOTween.To(() => startPos, changeVector =>
            {
                content.position = changeVector;
            }, scrollOffset, 0.8f);
        }
        else
        {
            content.position = scrollOffset;
        }

        //var height_Delta = item.rect.height - content.rect.height;
        //var width_Delta = item.rect.width - content.rect.width;
        //var ratio_x = width_Delta != 0 ? distance_vec.x / width_Delta : 0;
        //var ratio_y = height_Delta != 0 ? distance_vec.y / height_Delta : 0;
        //var ratioDistance = new Vector2(ratio_x, ratio_y);
        //var newPosition = scrollRect.normalizedPosition - ratioDistance;
        //var scrollOffset = new Vector2(Mathf.Clamp01(newPosition.x), Mathf.Clamp01(newPosition.y));

        //scrollRect.normalizedPosition = scrollOffset;


        //Vector3 sCenter = scrollRect.transform.position;
        //Debug.Log("Center Pos: " + sCenter);

        //Vector3 itemCenterPos = item.transform.position;
        //Debug.Log("Item Center Pos: " + itemCenterPos);

        //Vector3 difference = sCenter - itemCenterPos;
        //Vector3 newPos = viewport.transform.position + difference;

        //viewport.transform.DOMoveY(newPos.y, 1f);


        //dotweener = DOTween.To(
        //() => scrollRect.normalizedPosition,
        //pos => scrollRect.normalizedPosition = pos,
        //scrollOffset,
        //0.8f
        //);
        //dotweener = DOTween.To(() => scrollRect.normalizedPosition, x => scrollRect.normalizedPosition = x, pos, 0.8f);
    }

    private Vector3 Clear_Pivot_Offset(RectTransform rec)
    {
        var offset = new Vector3(
            (0.5f - rec.pivot.x) * rec.rect.width,
            (0.5f - rec.pivot.y) * rec.rect.height,
            0.0f
        );
        var newPosition = rec.localPosition + offset;
        return rec.parent.TransformPoint(newPosition);
    }
    #endregion
}