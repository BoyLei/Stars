using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using MoreMountains.Feedbacks;
//using MoreMountains.FeedbacksForThirdParty;
using SGF.Unity;
using StarProject.Service.Cam.Data;
using StarProject.Service.Cam;
using StarProjectDef;
using StarProject;

/// <summary>
/// 博哥语录：
/// GameEffect特效上  他有容纳分类，他是被动的。
///【因为所有特效都有这个类】
///
///
///而震屏是一个 可以被效果/特效  生成的
///如果基于效果：那一定是明确的
///如果基于特效：（现在）那就需要知道两个类型
///
///广播：不管，甚至关联相机渲染（渲染范围，空间范围）
///固定id：明确固定的某一个nttid
///
///要明确就效果
///不明确就特效

///客户端判定就好了
///但是未必能做（看网络优化，还是客户端判定）

///我打怪，是我就可以震动
///怪打我，看是不是在【效果】中包含MainPlayerId，就震动我的摄像机屏幕（我管理的人有多个，我管理的怪有多个，我的相机只有一个（MainPlayerId==绑定主相机==持久绑定关系））
/// </summary>
namespace StarProject.Game.StarsCamera
{
    public class GameCameraFeel : MonoBehaviour
    {
       /* [Header("Cooldown")]
        /// a duration, in seconds, between two jumps, during which jumps are prevented
        [Tooltip("a duration, in seconds, between two jumps, during which jumps are prevented")]
        public float CooldownDuration = 1f;

        public MMFeedbacks CameraFeedbacks;

        private string flagKey = "[GameCameraFeel]";

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            CameraFeedbacks = GameObjectUtils.EnsureComponent<MMFeedbacks>(this.gameObject);
        }

        public void InitFeedBacks(List<MMFeedback> mMFeedbacks)
        {
            CameraFeedbacks.Feedbacks.Clear();
            mMFeedbacks.ForEach((feedback) =>
            {
                CameraFeedbacks.Feedbacks.Add(feedback);
            });
            CameraFeedbacks.PlayFeedbacks();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="amplitude"></param>
        /// <param name="frequency"></param>
        /// <param name="amplitudeX"></param>
        /// <param name="amplitudeY"></param>
        /// <param name="amplitudeZ"></param>
        /// <returns></returns>
        private MMFeedbackCameraShake InitShakeFeedback(float duration, float amplitude, float frequency, float amplitudeX = 0f, float amplitudeY = 0f, float amplitudeZ = 0f)
        {
            MMFeedbackCameraShake mMFeedbackCameraShake = new MMFeedbackCameraShake();
            MMCameraShakeProperties mMCameraShakeProperties = mMFeedbackCameraShake.CameraShakeProperties;

            mMCameraShakeProperties.Duration = duration;
            mMCameraShakeProperties.Amplitude = amplitude;
            mMCameraShakeProperties.Frequency = frequency;
            mMCameraShakeProperties.AmplitudeX = amplitudeX;
            mMCameraShakeProperties.AmplitudeY = amplitudeY;
            mMCameraShakeProperties.AmplitudeZ = amplitudeZ;

            return mMFeedbackCameraShake;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="newFieldOfView"></param>
        /// <param name="transitionDuration"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private MMFeedbackCameraZoom InitZoomFeedback(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration)
        {
            MMFeedbackCameraZoom mMFeedbackCameraZoom = new MMFeedbackCameraZoom();
            mMFeedbackCameraZoom.ZoomMode = mode;
            mMFeedbackCameraZoom.ZoomFieldOfView = newFieldOfView;
            mMFeedbackCameraZoom.ZoomTransitionDuration = transitionDuration;
            mMFeedbackCameraZoom.ZoomDuration = duration;

            return mMFeedbackCameraZoom;
        }

        private enum EnumCurveValue
        {
            ZeroMin,
            ZeroMax,
            OneMin,
            OneMax,
        }

        private float CalculateCurveValue(float value, EnumCurveValue key)
        {
            if (value == 0)
            {
                return value;
            }
            else
            {
                switch (key)
                {
                    case EnumCurveValue.ZeroMin:
                        {
                            value = -value - 0.1f;
                        }
                        break;
                    case EnumCurveValue.ZeroMax:
                        {
                            value = -value;
                        }
                        break;
                    case EnumCurveValue.OneMin:
                        {
                            //dont do anything
                            // value = value;
                        }
                        break;
                    case EnumCurveValue.OneMax:
                        {
                            value = value + 0.1f;
                        }
                        break;
                }
                return value;
            }
        }

        private Vector3 CalculateCurveVector(float x, float y, float z, EnumCurveValue key)
        {
            Vector3 v3 = new Vector3();

            v3.x = CalculateCurveValue(x, key);
            v3.y = CalculateCurveValue(y, key);
            v3.z = CalculateCurveValue(z, key);
            return v3;
        }

        public void DoCameraAction(List<CameraFxParam> cameraFxParams, int cameraEventID)
        {
            for (int i = 0; i < cameraFxParams.Count; i++)
            {
                CameraFxParam item = cameraFxParams[i];
                switch (item.E_CameraEvent)
                {
                    case CameraEvent.ShakeCam:
                        {
                            List<float> paras = item.Paras;
                            if (paras.Count < 3)
                            {
                                SGF.Debuger.LogError($"{flagKey} DoCameraAction : {(CameraEvent)item.E_CameraEvent} paras : {paras.KJoin("")} , length : {paras.Count}");
                                return;
                            }

                            float duration = paras[0];
                            float amplitude = paras[1];
                            float frequency = paras[2];

                            float amplitudeX = paras.Count < 4 ? 0 : paras[3];
                            float amplitudeY = paras.Count < 5 ? 0 : paras[4];
                            float amplitudeZ = paras.Count < 6 ? 0 : paras[5];

                            // MMFeedbackCameraShake mMFeedbackCameraShake = InitShakeFeedback(duration, amplitude, frequency, amplitudeX, amplitudeY, amplitudeZ);
                            // CameraFeedbacks.Feedbacks.Add(mMFeedbackCameraShake);

                            //Feelbacks暂时不用，先直接触发
                            if (duration > 0)
                            {
                                //Curve 数据要做个修正
                                //目前采用了 Curve，所以对于Curve的参数，需要根据amplitude 去做单独的设置
                                {


                                    Vector3 remapCurveZeroMin = CalculateCurveVector(amplitudeX, amplitudeY, amplitudeZ, EnumCurveValue.ZeroMin);
                                    Vector3 remapCurveZeroMax = CalculateCurveVector(amplitudeX, amplitudeY, amplitudeZ, EnumCurveValue.ZeroMax);
                                    Vector3 remapCurveOneMin = CalculateCurveVector(amplitudeX, amplitudeY, amplitudeZ, EnumCurveValue.OneMin);
                                    Vector3 remapCurveOneMax = CalculateCurveVector(amplitudeX, amplitudeY, amplitudeZ, EnumCurveValue.OneMax);

                                    //frequency 频率阀值，超过这个阀值，发生转向
                                    frequency = frequency / 1000f;
                                    if (frequency < 0.1f)
                                    {
                                        frequency = 0.1f;
                                    }

                                    MMCameraShakeEvent.TriggerInitCurve(remapCurveZeroMin, remapCurveZeroMax, remapCurveOneMin, remapCurveOneMax);
                                }

                                // MMCameraShakeEvent.Trigger(duration, amplitude, frequency,
                                //                             amplitudeX, amplitudeY, amplitudeZ, true, 0, true);

                                //临时替换一种震屏方式，mm的方式，在时间长度较大时，效果会比较好，但对于时间比较小的
                                //比如 震屏0.2s，这种频率较高的方式表现很差

                                GlobalEvent.OnCameraShakeEvent.Invoke(cameraEventID, duration, frequency, new Vector3(amplitudeX, amplitudeY, amplitudeZ));
                                return;
                            }
                            else
                            {
                                SGF.Debuger.LogError($"{flagKey} DoCameraAction : {(CameraEvent)item.E_CameraEvent} duration == 0 ,cfg error!!!");
                                return;
                            }

                        }
                        break;
                    case CameraEvent.CameraZoom:
                        {
                            List<float> paras = item.Paras;
                            if (paras.Count < 3)
                            {
                                SGF.Debuger.LogError($"{flagKey} DoCameraAction : {(CameraEvent)item.E_CameraEvent} paras : {paras.KJoin("")} , length : {paras.Count}");


                                return;
                            }

                            float duration = paras[0];
                            float newFieldOfView = paras[1];
                            float transitionDuration = paras[2];

                            // MMFeedbackCameraZoom mMFeedbackCameraZoom = InitZoomFeedback(mode, newFieldOfView, transitionDuration, duration);
                            // CameraFeedbacks.Feedbacks.Add(mMFeedbackCameraZoom);

                            //Feelbacks暂时不用，先直接触发
                            if (duration > 0)
                            {
                                MMCameraZoomEvent.Trigger(MMCameraZoomModes.For, newFieldOfView, transitionDuration, duration, 0);
                                return;
                            }
                            else
                            {
                                SGF.Debuger.LogError($"{flagKey} DoCameraAction : {(CameraEvent)item.E_CameraEvent} duration == 0 ,cfg error!!!");
                                return;
                            }
                        }
                        break;

                    default:
                        {
                            SGF.Debuger.LogError($"{flagKey} DoCameraAction : {(CameraEvent)item.E_CameraEvent} need func");
                        }
                        break;
                }
            }

            {
                //TODO: dl
                //暂时不用FeelBacks，直接用 cameraShake和 cameraZoom
                // CameraFeedbacks.PlayFeedbacks();
            }
        }

        public void StopCameraAction(CameraEvent cameraEvent, int cameraEventID)
        {
            switch (cameraEvent)
            {
                case CameraEvent.ShakeCam:
                    {
                        // MMCameraShakeStopEvent.Trigger(0);
                        GlobalEvent.OnCameraStopShakeEvent.Invoke(cameraEventID);

                    }
                    break;
                case CameraEvent.CameraZoom:
                    {
                        //Feelbacks暂时不用，先直接触发
                        {
                            MMCameraZoomStopEvent.Trigger();
                        }
                    }
                    break;
                default:
                    {

                    }
                    break;
            }

        }*/
    }
}