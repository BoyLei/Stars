using System.Collections;
using System.Collections.Generic;
using SGF.Module.Framework;
using UnityEngine;
using StarProjectDef;
using SGF.UI.Framework;
using StarProject.Game;
using ProtoMsg;
using System;
using StarProject.Service.LocalData;
using Sirenix.Utilities;
using XLua;
using System.Text;
using StarProject.Service.Language;
using SGF.Unity;


namespace StarProject.Service.SystemOpen
{

    public partial class SystemOpenManager : ServiceModule<SystemOpenManager>
    {
        private const string DelayKey = "SystemTipsOpen";

        private List<SystemItemData> systemItems = new();

        private DictionaryEx<SystemOpenType, SystemItemData> openedSystems = new();

        private HashSet<SystemOpenType> highLightSystemsCache = new();

        /// <summary>
        /// 监听的 任务id 
        /// </summary>
        private HashSet<uint> listenTasks = new();

        /// <summary>
        /// 开放所有功能
        /// </summary>
        private bool OpenAll = false;

        public void SetOpenAll(bool isOpenAll)
        {
            OpenAll = isOpenAll;
        }

        internal void Init()
        {
            CheckSingleton();
            AddEventListener();

            InitCustomConditions();
            LoadSystemCache();
            InitLoadCfg();

            GlobalEvent.OnSystemOpenInit.Invoke(null);
        }

        private void LoadSystemCache()
        {
            bool isExists = SaveManager.Instance.KeyExists(GameConfig.SYSTEM_CACHE, "");
            if (isExists)
            {

                var localCahce = SaveManager.Instance.Load<HashSet<SystemOpenType>>(GameConfig.SYSTEM_CACHE, "");
                if (localCahce != null)
                {
                    highLightSystemsCache = localCahce;
                }
            }
        }

        private void InitLoadCfg()
        {
            systemItems.Clear();
            listenTasks.Clear();

            openedSystems.Clear();

            // 加载 systemOpen  表，生成 对应的 systemItems 数据
            var datas = LocalDataManager.Instance.M_GuidSysOpenData.StaticGuidSysOpenDatas;
            datas.ForEach((item) =>
            {
                // SGF.Debuger.LogError($"[] {item.Key} , {item.Value.GetNeedQuestCom()} , systemItems: {systemItems.Count}");
                systemItems.Add(new SystemItemData(item.Value));
            });

            // 生成 对应的 初始化数据
            systemItems.ForEach((SystemItemData systemItem) =>
            {
                if (systemItem.TaskID != 0 && !listenTasks.Contains(systemItem.TaskID))
                {
                    listenTasks.Add(systemItem.TaskID);
                }
            });
        }

        private void TestEvent(object o)
        {
            OpenAllSystem(false);
        }

        private void OpenAllSystem(bool tips)
        {
            systemItems.Clear();

            listenTasks.Clear();

            openedSystems.Clear();

            InitLoadCfg();

            RefreshSystemOpen(tips);


            GlobalEvent.OnRefreshSystemOpen?.Invoke(0);
        }

        public void AddEventListener()
        {
            GlobalEvent.OnPlayerDataChange.AddListener(OnPlayerDataChange);
            GlobalEvent.TaskStageChange.AddListener(OnTaskStageChange);

            GlobalEvent.TestEvent.AddListener(TestEvent);
        }


        private void OnPlayerDataChange(string key, object value)
        {
            switch (key)
            {
                case AOIAttrDefine.PlayerLevel:
                case AOIAttrDefine.RiskInfo:
                    {
                        TriggerRefresh();

                        // RefreshSystemOpen();
                    }
                    break;
                default: break;
            }
        }

        private void OnTaskStageChange(int type, int taskID)
        {
            // SGF.Debuger.Log($"[system] task: {taskID} , type: {type}");
            // 只关心 任务完成的 通知
            if (type != 3)
            {
                return;
            }
            TriggerRefresh();


            // // 只关心 需要监听的 任务id是否完成
            // if (!listenTasks.Contains((uint)taskID))
            // {
            //     SGF.Debuger.Log($"[system] task: {taskID} , type: {type} , 居然没监听");

            //     return;
            // }
        }

        bool dirty = false;
        public void TriggerRefresh()
        {
            RefreshSystemOpen();
            //if (dirty)
            //{
            //    return;
            //}
            //dirty = true;
            //DelayInvoker.DelayInvoke(this, 0.2f, (args) =>
            //{
            //    RefreshSystemOpen();
            //}, null);
        }




