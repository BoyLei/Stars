using Google.Protobuf;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.UI.Framework;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;
using SGF.Unity;
using UnityEngine;
using XLua;
using StarProject.Service.SDK;
using StarProject.Game;

public delegate bool TutorialConditionDelegate(TutorialConditionConfig cfg);

[CSharpCallLua]
public delegate void OnBeigenDelegate(object self, TutorialConfig Config);

[CSharpCallLua]
public delegate void OnBreakGuide(object self, string path, int guideID, bool stop);

[CSharpCallLua]
public delegate bool OnGuideIsRunning(object self, string path, bool replay);

/// <summary>
/// 获取下一个引导
/// </summary>
[LuaCallCSharp]
public delegate int GetNextGuideID(int id);

/// <summary>
/// 设置标记
/// </summary>
[LuaCallCSharp]
public delegate void SetMarkFlagDelegate(int id);


namespace StarProject.Module
{
    public class TutorialModule : BusinessModule
    {
        private OnBeigenDelegate OnBeigen;
        private GetNextGuideID GetNextGuideIDDelegete;
        private SetMarkFlagDelegate SetMarkFlag;

        private OnGuideIsRunning GuideIsRunning;
        private OnBreakGuide OnBreakGuide;

        private LuaTable ScriptTable;
        private Dictionary<int, TutorialInstance> Tutorials = new();
        private BaseBinary baseBinary;

        private Dictionary<int, TutorialFlagInstance> Flags = new();

        private Special_PartnerTutorial PartnerTutorial = new();

        private Special_TaskTutorial TaskTutorial = new();

        public const string GUIDESKIP = "GuideSkip";

        public List<int> m_IsRunnigGuide = new List<int>();

        public override void Create(object args = null)
        {
            GetNextGuideIDDelegete = GetNextGuildID;
            SetMarkFlag = OnMarkFlag;
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GuideSignSetRetID, OnGuideSignSetRet, this);
            GlobalEvent.OnRoleCreateComplete.AddListener(OnRoleCreamComplete);
            GlobalEvent.TaskStageChange.AddListener(OnTaskStateChange);
            GlobalEvent.OnClickTaskItem.AddListener(OnClickTaskItem);
            StarProject.GlobalEvent.OnExitScene.AddListener(OnExitScene);
            GlobalEvent.OnBackLogin.AddListener(OnReLoginerHandler);
        }

        private void OnReLoginerHandler(AgainLoginType arg0)
        {
            if (Tutorials != null)
            {
                foreach (var to in Tutorials)
                {
                    to.Value.Clean();
                }
                Tutorials.Clear();
            }
        }

        private void OnExitScene(int sceneID)
        {
            foreach (var flag in Flags)
            {
                flag.Value.OnExitScene((uint)sceneID, TryFinishTutorial);
            }
        }

        private void OnTaskStateChange(int type, int taskid)
        {
            if (type == 3)
            {
                foreach (var flag in Flags)
                {
                    flag.Value.OnTaskFinish((uint)taskid, TryFinishTutorial);
                }
            }
        }

        private void OnGuideSignSetRet(MessageHandleData data)
        {
            GuideSignSetRet msg = data.data as GuideSignSetRet;
            if (msg != null)
            {
                //存储
                UpData(msg.Index, 1);

                //事件回调
                StarProject.GlobalEvent.OnFinishTutorial.Invoke(msg.Index);
            }
        }

        public void OnClickTaskItem(int taskid)
        {
            foreach (var flag in Flags)
            {
                flag.Value.OnClickTaskItem((uint)taskid, TryBegineTutorial);
            }
        }

        public void TryBegineTutorial(int id)
        {
            if (GuideIsRunning != null)
            {
                if (LocalDataManager.Instance.M_TutorialConfig.list.ContainsKey(id))
                {
                    var config = LocalDataManager.Instance.M_TutorialConfig.list[id];
                    if (config != null)
                    {
                        if (IsFinishGuide(config.FlagIndex))
                        {
                            return;
                        }

                        if (Tutorials.TryGetValue(id, out var instance) && instance != null)
                        {
                            ///满足开启条件
                            if (instance.IsMateCondition())
                            {
                                string path = string.Empty;
                                if (config.TutorialTargets != null && config.TutorialTargets.Count > 0)
                                {
                                    path = config.TutorialTargets[0].Params;
                                }

                                if (!string.IsNullOrEmpty(path))
                                {
                                    //引导未开启
                                    bool isRuning = GuideIsRunning.Invoke(ScriptTable, path, true);
                                    if (!isRuning)
                                    {
                                        TestTutorial(id);
                                    }
                                }
                            }
                        }
                        /*else
                        {
                            string path = string.Empty;
                            if (config.TutorialTargets != null && config.TutorialTargets.Count > 0)
                            {
                                path = config.TutorialTargets[0].Params;
                            }

                            if (!string.IsNullOrEmpty(path))
                            {
                                //引导未开启
                                bool isRuning = GuideIsRunning.Invoke(ScriptTable, path, true);
                                if (!isRuning)
                                {
                                    TestTutorial(id);
                                }
                            }
                        }*/
                            
                    }
                }
            }
        }

