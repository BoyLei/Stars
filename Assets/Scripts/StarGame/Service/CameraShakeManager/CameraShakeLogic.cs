///--------------------------------------------------------------------
/// 文件名   :   CmaeraShakeLogic.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/18 09:51:29
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Cinemachine;
using StarProject.Game.Entity.Factory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Service.CameraShake
{
    public class CameraShakeLogic
    {
        /// <summary>
        /// 阶段
        /// </summary>
        private ShakeStage _shakeStage = ShakeStage.None;

        public ShakeStage ShakeStage { get { return _shakeStage; } }

        /// <summary>
        /// 虚拟相机
        /// </summary>
        public CinemachineVirtualCameraBase VirtualCamera { get; private set; }

        /// <summary>
        /// 振幅AnimationCurve
        /// </summary>
        public AnimationCurve AmplitudeGain { get; private set; }

        /// <summary>
        /// 频率AnimationCurve
        /// </summary>
        public AnimationCurve FrequencyGain { get; private set; }


        private List<CinemachineBasicMultiChannelPerlin> m_VirtualCameraPerlinList;

        public int Index { get; private set; }
        /// <summary>
        /// 震动时长
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// 当前时间
        /// </summary>
        private float CurTime;

        private float Process;

        /// <summary>
        /// 此处GC 分配可以使用内存池优化
        /// </summary>
        /// <returns></returns>
        static public CameraShakeLogic Create(CinemachineVirtualCameraBase virtualCamera, AnimationCurve amplitudeGain, AnimationCurve frequencyGain, float len,int index)
        {
            CameraShakeLogic cameraShake = new CameraShakeLogic();
            cameraShake.VirtualCamera = virtualCamera;
            cameraShake.AmplitudeGain = amplitudeGain;
            cameraShake.FrequencyGain = frequencyGain;
            cameraShake.Length = len;
            cameraShake.Index = index;
            cameraShake.CurTime = 0;
            cameraShake.Process = 0;
            bool _init = cameraShake.Init();
            cameraShake._shakeStage = _init ? ShakeStage.Init : ShakeStage.Finished;
            return cameraShake;
        }


        private bool Init()
        {
            if (m_VirtualCameraPerlinList == null)
            {
                m_VirtualCameraPerlinList = new List<CinemachineBasicMultiChannelPerlin>();
            }
            m_VirtualCameraPerlinList.Clear();

            if (VirtualCamera is CinemachineVirtualCamera cinemachine)
            {
                var perlin = cinemachine.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                if (!perlin)
                {
                    perlin = cinemachine.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                }
                m_VirtualCameraPerlinList.Add(perlin);
            }
            else if (VirtualCamera is CinemachineFreeLook freeLook)
            {
                for (int i = 0; i < 2; i++)
                {
                    var rig = freeLook.GetRig(i);
                    if (rig == null)
                    {
                        continue; ;
                    }

                    var perlin = rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                    if (!perlin)
                    {
                        perlin = rig.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                    }
                    m_VirtualCameraPerlinList.Add(perlin);
                }
            }
            if (m_VirtualCameraPerlinList.Count <= 0)
            {
                return false;
            }
            if (Length <= 0)
            {
                return false;
            }
            return true;

        }
        // Update is called once per frame
        public void OnUpdate()
        {
            if(_shakeStage==ShakeStage.Finished)
            {
                return;
            }

            if (_shakeStage == ShakeStage.Init)
            {
                _shakeStage = ShakeStage.Shaking;
            }

            if (_shakeStage == ShakeStage.Shaking)
            {
                if (m_VirtualCameraPerlinList != null)
                {
                    Process = CurTime / Length;
                    foreach (var perlin in m_VirtualCameraPerlinList)
                    {
                        perlin.m_AmplitudeGain = AmplitudeGain.Evaluate(Process);
                        perlin.m_FrequencyGain = FrequencyGain.Evaluate(Process);
                    }
                    if (Process >= 1)
                    {
                        OnFinishedHandler();
                    }
                }
                else
                {
                    OnFinishedHandler();
                }

                CurTime += UnityEngine.Time.deltaTime;
            }
        }

        public void OnStop()
        {
            OnFinishedHandler() ;
        }

        private void OnFinishedHandler()
        {
            _shakeStage = ShakeStage.Finished;
            if (m_VirtualCameraPerlinList != null)
            {
                foreach (var perlin in m_VirtualCameraPerlinList)
                {
                    perlin.m_AmplitudeGain =0;
                    perlin.m_FrequencyGain = 0;
                }
            }
        }
    }

    public enum ShakeStage
    {
        None,
        Init,
        Shaking,
        Finished,
    }
}