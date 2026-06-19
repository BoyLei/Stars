using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SkillEditor
{
    public partial class EffectTypeHitEffect
    {
        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            fxJson.EffectName = System.IO.Path.GetFileNameWithoutExtension(PrefabPathInEditor);
            fxJson.EffectPath = System.IO.Path.GetFileNameWithoutExtension(PrefabPathInEditor);
            if (fxJson.EffectName != null)
            {
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(PrefabPathInEditor))
                {

                    GameObject pvb = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>($"{PrefabPathInEditor}.prefab");


                    var rootPs = SkillEditorUtils.GetRootPs(pvb);

                    if (rootPs != null)
                    {
                        // 如果 特效 是粒子的时候,将 粒子的时间导出来, 避免 技能做 效果恢复的时候，
                        // 因为不知道 特效的长度,而要加载每个特效.
                        fxJson.FxDuration = SkillEditorUtils.GetFxRootReallyDuration(rootPs);
                    }
                }

                fxJson.IsCameraImpulse = fxJson.EffectName.StartsWith("Impulse_");
                fxJson.IsCamaeraOffset = fxJson.EffectName.StartsWith("CameraOffset_");
                if(fxJson.config==null)
                {
                    Debug.LogWarning("fxJson.config==NULL  !!!!!");
                    fxJson.config = new SpecEffect();
                }
                fxJson.config.HangPoint = HangPoint;
                fxJson.config.IsLoop = IsLoop;
                fxJson.config.IsFaceToBuilder = IsFaceToBuilder;
                fxJson.config.IsFollowMove = IsFollowMove;
                fxJson.config.IsFollowRot = IsFollowRot;
                fxJson.config.IsFollowOwnerHide = IsFollowOwnerHide;
                fxJson.config.IsFollowOwnerScale = IsFollowOwnerScale;
                fxJson.config.XOffset = XOffset;
                fxJson.config.YOffset = YOffset;
                fxJson.config.ZOffset = ZOffset;
                fxJson.config.XOffsetTowards = XOffsetTowards;
                fxJson.config.YOffsetTowards = YOffsetTowards;
                fxJson.config.ZOffsetTowards = ZOffsetTowards;
#endif

            }
        }
    }
}
