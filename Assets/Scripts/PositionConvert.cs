using SGF.UI.Framework;
using StarProject.Service.Cam;
using UnityEngine;

[XLua.LuaCallCSharp]
public class PositionConvert
{

    static Camera _BattleCamera;
    public static Camera BattleCamera
    {
        get
        {
            if (_BattleCamera == null)
            {
                _BattleCamera = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera;
            }
            return _BattleCamera;
        }
    }

    static Canvas _Canvas;
    public static Canvas Canvas
    {
        get
        {
            if (_Canvas == null)
            {
                _Canvas = UIManager.Instance.M_Canvas;
            }
            return _Canvas;
        }
        set => _Canvas = value;
    }

    /// <summary>
    /// 世界坐标转换为屏幕坐标
    /// </summary>
    /// <param name="worldPoint">屏幕坐标</param>
    /// <returns></returns>
    public static Vector2 WorldPointToScreenPoint(Camera camera, Vector3 worldPoint)
    {
        // Camera.main 世界摄像机
        Vector2 screenPoint = camera.WorldToScreenPoint(worldPoint);
        return screenPoint;
    }

    /// <summary>
    /// 屏幕坐标转换为世界坐标
    /// </summary>
    /// <param name="screenPoint">屏幕坐标</param>
    /// <param name="planeZ">距离摄像机 Z 平面的距离</param>
    /// <returns></returns>
    public static Vector3 ScreenPointToWorldPoint(Camera camera, Vector2 screenPoint, float planeZ)
    {
        // Camera.main 世界摄像机
        Vector3 position = new Vector3(screenPoint.x, screenPoint.y, planeZ);
        Vector3 worldPoint = camera.ScreenToWorldPoint(position);
        return worldPoint;
    }

    
    // RectTransformUtility.WorldToScreenPoint
    // RectTransformUtility.ScreenPointToWorldPointInRectangle
    // RectTransformUtility.ScreenPointToLocalPointInRectangle
    // 上面三个坐标转换的方法使用 Camera 的地方
    // 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 传递参数 canvas.worldCamera
    // 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 传递参数 null
    
    // UI 坐标转换为屏幕坐标
    public static Vector2 UIPointToScreenPoint(Camera uiCamera, Vector3 worldPoint)
    {
        // RectTransform：target
        // worldPoint = target.position;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPoint);
        return screenPoint;
    }

    /// <summary>
    /// 屏幕坐标转换为 UGUI 坐标
    /// </summary>
    /// <param name="uiCamera"></param>
    /// <param name="rt">需要改变坐标的父物体的RectTransform</param>
    /// <param name="screenPoint"></param>
    /// <returns></returns>
    public static Vector3 ScreenPointToUIPoint(Camera uiCamera, RectTransform rt, Vector2 screenPoint)
    {
        Vector3 globalMousePos;
        //UI屏幕坐标转换为世界坐标

        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 uiCamera 不能为空
        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 uiCamera 可以为空
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, screenPoint, uiCamera, out globalMousePos);
        // 转换后的 globalMousePos 使用下面方法赋值
        // target 为需要使用的 UI RectTransform
        // rt 可以是 target.GetComponent<RectTransform>(), 也可以是 target.parent.GetComponent<RectTransform>()
        // target.transform.position = globalMousePos;
        return globalMousePos;
    }

    // 屏幕坐标转换为 UGUI RectTransform 的 anchoredPosition
    public static Vector2 ScreenPointToUILocalPoint(Camera uiCamera, RectTransform parentRT, Vector2 screenPoint)
    {
        Vector2 localPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenPoint, uiCamera, out localPos);
        // 转换后的 localPos 使用下面方法赋值
        // target 为需要使用的 UI RectTransform
        // parentRT 是 target.parent.GetComponent<RectTransform>()
        // 最后赋值 target.anchoredPosition = localPos;
        return localPos;
    }

    public static Vector3 UIPostoScreenPos(Canvas canvas, Vector3 uipos)
    {
        float width = canvas.GetComponent<RectTransform>().rect.width;
        float height = canvas.GetComponent<RectTransform>().rect.height;
        float screenWidth = UnityEngine.Screen.width;
        float screenHeight = UnityEngine.Screen.height;
        uipos = new Vector3((uipos.x * (screenWidth / width)) + (screenWidth / 2),
            uipos.y * (screenHeight / height) + (screenHeight / 2),
            0);
        return uipos;
    }


    // 屏幕坐标转UI（canvas用overlay的模式）坐标
    public static Vector2 ScreenPointToLocalPointInRectangle(Vector3 screenPosition)
    {

        Vector2 canvasPosition = Vector2.zero;

        // 将屏幕坐标转换为Canvas坐标
        RectTransform canvasRectTransform = Canvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, null, out canvasPosition);

        return canvasPosition;
    }

    // 场景3D坐标转UI（canvas用overlay的模式）坐标
    public static Vector2 ConvertWorldToCanvasPosition(Vector3 worldPosition)
    {
        Vector2 canvasPosition = Vector2.one * 100000;
        Vector3 screenPosition = Vector3.zero;
        if (IsPointInFrustum(worldPosition) == false)
        {

        }
        else
        {
            // 将3D世界坐标转换为屏幕坐标
            screenPosition = BattleCamera.WorldToScreenPoint(worldPosition);
            // 将屏幕坐标转换为Canvas坐标
            RectTransform canvasRectTransform = Canvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, null, out canvasPosition);
        }
        //SGF.Debuger.LogWarning($"名字测试 主角创建 场景3D坐标转UI worldPosition={worldPosition},BattleCamera={BattleCamera.transform.position},canvasPosition={canvasPosition}");

        return canvasPosition;

        //Vector2 canvasPosition = Vector2.zero;

        //// 将3D世界坐标转换为屏幕坐标
        //Vector3 screenPosition = BattleCamera.WorldToScreenPoint(worldPosition);


        //return ScreenPointToLocalPointInRectangle(screenPosition);
    }

    /// <summary>
    /// 是否在可视范围内
    /// </summary>
    /// <param name="WorldPoint">世界坐标</param>
    /// <returns></returns>
    public static bool IsPointInFrustum(Vector3 WorldPoint)
    {
        bool isPointInFrustum = false;
        Vector3 viewSpaceCenter = BattleCamera.WorldToViewportPoint(WorldPoint);
        // 检查模型中心点是否在视锥之内
        if (viewSpaceCenter.x >= 0 && viewSpaceCenter.x <= 1 &&
            viewSpaceCenter.y >= 0 && viewSpaceCenter.y <= 1 &&
            viewSpaceCenter.z > 0)
        {
            isPointInFrustum = true;
            //SGF.Debuger.LogError("名字测试 坐标在相机视锥之内111111111111111");
        }

        var pos = BattleCamera.WorldToScreenPoint(WorldPoint);
        if (0 < pos.x && pos.x < Screen.width && pos.y > 0 && pos.y < Screen.height && pos.z > 0)
        {
            // 在屏幕内了
            isPointInFrustum = true;
            //SGF.Debuger.LogError("名字测试 坐标在相机视锥之内2222222222222222");
        }
        //Plane[] planes = GeometryUtility.CalculateFrustumPlanes(BattleCamera);
        //if (planes != null && planes.Length > 0)
        //{
        //    for (int i = 0, iMax = planes.Length; i < iMax; ++i)
        //    {
        //        //判断一个点是否在平面的正方向上
        //        if (!planes[i].GetSide(WorldPoint))
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}
        return isPointInFrustum;
    }

}

