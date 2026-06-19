using SGF;
using SGF.Unity;
using StarProject.Service.Cam;
using StarProject.Service.Cam.Data;
using StarProjectDef;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using StarProject.Game.Entity.VitalSigns;

namespace StarProject.Game.StarsCamera
{
    /// <summary>
    /// 触摸，拖拽，在其他脚本|长按在按钮上
    /// 这里是摄像机如何旋转
    /// 相机设定和RawImage我关联好了
    /// 使用者可以直接ui里面建立一个RawImage---关联我的物理RawTexture
    /// 理论上其实Rawtexture就是Camera渲染的实时结果101
    /// </summary>

    public class RawCamera : CameraBase
    {
     
        [SerializeField]
        private Transform M_myParentRoot;
        private float _cameraHeight;
        //DynamicCamRoot是放一堆相机的
        //BCameraRoot 才是控制节点（段磊控制这个）TODO

        private const string flagKey = "[RawCamera]";


        protected override void OnAwake()
        {

            //Camera.targetTexture = Service.Resource.ResourceManager.Instance.RenderTexture;

            //Camera.SetTargetBuffers(Service.Resource.ResourceManager.Instance.RenderTexture.colorBuffer, Service.Resource.ResourceManager.Instance.RenderTextureDepth.depthBuffer);
        }

        protected override E_CameraType GetCameraType()
        {
            return E_CameraType.SpecialCam;
        }

        public override void DoCameraAction(List<CameraFxParam> cameraFxParams, int cameraEventID)
        {
            throw new System.NotImplementedException();
        }

        public override void StopCameraAction(CameraEvent cameraEvent, int cameraEventID)
        {
            throw new System.NotImplementedException();
        }


        public void SetState(bool isActive)
        {
            gameObject.SetActive(isActive);
            if (isActive == false && Service.Resource.ResourceFormalManager.Instance.RenderTexture!=null)
            {
                Camera.targetTexture = null;
                Service.Resource.ResourceFormalManager.Instance.RenderTexture.Release();
                Destroy(Service.Resource.ResourceFormalManager.Instance.RenderTexture);
                Service.Resource.ResourceFormalManager.Instance.RenderTexture = null;//Des以后引用内存指针还在，只不过内存空间的图没了，所以取消堆栈关联
                //Destroy(tex);
            }
            else
            {
                Camera.targetTexture = Service.Resource.ResourceFormalManager.Instance.RenderTexture;
            }
            
        }
        public void Start()
        {
            gameObject.SetActive(false);
        }
    }
}
