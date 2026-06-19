using System.Collections;
using System.Collections.Generic;
using SGF.Time;
using StarProject.Service.LocalDynamic.Fx;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Service.Resource
{
    /// <summary>
    /// 特效的 资源类型信息
    /// 相同的 scene/map 可以存在多个不同的 特效节点.
    /// 当 map/scene 切换的时候， 需要去释放对应 map/scene 的特效.
    /// </summary>
    public sealed class FxResourceInfo : ResourceInfo
    {
        public GameEffect ge;

        private long createTime = 0;


        public void Create(string path, GameEffect fxGe, ResoruceReleaseType releaseType = ResoruceReleaseType.MapSceneAndServerID)
        {
            base.Create(path, releaseType);
            ge = fxGe;
            createTime = TimeUtils.ClientNowStampMilli;
        }

        /// <summary>
        /// 特效被重新使用 需要重置 createTime 
        /// </summary>
        public void ReUse()
        {
            createTime = TimeUtils.ClientNowStampMilli;
        }

        /// <summary>
        /// 检查特效是否可以从池里复用
        /// </summary>
        /// <returns></returns>
        public bool CheckFxCanReUse(LocalEffectTags tag)
        {
            //没有池要新增，没有可用的要新增，但是否可用这里限定
            //同特效，不同应用途径不可池共享属性，因为不想做重生
            //不想做迁移，情况少，如共享池处理都不如你把资源分成两部分
            if (ge != null && !ge.Playing && ge.M_localEffectTags == tag)
            {
                return true;
            }
            return false;
        }

        protected override void Release()
        {
            // SGF.Debuger.Log($"[Res] fx 准备释放: {SceneName}, {MapId}, {Path}");
#if UNITY_EDITOR
            // SGF.Debuger.Log($"[Res] fx 准备释放: {SceneName}, {Path}");
            // SGF.Debuger.Log($"[Res] fx 准备释放: , {Path}");
#endif
            if (ge != null)
            {
                ge.DestroyRelease();
            }
            ge = null;
            createTime = 0;

            base.Release();
        }

        /// <summary>
        /// 检查特效是否超时
        /// </summary>
        public bool IsOverTime(int maxTime = 300000)
        {
            if (ge == null)
            {
                return true;
            }
            // 正在播放的特效 不需要超时检测
            if (ge.Playing)
            {
                return false;
            }
            // 如果特效的 创建时间 > 5min 并且 特效没有在播放
            if (TimeUtils.ClientNowStampMilli - createTime >= maxTime)
            {
                return true;
            }

            return false;
        }
    }
}