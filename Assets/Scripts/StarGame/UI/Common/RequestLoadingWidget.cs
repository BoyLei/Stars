///--------------------------------------------------------------------
/// 文件名   :   RequestLoadingWidget
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   "2023/10/25 09:58:36"
/// 创建人   :   "zhaoerdong"
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using SGF.UI.Framework;
using StarProject;
using StarProject.Service.LocalData;
using System;
using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class RequestLoadingWidget : UIWidget
{
    private GameObject m_Image;

    private Vector3 EulerAngles;

    public float RotateSpeed = 100;

    /// <summary>
    /// 毫秒
    /// </summary>
    [Tooltip("毫秒")]
    public int OutTime = 500;

    private float RealOutTime;

    private float RunningTime;
    public Animator animator;
    private System.Action CallBack;

    /// <summary>
    /// 菊花node
    /// </summary>
    public GameObject JuHuaNode;
    /// <summary>
    /// 箭头node
    /// </summary>
    public GameObject ArrowNode;
    public GameObject WeakConnect1;
    public GameObject WeakConnect2;

    /// <summary>
    /// 菊花显示的 类型
    /// </summary>
    public LoadingWidgetTypeEnum LoadingWidgetType;

    protected override void Awake()
    {
        base.Awake();
        // m_Image = transform.Find("Cut4Cam90/AssemblyRatio2/Center/Image").gameObject;
        EulerAngles = JuHuaNode.transform.localRotation.eulerAngles;

        onCloseDestroy = true;
    }


    protected override void OnEnable()
    {
        animator.Play("RequestLoadingWidget_Loop_Anim");
    }


    void Update()
    {
        EulerAngles.z -= Time.deltaTime * RotateSpeed;
        JuHuaNode.transform.localRotation = Quaternion.Euler(EulerAngles);
        ArrowNode.transform.localRotation = Quaternion.Euler(EulerAngles);
        RunningTime += Time.deltaTime;
        if (RunningTime >= RealOutTime)
        {
            DoOutTime();
        }
    }


    private void DoOutTime()
    {
        UIAPI.CloseRequestLoading();
        CallBack?.Invoke();
    }

    #region private function

    #endregion

    protected override void OnOpen(object arg = null)
    {
        base.OnOpen(arg);
        CallBack = null;
        if (arg != null)
        {
            RequestInfo info = arg as RequestInfo;
            if (info != null)
            {
                if (info.outTime > 0)
                {
                    RealOutTime = info.outTime / 1000.0f;
                }
                else
                {
                    RealOutTime = OutTime / 1000.0f;
                }


                CallBack = info.callBack;
                RunningTime = 0;

                RefreshTypeUI(info.loadingWidgetType);

            }

        }
    }

    private void RefreshTypeUI(LoadingWidgetTypeEnum loadingWidgetTypeEnum)
    {
        LoadingWidgetType = loadingWidgetTypeEnum;
        JuHuaNode?.SetActive(false);
        ArrowNode?.SetActive(false);
        WeakConnect1?.SetActive(false);
        WeakConnect2?.SetActive(false);

        switch (loadingWidgetTypeEnum)
        {
            case LoadingWidgetTypeEnum.Default:
                {
                    JuHuaNode?.SetActive(true);
                }
                break;
            case LoadingWidgetTypeEnum.MsgOverTime:
                {
                    // JuHuaNode?.SetActive(true);
                    WeakConnect1?.SetActive(true);
                }
                break;
            case LoadingWidgetTypeEnum.WeakConnect1:
                {
                    // JuHuaNode?.SetActive(true);
                    WeakConnect1?.SetActive(true);
                }
                break;
            case LoadingWidgetTypeEnum.WeakConnect2:
                {
                    WeakConnect1?.SetActive(true);
                }
                break;

            default:
                {
                    JuHuaNode?.SetActive(true);
                }
                break;
        }
    }

    protected override void OnClose(object arg = null)
    {
        base.OnClose(arg);
    }
}