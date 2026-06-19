///--------------------------------------------------------------------
/// 文件名   :   EctypeModule.cs
/// 内  容   :   副本系统
/// 说  明   :  
/// 创建日期 :   2022/12/02 14:54:20
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using Cinemachine;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.UI.Framework;
using SGF.Unity;
using SGF.Utlis;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.SDK;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Module
{
    public class EctypeModule : BusinessModule
    {
        public int MapID { get; private set; }

        public SpaceType MapSpaceType { get; private set; }

        public EctypeData EctypeTargetData
        {
            get { return ectypeTargetData; }
        }

        private EctypeData ectypeTargetData;

        private int LockShowUI = 0;

        private bool EnterIsPlayWave = false;

        private Dictionary<int, List<EctypeTargetDataCell>> Configs = null;

        public long EctypeEndTime { get; private set; }

        public override void Create(object args = null)
        {
            ectypeTargetData = new EctypeData();
            ectypeTargetData.GoalId = 0;
            ectypeTargetData.IsNew = false;
            Configs = new Dictionary<int, List<EctypeTargetDataCell>>();
            // 场景切换
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.EnterSpaceNtfID, OnEnterSpace, this);
            //副本开始
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GameStartID, OnGameStart, this);
            //副本结束
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GameEndID, OnGameEnd, this);
            //副本任务目标更新
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.InstanceDataRetID, OnInstanceData, this);
            //GNG副本开始
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GNGStartNtfID, OnGNGStartNtf, this);

            Configs.Clear();

            var data = LocalDataManager.Instance.M_EctypeTarget;
            if (data != null && data.StaticEctypeTargetDatas.Count > 0)
            {
                foreach (var item in data.StaticEctypeTargetDatas)
                {
                    if (Configs.ContainsKey(item.Value.GetGoalId()))
                    {
                        Configs[item.Value.GetGoalId()].Add(item.Value);
                    }
                    else
                    {
                        List<EctypeTargetDataCell> list = new();
                        list.Add(item.Value);
                        Configs.Add(item.Value.GetGoalId(), list);
                    }
                }
            }
        }

        private void ShowMirrorTreasure(InstanceDataRet instacne)
        {
            if (instacne != null)
            {
                ectypeTargetData.IsNew = false;
                ectypeTargetData.GoalId = instacne.Goal;
                ectypeTargetData.FollowID = instacne.FollowID;
                ectypeTargetData.list.Clear();
                //ectypeTargetData.TileName = GameConfig.LocalStr["TreasureCave"];
                ectypeTargetData.TileName = LanguageManager.Instance.GetLanguageByKey("TreasureCave");
                if (instacne.Mons != null && instacne.Mons.Count > 0)
                {
                    foreach (var item in instacne.Mons)
                    {
                        var data = new MonData()
                        { MonsterID = item.Value.Index, Max = item.Value.Max, Dead = item.Value.Dead };
                        if (data != null)
                        {
                            ectypeTargetData.list.Add(item.Value.Index, data);
                            var showName = GetMonsterName(item.Value.Index);

                            TargetContent targetContent = new();
                            string result = "";
                            //result = string.Format(GameConfig.LocalStr["DefeatCount"], showName) + "(" + data.Dead + "/" + data.Max + ")";
                            result = string.Format(LanguageManager.Instance.GetLanguageByKey("DefeatCount"), showName) + "(" + data.Dead + "/" + data.Max + ")";
                            targetContent.Content = result;
                            ectypeTargetData.Contents.Add(targetContent);
                        }
                    }
                }

                GlobalEvent.OnEctypeTargetDataChange?.Invoke(ectypeTargetData);
            }
        }

        // 通知副本目标（任务进度）
        private void OnInstanceData(MessageHandleData data)
        {
            InstanceDataRet instacne = (InstanceDataRet)data.data;
            //SGF.Debuger.LogWarning($"通知副本目标 时间={SGF.Time.TimeUtils.ServerNow} 副本数据通知到了  -------------------- ");

            if (instacne != null)
            {
                /*if (MapSpaceType == SpaceType.SpaceMirrorTreasure)
                {
                    ShowMirrorTreasure(instacne);
                }
                else
                {*/


                if (instacne.Goal == 0)
                {
                    return;
                }

                ectypeTargetData.IsNew = false;
                if (ectypeTargetData.GoalId != 0 && instacne.Goal != ectypeTargetData.GoalId)
                {
                    ectypeTargetData.IsNew = true;
                    LockShowUI++;
                    // 延迟1秒是为了播放完动画
                    DelayInvoker.DelayInvoke(this, 1, (object[] args) =>
                    {
                        LockShowUI--;
                        if (LockShowUI < 1)
                        {
                            ectypeTargetData.IsNew = true;
                            // StarDebug.LogError("111111111111 ectypeTargetData.list.Count" +
                            //  ectypeTargetData.list.Count);
                            GlobalEvent.OnEctypeTargetDataChange?.Invoke(ectypeTargetData);
                        }
                        else
                        {
                            // StarDebug.LogError("LockShowUI Is Lock" + LockShowUI);
                        }
                    });
                }

                ectypeTargetData.GoalId = instacne.Goal;
                ectypeTargetData.FollowID = instacne.FollowID;
                ectypeTargetData.list.Clear();
                //StarDebug.LogError("=================begine============= " + instacne.Goal + "=========" +
                // ectypeTargetData.GoalId);
                ectypeTargetData.blackboardList.Clear();
                if (instacne.Mons != null && instacne.Mons.Count > 0)
                {
                    foreach (var item in instacne.Mons)
                    {
                        // StarDebug.LogError($"ID={item.Value.Index},Max={item.Value.Max} ");
                        ectypeTargetData.list.Add(item.Value.Index,
                            new MonData()
                            {
                                MonsterID = item.Value.Index,
                                Max = item.Value.Max,
                                Dead = item.Value.Dead,
                                Alias = item.Value.Title
                            });
                    }
                }

                if (instacne.BlackBoard != null && instacne.BlackBoard.Count > 0)
                {
                    foreach (var item in instacne.BlackBoard)
                    {
                        // StarDebug.LogError($"ID={item.Value.Index},Max={item.Value.Max} ");
                        Dictionary<string, int> dic = new();
                        foreach (var item2 in item.Value.SingleData)
                        {
                            dic.Add(item2.Key, item2.Value);
                        }
                        ectypeTargetData.blackboardList.Add(item.Key, dic);
                    }
                    CheckBlackBoardData();
                }
                //StarDebug.LogError("=================end============= ");

                Updata();
                if (LockShowUI < 1)
                {
                    //  StarDebug.LogError("222222222222 ectypeTargetData.list.Count" + ectypeTargetData.list.Count);
                    SpaceType spaceType = GameManager.Instance.GetCurMapType();
                    bool isShow = false;
                    if (spaceType == SpaceType.SpacePlot)
                    {
                        string mapSubType = GameManager.Instance.GetMapSubType();
                        bool isHide2 = mapSubType == GameConfig.INSTANCE_PLOT_FIRST || mapSubType == GameConfig.INSTANCE_PLOT_THIRD;
                        bool isShow2 = true;
                        if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
                        {
                            isShow2 = BusinessManager.Instance.GetXinSGStateDic("-1");
                        }
                        isShow = !isHide2 && isShow2;
                    }
                    else
                    {
                        isShow = true;
                    }
                    if (isShow)
                    {
                        GlobalEvent.OnEctypeTargetDataChange?.Invoke(ectypeTargetData);
                    }
                }
                else
                {
                    //  StarDebug.LogError("LockShowUI Is Lock" + LockShowUI);
                }
                // }
            }
        }

        private Dictionary<string, CameraEffectLoadInfo> m_CameraStateDic = new();

        private List<int> m_XinSGStateDic = new();

        private void CheckBlackBoardData()
        {
            if (ectypeTargetData == null)
            {
                return;
            }
            Dictionary<string, int> blackBoardList = ectypeTargetData.GetBlackBoardData(GameManager.Instance.mainPlayerId);
            if (blackBoardList != null)
            {
                foreach (var item in blackBoardList)
                {
                    //SGF.Debuger.Log($"副本黑板数据 key={item.Key},value={item.Value}");
                    bool isHaveCamera = item.Key.StartsWith("camera_");
                    if (isHaveCamera)
                    {
                        PlayCameraEffect(item.Value);
                    }
                    bool isHaveXinSGShowUI = item.Key.StartsWith("XinSGShowUI");
                    if (isHaveXinSGShowUI)
                    {
                        PlayXinSGShowUI(item.Value);
                    }
                }
            }
        }

        private void PlayCameraEffect(int configID)
        {
            SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切换相机角度 id={configID}======================开始");
            var cfgData = LocalDataManager.Instance.GetCMeffectDataCell(configID);
            if (cfgData != null)
            {
                string cameraName = cfgData.Args1;
                int state = int.Parse(cfgData.Args3);
                if (state == 1)
                {
                    //CameraEffectLoadInfo cameraEffectLoadInfo = null;
                    //if (m_CameraStateDic.TryGetValue(cameraName, out cameraEffectLoadInfo))
                    //{
                    //    return;
                    //}
                    //else
                    //{
                    //    cameraEffectLoadInfo = new CameraEffectLoadInfo(cameraName, UIAsyncLoadState.LoadFinish, configID);
                    //    GameObject CameraStatic = GameObject.Find($"CameraStatic");
                    //    if (CameraStatic != null)
                    //    {
                    //        GameObject virtualCameraGob = CameraStatic.transform.Find($"{cameraName}").gameObject;
                    //        if (virtualCameraGob != null)
                    //        {
                    //            cameraEffectLoadInfo.CameraGob = virtualCameraGob;
                    //        }
                    //        m_CameraStateDic.Add(cameraName, cameraEffectLoadInfo);
                    //    }
                    //    else
                    //    {
                    //        GameObject virtualCameraGob = GameObject.Find($"CameraStatic/{cameraName}");
                    //        if (virtualCameraGob != null)
                    //        {
                    //            cameraEffectLoadInfo.CameraGob = virtualCameraGob;
                    //        }
                    //        m_CameraStateDic.Add(cameraName, cameraEffectLoadInfo);
                    //    }
                    //}
                    //if (GameManager.Instance.M_MainPlayerCtrlBase != null)
                    //{
                    //    Transform m_container = UnityExtension.FindFunc(GameManager.Instance.M_MainPlayerCtrlBase.Container.transform, "ModelOffset");
                    //    if (m_container != null)
                    //    {
                    //        var cam = cameraEffectLoadInfo.CameraGob.GetComponent<CinemachineVirtualCamera>();
                    //        cam.Follow = m_container;
                    //        cameraEffectLoadInfo.CameraGob.SetActive(true);
                    //    }
                    //    else
                    //    {
                    //        GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnViewCreateFinish += OnActionOnViewCreateFinish;
                    //    }
                    //}
                    //else
                    //{
                    //    GlobalEvent.OnRoleCreateComplete.AddListener(OnRoleCreateComplete);
                    //}
                    //return;
                    CameraEffectLoadInfo cameraEffectLoadInfo = null;
                    if (m_CameraStateDic.TryGetValue(cameraName, out cameraEffectLoadInfo))
                    {
                        if (cameraEffectLoadInfo.State == UIAsyncLoadState.InLoaded)
                        {
                            return;
                        }
                        else if (cameraEffectLoadInfo.State == UIAsyncLoadState.LoadFinish && cameraEffectLoadInfo.CameraGob != null)
                        {
                            GameManager.Instance.EventPreNewPlayerEvent($"10_{configID}");
                            cameraEffectLoadInfo.CameraGob.SetActive(true);
                            return;
                        }
                    }
                    else
                    {
                        cameraEffectLoadInfo = new CameraEffectLoadInfo(cameraName, UIAsyncLoadState.InLoaded, configID);
                        m_CameraStateDic.Add(cameraName, cameraEffectLoadInfo);
                    }
                    GameManager.Instance.EventPreNewPlayerEvent($"10_{configID}");
                    string path = $"Prefabs/VirtualCamera/{cameraName}";
                    //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切换相机角度 id{configID}={cameraName}------开始");
                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(path,
                    (GameObject go) =>
                    {
                        if (go == null)
                        {
                            return;
                        }
                        {
                            CameraEffectLoadInfo cameraEffectLoadInfo = null;
                            if (m_CameraStateDic.TryGetValue(cameraName, out cameraEffectLoadInfo))
                            {
                                cameraEffectLoadInfo.State = UIAsyncLoadState.LoadFinish;
                            }
                            else
                            {
                                return;
                            }
                        }
                        cameraEffectLoadInfo.CameraGob = GameObject.Instantiate<GameObject>(go);
                        cameraEffectLoadInfo.CameraGob.name = cameraName;
                        //cameraEffectLoadInfo.CameraGob.transform.SetParent(EntityRoot.Instance.DotRemoveRoot.transform);
                        if (GameManager.Instance.M_MainPlayerCtrlBase != null)
                        {
                            Transform m_container = UnityExtension.FindFunc(GameManager.Instance.M_MainPlayerCtrlBase.Container.transform, "ModelOffset");
                            if (m_container != null)
                            {
                                var cam = cameraEffectLoadInfo.CameraGob.GetComponent<CinemachineVirtualCamera>();
                                cam.Follow = m_container;
                                cameraEffectLoadInfo.CameraGob.SetActive(true);
                               // object[] Os = new object[] { cameraEffectLoadInfo.CameraGob, true };
                               // DelayInvoker.DelayInvoke(0.01f,
                               //        //延迟处理--------------------
                               //        (object[] args) =>
                               //        {
                               //            GameObject gob = (GameObject)args[0];
                               //            if (gob != null)
                               //            {
                               //                bool isShow = (bool)args[1];
                               //                gob.SetActive(isShow);
                               //            }
                               //        }
                               //, Os);
                            }
                            else
                            {
                                GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnViewCreateFinish += OnActionOnViewCreateFinish;
                            }
                        }
                        else
                        {
                            GlobalEvent.OnRoleCreateComplete.AddListener(OnRoleCreateComplete);
                        }
                        //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切换相机角度 id{configID}={cameraName}--------完成");
                    });
                }
                else
                {
                    GameManager.Instance.GetGameCameraComponent().M_CinemachineBrain.m_CameraActivatedEvent.AddListener(OnCameraActivatedEvent);
                    //GameObject CameraStatic = GameObject.Find($"CameraStatic");
                    //if (CameraStatic != null)
                    //{
                    //    GameObject virtualCameraGob = CameraStatic.transform.Find($"{cameraName}").gameObject;
                    //    if (virtualCameraGob != null)
                    //    {
                    //        virtualCameraGob.SetActive(false);
                    //    }
                    //}
                    //m_CameraStateDic.Remove(cameraName);
                    //return;

                    SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切换相机角度  关闭 configID={configID}");
                    CameraEffectLoadInfo cameraEffectLoadInfo = null;
                    if (m_CameraStateDic.TryGetValue(cameraName, out cameraEffectLoadInfo))
                    {
                        // object[] Os = new object[] { cameraEffectLoadInfo.CameraGob, false };
                        // DelayInvoker.DelayInvoke(0.01f,
                        //        //延迟处理--------------------
                        //        (object[] args) =>
                        //        {
                        //            GameObject gob = (GameObject)args[0];
                        //            if (gob != null)
                        //            {
                        //                bool isShow = (bool)args[1];
                        //                gob.SetActive(isShow);
                        //            }
                        //        }
                        //, Os);
                        cameraEffectLoadInfo.CameraGob.SetActive(false);
                        //GameObject.Destroy(cameraEffectLoadInfo.CameraGob);
                    }
                    m_CameraStateDic.Remove(cameraName);
                }
            }
        }

        private void OnCameraActivatedEvent(ICinemachineCamera arg0, ICinemachineCamera arg1)
        {
            int effectID = 0;
            if (arg0 != null)
            {
                SGF.Debuger.LogWarning($"arg0={arg0.Name}");
            }
            if (arg1 != null)
            {
                SGF.Debuger.LogWarning($"arg1={arg1.Name}");
                if (m_CameraStateDic.TryGetValue(arg1.Name, out var cameraEffectLoadInfo))
                {
                    effectID = cameraEffectLoadInfo.EffectID;
                }
            }

            SocketBase battleSocket = NetworkManager.Instance.gameSocket;
            CurEffOverReq notify = new();
            notify.EffectID = effectID;
            notify.OptIndex = 0;
            battleSocket.SendRPCMsg(ServerType.ServerTypeSpace, notify, isEncrypt: false);

            GameManager.Instance.GetGameCameraComponent().M_CinemachineBrain.m_CameraActivatedEvent.RemoveListener(OnCameraActivatedEvent);
        }

        private void OnRoleCreateComplete(object arg0)
        {
            CameraEffectLoadInfo cameraEffectLoadInfo = null;

            foreach (var item in m_CameraStateDic)
            {
                if (cameraEffectLoadInfo == null)
                {
                    if (item.Value.CameraGob != null && !item.Value.CameraGob.activeSelf && item.Value.State == UIAsyncLoadState.LoadFinish)
                    {
                        cameraEffectLoadInfo = item.Value;
                        if (GameManager.Instance.M_MainPlayerCtrlBase != null)
                        {
                            Transform m_container = UnityExtension.FindFunc(GameManager.Instance.M_MainPlayerCtrlBase.Container.transform, "ModelOffset");
                            if (m_container != null)
                            {
                                var cam = cameraEffectLoadInfo.CameraGob.GetComponent<CinemachineVirtualCamera>();
                                cam.Follow = m_container;
                                cameraEffectLoadInfo.CameraGob.SetActive(true);
                                // object[] Os = new object[] { cameraEffectLoadInfo.CameraGob, true };
                                // DelayInvoker.DelayInvoke(0.01f,
                                //        //延迟处理--------------------
                                //        (object[] args) =>
                                //        {
                                //            GameObject gob = (GameObject)args[0];
                                //            if (gob != null)
                                //            {
                                //                bool isShow = (bool)args[1];
                                //                gob.SetActive(isShow);
                                //            }
                                //        }
                                //, Os);
                            }
                            else
                            {
                                GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnViewCreateFinish += OnActionOnViewCreateFinish;
                            }
                        }
                    }
                }
            }
            GlobalEvent.OnRoleCreateComplete.RemoveListener(OnRoleCreateComplete);
        }

        private void OnActionOnViewCreateFinish()
        {
            if (m_CameraStateDic != null)
            {
                foreach (var item in m_CameraStateDic)
                {
                    if (item.Value.CameraGob != null && !item.Value.CameraGob.activeSelf && item.Value.State == UIAsyncLoadState.LoadFinish)
                    {
                        Transform m_container = UnityExtension.FindFunc(GameManager.Instance.M_MainPlayerCtrlBase.Container.transform, "ModelOffset");
                        if (m_container != null)
                        {
                            var cam = item.Value.CameraGob.GetComponent<CinemachineVirtualCamera>();
                            cam.Follow = m_container;
                            item.Value.CameraGob.SetActive(true);
                            // object[] Os = new object[] { item.Value.CameraGob, true };
                            // DelayInvoker.DelayInvoke(0.01f,
                            //        //延迟处理--------------------
                            //        (object[] args) =>
                            //        {
                            //            GameObject gob = (GameObject)args[0];
                            //            if (gob != null)
                            //            {
                            //                bool isShow = (bool)args[1];
                            //                gob.SetActive(isShow);
                            //            }
                            //        }
                            //, Os);

                            if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.M_Curr != null)
                            {
                                GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnViewCreateFinish -= OnActionOnViewCreateFinish;
                            }
                        }
                        return;
                    }
                }
            }
        }

        private void PlayXinSGShowUI(int configID)
        {
            //bool isHave = GetXinSGStateIsHave(configID);
            //if (isHave)
            //{
            //    SGF.Debuger.LogWarning($"新手关新流程 UI显示效果 Id={configID} 已经处理过了");
            //    return;
            //}
            SetXinSGStateDic(configID);
            SGF.Debuger.LogWarning($"新手关新流程 UI显示效果 时间={SGF.Time.TimeUtils.ServerNow} 打开 id={configID}======================开始");
            var cfgData = LocalDataManager.Instance.GetCMeffectDataCell(configID);
            if (cfgData != null)
            {
                string[] UIIndexs = cfgData.Args3.Split(",");
                SGF.Debuger.LogWarning($"新手关新流程 UI显示效果 时间={SGF.Time.TimeUtils.ServerNow} 打开 id={configID}======================Args3={cfgData.Args3}");

                if (UIIndexs.Length <= 0)
                {
                    return;
                }
                foreach (var item in UIIndexs)
                {
                    BusinessManager.Instance.SetXinSGStateDic(item, true);
                    if (item == "-1")
                    {
                        GlobalEvent.OnEntityEctype?.Invoke(true);
                    }
                }
                GlobalEvent.OnXinSGUIShow?.Invoke(null);
            }
        }

        public bool GetXinSGStateIsHave(int cfgID)
        {
            return m_XinSGStateDic.Contains(cfgID);
        }

        public void SetXinSGStateDic(int cfgID)
        {
            if (m_XinSGStateDic.Contains(cfgID))
            {
                SGF.Debuger.LogWarning($"新手关新流程 UI显示效果 Id={cfgID} 已经存在了 err!!!");
            }
            else
            {
                m_XinSGStateDic.Add(cfgID);
            }
        }

        private void Updata()
        {
            ectypeTargetData.Contents.Clear();
            if (Configs.ContainsKey(ectypeTargetData.GoalId))
            {
                var list = Configs[ectypeTargetData.GoalId];

                int index = 1;
                Dictionary<int, GoalData> groups = new();
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        var group = item.GetTargetGroup();
                        if (group == 0)
                        {
                            GoalData data = new();
                            data.Desc = item.Desc;
                            data.Spid = item.GetSpid();
                            data.GoalType = item.GetGoalType();
                            data.Param = item.GetParam();
                            data.Dead = 0;
                            var monData = ectypeTargetData.GetData(item.GetParam());
                            data.Max = item.GetMaxNum();
                            if (monData != null)
                            {
                                data.Dead = monData.Dead;
                                if (data.Max < 1)
                                {
                                    data.Max = monData.Max;
                                }
                            }

                            groups.Add(index++, data);

                        }
                        else
                        {
                            if (groups.ContainsKey(item.GetTargetGroup()))
                            {
                                var data = groups[item.GetTargetGroup()];
                                if (data != null)
                                {
                                    var monData = ectypeTargetData.GetData(item.GetParam());
                                    int max = item.GetMaxNum();
                                    int dead = 0;
                                    if (monData != null)
                                    {
                                        dead = monData.Dead;
                                        if (max < 1)
                                        {
                                            max = monData.Max;
                                        }
                                    }

                                    data.Dead += dead;
                                    data.Max += max;
                                }

                            }
                            else
                            {
                                GoalData data = new();
                                data.Desc = item.Desc;
                                data.Spid = item.GetSpid();
                                data.GoalType = item.GetGoalType();
                                data.Param = item.GetParam();
                                data.Dead = 0;
                                var monData = ectypeTargetData.GetData(item.GetParam());
                                data.Max = item.GetMaxNum();
                                if (monData != null)
                                {
                                    data.Dead = monData.Dead;
                                    if (data.Max < 1)
                                    {
                                        data.Max = monData.Max;
                                    }
                                }

                                groups.Add(item.GetTargetGroup(), data);
                            }
                        }
                    }
                }
                if (groups != null && groups.Count > 0)
                {
                    ectypeTargetData.TileName = list[0].Title;
                    foreach (var item in groups)
                    {
                        //杀怪类型 并且 参数为0 直接全部显示服务器 同步过来的数据
                        if (item.Value.GoalType == (int)EctypeTargetType.KillMonster && item.Value.Param == 0)
                        {
                            foreach (var mon in ectypeTargetData.list)
                            {
                                var showName = mon.Value.Alias;
                                if (string.IsNullOrEmpty(showName))
                                {
                                    showName = GetMonsterName(mon.Value.MonsterID);
                                }

                                TargetContent targetContent = new();
                                string result = "";
                                targetContent.spid = item.Value.Spid;
                                //result = string.Format(GameConfig.LocalStr["DefeatCount"], showName) + "(" + mon.Value.Dead + "/" + mon.Value.Max + ")";
                                result = string.Format(LanguageManager.Instance.GetLanguageByKey("DefeatCount"), showName) + "(" + mon.Value.Dead + "/" + mon.Value.Max + ")";
                                targetContent.Content = result;
                                ectypeTargetData.Contents.Add(targetContent);
                            }
                        }
                        else
                        {
                            ectypeTargetData.Contents.Add(GetTargetDesc(item.Value));
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"没找到配置 {ectypeTargetData.GoalId}");
            }
        }

        private TargetContent GetTargetDesc(GoalData dataCell)
        {
            TargetContent targetContent = new();
            string result = dataCell.Desc;
            string showName = string.Empty;
            targetContent.spid = dataCell.Spid;
            switch (dataCell.GoalType)
            {
                case (int)EctypeTargetType.KillMonster:

                    showName = GetMonsterName(dataCell.Param);
                    result = string.Format(dataCell.Desc, showName) + "(" + dataCell.Dead + "/" + dataCell.Max + ")";
                    break;
                case (int)EctypeTargetType.InterAction:
                    showName = GetObjectName((int)dataCell.Param);
                    result = string.Format(dataCell.Desc, showName);
                    break;
                case (int)EctypeTargetType.NPCDialoge:
                    showName = GetNpcName((int)dataCell.Param);
                    result = string.Format(dataCell.Desc, showName);
                    break;
                case (int)EctypeTargetType.FindPath:
                    result = dataCell.Desc;
                    break;
            }

            targetContent.Content = result;
            return targetContent;
        }

        private string GetObjectName(int objectid)
        {
            var data = LocalDataManager.Instance.GetInteractDataCell(objectid);
            if (data != null)
            {
                return data.ModelName;
            }

            return string.Empty;
        }

        private string GetNpcName(int npcid)
        {
            var data = LocalDataManager.Instance.GetNPCDataCell((uint)npcid);
            if (data != null)
            {
                return data.Name;
            }

            return string.Empty;
        }

        private string GetMonsterName(long monsterId)
        {
            var data = LocalDataManager.Instance.GetMonsterDataCell(monsterId);
            if (data != null)
            {
                return data.Name;
            }

            return string.Empty;
        }




        private void OnGNGStartNtf(MessageHandleData data)
        {
            GNGStartNtf game = (GNGStartNtf)data.data;
            if (game != null)
            {
                EctypeEndTime = game.GameEndTime;
                SGF.Debuger.Log($"副本结束时间 EctypeEndTime={EctypeEndTime}");
                GlobalEvent.OnFreshExitTime.Invoke(EctypeEndTime);
            }


        }


        private void OnGameStart(MessageHandleData data)
        {
            GameStart game = (GameStart)data.data;
            if (game != null)
            {
                EctypeEndTime = game.GameEndTime;
                SGF.Debuger.Log($"副本结束时间 EctypeEndTime={EctypeEndTime}");
            }

            // 镜像副本判断[播放波纹]
            EnterIsPlayWave = false;
            if (GameManager.Instance.M_Map != null)
            {
                MapID = GameManager.Instance.M_Map.GetMapId();
                MapSpaceType = GameManager.Instance.M_Map.GetMapType();
                if (MapSpaceType == SpaceType.SpaceMirror)
                {
                    bool isPlay = true;
                    var cfg = LocalDataManager.Instance.GetMapCfgData(MapID);
                    if (cfg != null)
                    {
                        isPlay = cfg.IsShowEffect;
                    }

                    //if (isPlay)
                    {
                        PlayScreenWave();
                        EnterIsPlayWave = true;
                    }
                }

                if (MapSpaceType != SpaceType.SpaceSercet && MapSpaceType != SpaceType.SpaceWtask)
                {
                    GlobalEvent.OnFreshExitTime.Invoke(EctypeEndTime);
                }

                if (MapSpaceType == SpaceType.SpaceDaily)
                {
                    StarProject.Service.Battle.BattleManager.Instance.SetFindTargetPosFlag(true);
                }

                if (MapSpaceType == SpaceType.Space10V10)
                {
                    GlobalEvent.OnShowPvpWidget.Invoke(true);
                }


                //if(MapSpaceType == SpaceType.SpaceDaily)

                // {
                // StarProject.GlobalEvent.OnSetTaskWidgetShow.Invoke(false);
                //}
                // 除大场景、秘境、默认类型，其他副本都需要副本目标
                //if (MapSpaceType != SpaceType.SpaceSercet && MapSpaceType != SpaceType.SpaceScene && MapSpaceType != SpaceType.SpaceDefault)
                //{
                //    GlobalEvent.OnEntityEctype?.Invoke(true);
                //}
                // 只有大场景才打开任务左边挂件（副本里面基本是副本目标）
                // GlobalEvent.OnSetTaskWidgetShow?.Invoke(MapSpaceType == SpaceType.SpaceScene);
            }
        }


        public void OnCustomLevelEctype()
        {
            GlobalEvent.OnEntityEctype?.Invoke(false);
            GameInput.Instance?.ShowHideMedicine(true);

            ResetCacheData();

            SpaceType spaceType = GameManager.Instance.GetCurMapType();
            //公会领地
            if (spaceType != SpaceType.SpaceGuildTerritory)
            {
                //关闭公会领地侧边栏
                ModuleManager.Instance.SendMessage(ModuleDef.Name.PartyTimeModule, "OnPartyTimeLeave", new object[] { });
            }
        }

        private void ResetCacheData()
        {
            if (m_CameraStateDic != null)
            {
                foreach (var item in m_CameraStateDic)
                {
                    GameObject.Destroy(item.Value.CameraGob);
                }
                m_CameraStateDic.Clear();
            }

            if (m_XinSGStateDic != null)
            {
                m_XinSGStateDic.Clear();
            }

            BusinessManager.Instance.ClearXinSGStateDic();

            if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.M_Curr != null)
            {
                GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnViewCreateFinish -= OnActionOnViewCreateFinish;
            }
        }

        public void OnCustomEnterEctype()
        {
            ResetCacheData();

            //公会领地不处理
            if (MapSpaceType != SpaceType.SpaceGuildTerritory)
            {
                SpaceType spaceType = GameManager.Instance.GetCurMapType();
                bool isHide = false;
                string mapSubType = GameManager.Instance.GetMapSubType();
                if (spaceType == SpaceType.SpacePlot)
                {
                    isHide = mapSubType == GameConfig.INSTANCE_PLOT_FIRST || mapSubType == GameConfig.INSTANCE_PLOT_THIRD;
                }
                //公会领地
                if (spaceType == SpaceType.SpaceGuildTerritory)
                {
                    //判断公会领地是否开启了活动
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.PartyTimeModule, "OnCkeckAndOpenPartyTimeWidget", new object[] { });
                    GlobalEvent.OnSetTaskWidgetShow.Invoke(false);
                    isHide = true;//关闭左边侧边栏
                }
                bool isShow = true;
                if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
                {
                    isShow = BusinessManager.Instance.GetXinSGStateDic("-1");
                }
                GlobalEvent.OnEntityEctype?.Invoke(!isHide && isShow);
                if (MapSpaceType == SpaceType.Space10V10 || MapSpaceType == SpaceType.SpaceArena)
                {
                    GameInput.Instance?.ShowHideMedicine(false);
                }
            }
            else
            {
                GlobalEvent.OnEntityEctype?.Invoke(false);
                GlobalEvent.OnSetTaskWidgetShow.Invoke(false);
                //判断公会领地是否开启了活动
                ModuleManager.Instance.SendMessage(ModuleDef.Name.PartyTimeModule, "OnCkeckAndOpenPartyTimeWidget", new object[] { });
            }
        }

        private void OnGameEnd(MessageHandleData data)
        {
            GameEnd game = (GameEnd)data.data;
            if (game.MapID == MapID)
            {
                //GlobalEvent.OnEntityEctype?.Invoke(false);
                //GlobalEvent.OnSetTaskWidgetShow?.Invoke(true);

                LockShowUI = 0;
                MapID = 0;
                MapSpaceType = GameManager.Instance.M_Map.GetMapType();
                ectypeTargetData.GoalId = 0;
                ectypeTargetData.FollowID = 0;
                ectypeTargetData.IsNew = false;
                ectypeTargetData.list.Clear();
                GameManager.Instance.TriggerEvent(TriggerEventType.EctypeModuleGameEnd, false);
                EctypeEndTime = 0;
                if (EnterIsPlayWave)
                {
                    PlayScreenWave();
                }

                if (MapSpaceType == SpaceType.SpaceDaily)
                {
                    StarProject.Service.Battle.BattleManager.Instance.SetFindTargetPosFlag(false);

                }

                if (MapSpaceType == SpaceType.Space10V10)
                {
                    GlobalEvent.OnShowPvpWidget.Invoke(false);
                }
            }
        }

        private void PlayScreenWave(bool force = false)
        {
            /*
            string path = "Effects/UI/ScreenWave";
            StarProject.Service.Resource.ResourceManager.Instance.PopGameObject(path, (go) =>
            {
                OnLoadCallBack(path,go);
            });
            */

            /*Service.UniversalRenderPipeline.UniRenderPipline.Instance.SetScreenShockyRenderPassFeature(true);
            DelayInvoker.DelayInvoke(this, 1.0f,
                (object[] args) =>
                {
                    Service.UniversalRenderPipeline.UniRenderPipline.Instance.SetScreenShockyRenderPassFeature(false);
                });*/
            BusinessManager.Instance.PlayWaveEffect();
        }

        private void OnLoadCallBack(string path, GameObject go)
        {
            if (go != null)
            {
                go.transform.SetParent(UIFXRoot.UINttRoot.transform);
                go.transform.localPosition = UnityEngine.Vector3.zero;
                go.transform.localScale = UnityEngine.Vector3.one;
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.offsetMin = UnityEngine.Vector3.zero;
                rect.offsetMax = UnityEngine.Vector3.zero;
                DelayInvoker.DelayInvoke(this, 0.6f,
                    (object[] args) =>
                    {
                        StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(path, go);
                    });
            }
        }

        private void OnEnterSpace(MessageHandleData data)
        {
            if (GameManager.Instance.M_Map == null)
            {
                return;
            }
            
            var mst = GameManager.Instance.M_Map.GetMapType();
            GameInput.Instance?.ShowHideMedicine(mst != SpaceType.Space10V10 && mst != SpaceType.SpaceArena);
            if (data == null)
            {
                return;
            }
            //if (enterSpace != null)
            //{
            //    // StarDebug.Log(StarDebug.Green, $"进入场景,场景类型{enterSpace.SpType},副本ID{enterSpace.MapID}");

            //    if (enterSpace.SpType == SpaceType.SpaceInstance)
            //    {
            //        MapID = enterSpace.MapID;

            //        StarDebug.Log(StarDebug.Green, $"进入副本{MapID}");
            //        // InstanceDataReq();

            //    }
            //}
        }

        public override void Release()
        {
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.EnterSpaceNtfID, OnEnterSpace, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.InstanceDataRetID, OnInstanceData, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GameEndID, OnGameEnd, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GameStartID, OnGameStart, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GNGStartNtfID, OnGNGStartNtf, this);

            if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.M_Curr != null)
            {
                GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnViewCreateFinish -= OnActionOnViewCreateFinish;
            }

            base.Release();
        }

        /// <summary>
        /// 是否在副本
        /// </summary>
        /// <returns></returns>
        public bool InEctype()
        {
            return MapID > 0;
        }

        public bool IsCanPerfectReborn()
        {
            return MapSpaceType != SpaceType.Space10V10;
        }
    }
}

