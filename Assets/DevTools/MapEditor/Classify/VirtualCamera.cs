using Cinemachine;
using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using Sirenix.OdinInspector;
using StarProject.Game;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCamera : MonoBehaviour
{
    [LabelText("索引")] public int Index;

    [LabelText("激活时间")] public float ActiveTime;

    [LabelText("下一个索引")] public int NextPlayIndex = -1;

    [LabelText("退出时间")]
    [ReadOnly]
    public float OutTime;

    [LabelText("隐藏UI")] public bool HideUI = true;

    [LabelText("播放时隐藏主角")] public bool HideRole = false;

    [LabelText("播放时隐藏伙伴")] public bool HidePartner = false;

    [LabelText("是否可移动")] public bool IsCanMove = true;

    [LabelText("是否隐藏地图NPC")] public bool HideMapNPC = false;

    [LabelText("是否隐藏其他玩家")] public bool HideOtherPlayer = false;

    [LabelText("是否隐藏空气墙")] public bool HideAirWall = false;

    [LabelText("交互物")] public bool HideObj = false;

    [LabelText("怪物")] public bool HideMon = false;

    [LabelText("是否进入闪黑")] public bool IsInBlack = false;

    [LabelText("是否退出闪黑")] public bool IsOutBlack = false;

    private float _mRunnigTime;
    private System.Action<int, int> OnEndCall;

    private int EffectID;

    private CinemachineVirtualCamera _virtualCamera = null;
    private CinemachineVirtualCamera SelfVirtualCamera
    {
        get
        {
            if (_virtualCamera == null)
            {
                _virtualCamera = GetComponent<CinemachineVirtualCamera>();
            }
            return _virtualCamera;
        }
    }

    public void SetCallBack(System.Action<int, int> call)
    {
        OnEndCall = call;
    }

    public void SetOutTime(float time)
    {
        OutTime = time;
    }

    public void OnActive(int effectid)
    {
        EffectID = effectid;
        gameObject.SetActive(true);

        if (IsInBlack)
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.AvgLuaModule, "OnTransition", new object[] { 1, EffectID });
        }

        //隐藏UI
        if (HideUI)
        {
            UIManager.Instance.SetShowVirtualCameraToHideHud(true);
        }

        //隐藏主角
        if (HideRole)
        {
            GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnStartHidden?.Invoke(false);
        }

        //隐藏伙伴
        if (HidePartner)
        {
            if (PartnerManager.Instance.CurConcretizationPartner != null)
            {
                if (PartnerManager.Instance.CurConcretizationPartner.M_Curr != null)
                {
                    PartnerManager.Instance.CurConcretizationPartner.M_Curr.ActionOnStartHidden?.Invoke(false);
                }
            }
        }

        //主角是否可移动
        if (!IsCanMove)
        {
            SGF.Debuger.LogWarning($"虚拟相机 禁止 主角移动 ID={Index},EffectID={EffectID} 11111111111");
            GameManager.Instance.RegisterMainPlayerClientBattleStates();
        }

        List<E_EntityType> mTypes = new();
        //隐藏地图NPC
        if (HideMapNPC)
        {
            mTypes.Add(E_EntityType.Npc);
            // GameManager.Instance.SetNpcEntityHide(true);
        }

        // 隐藏其他玩家
        if (HideOtherPlayer)
        {
            mTypes.Add(E_EntityType.Player);
            // GameManager.Instance.SetPlayerEntityHide(true);
        }

        //怪物
        if (HideMon)
        {
            mTypes.Add(E_EntityType.Monster);
            mTypes.Add(E_EntityType.Summon);
            mTypes.Add(E_EntityType.ClientSummon);
        }

        //交互物
        if (HideObj)
        {
            mTypes.Add(E_EntityType.Interact);
        }

        GameManager.Instance.SetEntityHide(true, mTypes);

        // 隐藏空气墙
        if (HideAirWall)
        {
            GameManager.Instance.SetHideAirWallEffect(true);
        }

        // 进入UI队列
    }

    private void OnEnable()
    {
        _mRunnigTime = 0;
        if (SelfVirtualCamera != null)
        {
            SelfVirtualCamera.enabled = true;
        }
        SGF.Debuger.Log($"摄像机 打开 name={transform.name}");
    }

    // Update is called once per frame
    void Update()
    {
        _mRunnigTime += Time.deltaTime;
        if (_mRunnigTime >= ActiveTime)
        {
            if (NextPlayIndex != -1 || SelfVirtualCamera == null)
            {
                gameObject.SetActive(false);
            }
            else
            {
                SelfVirtualCamera.enabled = false;
            }
        }
        if (_mRunnigTime >= ActiveTime + OutTime)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnd(bool force = false)
    {
        if (EffectID != 0 || force)
        {
            SGF.Debuger.LogError($"摄像机 OnEnd name={transform.name}");
            //if (OnEndCall != null)
            //{
            //    OnEndCall.Invoke(Index, EffectID);
            //}

            //float time = 0;
            //time = SystemConstConfigs.BackToPlayerCFV * 0.001f;
            //DelayInvoker.DelayInvoke(time, (a) =>
            //{
                //隐藏UI
                if (HideUI)
                {
                    UIManager.Instance.SetShowVirtualCameraToHideHud(false);
                }

                //隐藏主角
                if (HideRole)
                {
                    if (GameManager.Instance.M_MainPlayerCtrlBase != null &&
                        GameManager.Instance.M_MainPlayerCtrlBase.M_Curr != null)
                    {
                        GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnStopHidden?.Invoke(false);
                    }
                }

                //隐藏伙伴
                if (HidePartner)
                {
                    if (PartnerManager.Instance.CurConcretizationPartner != null)
                    {
                        if (PartnerManager.Instance.CurConcretizationPartner.M_Curr != null)
                        {
                            PartnerManager.Instance.CurConcretizationPartner.M_Curr.ActionOnStopHidden?.Invoke(false);
                        }
                    }
                }

                //主角是否可移动
                if (!IsCanMove)
                {
                    SGF.Debuger.LogWarning($"摄像机 虚拟相机 禁止 主角移动 ID={Index},EffectID={EffectID} 2222222");

                    GameManager.Instance.UnRegisterMainPlayerClientBattleStates();
                }

                List<E_EntityType> mTypes = new();
                //隐藏地图NPC
                if (HideMapNPC)
                {
                    mTypes.Add(E_EntityType.Npc);
                    // GameManager.Instance.SetNpcEntityHide(true);
                }

                // 隐藏其他玩家
                if (HideOtherPlayer)
                {
                    mTypes.Add(E_EntityType.Player);
                    // GameManager.Instance.SetPlayerEntityHide(true);
                }

                //怪物
                if (HideMon)
                {
                    mTypes.Add(E_EntityType.Monster);
                    mTypes.Add(E_EntityType.Summon);
                    mTypes.Add(E_EntityType.ClientSummon);
                }

                //交互物
                if (HideObj)
                {
                    mTypes.Add(E_EntityType.Interact);
                }

                GameManager.Instance.SetEntityHide(false, mTypes);

                // 隐藏空气墙
                if (HideAirWall)
                {
                    GameManager.Instance.SetHideAirWallEffect(false);
                }
            //}, null);
            if (IsOutBlack)
            {
                ModuleManager.Instance.SendMessage(ModuleDef.Name.AvgLuaModule, "OnTransition",
                    new object[] { 0, EffectID });
            }
            if (OnEndCall != null)
            {
                OnEndCall.Invoke(Index, EffectID);
            }
            EffectID = 0;
        }
    }

    private void OnDisable()
    {
        SGF.Debuger.LogWarning($"摄像机 OnDisable name={transform.name}");
        OnEnd();
    }

    private void OnDestroy()
    {
        SGF.Debuger.LogWarning($"摄像机 OnDestroy name={transform.name}");
        OnEnd();
    }
}