using System.Collections.Generic;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Service.Input;
using StarProject.Service.SystemOpen;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;


public enum E_TutorialState
{
    Disabled = 0, //未激活
    Trigger = 1,
    Finish = 2,
}

public class TutorialInstance
{
    public E_TutorialState State { get; private set; }

    public int ID { get; private set; }

    public TutorialConfig Config { get; private set; }


    public Dictionary<E_TutorialConditionType, TutorialConditionDelegate> ConditionMap = new();

    public bool Active { get; private set; }

    //激活回调
    public System.Action<TutorialConfig> OnActiveCallBack { get; private set; }

    public System.Func<int, bool> OnIsFinishGuide { get; private set; }


    private DelayFunction DelayCall=null;

    private Button mClickBtn;
    public TutorialInstance(TutorialConfig config, System.Action<TutorialConfig> cb, System.Func<int, bool> func)
    {
        this.ID = config.ID;
        this.Config = config;
        this.Active = false;
        this.State = E_TutorialState.Disabled;
        this.OnActiveCallBack = cb;
        this.OnIsFinishGuide = func;
        ConditionMap.Clear();
        ConditionMap.Add(E_TutorialConditionType.OpenUI, OpenUIHandler);
        ConditionMap.Add(E_TutorialConditionType.FinishTutorial, OnFinishTutorialHandler);
        ConditionMap.Add(E_TutorialConditionType.FinishTask, OnFinishTaskHandler);
        ConditionMap.Add(E_TutorialConditionType.SystemOpen, OnSystemOpenHandler);
        ConditionMap.Add(E_TutorialConditionType.EntryScene, OnEntryScenekHandler);
        ConditionMap.Add(E_TutorialConditionType.ExitScene, OnExitSceneHandler);

        //打开UI
        StarProject.GlobalEvent.OnOpenUI.AddListener(OnOpenUIHandler);
        //任务完成
        StarProject.GlobalEvent.TaskStageChange.AddListener(OnTaskStageChangeHandler);
        //引导完成
        StarProject.GlobalEvent.OnFinishTutorial.AddListener(OnFinishTutorial);
        //系统开启
        StarProject.GlobalEvent.OnSystemOpen.AddListener(OnSystemOpen);

        StarProject.GlobalEvent.OnEntryScene.AddListener(OnEnryScene);


        StarProject.GlobalEvent.OnExitScene.AddListener(OnEnryScene);

        StarProject.GlobalEvent.OnSystemOpenInit.AddListener(OnSystemOpenInit);

        OnTriggerCheck();
    }


    public void Clean()
    {
        this.ID = 0;
        this.Config = null;
        this.Active = false;
        this.State = E_TutorialState.Disabled;
        this.OnActiveCallBack = null;
        this.OnIsFinishGuide = null;
        ConditionMap.Clear();
        ConditionMap = null;
        DelayCall=null;
        //打开UI
        StarProject.GlobalEvent.OnOpenUI.RemoveListener(OnOpenUIHandler);
        //任务完成
        StarProject.GlobalEvent.TaskStageChange.RemoveListener(OnTaskStageChangeHandler);
        //引导完成
        StarProject.GlobalEvent.OnFinishTutorial.RemoveListener(OnFinishTutorial);
        //系统开启
        StarProject.GlobalEvent.OnSystemOpen.RemoveListener(OnSystemOpen);

        StarProject.GlobalEvent.OnEntryScene.RemoveListener(OnEnryScene);
        StarProject.GlobalEvent.OnExitScene.RemoveListener(OnEnryScene);
        StarProject.GlobalEvent.OnSystemOpenInit.RemoveListener(OnSystemOpenInit);
    }
    private void OnSystemOpenInit(object t)
    {
        OnTriggerCheck();
    }
    private void OnEnryScene(int sceneID)
    {
        OnTriggerCheck();
    }


    private void OnOpenUIHandler(string viewName)
    {
        OnTriggerCheck();
    }

    private void OnTaskStageChangeHandler(int type, int id)
    {
        //任务完成
        if (type == 3)
        {
            OnTriggerCheck();
        }
    }

    private void OnFinishTutorial(int id)
    {
        OnTriggerCheck();
    }

    private void OnSystemOpen(SystemOpenType type, bool isOpen)
    {
        // 只有系统开放后,才会触发新手
        if (isOpen)
        {
            OnTriggerCheck();
        }
    }

    #region 条件检查

    private void OnTriggerCheck()
    {
        if (State != E_TutorialState.Disabled)
        {
            return;
        }

        bool result = IsMateCondition();

        if (result)
        {
            State = E_TutorialState.Trigger;
            //注册完成事件
            // RegisterFinishEvent();

            //开启引导
            DelayCall = (object[] args) =>
            {
                OnActiveCallBack?.Invoke(Config);
            };
            DelayInvoker.DelayInvoke(0.4f,DelayCall);

        }
    }

    

    public bool IsMateCondition()
    {
        bool result = true;

        foreach (var item in Config.TriggerConditions)
        {
            result &= CheckHandler(item);
        }

        return result;
    }




    private bool CheckHandler(TutorialConditionConfig cfg, params object[] args)
    {
        if (ConditionMap.ContainsKey(cfg.ConditionType))
        {
            return ConditionMap[cfg.ConditionType].Invoke(cfg);
        }

        return false;
    }

    private bool OnFinishTaskHandler(TutorialConditionConfig cfg)
    {
        if (cfg != null && cfg.TaskID > 0)
        {
            return TaskHelper.IsTaskFinsh((uint)cfg.TaskID);
        }

        return false;
    }

    private bool OnFinishTutorialHandler(TutorialConditionConfig cfg)
    {
        if (cfg != null && cfg.FlagIndex > 0)
        {
            return OnIsFinishGuide(cfg.FlagIndex);
        }

        return false;
    }

    private bool OpenUIHandler(TutorialConditionConfig cfg)
    {
        if (cfg != null && !string.IsNullOrEmpty(cfg.ViewName))
        {
            return UIManager.Instance.HasPanele(cfg.ViewName);
        }

        return false;
    }

    private bool OnSystemOpenHandler(TutorialConditionConfig cfg)
    {
        if (cfg != null && cfg.SystemID != SystemOpenType.None)
        {
            return SystemOpenManager.Instance.SystemIsOpen(cfg.SystemID);
        }
        return false;
    }

    private bool OnEntryScenekHandler(TutorialConditionConfig cfg)
    {
        if (cfg != null && cfg.SceneID > 0)
        {
            return GameManager.Instance.GetCurMapId() == cfg.SceneID;
        }
        return false;
    }
    private bool OnExitSceneHandler(TutorialConditionConfig cfg)
    {
        if (cfg != null && cfg.SceneID > 0)
        {

            return GameManager.Instance.OldMapID == cfg.SceneID;
        }
        return false;
    }

    #endregion
}