        public void EndGuide(int id)
        {
            if (LocalDataManager.Instance.M_TutorialConfig.list.ContainsKey(id))
            {
                var config = LocalDataManager.Instance.M_TutorialConfig.list[id];
                if (config != null)
                {
                    TryFinishTutorial(id,config.FlagIndex);
                }
            }
        }
        
        public void TryFinishTutorial(int id, int flagid)
        {
            //修改Flag
            if (!IsFinishGuide(flagid) && flagid > 0)
            {
                GuideSignSetReq req = new();
                req.Index = flagid;
                SocketBase battleSocket = NetworkManager.Instance.gameSocket;
                battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, req, isAutoChangeMsgTarget: false);
            }


            if (GuideIsRunning != null)
            {
                if (LocalDataManager.Instance.M_TutorialConfig.list.ContainsKey(id))
                {
                    var config = LocalDataManager.Instance.M_TutorialConfig.list[id];
                    if (config != null)
                    {
                        string path = string.Empty;
                        if (config.TutorialTargets != null && config.TutorialTargets.Count > 0)
                        {
                            path = config.TutorialTargets[0].Params;
                        }

                        if (!string.IsNullOrEmpty(path))
                        {
                            bool isRuning = GuideIsRunning.Invoke(ScriptTable, path, false);
                            if (isRuning)
                            {
                                if (OnBreakGuide != null)
                                {
                                    OnBreakGuide?.Invoke(ScriptTable, path, id, true);
                                }

                                //OnMarkFlag(id);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取下一个引导
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetNextGuildID(int id)
        {
            if (m_IsRunnigGuide.Contains(id))
            {
                m_IsRunnigGuide.Remove(id);
            }

            if (LocalDataManager.Instance.M_TutorialConfig.list.TryGetValue(id, out var config))
            {
                int nextID = config.NextID;
                if (nextID > 0)
                {
                    if (Tutorials.TryGetValue(nextID, out var instance) && instance != null)
                    {
                        if (instance.IsMateCondition())
                        {
                            TestTutorial(nextID);
                            return nextID;
                        }

                        return -1;
                    }
                    else
                    {
                        TestTutorial(nextID);
                        return nextID;
                    }
                }
            }

            return -1;
        }

        public bool IsFinishGuide(int index)
        {
            return ContainKey(index) == 1;
        }

        public int ContainKey(int index)
        {
            if (baseBinary == null)
            {
                return 0;
            }
            byte[] datas = baseBinary.Data.ToByteArray();
            int dlen = baseBinary.OneDataBitNum;
            int ai = index / (8 / dlen);
            byte bitnum = (byte)(index % (8 / dlen) * dlen);
            if (ai >= baseBinary.ArrayLen)
            {
                return 0;
            }

            byte d = datas[ai];
            if (dlen == 8)
            {
                return d;
            }

            byte mask = (byte)((1 << dlen) - 1);
            return (d & (mask << bitnum)) >> bitnum;
        }

        public bool UpData(int index, int val)
        {
            byte bval = (byte)val;
            byte[] datas = baseBinary.Data.ToByteArray();
            int dlen = baseBinary.OneDataBitNum;
            int ai = index / (8 / dlen);
            byte bitnum = (byte)(index % (8 / dlen) * dlen);
            if (ai >= baseBinary.ArrayLen)
            {
                return false;
            }

            byte key = datas[ai];
            if (dlen == 8)
            {
                key = bval;
            }
            else
            {
                int tmp = (1 << dlen) - 1;
                bval = (byte)(bval & tmp);
                tmp = (byte)(tmp << bitnum);
                tmp = key & ~tmp;
                key = (byte)(tmp + (bval << bitnum));
            }

            datas[ai] = key;
            baseBinary.Data = ByteString.CopyFrom(datas);
            return true;
        }

        private void InitFlag(System.Action finish)
        {
            Flags.Clear();
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<TutorialFlagConfig>(
                "config/guide/tutorialflagconfig", (config) =>
                {
                    for (int i = 1; i < config.Flags.Count; i++)
                    {
                        if (config.Flags[i].Delete)
                        {
                            continue;
                        }

                        //已经完成忽略
                        if (IsFinishGuide(i))
                        {
                            continue;
                        }

                        TutorialFlagInstance instance =
                            new(i, config.Flags[i].TaskID, config.Flags[i].MapID);
                        Flags.Add(i, instance);
                    }

                    finish?.Invoke();
                });
        }


        private void OnRoleCreamComplete(object arg0)
        {
            //return;
            bool skip = false;
            m_IsRunnigGuide.Clear();
            if (KeyExists(GUIDESKIP))
            {
                skip = Load<bool>(GUIDESKIP);
            }

            if (skip)
            {
                return;
            }

            Action<UIWidget> action = (UIWidget ui) =>
            {
                if (ui != null)
                {
                    LuaUIWidget luawidget = ui as LuaUIWidget;
                    if (luawidget != null)
                    {
                        ScriptTable = luawidget.GetLuaPanel().ScriptTable;
                        if (ScriptTable != null)
                        {
                            //C# Call Lua
                            OnBeigen = ScriptTable.Get<OnBeigenDelegate>("OnBeigen");
                            OnBreakGuide = ScriptTable.Get<OnBreakGuide>("FinishTutorial");
                            GuideIsRunning = ScriptTable.Get<OnGuideIsRunning>("GuideIsRunning");
                            //Lua Call C#
                            ScriptTable.Set("GetNextGuildID", GetNextGuideIDDelegete);
                            ScriptTable.Set("SetMarkFlag", SetMarkFlag);
                            Release();
                            PartnerTutorial?.Init();
                            TaskTutorial?.Init();
                            baseBinary = BusinessManager.Instance.M_UserSundryMD.GuideSign;
                            InitFlag(() =>
                            {
                                Tutorials.Clear();
                                var list = LocalDataManager.Instance.M_TutorialConfig.list;
                                foreach (var item in list)
                                {
                                    //已经完成忽略
                                    if (IsFinishGuide(item.Value.FlagIndex))
                                    {
                                        continue;
                                    }

                                    if (!Flags.ContainsKey(item.Value.FlagIndex))
                                    {
                                        continue;
                                    }


                                    //设置标记
                                    if (item.Value.FlagIndex > 0)
                                    {
                                        if (Flags.ContainsKey(item.Value.FlagIndex))
                                        {
                                            Flags[item.Value.FlagIndex].AppendTutorialID(item.Key);
                                        }
                                    }

                                    //有条件 且 非断点
                                    if (item.Value.TriggerConditions.Count > 0 /*&& item.Value.IsPoint*/)
                                    {
                                        Tutorials.Add(item.Key,
                                            new TutorialInstance(item.Value, OnTutorialActive, IsFinishGuide));
                                    }
                                }

                                DelayInvoker.DelayInvoke(2, (a) =>
                                {
                                    foreach (var instance in Flags)
                                    {
                                        if (instance.Value.TaskID > 0)
                                        {
                                            if (TaskHelper.IsTaskFinsh(instance.Value.TaskID))
                                            {
                                                OnTaskStateChange(3, (int)instance.Value.TaskID);
                                            }
                                        }
                                    }
                                });
                            });
                        }
                    }
                }
            };
            UIManager.Instance.OpenWidgetAsync(UIDef.TutorialWidget, action, false, null,
                UIRoot.UiGuideUIRoot.transform, MainPageCommond.HideNone, true);
        }

        public void OnMarkFlag(int id)
        {


            if (LocalDataManager.Instance.M_TutorialConfig.list.TryGetValue(id, out var config))
            {
                if (m_IsRunnigGuide.Contains(config.ID))
                {
                    m_IsRunnigGuide.Remove(config.ID);
                }

                if (config.SetFlagIndex > 0)
                {
                    GuideSignSetReq req = new();
                    req.Index = config.SetFlagIndex;
                    SocketBase battleSocket = NetworkManager.Instance.gameSocket;
                    battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, req, isAutoChangeMsgTarget: false);
                }
            }
        }

        public void TestTutorial(int id)
        {
            if (LocalDataManager.Instance.M_TutorialConfig.list.TryGetValue(id, out var config))
            {
                OnTutorialActive(config);
            }
        }

        public bool GuideFinished(int id)
        {
            if (LocalDataManager.Instance.M_TutorialConfig.list.TryGetValue(id, out var config))
            {
                return IsFinishGuide(config.FlagIndex);
            }
            return false;
        }

        private void OnTutorialActive(TutorialConfig Config)
        {
            GameManager.Instance.EventPreNewPlayerEvent($"3_{Config.ID}");
            OnBeigen?.Invoke(ScriptTable, Config);
            if (!m_IsRunnigGuide.Contains(Config.ID))
            {
                m_IsRunnigGuide.Add(Config.ID);
            }
        }

        public bool IsRuningGuide(int id)
        {
            if (m_IsRunnigGuide.Contains(id))
            {
                return true;
            }
            return false;
        }
        public override void Release()
        {
            m_IsRunnigGuide.Clear();
            PartnerTutorial?.OnRelease();
            TaskTutorial?.OnRelease();
        }
    }
}