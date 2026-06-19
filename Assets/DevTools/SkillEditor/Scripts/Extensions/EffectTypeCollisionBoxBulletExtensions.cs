using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace SkillEditor
{
    public partial class EffectTypeCollisionBoxBullet
    {
        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
#if UNITY_EDITOR
            //与服务器约定，如果这个字符串为空，则视为重复计算。如果这个字符串有值，则视为不重复计算碰撞盒
            if (IsNoDuplication == true)
            {
                IsNoDuplicationKey.Result = OutputKey.Result + "IsNoDuplicationKey";
                IsNoDuplicationKey.SaveSkill = true;
            }
            else 
            {
                IsNoDuplicationKey.Result = "";
                IsNoDuplicationKey.SaveSkill = true;
            }
#endif
        }

        public bool isNoDuplication()
        {
            return IsNoDuplication;
        }
    }
}
