///--------------------------------------------------------------------
/// 文件名   :   ClipStageBehaviour.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/01 18:35:22
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace SkillEditor
{
    [System.Serializable]
    public class ClipStageBehaviour : PlayableBehaviour
    {
        [ShowInInspector]
        [HideLabel]
        public StageNormal data =new StageNormal();
        
        [HideInInspector]
        public GameObject owner;

        [HideInInspector]
        public ClipStageAsset Asset;
        public override void OnPlayableCreate(Playable playable)
        {
            base.OnPlayableCreate(playable);
        }

        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);
        }

        public override void OnGraphStop(Playable playable)
        {
            base.OnGraphStop(playable);
        }
#if UNITY_EDITOR
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
        }
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (SkillEditorGlobal.PlayableDirector == null)
            {
                return;
            }
            double curTime = SkillEditorGlobal.PlayableDirector.time;
            var controlClips = TimelineTools.GetCurTimelineClips<StarControlTrack>(GetTrackType.After, SkillEditorGlobal.PlayableDirector.time);
            var effectClips = TimelineTools.GetCurTimelineClips<EffectTrack>(GetTrackType.Before, SkillEditorGlobal.PlayableDirector.time);

            //处理特效轴相关内容
            Vector3 cameraOffset = Vector3.zero;
            foreach (var clip in controlClips.curTracks)
            {
                ClipStarControlAsset clipStarControlAsset = (ClipStarControlAsset)clip.asset;
                if (clipStarControlAsset.prefabGameObject != null)
                {
                    CinemachineImpulseSource cis = clipStarControlAsset.prefabGameObject.GetComponent<CinemachineImpulseSource>();
                    if (cis != null)
                    {
                        double ratio = (curTime - clip.start) / cis.m_ImpulseDefinition.m_ImpulseDuration;
                        cameraOffset += cis.m_ImpulseDefinition.ImpulseCurve.Evaluate((float)ratio) * cis.m_DefaultVelocity;
                    }
                    else
                    {
                        clipStarControlAsset.template.SetTrans(clipStarControlAsset);
                    }

                }
            }
            SkillEditorGlobal.Instance.SetCameraCurOffset(cameraOffset);

            //处理位移相关效果
            var moveOffset = Vector3.zero;
            foreach (var clip in effectClips.curTracks)
            {
                ClipEffectAsset clipEffectAsset = (ClipEffectAsset)clip.asset;
                if (clipEffectAsset.template.frameEffect != null)
                {

                    foreach (var effect in clipEffectAsset.template.frameEffect)
                    {
                        if (effect != null)
                        {
                            if (effect.EffectArgs.BaseEffect != null && effect.EffectArgs.BaseEffect.GetType() == typeof(EffectTypeMoveWithRot))
                            {
                                //接受一下效果里面的数据
                                EffectTypeMoveWithRot moveWithRot = (EffectTypeMoveWithRot)effect.EffectArgs.BaseEffect;
                                if (moveWithRot.DurningTime != 0)
                                {
                                    //根据比值来进行计算
                                    double radio = (curTime - clip.start) / (moveWithRot.DurningTime / 1000f);
                                    //拿到位移终点
                                    Vector3 targetPos = new Vector3(moveWithRot.Distance / 100f * Mathf.Sin(Mathf.Deg2Rad * moveWithRot.SingleToward.TowardOffset), 0, moveWithRot.Distance / 100f * Mathf.Cos(Mathf.Deg2Rad * moveWithRot.SingleToward.TowardOffset));
                                    //根据比值计算
                                    moveOffset += radio > 1 ? targetPos : targetPos * (float)radio;
                                }
                            }
                        }
                    }
                }
            }
            foreach (var clip in effectClips.otherTracks)
            {
                ClipEffectAsset clipEffectAsset = (ClipEffectAsset)clip.asset;
                if (clipEffectAsset.template.frameEffect != null)
                {

                    foreach (var effect in clipEffectAsset.template.frameEffect)
                    {
                        if (effect.EffectArgs.BaseEffect != null)
                        {
                            if (effect.EffectArgs.BaseEffect.GetType() == typeof(EffectTypeMoveWithRot))
                            {
                                //接受一下效果里面的数据
                                EffectTypeMoveWithRot moveWithRot = (EffectTypeMoveWithRot)effect.EffectArgs.BaseEffect;
                                if (moveWithRot.DurningTime != 0)
                                {
                                    //根据比值来进行计算
                                    double radio = (curTime - clip.start) / (moveWithRot.DurningTime / 1000f);
                                    //拿到位移终点
                                    Vector3 targetPos = new Vector3(moveWithRot.Distance / 100f * Mathf.Sin(Mathf.Deg2Rad * moveWithRot.SingleToward.TowardOffset), 0, moveWithRot.Distance / 100f * Mathf.Cos(Mathf.Deg2Rad * moveWithRot.SingleToward.TowardOffset));
                                    //根据比值计算
                                    moveOffset += radio > 1 ? targetPos : targetPos * (float)radio;
                                }
                            }
                        }
                    }
                }
            }
            SkillEditorGlobal.SetCurOffset(moveOffset);
        }
#endif
    }
}
