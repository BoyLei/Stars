using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillEditor;
using StarProjectDef;
using StarProject.Game.Skill;
using Google.Protobuf.Collections;
using ProtoMsg;
using EffectData = SkillEditor.EffectData;

namespace StarProject.Game.Skill
{
    //效果数据 的节点
    public class EffectDataNode
    {
        public SkillEditor.EffectData data;
        public int EffectID => data.EffectID;

        private int[] next;
        public int[] Next
        {
            get
            {
                if (data.Next != null)
                {
                    return data.Next;
                }

                next = new int[0];
                return next;
            }
        }

        public EffectDataNode NextTrue;
        public EffectDataNode NextFalse;

        public bool Used = false;

        public EffectDataNode(SkillEditor.EffectData effectData)
        {
            data = effectData;
        }
    }

    public class HeadEffectDataNode
    {
        public int Time = 0;
        public EffectDataNode Head;
        public EffectDataNode Cur;


        public HeadEffectDataNode(EffectDataNode head, int time)
        {
            Head = head;
            Cur = head;
            Time = time;
        }

        public void UpdateCur(EffectDataNode head)
        {
            Cur = head;
        }
    }

    /// <summary>
    /// 技能阶段信息的封装层,用来封装StageJson 和 SkillConfig
    /// </summary>
    public class StageInfo
    {
        private StageJson _stageJson;

        private StageNormal _stageNormal;
        public int StageID => _stageJson.StageID;
        /// <summary>
        /// 阶段数据位于配置list中的idx
        /// </summary>
        private int _stageIdx = 0;

        public int StageIdx => _stageIdx;

        public int Time => _stageJson.Duration;

        /// <summary>
        /// 阶段的最大 时间，取的是串行效果 和 阶段时间的最大值
        /// </summary>
        public int MaxStageTime;

        /// <summary>
        /// 阶段配置循环的次数,不同类型的阶段处理不一样
        /// </summary>
        private int loopNum = 0;

        /// <summary>
        /// 阶段循环次数
        /// </summary>
        public int LoopNum => loopNum;

        /// <summary>
        /// 这个阶段是否是 循环的阶段
        /// </summary>
        public bool IsStageLoop => LoopNum == -1 || LoopNum > 1;



        private E_SkillStageBreakType stageBreakType = E_SkillStageBreakType.Default;
        public E_SkillStageBreakType StageBreakType
        {
            get
            {
                if (stageBreakType != E_SkillStageBreakType.Default)
                {
                    return stageBreakType;
                }
                else
                {
                    if (IsNormalStage)
                    {
                        if (_stageNormal.Interrupt == InterruptEvent.KillStage)
                        {
                            stageBreakType = E_SkillStageBreakType.SkipStage;
                        }
                        else if (_stageNormal.Interrupt == InterruptEvent.KillRunTime)
                        {
                            stageBreakType = E_SkillStageBreakType.BreakSkill;
                        }
                        else
                        {
                            stageBreakType = E_SkillStageBreakType.SkipStage;
                        }
                    }
                    else
                    {
                        stageBreakType = E_SkillStageBreakType.SkipStage;
                    }

                    return stageBreakType;
                }


            }
        }

        /// <summary>
        /// 阶段的跳转ID
        /// 目前策划已经删除这个字段,所以不要了
        /// </summary>
        // public int JumpStageID => stageNormal.JumpStageID;

        // 技能暂时没有,buff 后面可能会有
        // public List<int> AttrIDs;
        // public List<int> AttrValues;

        private List<int> _states = new List<int>();

        public List<int> States
        {
            get
            {
                // 不是普通的阶段,不会原子锁. 目前原子锁 的数据 在 InitStage的时候 只有normalStage 才会赋值
                return _states;
            }
        }
        public List<EffectJosn> Effects => _stageJson.Effects;

        /// <summary>
        /// 阶段内效果EffectDataDic,目前只有阶段内的所有效果
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <typeparam name="EffectData"></typeparam>
        /// <returns></returns>
        public Dictionary<int, SkillEditor.EffectData> EffectDatasDic = new Dictionary<int, SkillEditor.EffectData>();


        /// <summary>
        ///  0 : 打断的时候，整个技能运行时被打断
        ///  1 : (1,3) 打断这个阶段,跳转到 对应的阶段
        /// </summary>
        public string BreakEvent;

        private bool needActive = false;
        public bool NeedActive => needActive;

        private bool isEntryCD = false;
        public bool IsEntryCD => isEntryCD;

