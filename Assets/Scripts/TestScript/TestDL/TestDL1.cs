using UnityEngine;
using System.Collections.Generic;
using SGF.Unity;
using System.Collections;
using System;


public class TestDL1 : MonoBehaviour
{

    private List<int> lists = new();


    void Start()
    {
        Debug.LogError($"[Test] start");
    }

    private void OnDestroy()
    {
        Debug.LogError($"[Test] OnDestroy");
    }



    public void TestDestroy()
    {
        TestDL.DelayInvoker(0.1f, (args) =>
        {
            Destroy(gameObject);
        });
        TestDL.DelayInvoker(3, (args) =>
        {
            if (lists == null)
            {
                Debug.LogError($"[Test] list 销毁");
            }
            if (this == null)
            {
                Debug.LogError($"[Test] this 销毁");
            }

            if (transform == null)
            {
                Debug.LogError($"[Test] transform 销毁");
            }
        });
    }







}
