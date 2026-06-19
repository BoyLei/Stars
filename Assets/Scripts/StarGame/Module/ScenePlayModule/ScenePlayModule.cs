///--------------------------------------------------------------------
/// 文件名   :   ScenePlayModule
/// 内  容   :   场景玩法
/// 说  明   :  
/// 创建日期 :   2024/08/21 16:30:35
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections.Generic;
using SGF.Module.Framework;
using System;
using SGF;
using StarProject.Game;
using StarProject.Game.Map;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Module
{
    /// <summary>
    /// 定义 ScenePlayType 枚举，用于表示场景玩法类型。
    /// </summary>
    public enum ScenePlayType
    {
        /// <summary>
        /// 心灵追踪
        /// </summary>
        MindTracking = 1,
    }


    public class ScenePlayModule : BusinessModule
    {
        /// <summary>
        /// 场景播放类型工厂字典，用于存储场景播放类型及其对应的类型。
        /// 在这个字典中，场景播放类型（如心灵追踪）与实现该类型的类（如PlayMindTracking）关联。
        /// 这样，根据一个场景播放类型，程序可以找到相应的类型信息，以便进行实例化或其他操作。
        /// </summary>
        private Dictionary<ScenePlayType, BaseScenePlay> ScenePlayTypeFactory =
            new Dictionary<ScenePlayType, BaseScenePlay>()
            {
                //心灵追踪
                { ScenePlayType.MindTracking, new PlayMindTracking() }
            };

        /// <summary>
        /// 玩法状态字典，用于追踪特定玩法的状态。
        /// 这个字典将玩法ID映射到一个布尔值，指示该玩法的当前状态。
        /// 例如，如果字典中存在键值对 {1, true}，表示ID为1的玩法当前处于活动状态。
        /// 这个字典对于需要动态检查和更新特定玩法状态的应用程序非常有用。
        /// </summary>
        private Dictionary<int /*玩法ID*/, bool /*状态*/> PlayingStatus;

        /// <summary>
        /// 配置数据
        /// </summary>
        private Dictionary<int, List<TrackPlayDataCell>> mTrackPlayConfigDatas;

        /// <summary>
        /// 内部消息处理
        /// </summary>
        private Dictionary<string, System.Action<object[]>> InternalMessages;


        /// <summary>
        /// 当前的场景ID
        /// </summary>
        private int CurSceneID;

        // 定义一个字典，用于存储当前激活的场景播放类型及其对应的ID和时间
        private Dictionary<ScenePlayType, (int, int, float)> mCurrentActivedPlays;

        // 获取当前场景播放类型的属性，该属性为只读
        public ScenePlayType CurrentScenePlayType { get; private set; }

        // 获取当前播放ID的属性，该属性为只读
        public int CurrentPlayID { get; private set; }

        // 获取任务ID的属性，该属性为只读
        public int TaskID { get; private set; }


        /// <summary>
        /// 在对象创建时被调用的方法，可以在这里进行对象的初始化
        /// </summary>
        /// <param name="args">传入的参数，默认为空</param>
        public override void Create(object args = null)
        {
            if (InternalMessages == null)
            {
                InternalMessages = new Dictionary<string, System.Action<object[]>>();
            }

            InternalMessages.Clear();

            if (mTrackPlayConfigDatas == null)
            {
                mTrackPlayConfigDatas = new Dictionary<int, List<TrackPlayDataCell>>();
            }

            mTrackPlayConfigDatas.Clear();

            if (PlayingStatus == null)
            {
                PlayingStatus = new Dictionary<int, bool>();
            }

            PlayingStatus.Clear();

            if (mCurrentActivedPlays == null)
            {
                mCurrentActivedPlays = new Dictionary<ScenePlayType, (int, int, float)>();
            }

            mCurrentActivedPlays.Clear();

            var configs = LocalDataManager.Instance.M_TrackPlayData;
            if (configs != null)
            {
                foreach (var item in configs.StaticTrackPlayDatas)
                {
                    var status = item.Value.GetDefaultOpen();
                    status = LocalCache.GetScenePlaysStatusData.GetScenePlayStatus(item.Key, status);
                    PlayingStatus.Add(item.Key, status);

                    if (mTrackPlayConfigDatas.ContainsKey(item.Value.GetMapID()))
                    {
                        mTrackPlayConfigDatas[item.Value.GetMapID()].Add(item.Value);
                    }
                    else
                    {
                        mTrackPlayConfigDatas.Add(item.Value.GetMapID(), new List<TrackPlayDataCell>() { item.Value });
                    }
                }
            }

            InternalMessages.Add("OnSetPlayState", OnSetPlayStateHandler);
            InternalMessages.Add("OnPlayEffect", OnPlayEffectHandler);
            GlobalEvent.OnEntryScene.AddListener(OnEntrySceneHandler);
            GlobalEvent.OnRoleCreateComplete.AddListener(OnRoleCreateCompleteHandler);
            GlobalEvent.OnClickTaskItem.AddListener(OnClickTaskItemHandler);
            GlobalEvent.TaskStageChange.AddListener(OnTaskHandlerChangeHandler);
        }

        private void OnTaskHandlerChangeHandler(int type, int taskID)
        {
        }

        private void OnClickTaskItemHandler(int taskID)
        {
            if (TaskID == taskID)
            {
                return;
            }

            ClearOldPlays();
            TaskID = taskID;
            //CreatePlays();
            TryActivePlays();
        }

        private void OnRoleCreateCompleteHandler(object arg0)
        {
            //创建新玩法
            //CreatePlays();

            //激活玩法
            TryActivePlays();
        }

        /// <summary>
        /// 进入场景
        /// </summary>
        /// <param name="mapID">进入的场景ID</param>
        private void OnEntrySceneHandler(int mapID)
        {
            if (CurSceneID != mapID)
            {
                //清理旧玩法
                ClearOldPlays();
            }

            CurSceneID = mapID;
            //创建新玩法
            //CreatePlays();

            //激活玩法
            TryActivePlays();
        }

        private void TryActivePlays()
        {
            /*foreach (var item in mCurrentActivedPlays)
            {
                var play = ScenePlayTypeFactory[item.Key];
                if (play!= null)
                {
                    if (TaskID == 0 || TaskID == item.Value.Item2)
                    {
                        CurrentScenePlayType = item.Key;//玩法类型
                        CurrentPlayID = item.Value.Item1; //玩法ID
                        TaskID = item.Value.Item2;   //任务ID

                        break;
                    }
                    //play.OnPlayBegine(item.Value.Item1);
                }
            }*/
            foreach (var item in PlayingStatus)
            {
                if (item.Value)
                {
                    if (LocalDataManager.Instance.M_TrackPlayData.StaticTrackPlayDatas.ContainsKey(item.Key))
                    {
                        var cfg = LocalDataManager.Instance.M_TrackPlayData.StaticTrackPlayDatas[item.Key];
                        if (cfg != null)
                        {
                            if (GameManager.Instance.GetCurMapId() == cfg.GetMapID())
                            {
                                if (TaskID == 0 || TaskID == cfg.GetTaskID())
                                {
                                    CurrentScenePlayType = (ScenePlayType)cfg.GetScenePlayType();
                                    CurrentPlayID = cfg.GetID();
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            GlobalEvent.OnShowScenePlay.Invoke((int)CurrentScenePlayType, CurrentPlayID, TaskID);
            //广播事件到UI层
        }


        private void ClearOldPlays()
        {
            CurrentScenePlayType = 0;
            CurrentPlayID = 0;
            TaskID = 0;
            GlobalEvent.OnShowScenePlay.Invoke(0, 0, 0);
            foreach (var item in ScenePlayTypeFactory)
            {
                item.Value.OnPlayEnd();
            }

            mCurrentActivedPlays?.Clear();
        }

        private void CreatePlays(int taskid)
        {
            if (GameManager.Instance.M_MainPlayerCtrlBase == null ||
                GameManager.Instance.M_MainPlayerCtrlBase.M_Curr == null)
            {
                return;
            }

            mCurrentActivedPlays?.Clear();
            var playerPostion = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();

            if (mTrackPlayConfigDatas.ContainsKey(CurSceneID))
            {
                var list = mTrackPlayConfigDatas[CurSceneID];
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        bool state = false;
                        if (PlayingStatus.ContainsKey(item.GetID()))
                        {
                            state = PlayingStatus[item.GetID()];
                        }

                        if (state)
                        {
                            ScenePlayType playType = (ScenePlayType)item.GetScenePlayType();
                            if (mCurrentActivedPlays.ContainsKey(playType))
                            {
                                var old = mCurrentActivedPlays[playType];

                                if (GameMap.sceneJsonData.Spawners.TryGetValue(item.GetSpawnerID(),
                                        out var spawnerJsonData))
                                {
                                    var newdistance = Vector3.Distance(spawnerJsonData.Position.Convert(),
                                        playerPostion);
                                    if (newdistance < old.Item3)
                                    {
                                        if (taskid == 0 || taskid == item.GetTaskID())
                                        {
                                            mCurrentActivedPlays[playType] = (item.GetID(), item.GetTaskID(),
                                                newdistance);

                                            //Debug.LogError($"TaskID={taskid} osition ={spawnerJsonData.Position.Convert()} newdistance = {newdistance} id={item.GetID()}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ScenePlayTypeFactory != null &&
                                    ScenePlayTypeFactory.ContainsKey(playType))
                                {
                                    if (GameMap.sceneJsonData.Spawners.TryGetValue(item.GetSpawnerID(),
                                            out var spawnerJsonData))
                                    {
                                        var newdistance = Vector3.Distance(spawnerJsonData.Position.Convert(),
                                            playerPostion);
                                        if (taskid == 0 || taskid == item.GetTaskID())
                                        {
                                            mCurrentActivedPlays[playType] = (item.GetID(), item.GetTaskID(),
                                                newdistance);
                                            //Debug.LogError($"TaskID={taskid} osition ={spawnerJsonData.Position.Convert()} newdistance = {newdistance} id={item.GetID()}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var item in mCurrentActivedPlays)
            {
                var play = ScenePlayTypeFactory[item.Key];
                if (play != null)
                {
                    if (taskid == 0 || taskid == item.Value.Item2)
                    {
                        //Debug.LogError($"id={item.Value.Item1}");
                        play.OnPlayBegine(item.Value.Item1);
                        break;
                    }
                }
            }
        }

        private void OnPlayEffectHandler(object[] obj)
        {
            // 如果传入的参数为空或者参数长度不正确，打印错误日志并返回
            if (obj == null || obj.Length != 2)
            {
                Debuger.LogWarning("OnSetPlayStateHandler参数错误");
                return;
            }

            int Type = Convert.ToInt32(obj[0]);
            int TaskID = Convert.ToInt32(obj[1]);

            CreatePlays(TaskID);
            /*if (ScenePlayTypeFactory.ContainsKey((ScenePlayType)Type))
            {
                var play = ScenePlayTypeFactory[(ScenePlayType)Type];
                if (play!= null)
                {
                   // play.OnPlayBegine(Id);
                }
            }*/
        }

        /// <summary>
        /// 处理设置播放状态的事件
        /// </summary>
        /// <param name="obj">包含播放ID和状态的对象数组</param>
        private void OnSetPlayStateHandler(object[] obj)
        {
            // 如果传入的参数为空或者参数长度不正确，打印错误日志并返回
            if (obj == null || obj.Length != 2)
            {
                Debuger.LogWarning("OnSetPlayStateHandler参数错误");
                return;
            }

            // 将数组的第一个元素转换为整数，表示播放ID
            int id = Convert.ToInt32(obj[0]);

            // 将数组的第二个元素转换为布尔值，表示播放状态
            bool state = Convert.ToBoolean(obj[1]);

            // 如果PlayingStatus字典不为空且包含传入的ID
            if (PlayingStatus != null && PlayingStatus.ContainsKey(id))
            {
                if (!PlayingStatus[id].Equals(state))
                {
                    PlayingStatus[id] = state;
                    LocalCache.GetScenePlaysStatusData.SetScenePlayStatus(id, state);
                }
            }


            if (CurrentScenePlayType == 0 && CurrentPlayID == 0)
            {
                //创建新玩法
                var taskID = 0;
                var configs = LocalDataManager.Instance.M_TrackPlayData;
                if (configs != null)
                {
                    if (configs.StaticTrackPlayDatas.TryGetValue(id, out var cfg))
                    {
                        taskID = cfg.GetTaskID();
                    }
                }
                TaskID = taskID;
                CreatePlays(taskID);

                //激活玩法
                TryActivePlays();
            }
        }


        protected override void OnModuleMessage(string msg, object[] args)
        {
            // 如果消息处理函数字典为空，返回
            if (InternalMessages == null)
            {
                return;
            }

            // 如果消息处理函数字典包含传入的消息
            if (InternalMessages.ContainsKey(msg))
            {
                // 调用对应的处理函数
                InternalMessages[msg](args);
            }
        }

        public override void Release()
        {
            if (InternalMessages != null)
            {
                InternalMessages.Clear();
            }

            if (PlayingStatus != null)
            {
                PlayingStatus.Clear();
            }

            if (mTrackPlayConfigDatas != null)
            {
                mTrackPlayConfigDatas.Clear();
            }

            GlobalEvent.OnRoleCreateComplete.RemoveListener(OnRoleCreateCompleteHandler);
            GlobalEvent.OnEntryScene.RemoveListener(OnEntrySceneHandler);
            GlobalEvent.OnClickTaskItem.RemoveListener(OnClickTaskItemHandler);
        }
    }
}