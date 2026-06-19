//using MoreMountains.Feedbacks;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;

namespace StarProject.Service.Cam.Data
{

    public class CameraFxParam
    {
        public CameraEvent E_CameraEvent;
        public int Types = 1;//0是自定义模式，是完全按照参数来|其他种类都是我们写死完全不读参数
        public List<float> Paras = new List<float>();//参数组


        public Action<bool> PlayCallBack;
        public bool IsAutoKill;
        public float LifeTime;

        //我肯定时刻调用，
        public InterruptCameraFx M_InterruptCameraFx;

        public ulong entityID;

        private string TagFlag = "[CameraFxParam]";

        /// <summary>
        /// 震屏效果参数的统一封装接口
        /// </summary>
        public void InitCameraShakeParam(float duration, float amplitude, float frequency, float amplitudeX = 0f, float amplitudeY = 0f, float amplitudeZ = 0f)
        {
            E_CameraEvent = CameraEvent.ShakeCam;
            Paras.Clear();

            Paras.Add(duration);
            Paras.Add(amplitude);
            Paras.Add(frequency);

            Paras.Add(amplitudeX);
            Paras.Add(amplitudeY);
            Paras.Add(amplitudeZ);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration">摄像机zoom拉起后，持续的时间</param>
        /// <param name="newFieldOfView"></param>
        /// <param name="transitionDuration">摄像机zoom拉起的过渡时间</param>
        public void InitCameraZoomParam(float duration, float newFieldOfView, float transitionDuration)
        {
            E_CameraEvent = CameraEvent.CameraZoom;
            Paras.Clear();

            Paras.Add(duration);
            Paras.Add(newFieldOfView);
            Paras.Add(transitionDuration);
        }

        public void Init(/* ulong entityId,  */int cameraEffectId, E_CameraEffectType cameraEffectType)
        {
            // entityID = entityId;

            LocalDataManager LDM = LocalDataManager.Instance;

            switch (cameraEffectType)
            {
                case E_CameraEffectType.Shake:
                    {
                        CameraShakeDataCell cfg = LDM.GetCameraShakeDataCell(cameraEffectId);
                        if (cfg == null)
                        {
                            SGF.Debuger.LogError($"{TagFlag} Init e_CameraEffectType {cameraEffectType} cameraEffectId {cameraEffectId} cfg : null!!!");
                            return;
                        }
                        E_CameraShakeType cameraShakeType = (E_CameraShakeType)cfg.GetType();

                        float duration = cfg.GetDuration() / 1000f;
                        float amplitude = cfg.GetAmplitude() / 100f;
                        float frequency = cfg.GetFrequency();
                        float amplitudeX = 0;
                        float amplitudeY = 0;
                        float amplitudeZ = 0;

                        switch (cameraShakeType)
                        {
                            case E_CameraShakeType.X:
                                {
                                    amplitudeX = amplitude;
                                    amplitude = 0;
                                }
                                break;
                            case E_CameraShakeType.Y:
                                {
                                    amplitudeY = amplitude;
                                    amplitude = 0;
                                }
                                break;
                            case E_CameraShakeType.Z:
                                {
                                    amplitudeZ = amplitude;
                                    amplitude = 0;
                                }
                                break;
                            case E_CameraShakeType.Random:
                                {
                                    //就随机了，直接使用配置表中的amplitude
                                }
                                break;
                            default:
                                {
                                    SGF.Debuger.LogError($"{TagFlag} Init e_CameraEffectType {cameraEffectType} cameraEffectId {cameraEffectId} no handle!!!");
                                }
                                break;
                        }
                        SGF.Debuger.Log($@"{TagFlag} Init e_CameraEffectType {cameraEffectType} cameraEffectId {cameraEffectId} InitCameraShakeParam
                           duration {duration} , amplitude {amplitude} , frequency {frequency} , amplitudeX {amplitudeX} , amplitudeY {amplitudeY} , amplitudeZ {amplitudeZ}
                         ");


                        InitCameraShakeParam(duration, amplitude, frequency, amplitudeX, amplitudeY, amplitudeZ);
                    }
                    break;
                case E_CameraEffectType.Zoom:
                    {
                        CameraZoomDataCell cfg = LDM.GetCameraZoomDataCell(cameraEffectId);
                        if (cfg == null)
                        {
                            SGF.Debuger.LogError($"{TagFlag} Init e_CameraEffectType {cameraEffectType} cameraEffectId {cameraEffectId} cfg : null!!!");
                            return;
                        }
                        float duration = cfg.GetDuration() / 1000f;
                        float newFieldOfView = cfg.GetFieldView();
                        float transitionDuration = cfg.GetTransitionDuration() / 100f;
                        SGF.Debuger.Log($@"{TagFlag} Init e_CameraEffectType {cameraEffectType} cameraEffectId {cameraEffectId} InitCameraShakeParam
                           duration {duration} , newFieldOfView {newFieldOfView} , transitionDuration {transitionDuration} 
                         ");
                        InitCameraZoomParam(duration, newFieldOfView, transitionDuration);
                    }
                    break;

                default:
                    {
                        SGF.Debuger.LogError($"{TagFlag} Init e_CameraEffectType {cameraEffectType} cameraEffectId {cameraEffectId} no handle!!!");
                    }
                    break;
            }
        }

    }

    /// <summary>
    /// 引用打断器
    /// </summary>
    public interface InterruptCameraFx
    {
        //默认False
        bool IsInterrupt { set; get; }
        //打断这不必告知很多人
        //调用者也不必告知很多人
    }
}
