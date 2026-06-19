///--------------------------------------------------------------------
/// 文件名   :   ClientSceneManager.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/01/11 17:58:40
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSceneManager : MonoBehaviour
{
    public List<ClientSceneEvent> Events = new List<ClientSceneEvent>();


    private void OnEnable()
    {
        foreach (var item in Events)
        {
            item?.OnEnter();
        }
    }

    private void OnDisable()
    {
        foreach (var item in Events)
        {
            item?.OnExit();
        }

    }
}