        private void RefreshSystemOpen(bool tips = true)
        {
            dirty = false;
            // 新开放的系统
            bool result = GetCurNewOpenSystems();

            // if (GameManager.Instance.testInt > 0)
            // {
            //     foreach (var item in openedSystems)
            //     {
            //         GlobalEvent.OnSystemOpen?.Invoke(item.Key, true);
            //     }
            // }

            // 先刷新以及开放了的 系统节点
            {
                foreach (var item in openedSystems)
                {
                    GlobalEvent.OnSystemOpen?.Invoke(item.Value.systemOpenType, true);
                }
            }

            // 如果一个没找到, 那就啥都不干
            if (!result)
            {
                return;
            }

            if (newOpenSystems.Count > 0)
            {
                List<SystemItemData> needTipsSystem = new();

                newOpenSystems.ForEach((SystemItemData systemItem) =>
                {
                    // 将相关的系统 标记为 open
                    MarkOpen(systemItem);


                    // 如果 有拍脸图, 那就将数据 发送给 拍脸图tips
                    if (systemItem.IsHaveOpenPic)
                    {
                        // SGF.Debuger.LogError($"{systemItem.Name} type: {systemItem.systemOpenType} 开启 拍脸图");

                        needTipsSystem.Add(systemItem);
                    }
                    else
                    {
                        // 没有拍脸图的话，就直接 推送相应的系统类型开启
                        // SGF.Debuger.LogError($"{systemItem.Note} type: {systemItem.systemOpenType} 开启");

                        GlobalEvent.OnSystemOpen?.Invoke(systemItem.systemOpenType, true);
                    }
                    GlobalEvent.OnNewSystemOpen?.Invoke(systemItem.systemOpenType);
                });
                newOpenSystems.Clear();

                if (needTipsSystem.Count > 0 && tips)
                {
                    // 需要 拍脸图的 就通知 ui
                    //SGF.Unity.DelayInvoker.DelayInvoke(0, OpenSystemTips, null);

                    DelayInvoker.DelayInvoke(DelayKey, 0.2f,
                    (object[] args) =>
                    {
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.SystemOpenTipsModule, "OnOpen", needTipsSystem);
                    });
                }
            }

            if (newCloseSystems.Count > 0)
            {
                newCloseSystems.ForEach((systemItem) =>
                {
                    MarkClose(systemItem);
                    GlobalEvent.OnSystemOpen?.Invoke(systemItem.systemOpenType, false);
                });
                newCloseSystems.Clear();
            }
        }

        /// <summary>
        /// 当玩家 数据初始化的时候, 刷新 所有已经开启的系统数据
        /// </summary>
        public void RefreshOnPlayerDataInit()
        {
            // 重新load 一次本地缓存
            LoadSystemCache();
            // 重新 初始话加载一次 配置数据
            InitLoadCfg();

            var result = GetCurNewOpenSystems();

            if (!result)
            {
                // 通知一次 所有的 系统开放组件 刷新一次自己
                GlobalEvent.OnRefreshSystemOpen?.Invoke(0);
                return;
            }

            newOpenSystems.ForEach((systemItem) =>
            {
                // 将相关的系统 标记为 open


                MarkOpen(systemItem);

                // 如果初始化的时候 发现 本地缓存数据中 已经包含了这个系统 高亮过的记录，那就 标识 这个已经高亮
                if (systemItem.GetNeedHightLight() && highLightSystemsCache.Contains(systemItem.systemOpenType))
                {
                    systemItem.MarkHighLight();
                }
            });
            newOpenSystems.Clear();

            // 通知一次 所有的 系统开放组件 刷新一次自己
            GlobalEvent.OnRefreshSystemOpen?.Invoke(0);
        }

        private List<SystemItemData> newOpenSystems = new List<SystemItemData>();
        private List<SystemItemData> newCloseSystems = new List<SystemItemData>();


        /// <summary>
        /// 获得当前 条件下 玩家达到条件 可以开启的系统
        /// </summary>
        private bool GetCurNewOpenSystems()
        {

            int curRoleLevel = 0;
            int curAdvanceLevel = 0;
            if (GameManager.Instance.M_MainPlayerCtrlBase != null)
            {
                curRoleLevel = GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.PlayerLevel);

                var RiskInfo = GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(AOIAttrDefine.RiskInfo) as RiskLevelMD;
                if (RiskInfo != null)
                {
                    curAdvanceLevel = RiskInfo.CurRiskLevel;
                }
            }