        private bool isCDCloseTouch = true;
        /// <summary>
        /// CD 是否会关闭 按钮的点击事件，默认都是关闭.
        /// note:
        ///     关闭按钮点击事件,会弹起按钮
        /// </summary>
        public bool IsCDCloseTouch => isCDCloseTouch;

        private StageType _stageType;
        public StageType StageInfoType => _stageType;

        public bool IsNormalStage => StageInfoType == StageType.NormalStage;

        /// <summary>
        /// 阶段的 可触发阶段
        /// </summary>
        public int TriggerCount => StageInfoType != StageType.TriggerStage ? 1 : _stageJson.StageTrigger.TriggerCount;

        public List<SkillStageFrame> TimeLineFrames = new List<SkillStageFrame>();

        public Dictionary<int, SkillStageFrame> NextEffectFrameMap = new Dictionary<int, SkillStageFrame>();

        public Dictionary<int, int> Next2ParentEffectMap = new Dictionary<int, int>();

        public StageInfo(StageJson stage, TimeLineStage timeLineStage, int stageIdx, E_StageType e_StageType)
        {
            _stageJson = stage;
            _stageIdx = stageIdx;


            InitStage(stage, e_StageType);


            //SGF.Debuger.Log($"[StageInfo] StageInfo[{StageID}] Add timeLineFrames count : {timeLineFrames.Count}");
            RefreshTimeLineFrames(timeLineStage);

            RefreshNextEffectFrames(timeLineStage);

            RefreshNext2ParentEffectMap(timeLineStage);

            RefreshEffectDatas();
            InitEffectNodeTree();
        }

        private void InitStage(StageJson stage, E_StageType e_StageType)
        {
            _stageType = stage.StageType;

            // 原子锁的状态
            _states.Clear();

            switch (_stageType)
            {
                case StageType.NormalStage:
                    {
                        loopNum = stage.StageNormal.LoopCount;
                        _stageNormal = stage.StageNormal;

                        // 2023/12/28 
                        // gl 配置了 一个buff 阶段，抢占活跃，导致表现异常.
                        // 但是高磊不想通过配置屏蔽,所以程序在 配置信息生成时统一处理
                        needActive = e_StageType == E_StageType.Skill && _stageNormal.Active;
                        isEntryCD = _stageNormal.IsEntryCD;

                        // 不是普通的阶段,不会原子锁
                        _stageNormal.States.ForEach((BattleState normalState) =>
                        {
                            _states.Add((int)normalState);
                        });
                    }
                    break;
                case StageType.AddBuffStage:
                case StageType.EndBuffStage:
                    {
                        loopNum = 1;
                        needActive = false;
                    }
                    break;
                case StageType.BulletStage:
                    {
                        loopNum = stage.StageBulletMotion.LoopCount;
                        needActive = false;
                    }
                    break;
                case StageType.TriggerStage:
                    {
                        loopNum = stage.StageTrigger.TriggerCount;
                        needActive = false;
                    }
                    break;

                default:
                    {
                        //DB_Close    SGF.Debuger.LogError($"[StageInfo_{StageID}] RefreshStageLoopCount no handle on stageType: {_stageType}");
                    }
                    break;
            }
        }

        private void RefreshTimeLineFrames(TimeLineStage timeLineStage)
        {
            timeLineStage.frameEvents.ForEach((TimeLineFrameEvent timeLineFrameEvent) =>
            {
                // 相同的技能id的技能运行时能够存在多份
                SkillStageFrame frame = new SkillStageFrame(timeLineFrameEvent);
                TimeLineFrames.Add(frame);

                // SGF.Debuger.LogError($"Play effectData {effectData.EffectID} ");

            });

        }

        private void RefreshNextEffectFrames(TimeLineStage timeLineStage)
        {
            Dictionary<int, TimeLineFrameEvent> nextTimeLineFrames = timeLineStage.nextFrameEvents;

            foreach (KeyValuePair<int, TimeLineFrameEvent> item in nextTimeLineFrames)
            {
                SkillStageFrame frame = new SkillStageFrame(item.Value);

                if (NextEffectFrameMap.ContainsKey(item.Key))
                {
                    //SGF.Debuger.LogError($"[StageInfo_{StageID}] RefreshNextEffectFrames has same nextEffectID : {item.Key}");
                    continue;
                }
                // 生成 一份 next 事件帧
                NextEffectFrameMap.Add(item.Key, frame);
            }
        }


        private void RefreshNext2ParentEffectMap(TimeLineStage timeLineStage)
        {
            Dictionary<int, int> nextFrame2Parent = timeLineStage.nextFrame2ParentMap;

            foreach (KeyValuePair<int, int> item in nextFrame2Parent)
            {
                // 如果重复,说明 配置有问题
                if (Next2ParentEffectMap.ContainsKey(item.Key))
                {
                    continue;
                }
                // 生成 一份 next 事件帧
                Next2ParentEffectMap.Add(item.Key, item.Value);
            }
        }