public enum EctypeTargetType
{
    KillMonster = 1, //--杀怪
    InterAction = 2, //---交互物
    NPCDialoge = 3, //--NPC对话
    FindPath = 4, //--目标点移动
}

[XLua.LuaCallCSharp]
public class EctypeData
{
    public int GoalId;
    public bool IsNew;
    public Dictionary<long, MonData> list = new();
    public Dictionary<ulong, Dictionary<string, int>> blackboardList = new();
    public List<TargetContent> Contents = new();
    public string TileName;
    public ulong FollowID;


    public bool HasFollow()
    {
        return FollowID != 0;
    }

    public UnityEngine.Vector3 GetFollowPostion()
    {
        var m_EntityCtr = GameManager.Instance.GetEntityCtr(FollowID);
        if (m_EntityCtr != null)
        {
            return m_EntityCtr.M_Curr.Position();
        }
        else
        {
            StarDebug.LogError($"找不到实体 FollowID{FollowID}");
        }

        return UnityEngine.Vector3.zero;
    }

    public MonData GetData(long mosterID)
    {
        if (list.TryGetValue(mosterID, out var data))
        {
            return data;
        }

        return null;
    }
    public Dictionary<string, int> GetBlackBoardData(ulong entityID)
    {
        if (blackboardList.TryGetValue(entityID, out var data))
        {
            return data;
        }

        return null;
    }

    public int GetSpawnerID()
    {
        if (Contents != null && Contents.Count > 0)
        {
            foreach (var item in Contents)
            {
                if (!item.Finished)
                {
                    return item.spid;
                }
            }
        }

        return 0;
    }
}

public class MonData
{
    public long MonsterID;
    public int Max;

    public int Dead;

    //别名
    public string Alias;
}

public class TargetContent
{
    public bool Finished;
    public string Content;
    public int spid;
}

public class GoalData
{
    public string Desc;
    public int Spid;
    public int GoalType;
    public long Param;
    public int Dead;
    public int Max;
}

public class CameraEffectLoadInfo
{
    public string Name;
    public UIAsyncLoadState State = UIAsyncLoadState.None;
    public GameObject CameraGob = null;
    public int EffectID;
    public CameraEffectLoadInfo(string name, UIAsyncLoadState state, int effectID)
    {
        Name = name;
        State = state;
        EffectID = effectID;
    }
}
