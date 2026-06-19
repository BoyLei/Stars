using System;
using System.Collections;
using System.Collections.Generic;
using SkillEditor;
using StarProject.Game.Entity;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;
using EffectData = SkillEditor.EffectData;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 基于整个TimeLine的阶段信息
    /// 设计这个的目的是为了 将生成的事件帧基于整个Timeline的时间轴做一次排布.
    /// 特别是针对 跨越阶段的结束帧.
    /// </summary>
    public class TimeLineStageInfos : EntityRemoteStatic
    {
        private string TagFlag
        {
            get => $"[TimeLineStageInfos_[{skillId}]] ";
        }

        private int skillId = 0;
        private List<TimeLineFrameEvent> timeLineFrameEvents = new List<TimeLineFrameEvent>();

        /// <summary>
        /// timeLine上的阶段分部
        /// </summary>
        /// <typeparam name="TimeLineStage"></typeparam>
        /// <returns></returns>
        private List<TimeLineStage> timeLineStages = new List<TimeLineStage>();

        /// <summary>
        /// 刷新技能的事件帧,根据阶段str，存在一个Map里面
        /// </summary>
        public void InitFrameEvents(List<StageJson> stageJsons, int skillID)
        {
            skillId = skillID;
            if (stageJsons == null)
            {
                return;
            }
            int timeLineBaseTime = 0;
            for (int i = 0; i < stageJsons.Count; i++)
            {
                StageJson stageJson = stageJsons[i];


                int loopCount = stageJson.StageType == StageType.NormalStage ? stageJson.StageNormal.LoopCount : 1;
                int originLoopCount = loopCount;


                // 类似于buff的循环阶段,策划配置的是 -1,表示无限循环,
                // 此时,buff的结束根据 配置表中的Time（buff最大时间）来结束
                if (loopCount == -1)
                {
                    loopCount = 1;
                }

                /// 2023/2/28
                /// TODO: DL
                ///     此处 循环阶段 没必要直接创建多个 阶段信息, 阶段数据只需要创建一份就可以了, 具体生成的 stageInfo 再去区分 loopIdx;
                /// note:
                ///     1.在一开始的 版本中, 跟gl约定的是: 阶段上的 动画/特效/效果 都 可能跨越循环阶段.
                ///       这样,当时遇到的 问题就是 一个动画/效果等 的 结束帧 该 落在哪个循环阶段中的问题.
                ///       当时的解决方案 就是 提前 创建 所有的 阶段数据, 将所有的 阶段 在 timeLine 时间轴上展开,
                ///       通过 计算每个事件帧 的 timeLineTime, 将事件帧 重新排布 到对应的 阶段中.
                /// 
                ///     2.在后续做技能恢复的时候,发现 对于这种跨阶段(主要是 跨越了循环的第一次阶段 落在后面阶段的情况) 不太好做
                ///       阶段的恢复(服务器没啥数据,客户端无法恢复).
                ///       所以后面 约定 不允许 跨越循环阶段 配置方法(主要针对 跨越了第一个循环阶段 落在后面的情况).
                ///       对于这种 配置方式, 其实就没必要 在生成事件帧的时候, 生成 多个循环阶段.
                ///     
                ///     3.后续优化方案:
                ///       1.循环阶段 只需要创建一个, 在stageInfo 中 自己去加loopIdx 区分.
                ///       2.TimeLineStageInfo 只 提供一个简单 的 获取 TimeLinStage 数据接口;
                ///       3.需要区分 跨越 循环阶段 但是 事件结束帧 落在循环阶段的 情况, 可能稍微有点麻烦,
                ///         因为这种跨阶段的 结束帧 只能落在 第一个循环阶段,后面的循环阶段 不需要
                for (int j = 0; j < loopCount; j++)
                {
                    // 扩大阶段 之间的优先级
                    int priority = i * 10000 + j * 1000;

                    string stageIdStr = SkillStage.FormatStageIDStr(stageJson.StageID, j);

                    CreateStageFrameEvents(stageJson, stageIdStr, priority, timeLineBaseTime);

                    // 创建一个 阶段的时间段,有开始 和结束时间,后续 阶段的事件帧可以按时间顺序依次插入里面
                    {
                        bool isFirstStage = i == 0 && j == 0;

                        CreateTimeLineStage(stageJson, isFirstStage, j, timeLineBaseTime, originLoopCount);
                    }

                    // 更新 对应的阶段 开始时间
                    timeLineBaseTime += (stageJson.End - stageJson.Start);

                }
            }

            // 给timeLine 上的事件帧排序
            SortFrameEvents();

            // 初始化 timeLineStage 上的事件帧
            InitTimeLineStageFrames();

            // timeLineStages.ForEach((TimeLineStage timeLineStage) =>
            // {
            //     int start = timeLineStage.Start;
            //     int end = timeLineStage.End;
            //     string stageStr = SkillStage.FormatStageIDStr(timeLineStage.StageID, timeLineStage.Loop);

            //     SGF.Debuger.LogError($"{TagFlag} start [{start}  ---> {end}] ==========================");
            //     timeLineStage.frameEvents.ForEach((TimeLineFrameEvent frameEvent) =>
            //     {
            //         if (frameEvent.StageFrameEventType == E_StageFrameEventType.Animation)
            //         {

            //             // SGF.Debuger.Log($"{TagFlag}  stageStr {stageStr} Time {frameEvent.Time} , FrameTime {frame.FrameTime} , priority {frameEvent.priority} , isFrameStart {frame.isFrameStart} , FrameType {frame.FrameType} , name {((AnimationJson)frame.Value).ClipName}");
            //             // SGF.Debuger.Log($"{TagFlag}  stageStr {stageStr} TimeLineTime {frameEvent.TimeLineTime} , StageFrameTime {frameEvent.StageFrameTime} , StageStartTime {frameEvent.StageStartTime}, priority {frameEvent.priority} , isFrameStart {frameEvent.isFrameStart} , FrameType {frameEvent.StageFrameEventType} , name {((EffectData)frameEvent.frameData).EffectID}");
            //             SGF.Debuger.LogError($"{TagFlag}  stageStr {stageStr} TimeLineTime {frameEvent.TimeLineTime} , StageFrameTime {frameEvent.StageFrameTime} , priority {frameEvent.priority} , isFrameSart {frameEvent.isFrameStart} , FrameType {frameEvent.StageFrameEventType} , name {((AnimationJson)frameEvent.frameData).ClipName} ,IsRecoverCopyStartFrame : {frameEvent.IsRecoverCopyStartFrame} , RecoverJumpTime : {frameEvent.RecoverJumpTime}");
            //         }

            //     });
            //     SGF.Debuger.LogError($"{TagFlag}  end ==========================");
            // });
        }

        /// <summary>
        /// 创建 阶段的事件帧
        /// 如果 配置了 多次循环阶段, 那么 每个阶段 都会 创建一次 阶段事件帧
        /// </summary>
        /// <param name="stageJson"></param>
        /// <param name="stageIdStr"></param>
        /// <param name="priority"></param>
        /// <param name="timeLineBaseTime"></param>
        private void CreateStageFrameEvents(StageJson stageJson, string stageIdStr, int priority, int timeLineBaseTime)
        {
            RefreshEffectFrameEvents(stageJson.Effects, timeLineBaseTime, priority, stageIdStr);
            RefreshAnimationFrameEvents(stageJson.Animations, timeLineBaseTime, priority, stageIdStr);
            RefreshFxFrameEvents(stageJson.Fxs, timeLineBaseTime, priority, stageIdStr);
            RefreshSoundFrameEvents(stageJson.Sounds, timeLineBaseTime, priority, stageIdStr);
            RefreshCameraFrameEvents(stageJson.Cameras, timeLineBaseTime, priority, stageIdStr);
            RefreshCameraShakeFrameEvents(stageJson.ShakeCameras, timeLineBaseTime, priority, stageIdStr);
        }

        private void SortFrameEvents()
        {
            timeLineFrameEvents.Sort((TimeLineFrameEvent farme1, TimeLineFrameEvent frame2) =>
           {
               if (farme1.TimeLineTime != frame2.TimeLineTime)
               {
                   // 先按基于TimeLine的时间 比较排序
                   return farme1.TimeLineTime - frame2.TimeLineTime;
               }
               else
               {
                   // 如果时间相等了,那就比较优先级
                   return farme1.priority - frame2.priority;
               };
           });
        }

        /// <summary>
        /// 创建 stageJson 对应的 阶段
        /// </summary>
        /// <param name="stageJson"></param>
        /// <param name="isFirstStage"></param>
        /// <param name="loopCount"></param>
        /// <param name="timeLineBaseTime"></param>
        private void CreateTimeLineStage(StageJson stageJson, bool isFirstStage, int loopCount, int timeLineBaseTime, int originLoopCount)
        {
            TimeLineStage timeLineStag = new TimeLineStage(stageJson.StageID, loopCount, isFirstStage, originLoopCount);

            timeLineStag.Start = timeLineBaseTime;
            timeLineStag.End = timeLineBaseTime + (stageJson.End - stageJson.Start);

            timeLineStages.Add(timeLineStag);
        }

        /// <summary>
        /// 检查 事件帧 是否需要 导出到 timeLine的 阶段上
        /// </summary>
        /// <param name="frame">需要检查 导出的事件帧</param>
        /// <param name="timeLineStage">需要导出的 阶段 </param>
        /// <param name="exportType">导出的类型, 是左开右闭还是 咋样 </param>
        /// <returns></returns>
        private bool CheckFrameNeedExport2Stage(TimeLineFrameEvent frame, TimeLineStage timeLineStage, E_StageFrameExportType exportType)
        {
            switch (exportType)
            {
                case E_StageFrameExportType.LeftOpenRightClose:
                    {
                        return frame.TimeLineTime > timeLineStage.Start && frame.TimeLineTime <= timeLineStage.End;
                    }
                case E_StageFrameExportType.LeftCloseRightOpen:
                    {
                        return frame.TimeLineTime >= timeLineStage.Start && frame.TimeLineTime < timeLineStage.End;
                    }
                case E_StageFrameExportType.LeftCloseRightClose:
                    {
                        return frame.TimeLineTime >= timeLineStage.Start && frame.TimeLineTime <= timeLineStage.End;
                    }

                default:
                    {
                        return frame.TimeLineTime >= timeLineStage.Start && frame.TimeLineTime <= timeLineStage.End;
                    }
            }
        }

        /// <summary>
        /// 检查 事件帧 是否在 阶段上
        /// </summary>
        /// <param name="timeLineStage"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        private bool CheckIsFrameOnStage(TimeLineStage timeLineStage, TimeLineFrameEvent frame)
        {
            // 首帧直接按配置表 配置的方式导出
            if (frame.isFrameStart)
            {
                return frame.StageIdStr == timeLineStage.StageIDStr;
            }


            // 如果是 效果帧 , 效果需要有阶段结束帧执行的逻辑,所以效果帧导出到阶段的规则是 (] (左开右闭)， 而第一阶段是[]
            if (frame.StageFrameEventType == E_StageFrameEventType.Effect)
            {
                /// 事件帧 在 第一个阶段 的导出规则时 []
                if (timeLineStage.IsFirstStage)
                {
                    return CheckFrameNeedExport2Stage(frame, timeLineStage, E_StageFrameExportType.LeftCloseRightClose);
                }
                else
                {
                    /// 其它阶段的导出规则时 (]
                    return CheckFrameNeedExport2Stage(frame, timeLineStage, E_StageFrameExportType.LeftOpenRightClose);
                }
            }
            else
            {

                /// 其它的 类似于动画帧,关心的是在 首帧播放动画,不能导入上一个阶段的阶段尾,所以采用的是 [)(左闭右开)，超出阶段的统一放在最后一个阶段
                /// note：
                ///     如果是 尾帧(比如动画结束的尾帧,优先放在当前阶段里面),放在阶段尾部,规则为(] (左开右闭)

                if (frame.isFrameStart)
                {
                    // 首帧 [)
                    return CheckFrameNeedExport2Stage(frame, timeLineStage, E_StageFrameExportType.LeftCloseRightOpen);
                }
                else
                {
                    // 尾帧 (]
                    return CheckFrameNeedExport2Stage(frame, timeLineStage, E_StageFrameExportType.LeftOpenRightClose);
                }
            }
        }

        /// <summary>
        /// 初始化 timeLineStage 上的事件帧
        /// </summary>
        private void InitTimeLineStageFrames()
        {
            timeLineFrameEvents.ForEach((TimeLineFrameEvent frame) =>
            {
                TimeLineStage lineStage = timeLineStages.Find((TimeLineStage timeLineStage) =>
                {
                    return CheckIsFrameOnStage(timeLineStage, frame);
                });
                //如果尾帧超过 timeLine的时间长度
                if (lineStage == null)
                {
                    lineStage = timeLineStages[timeLineStages.Count - 1];
                }

                // 重新将 frame的 FrameTime 改为 基于阶段的时间
                frame.SetStageStartTime(lineStage.Start);

                lineStage.AddFrameEvent(frame);

                InitNextEffectFrame(frame, lineStage);

                InitRecoverCopyFrame(frame, lineStage);
            });
        }

        /// <summary>
        /// 初始化 frame 为效果类型时,它对应的next 效果的事件帧,将其添加到 timeLineStage中
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="timeLineStage"></param>
        private void InitNextEffectFrame(TimeLineFrameEvent frame, TimeLineStage timeLineStage)
        {
            if (frame.StageFrameEventType == E_StageFrameEventType.Effect)
            {
                AddNextEffectFrames(frame, timeLineStage);
            }
        }

        private void AddNextEffectFrames(TimeLineFrameEvent parentFrame, TimeLineStage timeLineStage)
        {
            EffectData effectData = (EffectData)parentFrame.frameData;
            int[] next = effectData.Next;

            if (next == null || next.Length == 0)
            {
                return;
            }
            // //SGF.Debuger.LogError($"frame_[{effectData.EffectID}] ready Add Next ");
            for (int i = 0; i < next.Length; i++)
            {
                int nextID = next[i];
                if (!effectDatasMap.ContainsKey(nextID))
                {
                    SGF.Debuger.LogError($"{TagFlag} AddNextEffectFrames() nextID={nextID},effectDatasMap 不存在,【高磊来改】【高磊来改】【高磊来改】【高磊来改】");
#if UNITY_EDITOR
                    UnityEngine.Debug.Break();
#endif
                }
                AddNextEffectFrame(parentFrame, effectDatasMap[nextID], timeLineStage);
            }
        }

        private void AddNextEffectFrame(TimeLineFrameEvent parentFrame, SkillEditor.EffectData nextEffectData, TimeLineStage timeLineStage)
        {
            if (nextEffectData == null)
            {
                return;
            }
            SkillEditor.EffectData effectData = (SkillEditor.EffectData)parentFrame.frameData;
            // next效果 在 timeLine 上的时间 = parentFrame 在timeLine上的时间 + parent 延迟ServerNextDelayTime 执行下个效果的时间
            int timeLineTime = parentFrame.TimeLineTime + effectData.ServerNextDelayTime;

            // next效果 的持续时间 = next的 结束时间 - next 效果的 客户端执行时间
            int duration = nextEffectData.EffectEndTime - nextEffectData.ClinetExecuteTime;

            E_StageFrameEventType stageFrameEventType = E_StageFrameEventType.Effect;

            int priority = parentFrame.priority + 1;

            string stageStr = parentFrame.StageIdStr;

            ////SGF.Debuger.LogError($"frame_[{effectData.EffectID}] Add Next[{nextEffectData.EffectID}], timeLineTime: {timeLineTime}, duration: {duration}, stageStr: {stageStr} ");

            TimeLineFrameEvent nextFrameEvent = CreateFrameEvent(timeLineTime, duration, nextEffectData, stageFrameEventType, true, priority, stageStr);

            timeLineStage.AddNextFrameEvent(nextEffectData.EffectID, nextFrameEvent, effectData.EffectID);

            AddNextEffectFrames(nextFrameEvent, timeLineStage);
        }

        /// <summary>
        /// 生成 用来阶段恢复的 恢复帧,主要是对于 跨阶段的动画这种,在跨越的阶段开始处,生成一份专门用来恢复的事件帧
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="lineStage"></param>
        private void InitRecoverCopyFrame(TimeLineFrameEvent frame, TimeLineStage lineStage)
        {
            /// 2023/2/6
            /// 目前约定 循环阶段 不允许配置跨阶段动画
            /// 如果是 开始帧, 就考虑事件帧 的长度是否跨越阶段,目前来说有 4种情况
            ///     1. frameStart >= stageStart && frameEnd <= stageEnd , 开始帧和 结束帧 都在 阶段内部, 不需要生成事件帧;
            ///     2. frameStart <= stageStart && frameEnd <= stageEnd , 开始帧在 阶段之前,结束帧在阶段内部:
            ///         a: 如果是普通阶段, 在阶段的开始点 生成一个 开始帧 ;
            /// 
            ///     3. frameStart >= stageStart && frameStart <= stageEnd && frameEnd >= stageEnd,开始帧在 阶段里,结束帧在阶段内部:
            ///        a: 如果是普通阶段, 在下一个 阶段 的开始点 生成一个 开始帧 ;
            ///     
            ///     4. frameStart <= stageStart && frameEnd >= stageEnd,开始帧在 阶段前,结束帧在阶段后:
            ///         a: 如果是普通阶段, 在阶段和下一个阶段 的开始点 生成一个 开始帧 ;

            if (frame.isFrameStart && frame.StageFrameEventType == E_StageFrameEventType.Animation)
            {
                int frameStart = frame.TimeLineTime;
                int frameEnd = frame.TimeLineEndTime;

                TimeLineStage curTimeLineStage = lineStage;
                do
                {
                    int stageStart = curTimeLineStage.Start;
                    int stageEnd = curTimeLineStage.End;

                    // 如果 事件帧 在单个阶段内,那其实 不用生成 阶段恢复帧
                    if (frameStart >= stageStart && frameEnd <= stageEnd)
                    {
                        break;
                    }
                    // 如果出现事件帧 的一部分在 循环阶段,目前不允许这种情况,就不做阶段的恢复帧
                    if (curTimeLineStage.isLoopStage)
                    {
                        //SGF.Debuger.LogError($"{TagFlag} [xx-xx] recover frame  on LoopStage : {curTimeLineStage.StageIDStr} , 不允许有跨越阶段的 事件帧 落在循环阶段内部 !!! ");
                        //Debug.Break();
                        break;
                    }
                    // 如果 事件帧的结束帧 超过了 这个阶段, 那么就跳入下一个阶段中
                    if (frameStart >= stageStart && frameEnd > stageEnd)
                    {
                        // jump to next timeLineStage
                        curTimeLineStage = GetNextTimeLineStage(curTimeLineStage);
                        continue;
                    }
                    // 如果 事件帧的 开始阶段在 阶段前，结束阶段在 阶段中,那在这个阶段的开始 补一个恢复帧
                    if (frameStart < stageStart && frameEnd > stageStart && frameEnd <= stageEnd)
                    {
                        // add recover frame
                        AddRecoverCopyFrame(frame, curTimeLineStage);
                        break;
                    }
                    // 如果 事件帧的 开始帧在 阶段前, 结束帧 在阶段后, 那就要先补一个 恢复帧,同时 跳到下一个阶段
                    if (frameStart < stageStart && stageEnd <= frameEnd)
                    {
                        // add recover frame
                        AddRecoverCopyFrame(frame, curTimeLineStage);
                        // jump to next timeLineStage
                        curTimeLineStage = GetNextTimeLineStage(curTimeLineStage);
                        continue;
                    }

                    break;

                } while (curTimeLineStage != null);
            }
        }

        private void AddRecoverCopyFrame(TimeLineFrameEvent frame, TimeLineStage timeLineStage)
        {
            TimeLineFrameEvent copy = frame.Copy(true);

            int stageStart = timeLineStage.Start;
            int frameStart = frame.TimeLineTime;

            int jumpTime = stageStart - frameStart;


            copy.StageFrameTime = 0;
            copy.TimeLineTime = stageStart;
            copy.Duration -= jumpTime;
            copy.RecoverJumpTime = jumpTime;
            //SGF.Debuger.LogError($"{TagFlag} [xx-xx] AddRecoverCopyFrame frame jumpTime {jumpTime}ms on stage : {timeLineStage.StageIDStr} on stageStart ");

            timeLineStage.AddFrameEvent(copy);
        }

        /// <summary>
        /// 返回的是纯数据的 TimeLineStage（包含了基于timeLine时间轴上的事件帧TimeLineFrameEvent数据）.
        /// note:
        ///    1. 多个相同技能,可以获取一份相同的 TimeLineStage 数据(循环阶段)
        ///    2. 可以根据 TimeLineStage 数据创建对应的事件帧,但是不应该持有这个 TimeLineStage的引用！！
        /// </summary>
        /// <param name="stageID"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public TimeLineStage GetTimeLineStage(int stageID, int loop)
        {
            return timeLineStages.Find((TimeLineStage timeLineStage) =>
            {
                return timeLineStage.Equals(stageID, loop);
            });
        }

        private TimeLineStage GetNextTimeLineStage(TimeLineStage curTimeLineStage)
        {
            int idx = timeLineStages.FindIndex((TimeLineStage timeLineStage) =>
            {
                return timeLineStage == curTimeLineStage;
            });
            if (-1 == idx || idx == timeLineStages.Count - 1)
            {
                return null;
            }
            return timeLineStages[idx + 1];

        }

        private Dictionary<int, EffectData> effectDatasMap = new Dictionary<int, EffectData>();
        private void RefreshEffectFrameEvents(List<EffectJosn> Effects, int timeLineBaseTime, int priority, string stageIdStr)
        {

            //第一次变量,构建 事件帧 和一个总的 效果 EffectDatasDic
            for (int i = 0; i < Effects.Count; i++)
            {
                EffectJosn effectJosn = Effects[i];
                List<EffectData> effectDatas = effectJosn.data;

                int count = effectDatas.Count;
                if (count == 0)
                {
                    continue;
                }
                // 每一层 的 effectDatas 可能有多个效果, 所以 每一层的 效果的优先级 + 50
                // 效果的 sort 排序 先 按时间， 相同时间就 按 优先级 从小到大 ，这样就能保重 同样的 执行时间 （A --->B , C ， ABC 都是 0帧执行 ）， 执行顺序是 ABC
                priority = priority + i * 50;

                effectDatas.ForEach((data) =>
                {
                    if (!effectDatasMap.ContainsKey(data.EffectID))
                    {
                        effectDatasMap.Add(data.EffectID, data);
                    }
                });

                // 效果帧采用效果的第一个效果执行，但需要把所有效果汇聚成一个技能的效果总表
                EffectData firstData = effectDatas[0];
                // 2023/2/2
                // 效果的 持续时间 采用 策划 配置的 效果结束时间 - 效果开始时间.
                /// note:
                ///     但是 需要注意,有些 效果 会播放 一个 特效/ 动作, 特效 / 动作 有它自己的时间,
                ///     长度 可能 会超过这个效果的实际长度. 
                ///     所以,
                ///         在实际的 效果帧的恢复的时候, 不能像 动作/特效 帧那样直接根据帧的长度 
                ///         判断是否需要恢复. 而是 需要根据 具体的效果的 实际长度 去做对应的恢复.
                int duration = firstData.EffectEndTime - firstData.ClinetExecuteTime;

                // 效果有自己的客户端开始时间(ClinetExecuteTime 策划配置的基于时间轴开始的时间)
                AddFrameEvent(firstData.ClinetExecuteTime, duration, firstData, E_StageFrameEventType.Effect, true, timeLineBaseTime, priority, stageIdStr);

                // 效果帧的结束 ,目前约定 效果的结束,就是下个效果开始时间
                switch (firstData.EffectType)
                {
                    case EffectType.UserInput:
                    case EffectType.ChargeInput:
                        {
                            // 增加输入轴效果的结束帧
                            AddFrameEvent(firstData.EffectEndTime, 0, firstData, E_StageFrameEventType.Effect, false, timeLineBaseTime, priority, stageIdStr);
                        }
                        break;

                    default: break;
                }
            }
        }


        private void RefreshAnimationFrameEvents(List<AnimationJson> animations, int timeLineBaseTime, int priority, string stageIdStr)
        {
            for (int i = 0; i < animations.Count; i++)
            {
                AnimationJson animation = animations[i];
                // 动画帧的 priority  增加 动画在阶段中的 index
                int animationPriority = priority + i + 1;
                InitFrameEvent(animation, E_StageFrameEventType.Animation, timeLineBaseTime, animationPriority, stageIdStr);
            }
        }

        private void RefreshFxFrameEvents(List<FXJson> fXJsons, int timeLineBaseTime, int priority, string stageIdStr)
        {
            RefreshFrameEvents<FXJson>(fXJsons, E_StageFrameEventType.SpecialEffects, timeLineBaseTime, priority, stageIdStr);
        }

        private void RefreshSoundFrameEvents(List<SoundJson> soundJsons, int timeLineBaseTime, int priority, string stageIdStr)
        {
            RefreshFrameEvents<SoundJson>(soundJsons, E_StageFrameEventType.Audio, timeLineBaseTime, priority, stageIdStr);
        }

        private void RefreshCameraFrameEvents(List<CameraJson> cameraJsons, int timeLineBaseTime, int priority, string stageIdStr)
        {
            RefreshFrameEvents<CameraJson>(cameraJsons, E_StageFrameEventType.Camera, timeLineBaseTime, priority, stageIdStr);
        }

        private void RefreshCameraShakeFrameEvents(List<CameraShakeJson> cameraJsons, int timeLineBaseTime, int priority, string stageIdStr)
        {
            RefreshFrameEvents<CameraShakeJson>(cameraJsons, E_StageFrameEventType.CameraShake, timeLineBaseTime, priority, stageIdStr);
        }

        private void RefreshFrameEvents<T>(List<T> frameJsons, E_StageFrameEventType stageFrameEventType, int timeLineBaseTime, int priority, string stageIdStr) where T : CommonClipJson
        {
            if (frameJsons == null)
            {
                return;
            }

            for (int i = 0; i < frameJsons.Count; i++)
            {
                T frameJson = frameJsons[i];
                InitFrameEvent(frameJson, stageFrameEventType, timeLineBaseTime, priority, stageIdStr);
            }
        }

        private void InitFrameEvent(CommonClipJson data, E_StageFrameEventType stageFrameEventType, int timeLineBaseTime, int priority, string stageIdStr)
        {
            if (data == null)
            {
                //SGF.Debuger.LogError($"{TagFlag} InitFrameEvent  stageFrameEventType: {stageFrameEventType}  cfg error!!!");
                return;
            }
            // 计算出 事件帧 的持续 时间
            int duration = data.End - data.Start;
            // 增加开始帧
            AddFrameEvent(data.Start, duration, data, stageFrameEventType, true, timeLineBaseTime, priority, stageIdStr);
            // 增加结束帧
            AddFrameEvent(data.End, 0, data, stageFrameEventType, false, timeLineBaseTime, priority, stageIdStr);
        }

        private void AddFrameEvent(int time, int duration, object data, E_StageFrameEventType stageFrameEventType, bool isStart, int timeLineBaseTime, int priority, string stageStr)
        {
            // 将帧的时间 设置为基于 整个timeLine时间轴的时间点
            int timeLineTime = time + timeLineBaseTime;
            TimeLineFrameEvent frameEvent = CreateFrameEvent(timeLineTime, duration, data, stageFrameEventType, isStart, priority, stageStr);
            timeLineFrameEvents.Add(frameEvent);
        }

        private TimeLineFrameEvent CreateFrameEvent(int timeLineTime, int duration, object data, E_StageFrameEventType stageFrameEventType, bool isStart, int priority, string stageStr)
        {
            // 创建一个基于TimeLine时间的 事件帧
            TimeLineFrameEvent frameEvent = new TimeLineFrameEvent(timeLineTime, duration, data, stageFrameEventType, stageStr);
            frameEvent.priority = priority;
            frameEvent.isFrameStart = isStart;

            return frameEvent;
        }

        protected override void Release()
        {
            base.Release();
            timeLineFrameEvents.Clear();
            timeLineStages.Clear();
            effectDatasMap.Clear();
        }
    }






}