        private int GetEffectTime(SkillEditor.EffectData effectData)
        {
            int effectEndTime = effectData.EffectEndTime;
            return effectEndTime;
        }

        private void VisitSkillStageFrame(SkillStageFrame frame)
        {
            // 先不考虑尾帧
            if (!frame.isFrameStart)
            {
                return;
            }

            // 先不考虑 恢复帧, 目前恢复帧 只应用于 动画/特效 帧, 不处理 效果帧的恢复帧
            if (frame.FrameType == E_StageFrameEventType.Effect && !frame.IsRecoverCopyStartFrame)
            {
                SkillEditor.EffectData effectData = (SkillEditor.EffectData)frame.Value;
                VisitTimeLineEffectData(effectData, null, frame.TimeLineTime);

                VisitEffectStageFrame(frame);

            }
        }
        /// <summary>
        /// 效果 基于 整个timeLine的时间 
        /// </summary>
        private Dictionary<int, int> effectDataTimeLineTime = new Dictionary<int, int>();
        private int GetEffectTimeLineTime(SkillEditor.EffectData effectData)
        {
            int timeLineTime = 0;
            if (effectData != null && effectDataTimeLineTime.TryGetValue(effectData.EffectID, out timeLineTime))
            {
                return timeLineTime;
            }
            return 0;
        }

        /// <summary>
        /// 遍历 基于timeLine的 效果数据
        /// </summary>
        /// <param name="effectData"></param>
        /// <param name="parent"></param>
        /// <param name="parentTimeLineBaseTime"></param>
        private void VisitTimeLineEffectData(SkillEditor.EffectData effectData, SkillEditor.EffectData parent, int parentTimeLineBaseTime)
        {
            if (effectData == null)
            {
                return;
            }
            int parentTimeLineTime = parentTimeLineBaseTime + GetEffectTimeLineTime(parent);
            int timeLineTime = parentTimeLineTime + GetEffectTime(effectData);
            if (effectDataTimeLineTime.ContainsKey(effectData.EffectID))
            {
                //SGF.Debuger.LogError($"[StageInfo_{StageID}] VisitTimeLineEffectData An item with the same key has already been added. Key: {effectData.EffectID},des={effectData.Desc}");
                return;
            }
            else
            {
                effectDataTimeLineTime.Add(effectData.EffectID, timeLineTime);
            }

            int[] next = effectData.Next;
            if (next != null)
            {
                // next_true 分支
                if (next.Length > 0)
                {
                    SkillEditor.EffectData nextTrue = GetEffectData(next[0]);
                    VisitTimeLineEffectData(nextTrue, effectData, 0);
                }

                if (next.Length > 1)
                {
                    SkillEditor.EffectData nextFalse = GetEffectData(next[1]);
                    VisitTimeLineEffectData(nextFalse, effectData, 0);
                }
            }

        }

        /// <summary>
        /// 基于阶段时间的 效果 map
        /// </summary>
        private Dictionary<int, int> effectDataStageTime = new Dictionary<int, int>();

        private void VisitEffectStageFrame(SkillStageFrame frame)
        {
            SkillEditor.EffectData effectData = (SkillEditor.EffectData)frame.Value;
            if (effectData == null)
            {
                return;
            }
            VisitStageTimeEffectData(effectData, frame.FrameTime);
        }


        private void VisitStageTimeEffectData(SkillEditor.EffectData effectData, int stageFrameBaseTime)
        {
            if (effectData == null)
            {
                return;
            }
            if (effectDataStageTime.ContainsKey(effectData.EffectID))
            {
                SGF.Debuger.LogWarning($"stage [{StageID}] VisitStageTimeEffectData [{effectData.EffectID}] 重复, 找 高磊!!!");
                return;
            }
            effectDataStageTime.Add(effectData.EffectID, stageFrameBaseTime);
            
            int[] next = effectData.Next;

            if (next != null)
            {
                // next_true 分支
                if (next.Length > 0)
                {
                    SkillEditor.EffectData nextTrue = GetEffectData(next[0]);
                    int nextTrueRunTime = stageFrameBaseTime + effectData.ServerNextDelayTime + effectData.ClinetExecuteTime;
                    VisitStageTimeEffectData(nextTrue, nextTrueRunTime);
                }

                if (next.Length > 1)
                {
                    SkillEditor.EffectData nextFalse = GetEffectData(next[1]);
                    int nextFalseRunTime = stageFrameBaseTime + effectData.ServerNextDelayTime + nextFalse.ClinetExecuteTime;
                    VisitStageTimeEffectData(nextFalse, nextFalseRunTime);
                }
            }

        }

