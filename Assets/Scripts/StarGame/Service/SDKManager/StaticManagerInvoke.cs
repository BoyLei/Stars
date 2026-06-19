using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SGF.Unity;
using System.Diagnostics;

public class StaticManagerInvoke : MonoBehaviour
{
    private void Awake()
    {
        UnityEngine.Debug.Log(StarProject.Service.SDK.SDKManager.Instance + "启动成功");
    }
}
