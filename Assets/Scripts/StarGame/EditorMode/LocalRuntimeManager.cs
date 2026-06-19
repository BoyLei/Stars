using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using SkillEditor;
using StarProject.Game.Skill;
using StarProjectDef;
using UnityEngine;
using UVec3 = UnityEngine.Vector3;

namespace EditorModeTest
{
    public class LocalRuntimeManager
    {
        /// <summary>
        /// 运行时 map， 主要是 最上级的 比如 子弹/buff/被动/技能运行时. 由 runtimeID --> runtime
        /// </summary>
        public DictionaryEx<ulong, BaseParentRuntime> runtimes = new();
        public DictionaryEx<ulong, BaseParentRuntime> uid2Runtimes = new();

        private void AddRuntime(BaseParentRuntime localServerRuntime)
        {
            runtimes.Add(localServerRuntime.RuntimeID, localServerRuntime);
            uid2Runtimes.Add(localServerRuntime.UID, localServerRuntime);
        }

        public LocalBulletRuntime CreateBulletRuntime(int bulletID, BaseBlackBoard bulletBlackBoard)
        {
            LocalBulletRuntime localBulletRuntime = new LocalBulletRuntime(bulletID, bulletBlackBoard);

            AddRuntime(localBulletRuntime);
            return localBulletRuntime;
        }



        public void OnTick()
        {
            runtimes.ForEach((KeyValuePair<ulong, BaseParentRuntime> item) =>
            {
                item.Value.OnTick();
            });
        }

        public BaseParentRuntime GetRuntime(ulong runtimeID)
        {
            return runtimes[runtimeID];
        }

        public BaseParentRuntime GetUIDRunime(ulong uid)
        {
            return uid2Runtimes[uid];
        }

        /// <summary>
        /// 设置 运行时的 目标ian
        /// </summary>
        /// <param name="runtimeID"></param>
        /// <param name="targetPos"></param> <summary>
        public void SetRuntimeTargetPos(ulong runtimeID, UVec3 targetPos)
        {
            GetRuntime(runtimeID)?.SetTargetPos(targetPos);
        }

        public void TriggerEvent(TriggerEvent triggerEvent, object triggerData)
        {
            runtimes.ForEach((item) =>
            {
                item.Value.TriggerEvent(triggerEvent, triggerData);
            });
        }

        public void TriggerRuntimeEvent(ulong runtimeID, TriggerEvent triggerEvent, object triggerData)
        {
            GetRuntime(runtimeID)?.TriggerEvent(triggerEvent, triggerData);
        }
    }

}