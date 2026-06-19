///--------------------------------------------------------------------
/// 文件名   :   StarCinemachineShot.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/15 10:44:10
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;
using Sirenix.OdinInspector;

namespace SkillEditor
{
    public class StarCinemachineShot : PlayableAsset, IPropertyPreview
    {
        [HideInInspector]
        public string DisplayName;

        /// <summary>The virtual camera to activate</summary>
        public ExposedReference<CinemachineVirtualCameraBase> VirtualCamera;

        [HideLabel]
        public ClipCinemachineBehaviour template = new ClipCinemachineBehaviour();

        /// <summary>PlayableAsset implementation</summary>
        /// <param name="graph"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<StarCinemachineShotPlayable>.Create(graph);
            playable.GetBehaviour().VirtualCamera = VirtualCamera.Resolve(graph.GetResolver());
            return playable;
        }

        /// <summary>IPropertyPreview implementation</summary>
        /// <param name="director"></param>
        /// <param name="driver"></param>
        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            driver.AddFromName<Transform>("m_LocalPosition.x");
            driver.AddFromName<Transform>("m_LocalPosition.y");
            driver.AddFromName<Transform>("m_LocalPosition.z");
            driver.AddFromName<Transform>("m_LocalRotation.x");
            driver.AddFromName<Transform>("m_LocalRotation.y");
            driver.AddFromName<Transform>("m_LocalRotation.z");
            driver.AddFromName<Transform>("m_LocalRotation.w");

            driver.AddFromName<Camera>("field of view");
            driver.AddFromName<Camera>("near clip plane");
            driver.AddFromName<Camera>("far clip plane");
        }
    }

}
