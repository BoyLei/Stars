using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using System;

//为了监听第一次点击进入事件
public class InputFieldEx:InputField
{
    bool editorend = true;

    public Action activeAction;

    public void LeaveEditor()
    {
        editorend = true;
    }

    public override void OnPointerClick(PointerEventData data)
    {
        base.OnPointerClick(data);
        if(editorend)
        {
            activeAction();
        }
        editorend = false;
    }
}