///--------------------------------------------------------------------
/// 文件名   :   TaskScrollRectChange
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/09/10 15:29:23
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using StarProject;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class TaskScrollRectChange : MonoBehaviour
{
    ScrollRect scrollRect;
    private Vector2 previousScrollPosition;
    private bool isDragging = false;
    void Start()
    {
        scrollRect=GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(OnDragScrollRect);
        previousScrollPosition = scrollRect.normalizedPosition;
    }

    public void OnDragScrollRect(Vector2 value)
    {
        if (!isDragging)
        {
            GlobalEvent.TaskScrollRectChange.Invoke(0);
            isDragging = true;
        }
        else
        {
            if (Vector2.Distance(previousScrollPosition, value) < 0.01f)
            {
                // 滑动结束
                isDragging = false;
                // 在这里执行滑动结束后的逻辑
            }
        }
        previousScrollPosition = value;

    }
}