            // SGF.Debuger.LogError($"curRoleLevel: {curRoleLevel}, curAdvanceLevel: {curAdvanceLevel}");

            newOpenSystems.Clear();
            newCloseSystems.Clear();

            systemItems.ForEach((systemItem) =>
              {
                  // 检查 系统类型 自定义的条件 检查
                  if (!CheckSystemItemCustomCondition(systemItem))
                  {
                      // 如果自定义条件没通过，但是系统已经开放过了, 那系统重新关闭
                      if (systemItem.IsOpen)
                      {
                          newCloseSystems.Add(systemItem);
                      }
                      return;
                  }

                  // 已经开启过的 直接跳过
                  if (systemItem.IsOpen)
                  {
                      return;
                  }

                  // 如果开启了所有的系统, 那就不需要每个系统 去设置开启了
                  if (OpenAll)
                  {
                      newOpenSystems.Add(systemItem);
                      return;
                  }

                  //   if (systemItem.systemOpenType == SystemOpenType.WorldLine)
                  //   {
                  //       var result = CheckConditions(systemItem, curRoleLevel, curAdvanceLevel);

                  //       SGF.Debuger.Log($"[xxx] WorldLine : curRoleLevel {curRoleLevel}, curAdvanceLevel: {curAdvanceLevel}, result: {result}");
                  //   }

                  // 检查是否满足条件
                  if (!CheckConditions(systemItem, curRoleLevel, curAdvanceLevel))
                  {
                      return;
                  }


                  // 新开放的系统
                  newOpenSystems.Add(systemItem);

              });

