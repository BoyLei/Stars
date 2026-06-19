///--------------------------------------------------------------------
/// 文件名   :   DelayDestroy.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/01/04 13:08:45
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HideMonoScript]
[InfoBox("延迟销毁自身")]
public class DelayDestroy : MonoBehaviour
{

    [LabelText("延迟时间")]
    public float DelayTime = 1;


    // Update is called once per frame
    void Update()
    {
        DelayTime -= Time.deltaTime;
        if (DelayTime <= 0)
        {
             Destroy(this.gameObject);
        }
    }
}