        /// <summary>
        /// 得到 效果 在阶段上的运行时间, 包含next 效果
        /// /// </summary>
        /// <param name="effectID"></param>
        /// <returns></returns>
        public int GetEffectStageTime(int effectID)
        {
            int stageTime = 0;
            if (!effectDataStageTime.TryGetValue(effectID, out stageTime))
            {
                //SGF.Debuger.LogError($"stage [{StageID}] not find effect [{effectID}] stageTime ");

            }
            return stageTime;
        }

        public EffectData GetEffectData(int effectID)
        {
            EffectData effectData = null;
            if (EffectDatasDic.TryGetValue(effectID, out effectData))
            {
                return effectData;
            }
            return null;
        }

        private void RefreshEffectDatas()
        {
            //第一次变量,构建 事件帧 和一个总的 效果 EffectDatasDic
            for (int i = 0; i < Effects.Count; i++)
            {
                EffectJosn effectJosn = Effects[i];
                List<SkillEditor.EffectData> effectDatas = effectJosn.data;

                int count = effectDatas.Count;
                if (count == 0)
                {
                    continue;
                }

                for (int j = 0; j < count; j++)
                {
                    SkillEditor.EffectData effectData = effectDatas[j];

                    if (!EffectDatasDic.ContainsKey(effectData.EffectID))
                    {
                        EffectDatasDic.Add(effectData.EffectID, effectData);
                    }
                    else
                    {
                        //SGF.Debuger.LogError($"[StageInfo] RefreshEffectFrameEvents Add same Effect ID {effectData.EffectID}");
                    }
                }
            }

            // 计算出 阶段 能够持续运行的最大时间, 如果是循环的阶段, 那也是循环创建多个阶段.
            // 所以 此处 MaxStageTime 就是 这个阶段能够运行的最大时间

            TimeLineFrames.ForEach((SkillStageFrame frame) =>
            {
                VisitSkillStageFrame(frame);
            });

            MaxStageTime = 0;
            foreach (KeyValuePair<int, int> item in effectDataTimeLineTime)
            {
                // SGF.Debuger.LogError($"stage {StageID} , effectID : {item.Key} , timeLineTime = {item.Value}");
                MaxStageTime = Mathf.Max(MaxStageTime, item.Value);
            }
            MaxStageTime = Mathf.Max(MaxStageTime, Time);
            //SGF.Debuger.LogError($"stage {StageID} , MaxStageTime : {MaxStageTime}");

        }

        public List<HeadEffectDataNode> effectDataNodeList = new List<HeadEffectDataNode>();

        /// <summary>
        /// 初始化效果节点树 , 设计效果节点数的 目的是为了 每次服务器效果同步的时候,
        /// 能够根据 服务器 效果线list种 的数据,恢复 出 当前 客户端 需要注册 哪些效果.
        /// </summary>
        private void InitEffectNodeTree()
        {
            //因为要依赖 整个 EffectDatasDic,所以构建效果 节点的时候,需要再次循环
            for (int i = 0; i < Effects.Count; i++)
            {
                EffectJosn effectJosn = Effects[i];
                List<SkillEditor.EffectData> effectDatas = effectJosn.data;
                int count = effectDatas.Count;
                if (count == 0)
                {
                    continue;
                }

                SkillEditor.EffectData firstData = effectDatas[0];

                EffectDataNode effectDataNode = new EffectDataNode(firstData);
                CreateNextEffectNode(effectDataNode);

                HeadEffectDataNode headEffectDataNode = new HeadEffectDataNode(effectDataNode, firstData.ClinetExecuteTime);

                effectDataNodeList.Add(headEffectDataNode);
            }
        }

        private void CreateNextEffectNode(EffectDataNode effectDataNode)
        {
            int[] next = effectDataNode.data.Next;
            if (next == null) return;

            if (next.Length > 0)
            {
                int nextTrue = next[0];
                if (EffectDatasDic.ContainsKey(nextTrue))
                {
                    EffectDataNode nextTrueNode = new EffectDataNode(EffectDatasDic[nextTrue]);
                    effectDataNode.NextTrue = nextTrueNode;
                    CreateNextEffectNode(nextTrueNode);
                }
                else
                {
                    //SGF.Debuger.LogError($"[StageInfo_{StageID}] CreateNextEffectNode nextTrue: {nextTrue} not find");
                }
            }

            if (next.Length > 1)
            {
                int nextFalse = next[1];
                if (EffectDatasDic.ContainsKey(nextFalse))
                {
                    EffectDataNode nextFalseNode = new EffectDataNode(EffectDatasDic[nextFalse]);
                    effectDataNode.NextFalse = nextFalseNode;
                    CreateNextEffectNode(nextFalseNode);
                }
                else
                {
                    //SGF.Debuger.LogError($"[StageInfo_{StageID}] CreateNextEffectNode nextFalse: {nextFalse} not find");
                }
            }
        }