            // 只要找到一个 系统 开关，就查找成功
            return (newOpenSystems.Count + newCloseSystems.Count) > 0;
        }

        private StringBuilder sb = new();

        /// <summary>
        /// 得到系统 未开放 tips
        /// </summary>
        /// <param name="systemItemData"></param>
        public string GetSystemNoOpenTips(SystemOpenType systemOpenType)
        {
            SystemItemData systemItemData = GetSystemItemData(systemOpenType);
            if (systemItemData == null || systemItemData.IsOpen)
            {
                return string.Empty;
            }

            sb.Clear();


            int curRoleLevel = 0;
            int curAdvanceLevel = 0;
            if (GameManager.Instance.M_MainPlayerCtrlBase != null)
            {
                curRoleLevel = GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.PlayerLevel);

                var RiskInfo = GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(AOIAttrDefine.RiskInfo) as RiskLevelMD;
                if (RiskInfo != null)
                {
                    curAdvanceLevel = RiskInfo.CurRiskLevel;
                }
            }

            if (systemItemData.RoleLevel > curRoleLevel)
            {
                //sb.Append("等级达到").Append(systemItemData.RoleLevel).Append("级");
                //sb.Append(string.Format(GameConfig.LocalStr["SystemOpenTips"], systemItemData.RoleLevel));
                sb.Append(string.Format(LanguageManager.Instance.GetLanguageByKey("SystemOpenTips"), systemItemData.RoleLevel));
            }

            if (systemItemData.AdvanceLevel > curAdvanceLevel)
            {
                if (sb.Length > 0)
                {
                    //sb.Append("并且");
                    //sb.Append(GameConfig.LocalStr["And"]);
                    sb.Append(LanguageManager.Instance.GetLanguageByKey("And"));
                }
                //sb.Append("冒险等级达到").Append(systemItemData.AdvanceLevel).Append("级");
                //sb.Append(string.Format(GameConfig.LocalStr["SystemOpenTips1"], systemItemData.AdvanceLevel));
                sb.Append(string.Format(LanguageManager.Instance.GetLanguageByKey("SystemOpenTips1"), systemItemData.AdvanceLevel));
            }

            // 检查 这个任务是否完成
            if (systemItemData.TaskID != 0 && !TaskHelper.IsTaskFinsh(systemItemData.TaskID))
            {
                var cfg = TaskHelper.GetTaskConfig(systemItemData.TaskID);
                if (cfg != null)
                {
                    if (sb.Length > 0)
                    {
                        //sb.Append("并且");
                        //sb.Append(GameConfig.LocalStr["And"]);
                        sb.Append(LanguageManager.Instance.GetLanguageByKey("And"));
                    }

                    //sb.Append("完成任务").Append(cfg.Base.TaskName);
                    //sb.Append(string.Format(GameConfig.LocalStr["FinishTask"], cfg.Base.TaskName));
                    sb.Append(string.Format(LanguageManager.Instance.GetLanguageByKey("FinishTask"), cfg.Base.TaskName));
                }

            }
            if (sb.Length > 0)
            {
                //sb.Append("后开放");
                //sb.Append(GameConfig.LocalStr["SystemOpenTips2"]);
                sb.Append(LanguageManager.Instance.GetLanguageByKey("SystemOpenTips2"));
            }
            return sb.ToString();
        }



        private bool CheckConditions(SystemItemData systemItemCfg, int curRoleLevel, int curAdvenceLevel)
        {
            if (systemItemCfg.RoleLevel > curRoleLevel)
            {
                return false;
            }

            if (systemItemCfg.AdvanceLevel > curAdvenceLevel)
            {
                return false;
            }

            // 检查 这个任务是否完成
            if (systemItemCfg.TaskID != 0 && !TaskHelper.IsTaskFinsh(systemItemCfg.TaskID))
            {
                return false;
            }

            return true;
        }

        public SystemItemData GetSystemItemData(SystemOpenType systemOpenType)
        {
            return systemItems.Find(systemItem => systemItem.systemOpenType == systemOpenType);
        }

        /// <summary>
        /// 获得 已经开放的 systemOpenType 类型数据，如果没有数据, 返回为null
        /// </summary>
        /// <param name="systemOpenType"></param>
        public SystemItemData GetOpenedSystemItemData(SystemOpenType systemOpenType)
        {
            return openedSystems[systemOpenType];
        }

        public void MarkOpen(SystemItemData systemItem)
        {
            systemItem.MarkOpen();
            if (!openedSystems.ContainsKey(systemItem.systemOpenType))
            {
                openedSystems.Add(systemItem.systemOpenType, systemItem);
            }
        }

        public void MarkClose(SystemItemData systemItem)
        {
            systemItem.MarkClose();
            if (openedSystems.ContainsKey(systemItem.systemOpenType))
            {
                openedSystems.Remove(systemItem.systemOpenType);
            }
        }

        public void MarkHighLight(SystemOpenType systemOpenType)
        {
            if (!highLightSystemsCache.Contains(systemOpenType))
            {
                highLightSystemsCache.Add(systemOpenType);

                SaveManager.Instance.Save<HashSet<SystemOpenType>>(GameConfig.SYSTEM_CACHE, highLightSystemsCache, "");
            }
        }


        public bool SystemIsOpen(SystemOpenType systemOpenType)
        {
            if (openedSystems.ContainsKey(systemOpenType))
            {
                return openedSystems[systemOpenType].IsOpen;
            }
            return false;
        }

        public bool SystemIsOpen(int systemOpenId)
        {
            foreach (var item in openedSystems)
            {
                if (item.Value.CfgID == systemOpenId)
                {
                    return true;
                }
            }

            return false;
        }

        Dictionary<SystemOpenType, List<SystemItem>> systemNodes = new();
        /// <summary>
        /// 注册 systemItem 到 manager
        /// </summary>
        public void RegSystemItemNode(SystemItem systemItem)
        {
            var systemType = systemItem.systemType;
            if (!systemNodes.ContainsKey(systemType))
            {
                systemNodes.Add(systemType, new List<SystemItem>());
            }

            systemNodes[systemType].Add(systemItem);
        }

        public void UnRegSystemItemNode(SystemItem systemItem)
        {
            var systemType = systemItem.systemType;
            if (!systemNodes.ContainsKey(systemType))
            {
                return;
            }

            systemNodes[systemType].Remove(systemItem);

            if (systemNodes[systemType].Count == 0)
            {
                systemNodes.Remove(systemType);
            }
        }



        [LuaCallCSharp]
        public List<SystemItem> GetSystemItemNodes(SystemOpenType systemOpenType)
        {
            if (!systemNodes.ContainsKey(systemOpenType))
            {
                return null;
            }

            return systemNodes[systemOpenType];
        }

        public bool CheckCanSystemJump(int systemJumpID)
        {
            var cfg = LocalDataManager.Instance.GetSystemJumpDataCell(systemJumpID);
            if (cfg == null)
            {
                return false;
            }

            if (cfg.SysOpen <= 0)
            {
                return true;
            }

            // 存在指定的 判定系统开放的 id
            return SystemIsOpen(cfg.SysOpen);

        }

        public void ContainTipsInvoke()
        {
            if (DelayInvoker.ContainInvoke(DelayKey))
            {
                DelayInvoker.CancelInvoke(DelayKey);
            }
        }

    }

}