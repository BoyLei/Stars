using StarProject.Service.Cam;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

[XLua.LuaCallCSharp]
public class DropBox : MonoBehaviour
{
    public Action raycastCallback = null;
    public static DropBox Instance = null;
    float totalTime = 0;
    float curTime = 0;
    private Camera mainCamera;
    public Camera MainCamera
    {
        get
        {
            if (mainCamera == null)
            {
                mainCamera = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera;
            }
            return mainCamera;
        }

    }
    void Awake()
    {
        Instance = this;
    }

    public void SetCallBack(Action act, float t)
    {
        totalTime = t;
        raycastCallback = act;
    }

    void OnEnd()
    {
        raycastCallback?.Invoke();
        GameObject.Destroy(gameObject);
    }


    void Update()
    {
        curTime += Time.deltaTime;
        if (curTime >= totalTime)
        {
            OnEnd();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var obj = hit.collider.gameObject;
                if (obj == this)
                {
                    OnEnd();
                }
            }
        }
    }
}