        List<int> tempEffectList = new List<int>();

        /// <summary>
        /// 更新效果树 的数据
        /// </summary>
        /// <param name="stageLines">服务器同步过来的阶段的 所有效果线</param>
        public void UpdateEffectDataNodeList(RepeatedField<RunLineData> stageLines, int stageTime)
        {
            tempEffectList.Clear();

            for (int i = 0; i < stageLines.Count; i++)
            {
                tempEffectList.Add(stageLines[i].CurEffectID);
            }

            tempEffectList.ForEach((int curEffectID) =>
            {
                UpdateItemEffectData(curEffectID, stageTime);
            });
            tempEffectList.Clear();
        }

        private List<SkillEditor.EffectData> tempEffectDataList = new List<SkillEditor.EffectData>();
        public List<SkillEditor.EffectData> GetCurNeedRegEffects(RepeatedField<RunLineData> stageLines, int stageTime)
        {
            UpdateEffectDataNodeList(stageLines, stageTime);

            tempEffectDataList.Clear();

            for (int i = 0; i < effectDataNodeList.Count; i++)
            {
                HeadEffectDataNode node = effectDataNodeList[i];
                //不需要恢复效果外的数据
                if (node.Time > stageTime)
                {
                    break;
                }

                EffectDataNode cur = node.Cur;
                if (cur == null)
                {
                    continue;
                }
                //如果这个节点没有使用,那这个节点 就需要等待服务器的数据
                if (!cur.Used)
                {
                    tempEffectDataList.Add(cur.data);
                    continue;
                }

                if (cur.NextTrue != null)
                {
                    tempEffectDataList.Add(cur.NextTrue.data);
                }

                if (cur.NextFalse != null)
                {
                    tempEffectDataList.Add(cur.NextFalse.data);
                }
            }

            return tempEffectDataList;
        }

        private void UpdateItemEffectData(int curEffectID, int stageTime)
        {
            for (int i = 0; i < effectDataNodeList.Count; i++)
            {
                HeadEffectDataNode node = effectDataNodeList[i];
                //不需要恢复效果外的数据
                if (node.Time > stageTime)
                {
                    break;
                }

                //如果在 效果树中 找到了这个 curEffectID 的效果节点,更新当前的效果节点,并结束这个curEffectID的查找
                bool result = UpdateCurHeadEffectNode(node, curEffectID);
                if (result)
                {
                    break;
                }
            }
        }

        private EffectDataNode FindNode(EffectDataNode node, int curEffectID)
        {
            if (node.EffectID == curEffectID)
            {
                return node;
            }
            EffectDataNode findResult = null;
            if (node.NextTrue != null)
            {
                findResult = FindNode(node.NextTrue, curEffectID);
            }

            if (findResult != null)
            {
                return findResult;
            }

            if (node.NextFalse != null)
            {
                findResult = FindNode(node.NextFalse, curEffectID);
            }
            return findResult;
        }

        private bool UpdateCurHeadEffectNode(HeadEffectDataNode headEffectDataNode, int curEffectID)
        {
            if (headEffectDataNode.Cur == null)
            {
                return false;
            }

            EffectDataNode curNode = headEffectDataNode.Cur;

            EffectDataNode findNode = FindNode(curNode, curEffectID);

            // 如果在节点数中找不到 这个effID 的节点,那说明不在这个效果节点中树中
            if (findNode == null)
            {
                return false;
            }

            // 更新找到的节点 为 used(当前效果 服务求已经用了)
            findNode.Used = true;

            bool hasTrueNode = findNode.NextTrue != null;
            bool hasFalseNode = findNode.NextFalse != null;

            if (hasTrueNode || hasFalseNode)
            {
                // 如果找到了这个效果节点,并且 还有子效果节点 , 那更新当前的 curNode 为 findNode
                headEffectDataNode.Cur = findNode;
            }
            else
            {
                // 如果没有子节点,但是找到了,这个效果节点，那更新当前节点为 null
                headEffectDataNode.Cur = null;
            }

            return true;
        }
    }
}