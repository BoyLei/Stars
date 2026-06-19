///--------------------------------------------------------------------
/// 文件名   :   StarCinemachineShotPlayable.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/15 10:48:30
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------


using UnityEngine.Playables;
using Cinemachine;
namespace SkillEditor
{
    internal sealed class StarCinemachineShotPlayable : PlayableBehaviour
    {
        public CameraCustomData cameraCustomData;

        public CinemachineVirtualCameraBase VirtualCamera;
        public bool IsValid { get { return VirtualCamera != null; } }
    }
}
