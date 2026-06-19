///--------------------------------------------------------------------
/// 文件名   :   ModifyCameraHeight.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/01/11 18:11:14
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProject;
using StarProject.Service.Cam;
using StarProjectDef;

public class ModifyCameraHeight : ClientSceneEvent
{
    public float Height;

    public override void OnEnter()
    {
        GlobalEvent.OnModifyCameraHeight?.Invoke(Height);
    }

    public override void OnExit()
    {
        GlobalEvent.OnModifyCameraHeight?.Invoke(CameraManager.CurDistance);
    }
}
