using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using SkillEditor;

namespace StarProject.Game.Entity.View.VitalSign.State
{
    public interface I_AnimParam
    {
        /// <summary>
        /// 动画路径, 目前 可能存在2种情况:
        /// 1.没有 avatarDataCell , 那 AnimationPath 就是一个完全 的动画路径;
        /// 2.存在 avatarDataCell , 那 AnimationPath 就是一个 动画名
        /// </summary>

        string AnimationPath { get; }
        bool Loop { get; }
        float Speed { get; }
        float StartTime { get; }

        /// <summary>
        /// 动画 播放的时候,采用上一个 状态的 动画时间 作为 本动画的 startTime 开始时间
        /// </summary>
        bool UseLastStateTimeAsStartTime { get; }

        /// <summary>
        /// 动画唯一的key,类似于uid这种,用来做标识
        /// note:
        ///     AnimationName可能重复,所以不能用它做key
        /// </summary>
        string Key { get; }
        /// <summary>
        ///  播放动画的类型, 技能/子弹/buff/被动 都是用他们自己的阶段类型. 默认的类型 是 E_StageType.None
        /// </summary>
        /// <value></value>
        E_StageType AnimType { get; }

        List<AnimAndCondition> AnimatorSp { get; }

        /// <summary>
        /// 进入这个动画的 进入融合时间
        /// </summary>
        /// <value></value>
        int FadeInTime { get; }

        /// <summary>
        /// 设置 当前的 条件动画, 动画播放时 会优先播放这个条件动画
        /// </summary>
        /// <param name="animAndCondition"></param>
        void SetCurAnimAndCondition(AnimAndCondition animAndCondition);
        /// <summary>
        /// 当前的 动画路径, 会 根据当前 是否 处于条件动画 这个状态来处理
        /// </summary>
        /// <value></value>
        string GetCurAnimationPath(AvatarDataCell avatarDataCell);
        string GetCurAnimationName();

        void CopyTo(AnimParam animParam);
    }

    public class AnimParam : I_AnimParam
    {
        // 保存动作路径
        private string animationPath;
        public string AnimationPath => animationPath;
        private bool loop;
        public bool Loop => loop;
        private float speed;
        public float Speed => speed;

        private float startTime;

        public float StartTime => startTime;

        private bool useLastStateTimeAsStartTime = false;
        public bool UseLastStateTimeAsStartTime => useLastStateTimeAsStartTime;
        private string key;
        public string Key => $"{animationPath}_{key}_{extraKey}";

        private string extraKey;

        private E_StageType animType = E_StageType.None;
        public E_StageType AnimType { get => animType; }

        private List<AnimAndCondition> animatorSp = null;
        public List<AnimAndCondition> AnimatorSp { get => animatorSp; }

        private int fadeInTime = 0;
        public int FadeInTime => fadeInTime;

        private AnimAndCondition curAnimAndCondition;

        private AnimationJson animationJson1;

        /// <summary>
        /// 恢复接口,增加的属性 需要在 Reset中 清除
        /// </summary>
        public void Reset()
        {
            animationPath = "";
            loop = false;
            speed = 1;
            animationJson1 = null;

            useLastStateTimeAsStartTime = false;

            fadeInTime = -1;
        }

        /// <summary>
        /// 初始化 动画参数
        /// </summary>
        /// <param name="animationJson"></param>
        /// <param name="e_StageType">创建动画 参数时, 动画对应的类型</param>
        public void InitAnimationJson(SkillEditor.AnimationJson animationJson, E_StageType e_StageType)
        {
            Reset();
            animationJson1 = animationJson;
            animationPath = animationJson.ClipPath;

            //默认约定 动画是 false;
            loop = false;
            //TODO DL 
            //get skill speed
            speed = (float)animationJson.SpeedMultiplier;

            key = animationJson.Index.ToString();

            animatorSp = animationJson.AnimatorCustomData.AnimatorSp;

            animType = e_StageType;

            // 动画融合的 时间
            SetFadeInTime(animationJson.AnimatorCustomData.FadeDuration);

        }

        public void SetFadeInTime(int time)
        {
            fadeInTime = time;
            // 动画融合的默认时间 如果没有配置,就设置为 -1
            fadeInTime = fadeInTime > 0 ? fadeInTime : -1;
        }

        public void SetExtraKey(string extraKeyStr)
        {
            extraKey = extraKeyStr;
        }

        public void SetBaseAnimSpeed(float _speed)
        {
            speed = _speed;
        }

        public void SetBaseAnimName(string animPath)
        {
            animationPath = animPath;
        }

        /// <summary>
        /// 设置 动画 播放的开始时间
        /// </summary>
        /// <param name="_startTime"></param>
        public void SetStartTime(float _startTime)
        {
            startTime = _startTime;
        }

        /// <summary>
        /// 设置 上个状态的 时间为本状态的开始时间
        /// note:
        ///     动画切换时,如果设置了这个变量, 这个动画会采用上个动画的播放时间 作为这个动画的起始播放时间
        /// </summary>
        /// <param name="useLastStatTime"></param>
        public void SetUseLastStateTimeAsStartTime(bool useLastStatTime)
        {
            useLastStateTimeAsStartTime = useLastStatTime;
        }

        public void SetCurAnimAndCondition(AnimAndCondition animAndCondition)
        {
            curAnimAndCondition = animAndCondition;
        }



        public string GetCurAnimationPath(AvatarDataCell avatarDataCell)
        {
            string path = AnimationPath;
            if (avatarDataCell == null)
            {
                return path;
            }
            if (curAnimAndCondition != null)
            {
                path = curAnimAndCondition.Anim;
            }
            else
            {
                // AnimationPath 可能传过来的就是全路径了
                if (!AnimationPath.Contains(avatarDataCell.AnimsPath))
                {
                    path = $"{avatarDataCell.AnimsPath}/{AnimationPath}";
                }
            }
            path = path.Replace("Assets/Res/", string.Empty);
            path = path.Replace(".anim", string.Empty);

            return path;
        }

        public string GetCurAnimationName()
        {
            string animName = AnimationPath;

            if (curAnimAndCondition != null)
            {
                animName = GetPathFileName(curAnimAndCondition.Anim);
            }
            else
            {
                animName = AnimationPath;
            }
            return animName;
        }

        private string GetPathFileName(string path)
        {
            int idx = path.LastIndexOf("/");
            string fileName = path.Substring(idx + 1).Replace(".anim", string.Empty);
            return fileName;
        }

        public void CopyTo(AnimParam clone)
        {
            clone.InitAnimationJson(animationJson1, animType);
            clone.SetExtraKey(extraKey);
            clone.SetBaseAnimSpeed(speed);
            clone.SetBaseAnimName(animationPath);
            clone.SetStartTime(startTime);
            clone.SetCurAnimAndCondition(curAnimAndCondition);
            clone.SetUseLastStateTimeAsStartTime(useLastStateTimeAsStartTime);

        }
    }
}
