///--------------------------------------------------------------------
/// 文件名   :   HideGameObjectOnInput.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/01/10 18:01:03
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProject.Service.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideGameObjectOnInput : ClientSceneEvent
{

    public override void OnEnter()
    {
        InputManager.Instance.OnVirtualInput += OnVKey;
    }


    public override void OnExit()
    {
        InputManager.Instance.OnVirtualInput -= OnVKey;
        gameObject.SetActive(true);

    }

    private void OnVKey(int arg1, float arg2)
    {
        gameObject.SetActive(false);
    }
}
