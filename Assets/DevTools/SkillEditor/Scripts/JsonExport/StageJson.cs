///--------------------------------------------------------------------
/// 文件名   :   StageJson.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/01 15:23:25
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using MessagePack;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Task;
using UnityEngine;
using static LogUtils;

namespace SkillEditor
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: true)]
    public class StageJson
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public int Start;

        /// <summary>
        /// 解释时间
        /// </summary>
        public int End;

        /// <summary>
        /// 阶段时长
        /// </summary>
        public int Duration;

        /// <summary>
        /// 阶段ID
        /// </summary>
       // [Newtonsoft.Json.JsonIgnore]
        public int StageID;

        /// <summary>
        /// 阶段
        /// </summary>
        public StageType StageType;

        /// <summary>
        /// 普通阶段参数
        /// </summary>
        public StageNormal StageNormal;

        /// <summary>
        /// 子弹阶段参数
        /// </summary>
        public StageBulletMotion StageBulletMotion;

        /// <summary>
        /// 触发阶段参数
        /// </summary>
        public StageTrigger StageTrigger;

        /// <summary>
        /// BUFF开始阶段参数
        /// </summary>
        public StageBUFFStart StageBUFFStart;

        /// <summary>
        /// Buff结束阶段参数
        /// </summary>
        public StageBUFFEnd StageBUFFEnd;

        /// <summary>
        /// 被动结束阶段参数
        /// </summary>
        public StagePassiveEnd StagePassiveEnd;


        /// <summary>
        /// 缓存的效果列表
        /// </summary>
        public List<EffectJosn> Effects = new List<EffectJosn>();

        /// <summary>
        /// 特效
        /// </summary>
        public List<FXJson> Fxs = new List<FXJson>();

        /// <summary>
        /// 音效
        /// </summary>
        public List<SoundJson> Sounds = new List<SoundJson>();

        /// <summary>
        /// 相机
        /// </summary>
        public List<CameraJson> Cameras = new List<CameraJson>();

        /// <summary>
        /// 相机
        /// </summary>
        public List<CameraShakeJson> ShakeCameras = new List<CameraShakeJson>();

        /// <summary>
        /// 动画
        /// </summary>
        public List<AnimationJson> Animations = new List<AnimationJson>();

        public void Sort()
        {

            Effects.Sort((ef1, ef2) =>
            {
                if (ef1.Start == ef2.Start)
                {
                    return 0;
                }
                else if (ef1.Start > ef2.Start)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            });
        }

        public bool InStage(double time,bool IsRight)
        {
            if (!IsRight)
            {
                time = (int)(time * 1000);
                return (time >= Start && time < End);
            }
            else
            {
                time = (int)(time * 1000);
                if (Start == 0)
                {
                    return (time >= Start && time <= End);
                }
                return (time > Start && time <= End);
            }
        }

        //public bool EffectInStage(double time)
        //{
        //    time = (int)(time * 1000);
        //    if (Start == 0)
        //    {
        //        return (time >= Start && time <= End);
        //    }

        //    return (time > Start && time <= End);
        //}
        public bool ShouldSerializeStageNormal()
        {
            return StageType == SkillEditor.StageType.NormalStage;
        }

        public bool ShouldSerializeBulletMotion()
        {
            return StageType == SkillEditor.StageType.BulletStage;
        }


        public bool ShouldSerializeStageTrigger()
        {
            return StageType == SkillEditor.StageType.TriggerStage;
        }

        public bool ShouldSerializeStageBUFFStart()
        {
            return StageType == SkillEditor.StageType.AddBuffStage;
        }

        public bool ShouldSerializeStageBUFFEnd()
        {
            return StageType == SkillEditor.StageType.EndBuffStage;
        }

        private int loopCount = -2;
        public int GetLoopCount()
        {
            if(loopCount != -2)
            {
                return loopCount;
            }

            loopCount = 1;
            switch (StageType)
            {
                case StageType.NormalStage:
                    {
                        loopCount = StageNormal.LoopCount;
                    }
                    break;
                case StageType.BulletStage:
                    {
                        loopCount = StageBulletMotion.LoopCount;
                    }
                    break;
            }
            return loopCount;
        }
    }
}