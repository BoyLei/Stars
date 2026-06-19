using DG.Tweening;
///--------------------------------------------------------------------
/// 文件名   :   InterActionPanel
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   #CREATETIME#
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Time;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject;
using StarProject.Game;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InterActionPanel : UIWidget
{

    private Button mInterBtn;
    private Slider mSlider;
    private Text mText;
    private Text mPoint;

    private Transform mInterRoot;
    private int mInterObjectConfigID;
    private ulong mInterUID;
    private long EndInterTime;
    private int InterTime;
    private string Icon;
    private bool IsShowbar;
    private string InterContent;

    private UnityAction callback;
    private UnityAction onCancel;

    protected override void Awake()
    {
        mInterBtn = transform.Find("InterBtn").GetComponent<Button>();
        mInterRoot = transform.Find("Inter");
        mSlider = transform.Find("Inter/Slider").GetComponent<Slider>();
        mText = transform.Find("Inter/bg/Text").GetComponent<Text>();
        mPoint = transform.Find("Inter/bg/Text/Point").GetComponent<Text>();
    }
    // Start is called before the first frame update
    protected override void Start()
    {
        mInterBtn.onClick.AddListener(OnClickInterBtn);
        GlobalEvent.OnTriggerEvent.AddListener(OnTriggerHandler);
        GlobalEvent.OnExitEvent.AddListener(OnExitHandler);
        GlobalEvent.InterEvent.AddListener(OnInterEventHandler);
        GlobalEvent.InterEventByStr.AddListener(OnInterEventByStrHandler);
        GlobalEvent.OnHitStarEvent.AddListener(OnHitStarEventHandler);
        GlobalEvent.OnHitEndEvent.AddListener(OnHitEndEventHandler);
        GlobalEvent.OnBackLogin.AddListener(OnBackLoginHandler);
        GlobalEvent.onSceneLoaded.AddListener(OnSceneLoadedHandler);
        mText.text = string.Empty;
        mSlider.value = 0;
        mInterBtn.gameObject.SetActive(false);
        mInterRoot.gameObject.SetActive(false);
        mPoint.gameObject.SetActive(false);
        EndInterTime = 0;
        mPoint.DOText("...", 1.5f).SetLoops(-1, LoopType.Restart);
    }

    private void OnSceneLoadedHandler(string arg0, bool arg1)
    {
        OnExitHandler(0,mInterUID);
    }

    private void OnBackLoginHandler(AgainLoginType arg0)
    {
        OnExitHandler(0,mInterUID);
    }

    private void OnInterEventByStrHandler(string text,int interTime,UnityAction cb, UnityAction cancel)
    {
        mInterUID = 99999;
        onCancel = cancel;
        callback = cb;

        EndInterTime = TimeUtils.ServerNowStampMilli+ interTime;
        InterTime = interTime;
        Icon = "";
        IsShowbar = true;
        InterContent = text;
        
        mInterRoot.gameObject.SetActive(true);
        mInterBtn.gameObject.SetActive(false);

        mSlider.value = 0;
        mSlider.gameObject.SetActive(IsShowbar);
        mText.text = InterContent;
    }

    private void OnInterEventHandler(ulong objUID, long endTime)
    {
        callback = null;
        //结束事件回调
        if (onCancel != null )
        {
            onCancel.Invoke();
            onCancel = null;
        }
        if (objUID == 0)
        {
            OnExitHandler(0, mInterUID);
        }
        else
        {
            EndInterTime = endTime;
            mInterObjectConfigID = (int)GameManager.Instance.GetEntityCfgID(objUID);
            InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell(mInterObjectConfigID);
            if (interactDataCell != null)
            {
                InterTime = interactDataCell.GetInterTime();
                Icon = interactDataCell.Icon;
                IsShowbar = interactDataCell.GetIsShowbar();
                InterContent = interactDataCell.InterContent;
            }
            mInterRoot.gameObject.SetActive(true);
            mInterBtn.gameObject.SetActive(false);
            long delay =EndInterTime- TimeUtils.ServerNowStampMilli;
            long time = Math.Max(0, delay);
            float rate = time * 1.0f / InterTime;
            mSlider.value = Math.Max(0, 1 - rate);
            mSlider.gameObject.SetActive(IsShowbar);
            mText.text = InterContent;
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnShowHideObjectInteractive", new object[] { false });
        }
    }

    private void Update()
    {
        if (EndInterTime > 0 && InterTime > 0)
        {
            long delay =EndInterTime- TimeUtils.ServerNowStampMilli;
            long time = Math.Max(0, delay);
            float rate = time * 1.0f / InterTime;
            mSlider.value = Math.Max(0, 1 - rate);

            //结束事件回调
            if(callback!=null && mSlider.value>=0.98f)
            {
                callback.Invoke();
                callback = null;
                EndInterTime = 0;
                InterTime = 0;
                mInterRoot.gameObject.SetActive(false);
                mInterBtn.gameObject.SetActive(false);
            }
        }

    }


    private void OnExitHandler(int mConfigID, ulong mUID)
    {
        if (mUID == mInterUID)
        {
            mInterObjectConfigID = 0;
            mInterUID = 0;
            //结束
            EndInterTime = 0;
            InterTime = 0;
            mInterRoot.gameObject.SetActive(false);
            mInterBtn.gameObject.SetActive(false);
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnShowHideObjectInteractive",
                new object[] { true });
        }
    }


    private void OnTriggerHandler(int mConfigID, ulong mUID)
    {
        mInterObjectConfigID = mConfigID;
        mInterUID = mUID;
        mInterRoot.gameObject.SetActive(false);
        mInterBtn.gameObject.SetActive(true);
    }

    private void OnInterHandler(int mConfigID)
    {
        if (mInterObjectConfigID == mConfigID)
        {

        }
    }

    private void OnClickInterBtn()
    {
        ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule, "QueryInter", mInterUID);
        //SendMsg
        /*SocketBase battleSocket = NetworkManager.Instance.gameSocket;
        InterEeq interEeq = new();
        interEeq.InterID = mInterUID;
        interEeq.Baseid = (int)GameManager.Instance.GetEntityCfgID(mInterUID);
        battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, interEeq);*/
    }

    protected override void OnDestroy()
    {
        mInterBtn.onClick.RemoveListener(OnClickInterBtn);
        GlobalEvent.OnTriggerEvent.RemoveListener(OnTriggerHandler);
        GlobalEvent.OnExitEvent.RemoveListener(OnExitHandler);
        GlobalEvent.InterEvent.RemoveListener(OnInterEventHandler);
        GlobalEvent.InterEventByStr.RemoveListener(OnInterEventByStrHandler);
        GlobalEvent.OnHitStarEvent.RemoveListener(OnHitStarEventHandler);
        GlobalEvent.OnHitEndEvent.RemoveListener(OnHitEndEventHandler);
        GlobalEvent.OnBackLogin.RemoveListener(OnBackLoginHandler);
        GlobalEvent.onSceneLoaded.RemoveListener(OnSceneLoadedHandler);

    }


    protected override void OnOpen(object arg = null)
    {
        base.OnOpen(arg);
    }

    protected override void OnClose(object arg = null)
    {
        base.OnClose(arg);
    }


    //----------------------------- 
    private void OnHitStarEventHandler(string hit)
    {
        mInterBtn.gameObject.SetActive(false);
        mInterRoot.gameObject.SetActive(true);
        mSlider.gameObject.SetActive(false);
        mPoint.gameObject.SetActive(true);

        //...
        mText.text = hit;
        // Tweener= mText.DOText(hit+"...",1.5f).SetLoops(-1, LoopType.Restart);
    }


    private void OnHitEndEventHandler(object arg0)
    {
        DelayInvoker.DelayInvoke(this, 0.2f, (a) =>
        {
            if (GameManager.Instance.M_MainPlayerCtrlBase!=null &&
                GameManager.Instance.M_MainPlayerCtrlBase.M_Curr!=null &&
                (!GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Is_MainPlayer_FindingPath))
            {
                if (EndInterTime > 0 && InterTime > 0)
                {
                    mPoint.gameObject.SetActive(false);
                    return;
                }
                mInterBtn.gameObject.SetActive(false);
                mInterRoot.gameObject.SetActive(false);
                mSlider.gameObject.SetActive(false);
                mText.text = string.Empty;
            }
            else
            {
                if (EndInterTime > 0 && InterTime > 0)
                {
                    mPoint.gameObject.SetActive(false);
                    return;
                }
                mInterBtn.gameObject.SetActive(false);
                mInterRoot.gameObject.SetActive(false);
                mSlider.gameObject.SetActive(false);
                mText.text = string.Empty;
            }
        }, null);

    }


}
