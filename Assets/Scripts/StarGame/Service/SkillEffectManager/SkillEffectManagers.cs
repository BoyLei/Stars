using System;
using System.Collections;
using System.Collections.Generic;
using SGF.Unity;
using UnityEngine;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 技能效果的 管理器 
    /// </summary>
    public class SkillEffectManager : Singleton<SkillEffectManager>
    {
        private List<Action> effectActions = new List<Action>();

        private List<Action> addEffectActions = new List<Action>();

        private List<Action> removeEffectActions = new List<Action>();


        public override void Init()
        {
            base.Init();
            MonoHelper.AddFixedUpdateListener(OnFixUpdate, MonoHelper.E_ModuleType.Battle);//碰撞也是逻辑层，不算表现要30fps

            effectActions.Clear();
            addEffectActions.Clear();
            removeEffectActions.Clear();
        }
        public override void Dispose()
        {
            base.Dispose();
            MonoHelper.RemoveFixedUpdateListener(OnFixUpdate, MonoHelper.E_ModuleType.Battle);//碰撞也是逻辑层，不算表现要30fps
        }
        public void OnFixUpdate()
        {
            if (addEffectActions.Count > 0)
            {
                addEffectActions.ForEach((ac) =>
                {
                    effectActions.Add(ac);
                });

                addEffectActions.Clear();
            }

            if (removeEffectActions.Count > 0)
            {
                removeEffectActions.ForEach((ac) =>
                {
                    effectActions.Remove(ac);
                });
                removeEffectActions.Clear();
            }


            effectActions.ForEach((effectAction) =>
            {
                try
                {
                    effectAction.Invoke();
                }
                catch (System.Exception e)
                {
                    removeEffectActions.Add(effectAction);
                    SGF.Debuger.LogError($"[SkillEffectManager] 执行effectAction 异常: {e.Message}");
                }
            });
        }

        public void RegisterFixedUpdateEffectAction(Action ac)
        {
            addEffectActions.Add(ac);
        }

        public void UnRegisterFixedUpdateEffectAction(Action ac)
        {
            removeEffectActions.Add(ac);
        }
    }
}


