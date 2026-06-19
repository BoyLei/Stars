///--------------------------------------------------------------------
/// 文件名   :   DelayHide.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/01/04 13:37:38
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HideMonoScript]
[InfoBox("延迟隐藏自身")]
public class DelayHide : ClientSceneEvent
{

    [LabelText("延迟时间")]
    public float DelayTime = 1;

    private float mRunnigTime;

    public override void OnEnter()
    {
        mRunnigTime = DelayTime;
    }

    public override void OnExit()
    {
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        mRunnigTime -= Time.deltaTime;
        if (mRunnigTime <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}

