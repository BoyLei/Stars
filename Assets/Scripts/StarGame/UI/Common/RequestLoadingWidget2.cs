using SGF.UI.Framework;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;

public class RequestLoadingWidget2 : UIWidget
{
    /// <summary> 菊花node </summary>
    public GameObject JuHuaNode;

    /// <summary> 提示文本 </summary>
    public Text Tips;

    /// <summary> 返回按钮 </summary>
    public JButton BackJbtn;

    /// <summary> 毫秒 </summary>
    [Tooltip("毫秒")]
    public int OutTime = 500;

    public float RotateSpeed = 100;

    private Vector3 EulerAngles;

    private float RealOutTime;
    private float RunningTime;
    private System.Action OutActionCallBack;
    private System.Action BtnCb;


    /// <summary>
    /// 菊花显示的 类型
    /// </summary>
    public LoadingWidgetTypeEnum LoadingWidgetType;

    private bool IsCountdown = false;

    protected override void Awake()
    {
        base.Awake();
        EulerAngles = JuHuaNode.transform.localRotation.eulerAngles;
        BackJbtn.OnClick += OnBackJbtn;
        onCloseDestroy = false;
    }

    private void OnBackJbtn(GameObject arg0)
    {
        BtnCb?.Invoke();
        UIAPI.CloseRequestLoading2();
    }

    void Update()
    {
        if (IsCountdown)
        {
            EulerAngles.z -= Time.deltaTime * RotateSpeed;
            JuHuaNode.transform.localRotation = Quaternion.Euler(EulerAngles);
            RunningTime += Time.deltaTime;
            if (RunningTime >= RealOutTime)
            {
                DoOutTime();
            }
        }
    }

    private void DoOutTime()
    {
        OutActionCallBack?.Invoke();
        UIAPI.CloseRequestLoading2();
    }

    protected override void OnOpen(object arg = null)
    {
        base.OnOpen(arg);
        OutActionCallBack = null;
        BtnCb = null;
        Tips.text = string.Empty;
        bool isShowBtn = false;
        if (arg != null)
        {
            RequestInfo2 info = arg as RequestInfo2;
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

                OutActionCallBack = info.onOutAction;
                BtnCb = info.btnCb;

                RunningTime = 0;

                Tips.text = info.tips;

                LoadingWidgetType = info.loadingWidgetType;
                IsCountdown = RealOutTime > 0;

                isShowBtn = BtnCb != null;
            }
        }
        else
        {
            DoOutTime();
        }
        BackJbtn.gameObject.SetActive(isShowBtn);
    }

    protected override void OnClose(object arg = null)
    {
        base.OnClose(arg);
        OutActionCallBack = null;
        BtnCb = null;
        IsCountdown = false;
    }
}