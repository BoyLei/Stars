using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
namespace SkillEditor
{
    public class CameraShakeMixer : PlayableBehaviour
    {
        private Camera m_Camera;
        private CinemachineBrain m_CameraBrain;
        private CinemachineVirtualCamera m_VirtualCamera;
        private CinemachineFreeLook m_FreeLookCamera;
        private List<CinemachineBasicMultiChannelPerlin> m_VirtualCameraPerlinList;


        bool InitVirtualCamera()
        {
            if (m_Camera == null)
            {
                //if (Application.isPlaying)
                //{
                //    m_Camera =CameraManager.Instance.CurrentPlayCamera.Camera;
                //}

                if (m_Camera == null)
                {
                    m_Camera = Camera.main;
                }
            }

            if (m_Camera == null)
            {
                return false;
            }

            if (m_CameraBrain == null)
            {
                m_CameraBrain = m_Camera.GetComponent<CinemachineBrain>();
            }

            if (m_CameraBrain == null)
            {
                return false;
            }

            if (m_VirtualCameraPerlinList == null)
            {
                m_VirtualCameraPerlinList = new List<CinemachineBasicMultiChannelPerlin>();
            }
            m_VirtualCameraPerlinList.Clear();

            m_VirtualCamera = m_CameraBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
            if (m_VirtualCamera)
            {
                var perlin = m_VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                if (!perlin)
                {
                    perlin = m_VirtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                }
                m_VirtualCameraPerlinList.Add(perlin);
            }

            m_FreeLookCamera = m_CameraBrain.ActiveVirtualCamera as CinemachineFreeLook;
            if (m_FreeLookCamera)
            {
                for (int i = 0; i < 2; i++)
                {
                    var rig = m_FreeLookCamera.GetRig(i);
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

#if UNITY_EDITOR
            bool isSolo = (CinemachineBrain.SoloCamera == (ICinemachineCamera)m_CameraBrain.ActiveVirtualCamera);
            if (!isSolo)
            {
                CinemachineBrain.SoloCamera = m_CameraBrain.ActiveVirtualCamera;
                Cinemachine.Editor.InspectorUtility.RepaintGameView();
            }
#endif
            //   m_VirtualCamera = m_CameraBrain.ActiveVirtualCamera.State=CameraState.
            return true;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            base.OnBehaviourPause(playable, info);
            if (m_VirtualCameraPerlinList == null || m_VirtualCameraPerlinList.Count < 1)
            {
                return;
            }

            for (int i = 0; i < playable.GetInputCount(); i++)
            {
                foreach (var perlin in m_VirtualCameraPerlinList)
                {
                    perlin.m_AmplitudeGain = 0;
                    perlin.m_FrequencyGain = 0;
                }
            }
        }
        public override void OnPlayableCreate(Playable playable)
        {

        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!InitVirtualCamera())
            {
                return;
            }
            for (int i = 0; i < playable.GetInputCount(); i++)
            {
                float weight = playable.GetInputWeight(i);
                var clipPlayable = (ScriptPlayable<CameraShakeBehavior>)playable.GetInput(i);
                CameraShakeBehavior behaviour = clipPlayable.GetBehaviour();
                float curTime = (float)(clipPlayable.GetTime() / clipPlayable.GetDuration());
                if (weight == 1.0f)
                {
                    bool pause = false;
                    //if (BattleCore.BattleHelper.BattleTime != null)
                    //{
                    //    pause = BattleCore.BattleHelper.BattleTime.PauseGame;
                    //}
                    foreach (var perlin in m_VirtualCameraPerlinList)
                    {
                        perlin.m_AmplitudeGain = pause ? 0 : behaviour.AmplitudeGain.Evaluate(curTime);
                        perlin.m_FrequencyGain = pause ? 0 : behaviour.FrequencyGain.Evaluate(curTime);
                    }
                }
            }
        }

        public void OnGraphStop(Playable playable)
        {
            if (m_VirtualCameraPerlinList == null || m_VirtualCameraPerlinList.Count <= 0)
            {
                return;
            }

            foreach (var perlin in m_VirtualCameraPerlinList)
            {
                perlin.m_AmplitudeGain = 0;
                perlin.m_FrequencyGain = 0;
            }
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            base.OnPlayableDestroy(playable);

            if (m_VirtualCameraPerlinList == null || m_VirtualCameraPerlinList.Count <= 0)
            {
                return;
            }

            foreach (var perlin in m_VirtualCameraPerlinList)
            {
                perlin.m_AmplitudeGain = 0;
                perlin.m_FrequencyGain = 0;
            }

        }
    }
}
